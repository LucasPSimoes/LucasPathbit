using OrderApi.Domain.Enums;

namespace OrderApi.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string Username { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }

    // EF Core precisa de um construtor sem parâmetros
    protected User()
    {
        Email = null!;
        Username = null!;
        PasswordHash = null!;
    }

    public User(string email, string username, string passwordHash, UserRole role)
    {
        Id = Guid.NewGuid();
        Email = email;
        Username = username;
        PasswordHash = passwordHash;
        Role = role;
    }
}