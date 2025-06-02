using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceAPI;

[ApiController]
[Route("api/[controller]")]
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
}
