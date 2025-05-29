using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAPIWebApp;
using System.ComponentModel;
using TaskAPIWebApp.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic; // Потрібно для IEnumerable

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

        // ЗМІНЕНО: Повертає IEnumerable<object>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetComments(int? taskSubmissionId, int? taskId)
        {
            var query = _context.Comments
                                .Include(c => c.User) // Для імені автора
                                .Include(c => c.Task) // Для опису завдання (якщо коментар до завдання)
                                .Include(c => c.TaskSubmission) // Для деталей подання
                                    .ThenInclude(ts => ts.Task) // Для опису завдання, до якого подання
                                .Include(c => c.TaskSubmission)
                                    .ThenInclude(ts => ts.User) // Для автора подання (якщо потрібно)
                                .AsQueryable();

            if (taskSubmissionId.HasValue)
            {
                query = query.Where(c => c.TaskSubmissionId == taskSubmissionId.Value);
            }
            if (taskId.HasValue)
            {
                query = query.Where(c => c.TaskId == taskId.Value);
            }

            var commentsProjection = await query
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.UserId, // Залишаємо для можливих внутрішніх потреб, але відображати будемо UserUsername
                    UserUsername = c.User != null ? c.User.Username : "N/A",
                    c.TaskId, // Залишаємо для фільтрації/посилань, але відображати будемо TaskDescription
                    TaskDescription = c.Task != null ? c.Task.Description : null,
                    c.TaskSubmissionId, // Залишаємо для фільтрації/посилань, але відображати будемо SubmissionInfo
                    // Формуємо інформацію про подання, наприклад: "Подання від [Автор Подання] до завдання '[Опис Завдання]'"
                    SubmissionInfo = c.TaskSubmission != null ?
                                     $"Подання ID: {c.TaskSubmissionId} від {(c.TaskSubmission.User != null ? c.TaskSubmission.User.Username : "N/A")} до завдання \"{Truncate(c.TaskSubmission.Task != null ? c.TaskSubmission.Task.Description : "N/A", 30)}\""
                                     : null,
                    c.CreatedAt
                }).ToListAsync();

            return Ok(commentsProjection);
        }

        // ЗМІНЕНО: Повертає object
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetComment(int id)
        {
            var comment = await _context.Comments
                                   .Include(c => c.User)
                                   .Include(c => c.Task)
                                   .Include(c => c.TaskSubmission)
                                       .ThenInclude(ts => ts.Task)
                                   .Include(c => c.TaskSubmission)
                                       .ThenInclude(ts => ts.User)
                                   .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                return NotFound(new { message = $"Коментар з ID {id} не знайдено." });
            }

            var commentProjection = new
            {
                comment.Id,
                comment.Content,
                comment.UserId,
                UserUsername = comment.User != null ? comment.User.Username : "N/A",
                comment.TaskId,
                TaskDescription = comment.Task != null ? comment.Task.Description : null,
                comment.TaskSubmissionId,
                SubmissionInfo = comment.TaskSubmission != null ?
                                 $"Подання ID: {comment.TaskSubmissionId} від {(comment.TaskSubmission.User != null ? comment.TaskSubmission.User.Username : "N/A")} до завдання \"{Truncate(comment.TaskSubmission.Task != null ? comment.TaskSubmission.Task.Description : "N/A", 30)}\""
                                 : null,
                comment.CreatedAt
            };

            return Ok(commentProjection);
        }

        // CreateComment ПОВИНЕН повертати проекцію, а не сиру сутність Comment
        [HttpPost]
        public async Task<ActionResult<object>> CreateComment(CommentInputDto commentDto) // Приймає CommentInputDto
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (commentDto.TaskId.HasValue && commentDto.TaskSubmissionId.HasValue)
                return BadRequest(new { message = "Коментар не може бути пов’язаний одночасно з завданням і поданням." });
            if (!commentDto.TaskId.HasValue && !commentDto.TaskSubmissionId.HasValue)
                return BadRequest(new { message = "Коментар має бути пов’язаний із завданням або поданням." });

            if (!await _context.Users.AnyAsync(u => u.Id == commentDto.UserId))
                return BadRequest(new { message = $"Користувач з ID {commentDto.UserId} не існує." });

            Models.Task? associatedTask = null;
            if (commentDto.TaskId.HasValue)
            {
                associatedTask = await _context.Tasks.FindAsync(commentDto.TaskId.Value);
                if (associatedTask == null) return BadRequest(new { message = $"Завдання із ID {commentDto.TaskId.Value} не існує." });
            }
            else if (commentDto.TaskSubmissionId.HasValue)
            {
                var submission = await _context.TaskSubmissions.Include(s => s.Task).FirstOrDefaultAsync(s => s.Id == commentDto.TaskSubmissionId.Value);
                if (submission == null) return BadRequest(new { message = $"Подання із ID {commentDto.TaskSubmissionId.Value} не існує." });
                associatedTask = submission.Task;
                if (associatedTask == null) return BadRequest(new { message = "Завдання, пов’язане з поданням, не знайдено." });
            }

            if (associatedTask != null && associatedTask.TaskGroupId.HasValue)
            {
                if (!await _context.GroupMembers.AnyAsync(gm => gm.UserId == commentDto.UserId && gm.TaskGroupId == associatedTask.TaskGroupId.Value))
                    return BadRequest(new { message = "Користувач не є членом групи, пов’язаної з цим завданням/поданням." });
            }

            var comment = new Comment
            {
                Content = commentDto.Content,
                UserId = commentDto.UserId,
                TaskId = commentDto.TaskId,
                TaskSubmissionId = commentDto.TaskSubmissionId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Завантажуємо з Includes для правильного мапування на проекцію
            var createdCommentWithDetails = await _context.Comments
                .Where(c => c.Id == comment.Id)
                .Include(c => c.User)
                .Include(c => c.Task)
                .Include(c => c.TaskSubmission).ThenInclude(ts => ts.Task)
                .Include(c => c.TaskSubmission).ThenInclude(ts => ts.User)
                .Select(c => new {
                    c.Id,
                    c.Content,
                    c.UserId,
                    UserUsername = c.User != null ? c.User.Username : "N/A",
                    c.TaskId,
                    TaskDescription = c.Task != null ? c.Task.Description : null,
                    c.TaskSubmissionId,
                    SubmissionInfo = c.TaskSubmission != null ?
                                     $"Подання ID: {c.TaskSubmissionId} від {(c.TaskSubmission.User != null ? c.TaskSubmission.User.Username : "N/A")} до завдання \"{Truncate(c.TaskSubmission.Task != null ? c.TaskSubmission.Task.Description : "N/A", 30)}\""
                                     : null,
                    c.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (createdCommentWithDetails == null) return Problem("Помилка отримання створеного коментаря з деталями.");

            return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, createdCommentWithDetails);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, CommentInputDto commentDto) // Приймає DTO
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var commentToUpdate = await _context.Comments.FindAsync(id);
            if (commentToUpdate == null) return NotFound(new { message = $"Коментар з ID {id} не знайдено." });

            // Забороняємо зміну автора та прив'язки через цей метод, тільки вміст
            if (commentToUpdate.UserId != commentDto.UserId)
                return BadRequest(new { message = "Зміна автора коментаря не дозволена через цей метод." });
            if ((commentToUpdate.TaskId != commentDto.TaskId && commentDto.TaskId.HasValue) ||
                (commentToUpdate.TaskSubmissionId != commentDto.TaskSubmissionId && commentDto.TaskSubmissionId.HasValue) ||
                (commentToUpdate.TaskId.HasValue != commentDto.TaskId.HasValue) ||
                (commentToUpdate.TaskSubmissionId.HasValue != commentDto.TaskSubmissionId.HasValue))
            {
                return BadRequest(new { message = "Зміна прив'язки коментаря (до завдання/подання) не дозволена через цей метод." });
            }


            commentToUpdate.Content = commentDto.Content;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CommentExists(id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound(new { message = $"Коментар з ID {id} не знайдено." });

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<bool> CommentExists(int id)
        {
            return await _context.Comments.AnyAsync(e => e.Id == id);
        }

        // Допоміжна функція для скорочення рядків
        private static string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }
    }
}