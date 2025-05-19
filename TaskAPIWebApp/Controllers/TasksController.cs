using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAPIWebApp;
using System.ComponentModel;
using TaskAPIWebApp.Models; // Дозволяє використовувати TaskInputDto, якщо він там
// using TaskAPIWebApp.Models.Dtos; // Якщо TaskInputDto в підпапці Dtos
using System.Threading.Tasks; // Додано для Task
using System.Linq; // Додано для .Any()


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
        public async Task<ActionResult<IEnumerable<TaskAPIWebApp.Models.Task>>> GetTasks(string? status)
        {
            var query = _context.Tasks.AsQueryable();
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }
            return await query.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskAPIWebApp.Models.Task>> GetTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound(new { message = "Завдання не знайдено." });
            }
            return task;
        }

        [HttpPost]
        public async Task<ActionResult<TaskAPIWebApp.Models.Task>> CreateTask(TaskInputDto taskDto) // Змінено на TaskInputDto
        {
            // ModelState.IsValid перевіряється автоматично завдяки [ApiController]
            // але можна додати явну перевірку для ясності або кастомної логіки
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Поверне стандартні помилки валідації DTO
            }

            if (!await UserExists(taskDto.UserId))
            {
                return BadRequest(new { message = "Користувач із вказаним ID не існує." });
            }

            if (taskDto.TaskGroupId.HasValue)
            {
                if (!await _context.TaskGroups.AnyAsync(tg => tg.Id == taskDto.TaskGroupId.Value)) // Використовуємо AnyAsync
                {
                    return BadRequest(new { message = "Група із таким ID не існує." });
                }
                if (!await _context.GroupMembers.AnyAsync(gm => gm.UserId == taskDto.UserId && gm.TaskGroupId == taskDto.TaskGroupId.Value)) // Використовуємо AnyAsync
                {
                    return BadRequest(new { message = "Користувач не є членом групи, до якої належить завдання." });
                }
            }

            var task = new TaskAPIWebApp.Models.Task
            {
                Description = taskDto.Description,
                Status = taskDto.Status,
                UserId = taskDto.UserId,
                TaskGroupId = taskDto.TaskGroupId
                // Інші властивості моделі Task, якщо є, будуть мати значення за замовчуванням або null
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            // Повертаємо повний об'єкт Task, який включає згенерований Id та інші поля
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskInputDto taskDto) // Змінено на TaskInputDto
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

            // Перевірка існування користувача, якому призначається завдання
            if (!await UserExists(taskDto.UserId))
            {
                return BadRequest(new { message = "Користувач (UserId з тіла запиту) із вказаним ID не існує." });
            }

            // Перевірка групи та членства, якщо група вказана
            if (taskDto.TaskGroupId.HasValue)
            {
                if (!await _context.TaskGroups.AnyAsync(tg => tg.Id == taskDto.TaskGroupId.Value))
                {
                    return BadRequest(new { message = "Група із таким ID не існує." });
                }
                // Важливо: перевірка членства для нового UserId, якщо UserId змінюється
                if (!await _context.GroupMembers.AnyAsync(gm => gm.UserId == taskDto.UserId && gm.TaskGroupId == taskDto.TaskGroupId.Value))
                {
                    return BadRequest(new { message = "Користувач (UserId з тіла запиту) не є членом вказаної групи." });
                }
            }

            // Оновлюємо властивості існуючого завдання з DTO
            taskToUpdate.Description = taskDto.Description;
            taskToUpdate.Status = taskDto.Status;
            taskToUpdate.UserId = taskDto.UserId; // Дозволяємо зміну UserId
            taskToUpdate.TaskGroupId = taskDto.TaskGroupId; // Дозволяємо зміну TaskGroupId

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TaskExists(id)) // Додано await
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

            if (await _context.TaskSubmissions.AnyAsync(ts => ts.TaskId == id)) // Використовуємо AnyAsync
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
                .Select(t => new { t.Id, t.Description })
                .OrderBy(t => t.Description)
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