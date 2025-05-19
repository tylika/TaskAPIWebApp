using System;
using System.Collections.Generic;

namespace TaskAPIWebApp.Models;

public partial class TaskAttachment
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public string FilePath { get; set; } = null!;

    public DateTime? UploadedAt { get; set; }

    public virtual Task Task { get; set; } = null!;
}
