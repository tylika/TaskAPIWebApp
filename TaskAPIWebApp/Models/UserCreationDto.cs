using System.ComponentModel.DataAnnotations;

public class UserCreationDto
{
    [Required(ErrorMessage = "Ім'я користувача є обов'язковим.")]
    [StringLength(255)]
    public string Username { get; set; }
}