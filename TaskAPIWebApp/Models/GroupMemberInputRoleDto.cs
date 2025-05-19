using System.ComponentModel.DataAnnotations;

namespace TaskAPIWebApp.Models // Або TaskAPIWebApp.Models.Dtos
{
    public class GroupMemberInputDto
    {
        [Required(ErrorMessage = "ID Користувача є обов'язковим.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "ID Групи завдань є обов'язковим.")]
        public int TaskGroupId { get; set; }

        [Required(ErrorMessage = "Роль є обов'язковою.")]
        [StringLength(50, ErrorMessage = "Роль не може перевищувати 50 символів.")]
        public string Role { get; set; } // Наприклад, "Member", "Admin", "Viewer"
    }
}