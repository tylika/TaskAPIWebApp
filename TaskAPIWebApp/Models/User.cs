using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskAPIWebApp.Models
{
    public partial class User
    {
        public int Id { get; set; } // Генерується БД

        [Required(ErrorMessage = "Ім'я користувача є обов'язковим.")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Ім'я користувача повинно містити від 3 до 255 символів.")]
        // Потрібно буде перевіряти унікальність Username на рівні контролера/сервісу
        public string Username { get; set; } = null!;

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
        public virtual ICollection<TaskGroup> TaskGroups { get; set; } = new List<TaskGroup>();
        public virtual ICollection<TaskSubmission> TaskSubmissions { get; set; } = new List<TaskSubmission>();
        public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
    }
}