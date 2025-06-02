namespace DeviceAPI;

public class Account
{
    public int Id { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public int EmployeeId { get; set; }

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
}