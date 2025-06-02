using System.ComponentModel.DataAnnotations;

namespace DeviceAPI.DTO;

public class RegisterAccountDTO
{
    [Required]
    [RegularExpression(@"^[^\d]\w*$", ErrorMessage = "Username cannot start with a number.")]
    public string Username { get; set; } = null!;

    [Required]
    [MinLength(12)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
        ErrorMessage = "Password must contain lowercase, uppercase, number, and symbol.")]
    public string Password { get; set; } = null!;

    [Required]
    public int EmployeeId { get; set; }
}
