using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OrderApi.Common.Helpers;
using OrderApi.Domain.Entities;
using OrderApi.Domain.Enums;
using OrderApi.Domain.Interfaces.Repositories;

namespace OrderApi.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        ICustomerRepository customerRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _customerRepository = customerRepository;
        _configuration = configuration;
    }

    public async Task SignUpAsync(string name, string email, string password)
    {
        var existing = await _userRepository.GetByEmailAsync(email);
        if (existing is not null)
            throw new InvalidOperationException("Já existe um usuário com esse e-mail.");

        var passwordHash = HashHelper.ToSha256(password);

        var customer = new Customer(name, email);
        await _customerRepository.AddAsync(customer);

        var user = new User(email, email, passwordHash, UserRole.Cliente);
        await _userRepository.AddAsync(user);
    }

    public async Task<string> LoginAsync(string username, string password)
    {
        var passwordHash = HashHelper.ToSha256(password);

        var user = await _userRepository.GetByUsernameAsync(username);
        if (user is null || user.PasswordHash != passwordHash)
            throw new UnauthorizedAccessException("Usuário ou senha inválidos.");

        var customer = await _customerRepository.GetByEmailAsync(user.Email);
        if (customer is null)
            throw new KeyNotFoundException("Cliente não encontrado.");

        return GenerateToken(user, customer);
    }

    private string GenerateToken(User user, Customer customer)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("customerId", customer.Id.ToString()),
            new Claim(ClaimTypes.Name, customer.Name),
            new Claim(ClaimTypes.Email, customer.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}