using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAPIWebApp;
using System.ComponentModel;
using TaskAPIWebApp.Models; // Для Comment та CommentInputDto
using System.Threading.Tasks; // Для Task
using System.Linq; // Для .Any()

namespace TaskAPIWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [DisplayName("Коментарі")]
    [Produces("application/json")]
    public class CommentsController : ControllerBase
    {
        private readonly TaskManagementApiContext _context;

        public CommentsController(TaskManagementApiContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments(int? taskSubmissionId, int? taskId)
        {
            var query = _context.Comments.AsQueryable();
            if (taskSubmissionId.HasValue)
            {
                query = query.Where(c => c.TaskSubmissionId == taskSubmissionId.Value);
            }
            if (taskId.HasValue)
            {
                query = query.Where(c => c.TaskId == taskId.Value);
            }
            // Для кращого відображення можна додати .Include(c => c.User)
            return await query.Include(c => c.User)
                              .OrderByDescending(c => c.CreatedAt) // Показуємо новіші спочатку
                              .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Comment>> GetComment(int id)
        {
            // Для кращого відображення можна додати .Include(c => c.User)
            var comment = await _context.Comments
                                        .Include(c => c.User)
                                        .FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null)
            {
                return NotFound(new { message = $"Коментар з ID {id} не знайдено." });
            }
            return comment;
        }

        [HttpPost]
        public async Task<ActionResult<Comment>> CreateComment(CommentInputDto commentDto) // Використовуємо DTO
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (commentDto.TaskId.HasValue && commentDto.TaskSubmissionId.HasValue)
            {
                return BadRequest(new { message = "Коментар не може бути пов’язаний одночасно з завданням і поданням." });
            }
            if (!commentDto.TaskId.HasValue && !commentDto.TaskSubmissionId.HasValue)
            {
                return BadRequest(new { message = "Коментар має бути пов’язаний із завданням або поданням." });
            }

            // Перевірка існування користувача
            if (!await _context.Users.AnyAsync(u => u.Id == commentDto.UserId))
            {
                return BadRequest(new { message = $"Користувач з ID {commentDto.UserId} не існує." });
            }

            TaskAPIWebApp.Models.Task? associatedTask = null; // Для перевірки членства в групі

            if (commentDto.TaskId.HasValue)
            {
                associatedTask = await _context.Tasks.FindAsync(commentDto.TaskId.Value);
                if (associatedTask == null)
                {
                    return BadRequest(new { message = $"Завдання із ID {commentDto.TaskId.Value} не існує." });
                }
            }
            else if (commentDto.TaskSubmissionId.HasValue) // else if, бо тільки один з них може бути
            {
                var submission = await _context.TaskSubmissions.FindAsync(commentDto.TaskSubmissionId.Value);
                if (submission == null)
                {
                    return BadRequest(new { message = $"Подання із ID {commentDto.TaskSubmissionId.Value} не існує." });
                }
                associatedTask = await _context.Tasks.FindAsync(submission.TaskId);
                if (associatedTask == null)
                {
                    // Ця помилка малоймовірна, якщо submission.TaskId валідний, але для повноти
                    return BadRequest(new { message = "Завдання, пов’язане з поданням, не знайдено." });
                }
            }

            // Перевірка, чи користувач є членом групи, пов’язаної з завданням (якщо завдання належить групі)
            if (associatedTask != null && associatedTask.TaskGroupId.HasValue)
            {
                if (!await _context.GroupMembers.AnyAsync(gm => gm.UserId == commentDto.UserId && gm.TaskGroupId == associatedTask.TaskGroupId.Value))
                {
                    return BadRequest(new { message = "Користувач не є членом групи, пов’язаної з цим завданням/поданням." });
                }
            }

            var comment = new Comment
            {
                Content = commentDto.Content,
                UserId = commentDto.UserId,
                TaskId = commentDto.TaskId,
                TaskSubmissionId = commentDto.TaskSubmissionId,
                CreatedAt = DateTime.UtcNow // Встановлюємо тут
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            // Повертаємо повний об'єкт Comment, включаючи User для відображення
            var createdComment = await _context.Comments
                                              .Include(c => c.User)
                                              .FirstOrDefaultAsync(c => c.Id == comment.Id);
            return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, createdComment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, CommentInputDto commentDto) // Використовуємо DTO
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var commentToUpdate = await _context.Comments.FindAsync(id);
            if (commentToUpdate == null)
            {
                return NotFound(new { message = $"Коментар з ID {id} не знайдено." });
            }

            // Основні перевірки з CreateComment можна повторити, якщо логіка дозволяє змінювати UserId або прив'язку
            // Зазвичай UserId коментаря не змінюють, але Content - так.
            // Також не дозволяємо змінювати TaskId/TaskSubmissionId після створення.
            if (commentToUpdate.UserId != commentDto.UserId)
            {
                // Якщо зміна UserId дозволена, потрібні перевірки існування нового користувача та його прав
                return BadRequest(new { message = "Зміна автора коментаря не дозволена." });
            }
            if (commentToUpdate.TaskId != commentDto.TaskId || commentToUpdate.TaskSubmissionId != commentDto.TaskSubmissionId)
            {
                return BadRequest(new { message = "Зміна прив'язки коментаря (до завдання/подання) не дозволена." });
            }
            // Перевірка, що нові TaskId/TaskSubmissionId (якщо їх дозволено змінювати) валідні, як у Create

            commentToUpdate.Content = commentDto.Content;
            // CreatedAt не оновлюємо, але можна додати поле UpdatedAt

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CommentExists(id)) // Додав await
                {
                    return NotFound();
                }
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound(new { message = $"Коментар з ID {id} не знайдено." });
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<bool> CommentExists(int id) // Зробив асинхронним
        {
            return await _context.Comments.AnyAsync(e => e.Id == id);
        }
    }
}