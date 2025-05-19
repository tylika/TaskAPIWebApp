using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAPIWebApp;
using System.ComponentModel;
using TaskAPIWebApp.Models;
using System.Linq; // Для .ToLower(), .Contains()
using System.Threading.Tasks; // Для Task
using System.Collections.Generic; // Для IEnumerable


namespace TaskAPIWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [DisplayName("Користувачі")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly TaskManagementApiContext _context;

        public UsersController(TaskManagementApiContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Отримати список усіх користувачів
        /// </summary>
        /// <param name="search">Пошук за ім'ям користувача (опціонально)</param>
        /// <returns>Список користувачів</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers(string? search)
        {
            var query = _context.Users.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                // Пошук тільки за Username, оскільки Email видалено
                query = query.Where(u => u.Username.ToLower().Contains(search));
            }
            return await query.OrderBy(u => u.Username).ToListAsync(); // Додав сортування
        }

        /// <summary>
        /// Отримати користувача за ID
        /// </summary>
        /// <param name="id">Ідентифікатор користувача</param>
        /// <returns>Користувач з указаним ID</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = $"Користувача з ID {id} не знайдено." });
            }
            return user;
        }

        /// <summary>
        /// Отримати список користувачів для випадних списків (ID та Username)
        /// </summary>
        /// <returns>Список користувачів з ID та Username</returns>
        [HttpGet("lookup")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsersLookup()
        {
            var users = await _context.Users
                .Select(u => new { u.Id, u.Username })
                .OrderBy(u => u.Username)
                .ToListAsync();
            return Ok(users);
        }


        // --- Методи CreateUser, UpdateUser, DeleteUser ---
        // Якщо ти не плануєш керувати користувачами через API (тільки читати їх),
        // ці методи можна закоментувати або видалити.
        // Якщо вони потрібні, їх треба адаптувати до спрощеної моделі User.
        // Нижче приклад, як вони могли б виглядати для спрощеної моделі (тільки Username).

        /// <summary>
        /// Створити нового користувача (спрощена версія)
        /// </summary>
        /// <param name="userInput">Дані нового користувача (тільки Username)</param>
        /// <returns>Створений користувач</returns>
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser([FromBody] UserCreationDto userInput) // Використовуємо DTO
        {
            if (string.IsNullOrWhiteSpace(userInput.Username))
            {
                return BadRequest(new { message = "Ім'я користувача не може бути порожнім." });
            }

            if (await _context.Users.AnyAsync(u => u.Username == userInput.Username))
            {
                return BadRequest(new { message = "Користувач із таким ім'ям вже існує." });
            }

            var user = new User { Username = userInput.Username };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        /// <summary>
        /// Оновити існуючого користувача (спрощена версія)
        /// </summary>
        /// <param name="id">Ідентифікатор користувача</param>
        /// <param name="userInput">Оновлені дані користувача (тільки Username)</param>
        /// <returns>Статус операції</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserCreationDto userInput) // Використовуємо DTO
        {
            if (string.IsNullOrWhiteSpace(userInput.Username))
            {
                return BadRequest(new { message = "Ім'я користувача не може бути порожнім." });
            }

            var userToUpdate = await _context.Users.FindAsync(id);
            if (userToUpdate == null)
            {
                return NotFound(new { message = $"Користувача з ID {id} не знайдено." });
            }

            // Перевірка унікальності нового Username, якщо він змінюється і не поточний користувач
            if (userToUpdate.Username != userInput.Username && await _context.Users.AnyAsync(u => u.Username == userInput.Username && u.Id != id))
            {
                return BadRequest(new { message = "Користувач із таким новим ім'ям вже існує." });
            }

            userToUpdate.Username = userInput.Username;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UserExists(id)) // Додав await
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        /// <summary>
        /// Видалити користувача
        /// </summary>
        /// <param name="id">Ідентифікатор користувача</param>
        /// <returns>Статус операції</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = $"Користувача з ID {id} не знайдено." });
            }

            // Перевірка: не дозволяємо видаляти, якщо користувач є єдиним адміністратором групи
            // Ця логіка може залишитися, якщо GroupMembers і ролі використовуються
            var isAdminInAnyGroupAsSoleAdmin = await _context.GroupMembers
                .Where(gm => gm.UserId == id && gm.Role == "Адмін") // Припускаємо, що "Адмін" це значення ролі
                .GroupBy(gm => gm.TaskGroupId)
                .Select(g => new { TaskGroupId = g.Key, AdminCount = g.Count(gm_admin => gm_admin.Role == "Адмін") })
                .AnyAsync(g_info => !_context.GroupMembers.Any(other_gm => other_gm.TaskGroupId == g_info.TaskGroupId && other_gm.Role == "Адмін" && other_gm.UserId != id));


            if (isAdminInAnyGroupAsSoleAdmin)
            {
                return BadRequest(new { message = "Неможливо видалити користувача: він є єдиним адміністратором принаймні в одній групі." });
            }

            // Додаткова перевірка: чи пов'язаний користувач з завданнями, коментарями тощо.
            // Якщо так, можливо, краще не видаляти або видаляти каскадно (обережно!)
            // Або заборонити видалення, якщо є залежності.
            if (await _context.Tasks.AnyAsync(t => t.UserId == id) ||
                await _context.Comments.AnyAsync(c => c.UserId == id) ||
                await _context.TaskSubmissions.AnyAsync(ts => ts.UserId == id) ||
                await _context.TaskGroups.AnyAsync(tg => tg.UserId == id) // Якщо User є власником групи
                )
            {
                return BadRequest(new { message = "Неможливо видалити користувача: він пов'язаний з іншими даними (завдання, коментарі, групи тощо)." });
            }


            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<bool> UserExists(int id) // Зробив асинхронним
        {
            return await _context.Users.AnyAsync(e => e.Id == id);
        }
    }

    // Додай простий DTO для створення/оновлення користувача, якщо потрібно
    
}