using System.Collections.Generic;

namespace YAERP.Domain.Common;

public static class SecurityContext
{
    public static UserContext CurrentUser { get; set; } = new UserContext();
}

public class UserContext
{
    public string Username { get; set; } = string.Empty;
    public HashSet<string> Permissions { get; set; } = new HashSet<string>();
}
