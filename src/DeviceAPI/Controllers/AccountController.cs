using DeviceAPI.DAL;
using DeviceAPI.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeviceAPI.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly MasterContext _context;
    private readonly PasswordHasher<Account> _hasher;

    public AccountsController(MasterContext context)
    {
        _context = context;
        _hasher = new PasswordHasher<Account>();
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterAccountDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _context.Accounts.AnyAsync(a => a.Username == dto.Username))
            return Conflict("Username already exists.");

        var account = new Account
        {
            Username = dto.Username,
            EmployeeId = dto.EmployeeId,
            RoleId = 2 
        };

        account.Password = _hasher.HashPassword(account, dto.Password);

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return Created("", new { message = "Account created." });
    }
}
