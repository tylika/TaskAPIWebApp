using System;
using System.Collections.Generic;

namespace TaskAPIWebApp.Models;

public partial class GroupMember
{
    public int UserId { get; set; }

    public int TaskGroupId { get; set; }

    public string Role { get; set; } = null!;

    public DateTime? JoinedAt { get; set; }

    public virtual TaskGroup TaskGroup { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
