using FluentAssertions;
using Moq;
using OrderApi.Application.Services;
using OrderApi.Common.Helpers;
using OrderApi.Domain.Entities;
using OrderApi.Domain.Enums;
using OrderApi.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;

namespace UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ICustomerRepository> _customerRepoMock = new();
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _configMock.Setup(c => c["Jwt:Key"]).Returns("chave-secreta-super-segura-minimo-32-chars!!");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("OrderApi");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("OrderApi");
        _authService = new AuthService(_userRepoMock.Object, _customerRepoMock.Object, _configMock.Object);
    }

    [Fact]
    public async Task SignUpAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        var existingUser = new User("test@email.com", "test@email.com", "hash", UserRole.Cliente);
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@email.com")).ReturnsAsync(existingUser);

        var act = async () => await _authService.SignUpAsync("Lucas", "test@email.com", "senha123");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*e-mail*");
    }

    [Fact]
    public async Task SignUpAsync_ShouldCreateUserAndCustomer_WhenEmailIsNew()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync("new@email.com")).ReturnsAsync((User?)null);
        _customerRepoMock.Setup(r => r.AddAsync(It.IsAny<Customer>())).Returns(Task.CompletedTask);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        var act = async () => await _authService.SignUpAsync("Lucas", "new@email.com", "senha123");

        await act.Should().NotThrowAsync();
        _customerRepoMock.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Once);
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenUserNotFound()
    {
        _userRepoMock.Setup(r => r.GetByUsernameAsync("notfound")).ReturnsAsync((User?)null);

        var act = async () => await _authService.LoginAsync("notfound", "senha123");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordIsWrong()
    {
        var user = new User("test@email.com", "test@email.com", HashHelper.ToSha256("correta"), UserRole.Cliente);
        _userRepoMock.Setup(r => r.GetByUsernameAsync("test@email.com")).ReturnsAsync(user);

        var act = async () => await _authService.LoginAsync("test@email.com", "errada");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var hash = HashHelper.ToSha256("senha123");
        var user = new User("test@email.com", "test@email.com", hash, UserRole.Cliente);
        var customer = new Customer("Lucas", "test@email.com");

        _userRepoMock.Setup(r => r.GetByUsernameAsync("test@email.com")).ReturnsAsync(user);
        _customerRepoMock.Setup(r => r.GetByEmailAsync("test@email.com")).ReturnsAsync(customer);

        var token = await _authService.LoginAsync("test@email.com", "senha123");

        token.Should().NotBeNullOrEmpty();
    }
}