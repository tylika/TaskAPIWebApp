using System.ComponentModel.DataAnnotations;

public class GroupMemberUpdateRoleDto
{
    [Required(ErrorMessage = "Роль є обов'язковою.")]
    [StringLength(50, ErrorMessage = "Роль не може перевищувати 50 символів.")]
    public string Role { get; set; }
}