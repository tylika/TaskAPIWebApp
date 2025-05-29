using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskAPIWebApp.Models
{
    // Атрибут [PrimaryKey] можна використовувати з EF Core 7.0+ для визначення складених ключів.
    // Якщо у вас старіша версія, налаштування складеного ключа робиться через Fluent API в OnModelCreating,
    // що у вас вже є: entity.HasKey(e => new { e.UserId, e.TaskGroupId });
    // Тому атрибути валідації тут більше стосуватимуться інших полів.

    public partial class GroupMember
    {
        [Required(ErrorMessage = "ID користувача є обов'язковим.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID користувача повинен бути дійсним.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "ID групи завдань є обов'язковим.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID групи завдань повинен бути дійсним.")]
        public int TaskGroupId { get; set; }

        [Required(ErrorMessage = "Роль не може бути порожньою.")]
        [StringLength(50, ErrorMessage = "Роль не повинна перевищувати 50 символів.")]
        // Можна додати регулярний вираз, якщо є строго визначений список ролей, наприклад, "Admin", "Member"
        // [RegularExpression("^(Admin|Member|Viewer)$", ErrorMessage = "Неприпустима роль.")]
        public string Role { get; set; } = null!; // null! означає, що ви очікуєте, що це поле завжди буде ініціалізоване

        // JoinedAt: Зазвичай встановлюється сервером.
        // Якщо клієнт може його передавати:
        // [DataType(DataType.DateTime, ErrorMessage = "Некоректний формат дати приєднання.")]
        public DateTime? JoinedAt { get; set; }


        // Навігаційні властивості
        [ForeignKey(nameof(TaskGroupId))]
        public virtual TaskGroup TaskGroup { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
    }
}