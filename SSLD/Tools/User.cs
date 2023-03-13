namespace SSLD.Tools;

public class User
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public List<Role> Roles { get; set; } = new List<Role>();
}

public class Role
{
    public string Name { get; set; }
    public bool IsSelected { get; set; }
}