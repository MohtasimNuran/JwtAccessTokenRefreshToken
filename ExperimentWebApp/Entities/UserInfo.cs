using System;
using System.Collections.Generic;

namespace ExperimentWebApp.Entities;

public partial class UserInfo
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Email { get; set; }
}
