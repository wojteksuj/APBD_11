namespace DeviceAPI.Services.Tokens;

public interface ITokenService
{
    string GenerateToken(int employeeId, string username, string role);
}
