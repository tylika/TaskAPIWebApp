using System.ComponentModel.DataAnnotations;

namespace TaskAPIWebApp.Models // Або TaskAPIWebApp.DTOs
{
    public class TaskInputDto
    {
        [Required(ErrorMessage = "Опис завдання є обов'язковим.")]
        [StringLength(2000, MinimumLength = 5, ErrorMessage = "Опис завдання повинен містити від 5 до 2000 символів.")]
        public string Description { get; set; } = null!; // Ініціалізація для уникнення попереджень

        [Required(ErrorMessage = "Статус завдання є обов'язковим.")]
        [StringLength(50, ErrorMessage = "Статус не повинен перевищувати 50 символів.")]
        // Якщо у вас є список допустимих статусів, краще використовувати Enum або спеціальний валідатор.
        // Наприклад: [AllowedTaskStatuses("Draft", "InProgress", "PendingReview", "Completed", "Archived")]
        public string Status { get; set; } = "Draft"; // Можна встановити значення за замовчуванням для DTO

        [Required(ErrorMessage = "ID користувача (відповідального) є обов'язковим.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID користувача повинен бути позитивним числом.")]
        public int UserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ID групи завдань повинен бути позитивним числом, якщо вказано.")]
        public int? TaskGroupId { get; set; } // Може бути не вказано, якщо завдання не належить до групи
    }
}