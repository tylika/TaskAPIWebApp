using System.ComponentModel.DataAnnotations;

namespace TaskAPIWebApp.Models // Або TaskAPIWebApp.DTOs
{
    public class TaskAttachmentUpdateDto
    {
        // TaskId зазвичай не змінюється при оновленні вкладення,
        // воно ідентифікує, до якого завдання належить вкладення.
        // Якщо ж TaskId може змінюватися, його потрібно додати сюди з валідацією.

        [Required(ErrorMessage = "Новий шлях до файлу є обов'язковим.")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Шлях до файлу повинен містити від 3 до 255 символів.")]
        // [Url(ErrorMessage = "Шлях до файлу повинен бути коректною URL-адресою.")]
        public string FilePath { get; set; } = null!;
    }
}