using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskAPIWebApp.Models
{
    public partial class TaskAttachment
    {
        public int Id { get; set; } // Генерується БД

        [Required(ErrorMessage = "ID завдання є обов'язковим.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID завдання повинен бути дійсним.")]
        public int TaskId { get; set; }

        [Required(ErrorMessage = "Шлях до файлу є обов'язковим.")]
        [StringLength(1024, ErrorMessage = "Шлях до файлу не повинен перевищувати 1024 символів.")]
        // Можна додати валідацію формату шляху, якщо є специфічні вимоги,
        // але це може бути складно зробити універсально.
        // [Url(ErrorMessage = "Некоректний формат URL-адреси файлу.")] // Якщо це URL
        // [RegularExpression(@"^(?:[a-zA-Z]\:|\\\\[\w\.]+\\[\w.$]+)\\(?:[\w]+\\)*\w([\w.])+$", ErrorMessage="Некоректний формат локального шляху")] // Приклад для Windows шляху
        public string FilePath { get; set; } = null!; // У вас в OnModelCreating є HasMaxLength(255)

        // UploadedAt: Зазвичай встановлюється сервером.
        // Якщо клієнт може його передавати:
        // [DataType(DataType.DateTime, ErrorMessage = "Некоректний формат дати завантаження.")]
        public DateTime? UploadedAt { get; set; } // У вас в OnModelCreating є HasDefaultValueSql("(getdate())")


        // Навігаційні властивості
        [ForeignKey(nameof(TaskId))]
        public virtual Task Task { get; set; } = null!;
    }
}