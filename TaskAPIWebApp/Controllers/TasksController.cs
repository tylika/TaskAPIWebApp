using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAPIWebApp;
using System.ComponentModel;
using TaskAPIWebApp.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace TaskAPIWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [DisplayName("Завдання")]
    [Produces("application/json")]
    public class TasksController : ControllerBase
    {
        private readonly TaskManagementApiContext _context;

        public TasksController(TaskManagementApiContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTasks(string? status)
        {
            var query = _context.Tasks
                                .Include(t => t.User) // ОБОВ'ЯЗКОВО для доступу до t.User.Username
                                .Include(t => t.TaskGroup) // ОБОВ'ЯЗКОВО для доступу до t.TaskGroup.Name
                                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }

            // Проектування на анонімний об'єкт з потрібними полями
            var tasksProjection = await query.Select(t => new
            {
                t.Id,
                t.Description,
                t.Status,
                t.UserId, // Це UserId числове, воно потрібне для форм редагування
                UserUsername = t.User != null ? t.User.Username : null, // Ось поле з ім'ям користувача
                t.TaskGroupId, // Це TaskGroupId числове, потрібне для форм редагування
                TaskGroupName = t.TaskGroup != null ? t.TaskGroup.Name : null // Ось поле з назвою групи
            }).ToListAsync();

            return Ok(tasksProjection);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTask(int id)
        {
            var task = await _context.Tasks
                                   .Include(t => t.User)
                                   .Include(t => t.TaskGroup)
                                   .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound(new { message = "Завдання не знайдено." });
            }

            var taskProjection = new
            {
                task.Id,
                task.Description,
                task.Status,
                task.UserId,
                UserUsername = task.User != null ? task.User.Username : null,
                task.TaskGroupId,
                TaskGroupName = task.TaskGroup != null ? task.TaskGroup.Name : null
            };

            return Ok(taskProjection);
        }

        // Методи CreateTask, UpdateTask, DeleteTask і т.д.
        // Важливо, щоб CreateTask також повертав об'єкт з UserUsername та TaskGroupName, якщо це потрібно фронтенду одразу після створення

        [HttpPost]
        public async Task<ActionResult<object>> CreateTask(TaskInputDto taskDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // ... (ваші перевірки UserExists, TaskGroupExists, GroupMemberExists) ...

            var task = new Models.Task
            {
                Description = taskDto.Description,
                Status = taskDto.Status,
                UserId = taskDto.UserId,
                TaskGroupId = taskDto.TaskGroupId
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Повторно завантажуємо з Include для отримання імен
            var createdTaskWithDetails = await _context.Tasks
                .Where(t => t.Id == task.Id)
                .Include(t => t.User)
                .Include(t => t.TaskGroup)
                .Select(t => new {
                    t.Id,
                    t.Description,
                    t.Status,
                    t.UserId,
                    UserUsername = t.User != null ? t.User.Username : null,
                    t.TaskGroupId,
                    TaskGroupName = t.TaskGroup != null ? t.TaskGroup.Name : null
                })
                .FirstOrDefaultAsync();

            if (createdTaskWithDetails == null)
            {
                return Problem("Помилка отримання створеного завдання з деталями.");
            }

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, createdTaskWithDetails);
        }

        // ... (решта методів контролера: UpdateTask, DeleteTask, TaskExists, UserExists) ...
        // Переконайтеся, що TaskInputDto використовується для вхідних даних в UpdateTask
        // і що логіка там коректна.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskInputDto taskDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var taskToUpdate = await _context.Tasks.FindAsync(id);

            if (taskToUpdate == null)
            {
                return NotFound(new { message = "Завдання не знайдено." });
            }

            if (!await UserExists(taskDto.UserId))
            {
                return BadRequest(new { message = "Користувач (UserId з тіла запиту) із вказаним ID не існує." });
            }

            if (taskDto.TaskGroupId.HasValue)
            {
                if (!await _context.TaskGroups.AnyAsync(tg => tg.Id == taskDto.TaskGroupId.Value))
                {
                    return BadRequest(new { message = "Група із таким ID не існує." });
                }
                if (!await _context.GroupMembers.AnyAsync(gm => gm.UserId == taskDto.UserId && gm.TaskGroupId == taskDto.TaskGroupId.Value))
                {
                    return BadRequest(new { message = "Користувач (UserId з тіла запиту) не є членом вказаної групи." });
                }
            }

            taskToUpdate.Description = taskDto.Description;
            taskToUpdate.Status = taskDto.Status;
            taskToUpdate.UserId = taskDto.UserId;
            taskToUpdate.TaskGroupId = taskDto.TaskGroupId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TaskExists(id))
                {
                    return NotFound(new { message = "Завдання не знайдено під час спроби зберегти зміни (конфлікт)." });
                }
                else
                {
                    return Conflict(new { message = "Конфлікт оновлення: завдання було змінено іншим користувачем." });
                }
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound(new { message = "Завдання не знайдено." });
            }

            if (await _context.TaskSubmissions.AnyAsync(ts => ts.TaskId == id))
            {
                return BadRequest(new { message = "Неможливо видалити завдання: у нього є подання." });
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("lookup")]
        public async Task<ActionResult<IEnumerable<object>>> GetTasksLookup()
        {
            return await _context.Tasks
                .Select(t => new { t.Id, Name = t.Description }) // Використовуємо Name для уніфікації з іншими lookup
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        private async Task<bool> TaskExists(int id)
        {
            return await _context.Tasks.AnyAsync(e => e.Id == id);
        }

        private async Task<bool> UserExists(int userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }
    }
}