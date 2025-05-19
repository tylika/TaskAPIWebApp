using System;
using System.Collections.Generic;

namespace TaskAPIWebApp.Models;

public partial class Comment
{
    public int Id { get; set; }

    public string Content { get; set; } = null!;

    public int UserId { get; set; }

    public int? TaskId { get; set; }

    public int? TaskSubmissionId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Task? Task { get; set; }

    public virtual TaskSubmission? TaskSubmission { get; set; }

    public virtual User User { get; set; } = null!;
}
