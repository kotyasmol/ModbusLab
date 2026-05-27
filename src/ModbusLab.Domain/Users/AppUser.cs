namespace ModbusLab.Domain.Users;

public sealed class AppUser
{
    public Guid Id { get; private set; }

    public string UserName { get; private set; }

    public string? Email { get; private set; }

    public string PasswordHash { get; private set; }

    public string Role { get; private set; }

    public bool IsEnabled { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private AppUser()
    {
        UserName = string.Empty;
        PasswordHash = string.Empty;
        Role = string.Empty;
    }

    public AppUser(string userName, string? email, string passwordHash, string role)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User name cannot be empty.", nameof(userName));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role cannot be empty.", nameof(role));

        Id = Guid.NewGuid();
        UserName = userName.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        PasswordHash = passwordHash;
        Role = role.Trim();
        IsEnabled = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        PasswordHash = passwordHash;
    }

    public void ChangeRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role cannot be empty.", nameof(role));

        Role = role.Trim();
    }

    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }
}
