using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceAPI;
using DeviceAPI.DTO;
using FluentValidation;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class DeviceController : ControllerBase
{
    private readonly MasterContext _context;
    private readonly IValidator<DeviceDTO> _validator;

    public DeviceController(MasterContext context, IValidator<DeviceDTO> validator)
    {
        _context = context;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> GetDevices(CancellationToken cancellationToken)
    {
        try
        {
            var devices = await _context.Devices
                .Select(d => new { d.Id, d.Name })
                .ToListAsync(cancellationToken);

            return Ok(devices);
        }
        catch
        {
            return Problem("Failed to retrieve devices.");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDevice(int id, CancellationToken cancellationToken)
    {
        try
        {
            var device = await _context.Devices
                .Include(d => d.DeviceType)
                .Include(d => d.DeviceEmployees.OrderByDescending(de => de.IssueDate))
                    .ThenInclude(de => de.Employee)
                        .ThenInclude(e => e.Person)
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

            if (device == null) return NotFound();

            var lastUser = device.DeviceEmployees.FirstOrDefault(de => de.ReturnDate == null);

            return Ok(new
            {
                device.Name,
                DeviceTypeName = device.DeviceType?.Name,
                device.IsEnabled,
                AdditionalProperties = JsonSerializer.Deserialize<object>(device.AdditionalProperties),
                Employee = lastUser != null ? new
                {
                    lastUser.Employee.Id,
                    Name = $"{lastUser.Employee.Person.FirstName} {lastUser.Employee.Person.LastName}"
                } : null
            });
        }
        catch
        {
            return Problem("Failed to retrieve device.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateDevice(DeviceDTO dto, CancellationToken cancellationToken)
    {
        var result = await _validator.ValidateAsync(dto, cancellationToken);
        if (!result.IsValid)
        {
            var errors = result.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
            return BadRequest(new { Errors = errors });
        }

        var deviceType = await _context.DeviceTypes
            .FirstOrDefaultAsync(dt => dt.Name == dto.DeviceTypeName, cancellationToken);

        if (deviceType == null)
            return BadRequest($"Device type '{dto.DeviceTypeName}' not found.");

        var device = new Device
        {
            Name = dto.Name,
            IsEnabled = dto.IsEnabled.Value,
            AdditionalProperties = JsonSerializer.Serialize(dto.AdditionalProperties),
            DeviceTypeId = deviceType.Id
        };

        _context.Devices.Add(device);
        await _context.SaveChangesAsync(cancellationToken);

        return Created($"/api/devices/{device.Id}", new { device.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDevice(int id, DeviceDTO dto, CancellationToken cancellationToken)
    {
        try
        {
            var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
            if (device == null) return NotFound();

            var deviceType = await _context.DeviceTypes
                .FirstOrDefaultAsync(dt => dt.Name == dto.DeviceTypeName, cancellationToken);

            if (deviceType == null)
                return BadRequest($"Device type '{dto.DeviceTypeName}' not found.");

            device.Name = dto.Name;
            device.IsEnabled = dto.IsEnabled.Value;
            device.AdditionalProperties = JsonSerializer.Serialize(dto.AdditionalProperties);
            device.DeviceTypeId = deviceType.Id;

            await _context.SaveChangesAsync(cancellationToken);
            return Ok(new { device.Id });
        }
        catch
        {
            return Problem("Failed to update device.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDevice(int id, CancellationToken cancellationToken)
    {
        try
        {
            var device = await _context.Devices.FindAsync(new object[] { id }, cancellationToken);
            if (device == null) return NotFound();

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync(cancellationToken);
            return NoContent();
        }
        catch
        {
            return Problem("Failed to delete device.");
        }
    }
} 


