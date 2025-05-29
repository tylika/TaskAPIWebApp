using System.ComponentModel.DataAnnotations;

namespace TaskAPIWebApp.Models // Або TaskAPIWebApp.DTOs
{
    public class TaskAttachmentInputDto
    {
        [Required(ErrorMessage = "ID Завдання є обов'язковим.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID завдання повинен бути позитивним числом.")]
        public int TaskId { get; set; }

        [Required(ErrorMessage = "Шлях до файлу є обов'язковим.")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Шлях до файлу повинен містити від 3 до 255 символів.")]
        // Як і в моделі, можна додати валідацію формату шляху.
        // Якщо це має бути URL:
        // [Url(ErrorMessage = "Шлях до файлу повинен бути коректною URL-адресою.")]
        public string FilePath { get; set; } = null!; // Ініціалізація для уникнення попереджень
    }
}