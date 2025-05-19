using System.ComponentModel.DataAnnotations;

namespace TaskAPIWebApp.Models // Або TaskAPIWebApp.Models.Dtos
{
    public class TaskSubmissionInputDto
    {
        [Required(ErrorMessage = "ID Завдання є обов'язковим.")]
        public int TaskId { get; set; }

        [Required(ErrorMessage = "ID Користувача є обов'язковим.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Текст подання є обов'язковим.")]
        public string Submission { get; set; } // Текст відповіді або посилання

        [Required(ErrorMessage = "Статус подання є обов'язковим.")]
        [StringLength(50)]
        public string Status { get; set; }

        [Range(0, 100, ErrorMessage = "Оцінка має бути в межах від 0 до 100.")]
        public int? Score { get; set; }
    }

    // DTO для оновлення, якщо потрібно оновлювати тільки певні поля, наприклад, статус та оцінку
    public class TaskSubmissionUpdateDto
    {
        [Required(ErrorMessage = "Статус подання є обов'язковим.")]
        [StringLength(50)]
        public string Status { get; set; }

        [Range(0, 100, ErrorMessage = "Оцінка має бути в межах від 0 до 100.")]
        public int? Score { get; set; }

        // Можливо, оновлення самого тексту подання
        // public string? Submission { get; set; } 
    }
}