using System.ComponentModel.DataAnnotations;

namespace TaskAPIWebApp.Models // Або TaskAPIWebApp.DTOs
{
    public class TaskSubmissionInputDto
    {
        [Required(ErrorMessage = "ID завдання є обов'язковим.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID завдання повинен бути позитивним числом.")]
        public int TaskId { get; set; }

        [Required(ErrorMessage = "ID користувача (автора подання) є обов'язковим.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID користувача повинен бути позитивним числом.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Текст подання є обов'язковим.")]
        [StringLength(4000, MinimumLength = 1, ErrorMessage = "Текст подання повинен містити від 1 до 4000 символів.")]
        public string Submission { get; set; } = null!; // Текст відповіді або посилання

        [Required(ErrorMessage = "Статус подання є обов'язковим.")]
        [StringLength(50, ErrorMessage = "Статус не повинен перевищувати 50 символів.")]
        // Приклад для обмеженого набору статусів:
        // [RegularExpression("^(Pending|Submitted)$", ErrorMessage = "Початковий статус може бути 'Pending' або 'Submitted'.")]
        public string Status { get; set; } = "Submitted"; // Логічне значення за замовчуванням при створенні

        [Range(0, 100, ErrorMessage = "Оцінка (Score) має бути в межах від 0 до 100, якщо вказана.")]
        public int? Score { get; set; } // Оцінка може бути встановлена пізніше, тому не Required
    }
}