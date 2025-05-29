using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskAPIWebApp.Models
{
    public partial class TaskSubmission
    {
        public int Id { get; set; } // Генерується БД

        [Required(ErrorMessage = "ID завдання є обов'язковим.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID завдання повинен бути дійсним.")]
        public int TaskId { get; set; }

        [Required(ErrorMessage = "ID користувача (автора подання) є обов'язковим.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID користувача повинен бути дійсним.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Текст подання (відповідь або посилання) є обов'язковим.")]
        [StringLength(4000, MinimumLength = 1, ErrorMessage = "Текст подання повинен містити від 1 до 4000 символів.")] // Або інша відповідна довжина
        public string Submission { get; set; } = null!;

        [Required(ErrorMessage = "Статус подання є обов'язковим.")]
        [StringLength(50, ErrorMessage = "Статус подання не повинен перевищувати 50 символів.")]
        // Приклад валідації на конкретні значення:
        // [RegularExpression("^(Pending|Submitted|UnderReview|Approved|Rejected)$", ErrorMessage = "Неприпустимий статус подання.")]
        public string Status { get; set; } = null!; // У вас в OnModelCreating є HasDefaultValue("Pending")

        [Range(0, 100, ErrorMessage = "Оцінка (Score) повинна бути в межах від 0 до 100, якщо вказана.")]
        public int? Score { get; set; }

        // SubmittedAt: Зазвичай встановлюється сервером.
        // Якщо клієнт може його передавати:
        // [DataType(DataType.DateTime, ErrorMessage = "Некоректний формат дати подання.")]
        public DateTime? SubmittedAt { get; set; } // У вас в OnModelCreating є HasDefaultValueSql("(getdate())")


        // Навігаційні властивості
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        [ForeignKey(nameof(TaskId))]
        public virtual Task Task { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
    }
}