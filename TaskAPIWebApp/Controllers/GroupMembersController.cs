using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAPIWebApp;
using System.ComponentModel;
using TaskAPIWebApp.Models; // Для GroupMember та GroupMemberInputDto
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic; // Для IEnumerable

namespace TaskAPIWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [DisplayName("Члени груп")]
    [Produces("application/json")]
    public class GroupMembersController : ControllerBase
    {
        private readonly TaskManagementApiContext _context;

        public GroupMembersController(TaskManagementApiContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Отримати список усіх членів груп
        /// </summary>
        /// <param name="taskGroupId">Фільтр за ID групи (опціонально)</param>
        /// <returns>Список членів груп з іменами користувачів та назвами груп</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetGroupMembers(int? taskGroupId)
        {
            var query = _context.GroupMembers
                .Include(gm => gm.User) // Включаємо дані користувача
                .Include(gm => gm.TaskGroup) // Включаємо дані групи
                .Select(gm => new
                {
                    gm.UserId,
                    Username = gm.User.Username, // Додаємо ім'я користувача
                    gm.TaskGroupId,
                    TaskGroupName = gm.TaskGroup.Name, // Додаємо назву групи
                    gm.Role,
                    gm.JoinedAt
                });

            if (taskGroupId.HasValue)
            {
                query = query.Where(gm => gm.TaskGroupId == taskGroupId.Value);
            }
            return await query.OrderBy(gm => gm.TaskGroupName).ThenBy(gm => gm.Username).ToListAsync();
        }

        /// <summary>
        /// Отримати члена групи за складеним ключем (ID користувача та ID групи)
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача</param>
        /// <param name="taskGroupId">Ідентифікатор групи завдань</param>
        /// <returns>Член групи з деталями</returns>
        [HttpGet("user/{userId}/group/{taskGroupId}")]
        public async Task<ActionResult<object>> GetGroupMember(int userId, int taskGroupId)
        {
            var groupMember = await _context.GroupMembers
                .Include(gm => gm.User)
                .Include(gm => gm.TaskGroup)
                .Where(gm => gm.UserId == userId && gm.TaskGroupId == taskGroupId)
                .Select(gm => new
                {
                    gm.UserId,
                    Username = gm.User.Username,
                    gm.TaskGroupId,
                    TaskGroupName = gm.TaskGroup.Name,
                    gm.Role,
                    gm.JoinedAt
                })
                .FirstOrDefaultAsync();

            if (groupMember == null)
            {
                return NotFound(new { message = $"Членство для користувача ID {userId} у групі ID {taskGroupId} не знайдено." });
            }
            return groupMember;
        }

        /// <summary>
        /// Додати нового члена до групи
        /// </summary>
        /// <param name="dto">Дані нового члена групи</param>
        /// <returns>Створений член групи</returns>
        [HttpPost]
        public async Task<ActionResult<GroupMember>> CreateGroupMember(GroupMemberInputDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _context.GroupMembers.AnyAsync(gm => gm.UserId == dto.UserId && gm.TaskGroupId == dto.TaskGroupId))
            {
                return BadRequest(new { message = "Користувач уже є членом цієї групи." });
            }

            if (!await _context.Users.AnyAsync(u => u.Id == dto.UserId))
            {
                return BadRequest(new { message = $"Користувач із ID {dto.UserId} не існує." });
            }
            if (!await _context.TaskGroups.AnyAsync(tg => tg.Id == dto.TaskGroupId))
            {
                return BadRequest(new { message = $"Група із ID {dto.TaskGroupId} не існує." });
            }

            var groupMember = new GroupMember
            {
                UserId = dto.UserId,
                TaskGroupId = dto.TaskGroupId,
                Role = dto.Role,
                JoinedAt = DateTime.UtcNow
            };

            _context.GroupMembers.Add(groupMember);
            await _context.SaveChangesAsync();

            // Повертаємо розширений об'єкт для відповідності GetGroupMember
            var createdMembershipDetails = await _context.GroupMembers
                .Include(gm => gm.User)
                .Include(gm => gm.TaskGroup)
                .Where(gm => gm.UserId == groupMember.UserId && gm.TaskGroupId == groupMember.TaskGroupId)
                .Select(gm => new
                {
                    gm.UserId,
                    Username = gm.User.Username,
                    gm.TaskGroupId,
                    TaskGroupName = gm.TaskGroup.Name,
                    gm.Role,
                    gm.JoinedAt
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetGroupMember), new { userId = groupMember.UserId, taskGroupId = groupMember.TaskGroupId }, createdMembershipDetails);
        }

        /// <summary>
        /// Оновити роль члена групи
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача</param>
        /// <param name="taskGroupId">Ідентифікатор групи завдань</param>
        /// <param name="dto">Оновлені дані (тільки роль)</param>
        /// <returns>Статус операції</returns>
        [HttpPut("user/{userId}/group/{taskGroupId}")]
        public async Task<IActionResult> UpdateGroupMember(int userId, int taskGroupId, [FromBody] GroupMemberUpdateRoleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var groupMember = await _context.GroupMembers.FindAsync(userId, taskGroupId);
            if (groupMember == null)
            {
                return NotFound(new { message = "Членство не знайдено." });
            }

            // Перевірка: не дозволяємо змінювати роль єдиного адміністратора на іншу, якщо він єдиний
            if (groupMember.Role == "Адмін" && dto.Role != "Адмін")
            {
                var otherAdmins = await _context.GroupMembers
                    .CountAsync(gm => gm.TaskGroupId == taskGroupId && gm.Role == "Адмін" && gm.UserId != userId);
                if (otherAdmins == 0)
                {
                    return BadRequest(new { message = "Неможливо змінити роль єдиного адміністратора групи." });
                }
            }

            groupMember.Role = dto.Role;
            // JoinedAt не оновлюємо, UserId та TaskGroupId є частиною ключа і не змінюються тут

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await GroupMemberExists(userId, taskGroupId)) // Додав await
                {
                    return NotFound();
                }
                throw;
            }
            return NoContent();
        }

        [HttpDelete("user/{userId}/group/{taskGroupId}")]
        public async Task<IActionResult> DeleteGroupMember(int userId, int taskGroupId)
        {
            var groupMember = await _context.GroupMembers.FindAsync(userId, taskGroupId);
            if (groupMember == null)
            {
                return NotFound(new { message = "Членство не знайдено." });
            }

            if (groupMember.Role == "Адмін")
            {
                var otherAdmins = await _context.GroupMembers
                    .CountAsync(gm => gm.TaskGroupId == taskGroupId && gm.Role == "Адмін" && gm.UserId != userId);
                if (otherAdmins == 0)
                {
                    return BadRequest(new { message = "Неможливо видалити єдиного адміністратора групи." });
                }
            }

            _context.GroupMembers.Remove(groupMember);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<bool> GroupMemberExists(int userId, int taskGroupId) // Зробив асинхронним
        {
            return await _context.GroupMembers.AnyAsync(e => e.UserId == userId && e.TaskGroupId == taskGroupId);
        }
    }

    
}