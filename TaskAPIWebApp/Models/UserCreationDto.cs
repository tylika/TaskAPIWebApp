using System.ComponentModel.DataAnnotations;

namespace TaskAPIWebApp.Models // Або TaskAPIWebApp.DTOs
{
    public class UserCreationDto
    {
        [Required(ErrorMessage = "Ім'я користувача є обов'язковим.")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Ім'я користувача повинно містити від 3 до 255 символів.")]
        // Можна додати [RegularExpression] для валідації дозволених символів в імені користувача,
        // наприклад, тільки літери, цифри та підкреслення:
        // [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Ім'я користувача може містити лише літери, цифри та підкреслення.")]
        public string Username { get; set; } = null!; // Ініціалізація для уникнення попереджень

        // Якщо потрібна повноцінна реєстрація, додайте ці поля:
        // [Required(ErrorMessage = "Електронна пошта є обов'язковою.")]
        // [EmailAddress(ErrorMessage = "Некоректний формат електронної пошти.")]
        // [StringLength(255, ErrorMessage = "Електронна пошта не повинна перевищувати 255 символів.")]
        // public string Email { get; set; } = null!;

        // [Required(ErrorMessage = "Пароль є обов'язковим.")]
        // [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль повинен містити щонайменше 8 символів.")]
        // [DataType(DataType.Password)]
        // public string Password { get; set; } = null!;

        // Якщо потрібно підтвердження пароля:
        // [DataType(DataType.Password)]
        // [Compare("Password", ErrorMessage = "Пароль та його підтвердження не співпадають.")]
        // public string? ConfirmPassword { get; set; }
    }
}