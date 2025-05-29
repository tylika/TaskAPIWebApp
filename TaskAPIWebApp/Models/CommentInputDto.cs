using System.Collections.Generic; // Для IValidatableObject
using System.ComponentModel.DataAnnotations;

namespace TaskAPIWebApp.Models // Або TaskAPIWebApp.DTOs, якщо ви вирішите їх перемістити
{
    public class CommentInputDto : IValidatableObject // Додаємо IValidatableObject
    {
        [Required(ErrorMessage = "Текст коментаря є обов'язковим.")]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "Текст коментаря повинен містити від 1 до 2000 символів.")]
        public string Content { get; set; } = null!; // Ініціалізація, щоб уникнути попереджень про null

        [Required(ErrorMessage = "ID користувача є обов'язковим.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID користувача повинен бути дійсним.")]
        public int UserId { get; set; }

        // TaskId та TaskSubmissionId не є [Required], оскільки коментар може бути прив'язаний або до одного, або до іншого
        [Range(1, int.MaxValue, ErrorMessage = "ID завдання повинен бути дійсним, якщо вказано.")]
        public int? TaskId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ID подання завдання повинен бути дійсним, якщо вказано.")]
        public int? TaskSubmissionId { get; set; }

        // Метод для комплексної валідації
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TaskId.HasValue && TaskSubmissionId.HasValue)
            {
                yield return new ValidationResult(
                    "Коментар не може бути пов'язаний одночасно і з завданням (TaskId), і з поданням (TaskSubmissionId).",
                    new[] { nameof(TaskId), nameof(TaskSubmissionId) }); // Вказуємо, до яких полів відноситься помилка
            }
            else if (!TaskId.HasValue && !TaskSubmissionId.HasValue)
            {
                yield return new ValidationResult(
                    "Коментар має бути пов'язаний або з завданням (TaskId), або з поданням (TaskSubmissionId).",
                    new[] { nameof(TaskId), nameof(TaskSubmissionId) });
            }
        }
    }
}