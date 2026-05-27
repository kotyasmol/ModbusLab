using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ModbusLab.Domain.Users;
using ModbusLab.Infrastructure.Persistence;

namespace ModbusLab.Api.Auth;

public sealed class AuthService
{
    private readonly ModbusLabDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    public AuthService(ModbusLabDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!_configuration.GetValue("Auth:AllowPublicRegistration", true))
            throw new InvalidOperationException("Public registration is disabled.");

        var userName = NormalizeUserName(request.UserName);
        var email = NormalizeEmail(request.Email);

        if (userName.Length < 3)
            throw new ArgumentException("User name must contain at least 3 characters.");

        if (request.Password.Length < 6)
            throw new ArgumentException("Password must contain at least 6 characters.");

        if (await _dbContext.AppUsers.AnyAsync(
                user => user.UserName.ToLower() == userName.ToLower(),
                cancellationToken))
        {
            throw new InvalidOperationException("User name is already taken.");
        }

        if (email is not null && await _dbContext.AppUsers.AnyAsync(
                user => user.Email != null && user.Email.ToLower() == email.ToLower(),
                cancellationToken))
        {
            throw new InvalidOperationException("Email is already taken.");
        }

        var user = new AppUser(userName, email, passwordHash: "__pending__", role: AuthRoles.Viewer);
        user.SetPasswordHash(_passwordHasher.HashPassword(user, request.Password));

        await _dbContext.AppUsers.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var userName = NormalizeUserName(request.UserName);

        var user = await _dbContext.AppUsers
            .FirstOrDefaultAsync(
                candidate => candidate.UserName.ToLower() == userName.ToLower(),
                cancellationToken);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid user name or password.");

        if (!user.IsEnabled)
            throw new UnauthorizedAccessException("User account is disabled.");

        var passwordResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid user name or password.");

        return CreateAuthResponse(user);
    }

    public async Task<AuthUserDto?> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
            return null;

        var user = await _dbContext.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken);

        return user is null ? null : ToDto(user);
    }

    private AuthResponse CreateAuthResponse(AppUser user)
    {
        return new AuthResponse(CreateJwt(user), ToDto(user));
    }

    private string CreateJwt(AppUser user)
    {
        var issuer = GetRequiredJwtSetting("Issuer");
        var audience = GetRequiredJwtSetting("Audience");
        var secret = GetRequiredJwtSetting("Secret");

        if (secret.Length < 32)
            throw new InvalidOperationException("Jwt:Secret must contain at least 32 characters.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, user.Role)
        };

        if (user.Email is not null)
            claims.Add(new Claim(ClaimTypes.Email, user.Email));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GetRequiredJwtSetting(string key)
    {
        return _configuration[$"Jwt:{key}"]
               ?? throw new InvalidOperationException($"Jwt:{key} is not configured.");
    }

    private static AuthUserDto ToDto(AppUser user)
    {
        return new AuthUserDto(user.Id, user.UserName, user.Email, user.Role, user.IsEnabled);
    }

    private static string NormalizeUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User name is required.");

        return userName.Trim();
    }

    private static string? NormalizeEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
    }
}
