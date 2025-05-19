using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAPIWebApp;
using System.ComponentModel;
using TaskAPIWebApp.Models; // Для TaskSubmission та DTO
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic; // Для IEnumerable

namespace TaskAPIWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [DisplayName("Подання завдань")]
    [Produces("application/json")]
    public class TaskSubmissionsController : ControllerBase
    {
        private readonly TaskManagementApiContext _context;

        public TaskSubmissionsController(TaskManagementApiContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Отримати список усіх подань завдань
        /// </summary>
        /// <param name="userIdParam">Фільтр за ID користувача (опціонально) - змінив назву параметра</param>
        /// <param name="taskIdParam">Фільтр за ID завдання (опціонально) - додав новий параметр</param>
        /// <returns>Список подань завдань з деталями</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTaskSubmissions(
            [FromQuery(Name = "userId")] int? userIdParam,
            [FromQuery(Name = "taskId")] int? taskIdParam)
        {
            var query = _context.TaskSubmissions
                .Include(ts => ts.User)   // Включаємо дані користувача
                .Include(ts => ts.Task)   // Включаємо дані завдання
                .Select(ts => new
                {
                    ts.Id,
                    ts.TaskId,
                    TaskDescription = ts.Task.Description,
                    ts.UserId,
                    Username = ts.User.Username,
                    ts.Submission,
                    ts.Status,
                    ts.Score,
                    ts.SubmittedAt
                });

            if (userIdParam.HasValue)
            {
                query = query.Where(ts => ts.UserId == userIdParam.Value);
            }
            if (taskIdParam.HasValue)
            {
                query = query.Where(ts => ts.TaskId == taskIdParam.Value);
            }
            return await query.OrderByDescending(ts => ts.SubmittedAt).ToListAsync();
        }

        [HttpGet("lookup")]
        public async Task<ActionResult<IEnumerable<object>>> GetTaskSubmissionsLookup()
        {
            return await _context.TaskSubmissions
                .Include(ts => ts.User)
                .Include(ts => ts.Task)
                .OrderByDescending(ts => ts.SubmittedAt) // Новіші спочатку для вибору
                .Take(50) // Обмеження для lookup, щоб не завантажувати тисячі
                .Select(ts => new
                {
                    ts.Id,
                    DisplayText = $"Подання #{ts.Id} (Завд: {(ts.Task != null && ts.Task.Description != null ? ts.Task.Description.Substring(0, Math.Min(ts.Task.Description.Length, 20)) : "N/A")}... Користувач: {(ts.User != null ? ts.User.Username : "N/A")})"
                })
                .ToListAsync();
        }

        /// <summary>
        /// Отримати подання завдання за ID
        /// </summary>
        /// <param name="id">Ідентифікатор подання</param>
        /// <returns>Подання з указаним ID та деталями</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTaskSubmission(int id) // Змінив тип повернення
        {
            var taskSubmission = await _context.TaskSubmissions
                .Include(ts => ts.User)
                .Include(ts => ts.Task)
                .Where(ts => ts.Id == id)
                .Select(ts => new
                {
                    ts.Id,
                    ts.TaskId,
                    TaskDescription = ts.Task.Description,
                    ts.UserId,
                    Username = ts.User.Username,
                    ts.Submission,
                    ts.Status,
                    ts.Score,
                    ts.SubmittedAt
                })
                .FirstOrDefaultAsync();

            if (taskSubmission == null)
            {
                return NotFound(new { message = $"Подання з ID {id} не знайдено." });
            }
            return taskSubmission;
        }

        /// <summary>
        /// Створити нове подання завдання
        /// </summary>
        /// <param name="dto">Дані нового подання</param>
        /// <returns>Створене подання з деталями</returns>
        [HttpPost]
        public async Task<ActionResult<object>> CreateTaskSubmission(TaskSubmissionInputDto dto) // Використовуємо DTO
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var task = await _context.Tasks.FindAsync(dto.TaskId);
            if (task == null)
            {
                return BadRequest(new { message = $"Завдання із ID {dto.TaskId} не існує." });
            }
            if (!await _context.Users.AnyAsync(u => u.Id == dto.UserId)) // Перевірка існування користувача
            {
                return BadRequest(new { message = $"Користувач із ID {dto.UserId} не існує." });
            }


            if (task.TaskGroupId.HasValue)
            {
                if (!await _context.GroupMembers.AnyAsync(gm => gm.UserId == dto.UserId && gm.TaskGroupId == task.TaskGroupId.Value))
                {
                    return BadRequest(new { message = "Користувач не є членом групи, до якої належить завдання." });
                }
            }
            // Score валідується атрибутом Range в DTO

            var taskSubmission = new TaskSubmission
            {
                TaskId = dto.TaskId,
                UserId = dto.UserId,
                Submission = dto.Submission,
                Status = dto.Status,
                Score = dto.Score,
                SubmittedAt = DateTime.UtcNow
            };

            _context.TaskSubmissions.Add(taskSubmission);
            await _context.SaveChangesAsync();

            // Повертаємо розширений об'єкт
            var createdSubmissionDetails = await _context.TaskSubmissions
                .Include(ts => ts.User).Include(ts => ts.Task)
                .Where(ts => ts.Id == taskSubmission.Id)
                .Select(ts => new { ts.Id, ts.TaskId, TaskDescription = ts.Task.Description, ts.UserId, Username = ts.User.Username, ts.Submission, ts.Status, ts.Score, ts.SubmittedAt })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetTaskSubmission), new { id = taskSubmission.Id }, createdSubmissionDetails);
        }

        /// <summary>
        /// Оновити існуюче подання завдання (наприклад, статус та оцінку)
        /// </summary>
        /// <param name="id">Ідентифікатор подання</param>
        /// <param name="dto">Оновлені дані подання</param>
        /// <returns>Статус операції</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTaskSubmission(int id, TaskSubmissionUpdateDto dto) // Використовуємо DTO для оновлення
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var taskSubmissionToUpdate = await _context.TaskSubmissions.FindAsync(id);
            if (taskSubmissionToUpdate == null)
            {
                return NotFound(new { message = $"Подання з ID {id} не знайдено." });
            }

            // Зазвичай TaskId та UserId не змінюються для існуючого подання.
            // Оновлюємо тільки ті поля, які є в TaskSubmissionUpdateDto
            taskSubmissionToUpdate.Status = dto.Status;
            taskSubmissionToUpdate.Score = dto.Score;
            // if(dto.Submission != null) taskSubmissionToUpdate.Submission = dto.Submission; // Якщо дозволено оновлювати текст подання

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TaskSubmissionExists(id)) // Додав await
                {
                    return NotFound();
                }
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskSubmission(int id)
        {
            var taskSubmission = await _context.TaskSubmissions.FindAsync(id);
            if (taskSubmission == null)
            {
                return NotFound(new { message = $"Подання з ID {id} не знайдено." });
            }

            if (await _context.Comments.AnyAsync(c => c.TaskSubmissionId == id))
            {
                return BadRequest(new { message = "Неможливо видалити подання: до нього є коментарі." });
            }

            _context.TaskSubmissions.Remove(taskSubmission);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<bool> TaskSubmissionExists(int id) // Зробив асинхронним
        {
            return await _context.TaskSubmissions.AnyAsync(e => e.Id == id);
        }
    }
}