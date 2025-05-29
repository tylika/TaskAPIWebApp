using System.ComponentModel.DataAnnotations;

namespace TaskAPIWebApp.Models // Або TaskAPIWebApp.DTOs
{
    public class TaskGroupInputDto
    {
        [Required(ErrorMessage = "Назва групи є обов'язковою.")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Назва групи повинна містити від 3 до 255 символів.")]
        public string Name { get; set; } = null!; // Ініціалізація для уникнення попереджень

        [Required(ErrorMessage = "ID власника групи є обов'язковим.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID власника групи повинен бути позитивним числом.")]
        public int UserId { get; set; }
    }
}