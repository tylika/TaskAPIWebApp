using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAPIWebApp;
using System.ComponentModel;
using TaskAPIWebApp.Models; // Для TaskAttachment та DTO
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace TaskAPIWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [DisplayName("Вкладення завдань")]
    [Produces("application/json")]
    public class TaskAttachmentsController : ControllerBase
    {
        private readonly TaskManagementApiContext _context;
        // Для реального завантаження файлів потрібен IWebHostEnvironment
        // private readonly IWebHostEnvironment _hostingEnvironment;

        public TaskAttachmentsController(TaskManagementApiContext context /*, IWebHostEnvironment hostingEnvironment */)
        {
            _context = context;
            // _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Отримати список усіх вкладень завдань
        /// </summary>
        /// <param name="taskId">Фільтр за ID завдання (опціонально)</param>
        /// <returns>Список вкладень завдань з деталями завдання</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTaskAttachments(int? taskId)
        {
            var query = _context.TaskAttachments
                .Include(ta => ta.Task) // Включаємо дані завдання
                .Select(ta => new
                {
                    ta.Id,
                    ta.TaskId,
                    TaskDescription = ta.Task != null ? ta.Task.Description : "N/A", // Додаємо опис завдання
                    ta.FilePath,
                    ta.UploadedAt
                });

            if (taskId.HasValue)
            {
                query = query.Where(ta => ta.TaskId == taskId.Value);
            }
            return await query.OrderByDescending(ta => ta.UploadedAt).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTaskAttachment(int id)
        {
            var taskAttachment = await _context.TaskAttachments
                .Include(ta => ta.Task)
                .Where(ta => ta.Id == id)
                .Select(ta => new
                {
                    ta.Id,
                    ta.TaskId,
                    TaskDescription = ta.Task != null ? ta.Task.Description : "N/A",
                    ta.FilePath,
                    ta.UploadedAt
                })
                .FirstOrDefaultAsync();

            if (taskAttachment == null)
            {
                return NotFound(new { message = $"Вкладення з ID {id} не знайдено." });
            }
            return taskAttachment;
        }

        /// <summary>
        /// Створити нове вкладення завдання (поточна версія приймає FilePath)
        /// </summary>
        /// <param name="dto">Дані нового вкладення</param>
        /// <returns>Створене вкладення</returns>
        [HttpPost]
        // Для реального завантаження: public async Task<ActionResult<TaskAttachment>> CreateTaskAttachment([FromForm] TaskAttachmentUploadDto dto)
        public async Task<ActionResult<TaskAttachment>> CreateTaskAttachment(TaskAttachmentInputDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.FilePath.Length > 1000) // Існуюча імітація
            {
                return BadRequest(new { message = "Шлях до файлу занадто довгий (імітація обмеження розміру)." });
            }

            if (!await _context.Tasks.AnyAsync(t => t.Id == dto.TaskId))
            {
                return BadRequest(new { message = $"Завдання із ID {dto.TaskId} не існує." });
            }

            // Тут мала б бути логіка завантаження файлу на сервер,
            // отримання реального шляху збереженого файлу і запис його в FilePath.
            // Наприклад:
            // string uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.File.FileName;
            // string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", uniqueFileName);
            // using (var fileStream = new FileStream(filePath, FileMode.Create))
            // {
            //     await dto.File.CopyToAsync(fileStream);
            // }
            // savedFilePath = "/uploads/" + uniqueFileName; // Шлях для доступу через веб

            var taskAttachment = new TaskAttachment
            {
                TaskId = dto.TaskId,
                FilePath = dto.FilePath, // Поки що беремо з DTO
                UploadedAt = DateTime.UtcNow
            };

            _context.TaskAttachments.Add(taskAttachment);
            await _context.SaveChangesAsync();

            // Повертаємо розширений об'єкт
            var createdAttachmentDetails = await _context.TaskAttachments
               .Include(ta => ta.Task)
               .Where(ta => ta.Id == taskAttachment.Id)
               .Select(ta => new {
                   ta.Id,
                   ta.TaskId,
                   TaskDescription = ta.Task.Description,
                   ta.FilePath,
                   ta.UploadedAt
               })
               .FirstOrDefaultAsync();


            return CreatedAtAction(nameof(GetTaskAttachment), new { id = taskAttachment.Id }, createdAttachmentDetails);
        }


        /// <summary>
        /// Оновити існуюче вкладення завдання (поточна версія приймає TaskAttachment)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTaskAttachment(int id, TaskAttachmentUpdateDto dto) // Можна використати DTO
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var taskAttachmentToUpdate = await _context.TaskAttachments.FindAsync(id);
            if (taskAttachmentToUpdate == null)
            {
                return NotFound(new { message = "Вкладення не знайдено." });
            }

            // Логіка оновлення. Зазвичай FilePath не змінюють напряму так просто.
            // Можливо, оновлюють метадані або замінюють файл (що є складнішою операцією).
            // Поки що, якщо DTO містить тільки FilePath:
            if (dto.FilePath.Length > 1000)
            {
                return BadRequest(new { message = "Новий шлях до файлу занадто довгий." });
            }
            taskAttachmentToUpdate.FilePath = dto.FilePath;
            // taskAttachmentToUpdate.TaskId не змінюємо, бо це прив'язка до іншої сутності.

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TaskAttachmentExists(id)) // Додав await
                {
                    return NotFound();
                }
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskAttachment(int id)
        {
            var taskAttachment = await _context.TaskAttachments.FindAsync(id);
            if (taskAttachment == null)
            {
                return NotFound(new { message = "Вкладення не знайдено." });
            }

            // Поточна логіка перевірки пов'язаних подань
            if (await _context.TaskSubmissions.AnyAsync(ts => ts.TaskId == taskAttachment.TaskId))
            {
                // Ця логіка може бути не зовсім коректною, якщо вкладення стосується конкретного завдання,
                // а не всіх подань до нього. Можливо, її варто переглянути або видалити.
                // return BadRequest(new { message = "Неможливо видалити вкладення: воно пов’язане з поданням." });
            }

            // TODO: Додати логіку фізичного видалення файлу з сервера/сховища
            // string filePathToDelete = Path.Combine(_hostingEnvironment.WebRootPath, taskAttachment.FilePath.TrimStart('/'));
            // if (System.IO.File.Exists(filePathToDelete))
            // {
            //     System.IO.File.Delete(filePathToDelete);
            // }

            _context.TaskAttachments.Remove(taskAttachment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<bool> TaskAttachmentExists(int id) // Зробив асинхронним
        {
            return await _context.TaskAttachments.AnyAsync(e => e.Id == id);
        }
    }
}