using System;
using System.Collections.Generic;

namespace TaskAPIWebApp.Models;

public partial class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!; // Залишаємо ім'я як основний ідентифікатор

    // Навігаційні властивості залишаються, оскільки вони потрібні для зв'язків EF Core
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
    public virtual ICollection<TaskGroup> TaskGroups { get; set; } = new List<TaskGroup>(); // Де користувач є власником групи
    public virtual ICollection<TaskSubmission> TaskSubmissions { get; set; } = new List<TaskSubmission>();
    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>(); // Де користувач є виконавцем/створювачем завдання
}