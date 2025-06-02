using DeviceAPI.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceAPI.DTO;

namespace DeviceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly MasterContext _context;

    public EmployeesController(MasterContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetEmployees(CancellationToken cancellationToken)
    {
        try
        {
            var employees = await _context.Employees
                .Include(e => e.Person)
                .Select(e => new
                {
                    e.Id,
                    FullName = $"{e.Person.FirstName} {e.Person.LastName}"
                })
                .ToListAsync(cancellationToken);

            return Ok(employees);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GET /api/employees] {ex.Message}");
            return Problem("Failed to retrieve employees.");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployee(int id, CancellationToken cancellationToken)
    {
        try
        {
            var employee = await _context.Employees
                .Include(e => e.Person)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (employee == null)
                return NotFound();

            return Ok(new
            {
                Person = new
                {
                    employee.Person.FirstName,
                    employee.Person.MiddleName,
                    employee.Person.LastName,
                    employee.Person.Email,
                    employee.Person.PhoneNumber,
                    employee.Person.PassportNumber
                },
                employee.Salary,
                Position = new { employee.Position.Id, employee.Position.Name },
                employee.HireDate
            });
        }
        catch
        {
            return Problem("Failed to retrieve employee.");
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateEmployee([FromBody] EmployeeDTO dto, CancellationToken cancellationToken)
    {
        try
        {
            var position = await _context.Positions.FirstOrDefaultAsync(p => p.Id == dto.PositionId, cancellationToken);
            if (position == null)
                return BadRequest("Invalid position ID.");

            var employee = new Employee
            {
                HireDate = dto.HireDate,
                Salary = dto.Salary,
                Position = position,
                Person = new Person
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    MiddleName = dto.MiddleName,
                    Email = dto.Email,
                    PassportNumber = dto.PassportNumber,
                    PhoneNumber = dto.PhoneNumber
                }
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, new { employee.Id });
        }
        catch
        {
            return Problem("Failed to create employee.");
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeDTO dto, CancellationToken cancellationToken)
    {
        try
        {
            var employee = await _context.Employees
                .Include(e => e.Person)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (employee == null)
                return NotFound();

            var position = await _context.Positions.FirstOrDefaultAsync(p => p.Id == dto.PositionId, cancellationToken);
            if (position == null)
                return BadRequest("Invalid position ID.");

            employee.Salary = dto.Salary;
            employee.HireDate = dto.HireDate;
            employee.Position = position;

            employee.Person.FirstName = dto.FirstName;
            employee.Person.MiddleName = dto.MiddleName;
            employee.Person.LastName = dto.LastName;
            employee.Person.Email = dto.Email;
            employee.Person.PhoneNumber = dto.PhoneNumber;
            employee.Person.PassportNumber = dto.PassportNumber;

            await _context.SaveChangesAsync(cancellationToken);
            return Ok(new { employee.Id });
        }
        catch
        {
            return Problem("Failed to update employee.");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEmployee(int id, CancellationToken cancellationToken)
    {
        try
        {
            var employee = await _context.Employees
                .Include(e => e.Person)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (employee == null)
                return NotFound();

            _context.People.Remove(employee.Person);
            _context.Employees.Remove(employee);

            await _context.SaveChangesAsync(cancellationToken);
            return NoContent();
        }
        catch
        {
            return Problem("Failed to delete employee.");
        }
    }
}
