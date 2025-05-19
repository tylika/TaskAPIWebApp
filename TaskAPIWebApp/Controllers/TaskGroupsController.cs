using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAPIWebApp;
using System.ComponentModel;
using TaskAPIWebApp.Models; // Для TaskGroup та TaskGroupInputDto
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace TaskAPIWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [DisplayName("Групи завдань")]
    [Produces("application/json")]
    public class TaskGroupsController : ControllerBase
    {
        private readonly TaskManagementApiContext _context;

        public TaskGroupsController(TaskManagementApiContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Отримати список усіх груп завдань
        /// </summary>
        /// <param name="sortByDate">Сортувати за датою створення (asc/desc) - використовує Id як проксі</param>
        /// <returns>Список груп завдань з іменами власників</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTaskGroups(string sortByDate = "asc")
        {
            var query = _context.TaskGroups
                .Include(tg => tg.User) // Включаємо дані власника
                .Select(tg => new
                {
                    tg.Id,
                    tg.Name,
                    tg.UserId,
                    OwnerUsername = tg.User.Username // Додаємо ім'я власника
                    // Можна додати кількість завдань або членів, якщо потрібно
                    // TaskCount = tg.Tasks.Count(),
                    // MemberCount = tg.GroupMembers.Count()
                });

            if (sortByDate.ToLower() == "desc")
            {
                query = query.OrderByDescending(tg => tg.Id);
            }
            else
            {
                query = query.OrderBy(tg => tg.Id);
            }
            return await query.ToListAsync();
        }

        [HttpGet("lookup")]
        public async Task<ActionResult<IEnumerable<object>>> GetTaskGroupsLookup()
        {
            var taskGroups = await _context.TaskGroups
                .Select(tg => new { tg.Id, tg.Name })
                .OrderBy(tg => tg.Name)
                .ToListAsync();
            return Ok(taskGroups);
        }

        /// <summary>
        /// Отримати групу завдань за ID
        /// </summary>
        /// <param name="id">Ідентифікатор групи завдань</param>
        /// <returns>Група завдань з указаним ID та ім'ям власника</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTaskGroup(int id) // Змінив тип повернення для деталей
        {
            var taskGroup = await _context.TaskGroups
                .Include(tg => tg.User)
                .Where(tg => tg.Id == id)
                .Select(tg => new
                {
                    tg.Id,
                    tg.Name,
                    tg.UserId,
                    OwnerUsername = tg.User.Username
                })
                .FirstOrDefaultAsync();

            if (taskGroup == null)
            {
                return NotFound(new { message = $"Група завдань з ID {id} не знайдена." });
            }
            return taskGroup;
        }

        /// <summary>
        /// Створити нову групу завдань
        /// </summary>
        /// <param name="dto">Дані нової групи завдань</param>
        /// <returns>Створена група завдань з деталями</returns>
        [HttpPost]
        public async Task<ActionResult<object>> CreateTaskGroup(TaskGroupInputDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await _context.Users.AnyAsync(u => u.Id == dto.UserId))
            {
                return BadRequest(new { message = $"Власник групи з ID {dto.UserId} не існує." });
            }

            var taskGroup = new TaskGroup
            {
                Name = dto.Name,
                UserId = dto.UserId
            };

            _context.TaskGroups.Add(taskGroup);
            await _context.SaveChangesAsync();

            // Повертаємо розширений об'єкт для відповідності GetTaskGroup
            var createdGroupDetails = await _context.TaskGroups
                .Include(tg => tg.User)
                .Where(tg => tg.Id == taskGroup.Id)
                .Select(tg => new { tg.Id, tg.Name, tg.UserId, OwnerUsername = tg.User.Username })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetTaskGroup), new { id = taskGroup.Id }, createdGroupDetails);
        }

        /// <summary>
        /// Оновити існуючу групу завдань
        /// </summary>
        /// <param name="id">Ідентифікатор групи завдань</param>
        /// <param name="dto">Оновлені дані групи завдань</param>
        /// <returns>Статус операції</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTaskGroup(int id, TaskGroupInputDto dto) // Використовуємо DTO
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var taskGroupToUpdate = await _context.TaskGroups.FindAsync(id);
            if (taskGroupToUpdate == null)
            {
                return NotFound(new { message = "Група завдань не знайдена." });
            }

            if (!await _context.Users.AnyAsync(u => u.Id == dto.UserId))
            {
                return BadRequest(new { message = $"Новий власник групи з ID {dto.UserId} не існує." });
            }

            taskGroupToUpdate.Name = dto.Name;
            taskGroupToUpdate.UserId = dto.UserId; // Дозволяємо зміну власника

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TaskGroupExists(id)) // Додав await
                {
                    return NotFound();
                }
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskGroup(int id)
        {
            var taskGroup = await _context.TaskGroups.FindAsync(id);
            if (taskGroup == null)
            {
                return NotFound(new { message = "Група завдань не знайдена." });
            }

            if (await _context.Tasks.AnyAsync(t => t.TaskGroupId == id))
            {
                return BadRequest(new { message = "Неможливо видалити групу: у ній є активні завдання." });
            }

            // Додатково: що робити з членами групи (GroupMembers)?
            // За замовчуванням, якщо є зовнішній ключ з GroupMembers на TaskGroups,
            // і не налаштовано каскадне видалення, база даних може заборонити видалення.
            // Потрібно або видаляти членів групи спочатку, або налаштувати каскадне видалення.
            // Або, якщо логіка дозволяє, залишити цю перевірку.
            if (await _context.GroupMembers.AnyAsync(gm => gm.TaskGroupId == id))
            {
                // Можна або видалити членів, або повернути помилку
                // _context.GroupMembers.RemoveRange(_context.GroupMembers.Where(gm => gm.TaskGroupId == id));
                return BadRequest(new { message = "Неможливо видалити групу: у ній є зареєстровані члени. Спочатку видаліть членів." });
            }


            _context.TaskGroups.Remove(taskGroup);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<bool> TaskGroupExists(int id) // Зробив асинхронним
        {
            return await _context.TaskGroups.AnyAsync(e => e.Id == id);
        }
    }
}