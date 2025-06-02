using System.ComponentModel.DataAnnotations;

namespace DeviceAPI.DTO;

public class DeviceDTO
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public bool? IsEnabled { get; set; }

    [Required]
    [StringLength(100)]
    public string DeviceTypeName { get; set; } = string.Empty;

    [Required]
    public object AdditionalProperties { get; set; }
}