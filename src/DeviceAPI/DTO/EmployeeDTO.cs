using System.ComponentModel.DataAnnotations;

namespace DeviceAPI.DTO;

public class EmployeeDTO
{
    [Required]
    [StringLength(150)]
    public string FirstName { get; set; }
    
    [Required]
    [StringLength(150)]
    public string? MiddleName { get; set; }
    
    [Required]
    [StringLength(150)]
    public string LastName { get; set; }
    
    public string Email { get; set; }
    
    public string PassportNumber { get; set; }
    
    public string PhoneNumber { get; set; }
    
    public decimal Salary { get; set; }
    public int PositionId { get; set; }
    public DateTime HireDate { get; set; }
}
