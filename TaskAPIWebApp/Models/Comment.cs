using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Потрібно для атрибутів валідації
using System.ComponentModel.DataAnnotations.Schema; // Для [ForeignKey]

namespace TaskAPIWebApp.Models
{
    public partial class Comment
    {
        public int Id { get; set; } // Зазвичай не валідується, генерується БД

        [Required(ErrorMessage = "Зміст коментаря не може бути порожнім.")]
        [StringLength(2000, ErrorMessage = "Зміст коментаря не повинен перевищувати 2000 символів.")] // Приклад обмеження довжини
        public string Content { get; set; } = null!;

        [Required(ErrorMessage = "ID користувача є обов'язковим.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID користувача повинен бути дійсним.")]
        public int UserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ID завдання повинен бути дійсним, якщо вказано.")]
        public int? TaskId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ID подання повинен бути дійсним, якщо вказано.")]
        public int? TaskSubmissionId { get; set; }

        // CreatedAt: Зазвичай встановлюється сервером, тому валідація на вході може бути не потрібна.
        // Якщо ж клієнт може його передавати, то:
        // [DataType(DataType.DateTime, ErrorMessage = "Некоректний формат дати створення.")]
        public DateTime? CreatedAt { get; set; }


        // Навігаційні властивості
        [ForeignKey(nameof(TaskId))]
        public virtual Task? Task { get; set; }

        [ForeignKey(nameof(TaskSubmissionId))]
        public virtual TaskSubmission? TaskSubmission { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!; // Якщо User завжди має бути, то null! є доречним

        // Додаткова логіка валідації, якщо потрібно (менш поширено для моделей, частіше для DTO)
        // public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        // {
        //     if (TaskId.HasValue && TaskSubmissionId.HasValue)
        //     {
        //         yield return new ValidationResult(
        //             "Коментар не може бути пов'язаний одночасно і з завданням, і з поданням.",
        //             new[] { nameof(TaskId), nameof(TaskSubmissionId) });
        //     }
        //     if (!TaskId.HasValue && !TaskSubmissionId.HasValue)
        //     {
        //         yield return new ValidationResult(
        //             "Коментар має бути пов'язаний або з завданням, або з поданням.",
        //             new[] { nameof(TaskId), nameof(TaskSubmissionId) });
        //     }
        // }
    }
}