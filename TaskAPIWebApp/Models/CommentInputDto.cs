using System.ComponentModel.DataAnnotations;

namespace TaskAPIWebApp.Models // Або TaskAPIWebApp.Models.Dtos
{
    public class CommentInputDto
    {
        [Required(ErrorMessage = "Текст коментаря є обов'язковим.")]
        public string Content { get; set; }

        [Required(ErrorMessage = "ID Користувача є обов'язковим.")]
        public int UserId { get; set; }

        public int? TaskId { get; set; } // ID Завдання, до якого коментар

        public int? TaskSubmissionId { get; set; } // ID Подання завдання, до якого коментар
    }
}