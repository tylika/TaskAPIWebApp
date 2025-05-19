using System;
using System.Collections.Generic;

namespace TaskAPIWebApp.Models;

public partial class Task
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int UserId { get; set; }

    public int? TaskGroupId { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<TaskAttachment> TaskAttachments { get; set; } = new List<TaskAttachment>();

    public virtual TaskGroup? TaskGroup { get; set; }

    public virtual ICollection<TaskSubmission> TaskSubmissions { get; set; } = new List<TaskSubmission>();

    public virtual User User { get; set; } = null!;
}
