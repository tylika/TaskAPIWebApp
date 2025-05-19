using System.ComponentModel.DataAnnotations;

namespace TaskAPIWebApp.Models // Або TaskAPIWebApp.Models.Dtos
{
    public class TaskInputDto
    {
        [Required(ErrorMessage = "Опис є обов'язковим.")]
        [StringLength(1000, ErrorMessage = "Опис не може перевищувати 1000 символів.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Статус є обов'язковим.")]
        [StringLength(50, ErrorMessage = "Статус не може перевищувати 50 символів.")]
        public string Status { get; set; }

        [Required(ErrorMessage = "ID Користувача є обов'язковим.")]
        public int UserId { get; set; }

        public int? TaskGroupId { get; set; }
    }
}