using System.ComponentModel.DataAnnotations;

namespace TaskAPIWebApp.Models // Або TaskAPIWebApp.DTOs
{
    public class TaskSubmissionUpdateDto
    {
        [Required(ErrorMessage = "Статус подання є обов'язковим.")]
        [StringLength(50, ErrorMessage = "Статус не повинен перевищувати 50 символів.")]
        // Тут можуть бути інші статуси, ніж при створенні, наприклад, "UnderReview", "Graded", "Rejected"
        // [RegularExpression("^(UnderReview|Graded|Approved|Rejected|NeedsRevision)$", ErrorMessage = "Неприпустимий статус для оновлення.")]
        public string Status { get; set; } = null!;

        [Range(0, 100, ErrorMessage = "Оцінка (Score) має бути в межах від 0 до 100.")]
        // Якщо статус "Graded", то оцінка може стати обов'язковою. Це можна реалізувати через IValidatableObject.
        public int? Score { get; set; }

        // Якщо дозволено редагувати сам текст подання після відправки:
        // [StringLength(4000, MinimumLength = 1, ErrorMessage = "Текст подання повинен містити від 1 до 4000 символів.")]
        // public string? Submission { get; set; }
    }
}