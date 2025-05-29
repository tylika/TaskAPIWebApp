using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskAPIWebApp.Models
{
    // Оскільки у вас в OnModelCreating є modelBuilder.Entity<AppTask>(entity => { ... });
    // де AppTask - це псевдонім для TaskAPIWebApp.Models.Task, то повне ім'я класу тут Task.
    public partial class Task // Повне ім'я класу, якщо немає псевдоніма в цьому файлі
    {
        public int Id { get; set; } // Генерується БД

        [Required(ErrorMessage = "Опис завдання є обов'язковим.")]
        [StringLength(2000, MinimumLength = 5, ErrorMessage = "Опис завдання повинен містити від 5 до 2000 символів.")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Статус завдання є обов'язковим.")]
        [StringLength(50, ErrorMessage = "Статус завдання не повинен перевищувати 50 символів.")]
        // Можна додати валідацію на допустимі значення статусу, якщо вони фіксовані
        // наприклад, [RegularExpression("^(Draft|InProgress|Completed|Cancelled)$", ErrorMessage = "Неприпустимий статус завдання.")]
        public string Status { get; set; } = null!; // У вас в OnModelCreating є HasDefaultValue("Draft")

        [Required(ErrorMessage = "ID користувача (відповідального) є обов'язковим.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID користувача повинен бути дійсним.")]
        public int UserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ID групи завдань повинен бути дійсним, якщо вказано.")]
        public int? TaskGroupId { get; set; }

        // Навігаційні властивості
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<TaskAttachment> TaskAttachments { get; set; } = new List<TaskAttachment>();

        [ForeignKey(nameof(TaskGroupId))]
        public virtual TaskGroup? TaskGroup { get; set; }

        public virtual ICollection<TaskSubmission> TaskSubmissions { get; set; } = new List<TaskSubmission>();

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
    }
}