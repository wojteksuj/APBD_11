using DeviceAPI.DAL;
using DeviceAPI.Services.Tokens;
using DeviceAPI.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeviceAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly MasterContext _context;
    private readonly IPasswordHasher<Account> _hasher;
    private readonly ITokenService _tokenService;

    public AuthController(MasterContext context, IPasswordHasher<Account> hasher, ITokenService tokenService)
    {
        _context = context;
        _hasher = hasher;
        _tokenService = tokenService;
    }

    [HttpPost]
    public async Task<IActionResult> Authenticate(LoginDTO dto)
    {
        var account = await _context.Accounts
            .Include(a => a.Role)
            .FirstOrDefaultAsync(a => a.Username == dto.Username);

        if (account == null)
            return Unauthorized("Invalid credentials.");

        var result = _hasher.VerifyHashedPassword(account, account.Password, dto.Password);

        if (result == PasswordVerificationResult.Failed)
            return Unauthorized("Invalid credentials.");

        var token = _tokenService.GenerateToken(account.EmployeeId, account.Username, account.Role.Name);
        return Ok(new { token });
    }
}

