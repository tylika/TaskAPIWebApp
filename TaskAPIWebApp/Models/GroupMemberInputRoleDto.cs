using System.ComponentModel.DataAnnotations;

namespace TaskAPIWebApp.Models // Або TaskAPIWebApp.DTOs
{
    public class GroupMemberInputDto
    {
        [Required(ErrorMessage = "ID користувача є обов'язковим.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID користувача повинен бути позитивним числом.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "ID групи завдань є обов'язковим.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID групи завдань повинен бути позитивним числом.")]
        public int TaskGroupId { get; set; }

        [Required(ErrorMessage = "Роль є обов'язковою.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Роль повинна містити від 3 до 50 символів.")]
        // Якщо є фіксований набір ролей, можна використати Enum або атрибут з перевіркою на допустимі значення.
        // Наприклад, можна створити власний атрибут валідації для перевірки ролі по списку.
        // [AllowedRoles("Admin", "Member", "Viewer", ErrorMessage = "Вказана роль не підтримується.")]
        public string Role { get; set; } = "Member"; // Встановлюємо значення за замовчуванням, якщо логічно
    }
}