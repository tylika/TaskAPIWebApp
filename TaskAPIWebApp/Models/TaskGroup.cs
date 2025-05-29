using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskAPIWebApp.Models
{
    public partial class TaskGroup
    {
        public int Id { get; set; } // Генерується БД

        [Required(ErrorMessage = "Назва групи є обов'язковою.")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Назва групи повинна містити від 3 до 255 символів.")]
        public string Name { get; set; } = null!; // У вас в OnModelCreating є HasMaxLength(255)

        [Required(ErrorMessage = "ID власника групи (користувача) є обов'язковим.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID власника групи повинен бути дійсним.")]
        public int UserId { get; set; }


        // Навігаційні властивості
        public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
        public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!; // Власник групи
    }
}