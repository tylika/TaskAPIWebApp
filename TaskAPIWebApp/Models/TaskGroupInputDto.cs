using System.ComponentModel.DataAnnotations;

namespace TaskAPIWebApp.Models // Або TaskAPIWebApp.Models.Dtos
{
    public class TaskGroupInputDto
    {
        [Required(ErrorMessage = "Назва групи є обов'язковою.")]
        [StringLength(255, ErrorMessage = "Назва групи не може перевищувати 255 символів.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "ID Власника групи є обов'язковим.")]
        public int UserId { get; set; }
    }
}