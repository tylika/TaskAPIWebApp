using System;
using System.Collections.Generic;

namespace TaskAPIWebApp.Models;

public partial class TaskSubmission
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int UserId { get; set; }

    public string Submission { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int? Score { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual Task Task { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
