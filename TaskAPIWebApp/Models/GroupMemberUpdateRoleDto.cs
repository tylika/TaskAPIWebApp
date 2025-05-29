using System.ComponentModel.DataAnnotations;

namespace TaskAPIWebApp.Models // Або TaskAPIWebApp.DTOs
{
    public class GroupMemberUpdateRoleDto
    {
        [Required(ErrorMessage = "Нова роль є обов'язковою.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Роль повинна містити від 3 до 50 символів.")]
        // Аналогічно до GroupMemberInputDto, можна додати більш строгу валідацію ролей
        // [AllowedRoles("Admin", "Member", "Viewer", ErrorMessage = "Вказана роль не підтримується.")]
        public string Role { get; set; } = null!; // null!, оскільки [Required] гарантує, що значення буде надано
    }
}