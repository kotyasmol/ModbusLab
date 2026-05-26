namespace ModbusLab.Api.Auth;

public sealed record RegisterRequest(
    string UserName,
    string? Email,
    string Password);

public sealed record LoginRequest(
    string UserName,
    string Password);

public sealed record AuthUserDto(
    Guid Id,
    string UserName,
    string? Email,
    string Role);

public sealed record AuthResponse(
    string AccessToken,
    AuthUserDto User);
