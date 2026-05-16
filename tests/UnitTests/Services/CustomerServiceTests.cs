using FluentAssertions;
using Moq;
using OrderApi.Domain.Entities;
using OrderApi.Domain.Interfaces.Repositories;
using OrderApi.src.Application.Services;

namespace UnitTests.Services;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _repoMock = new();
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _service = new CustomerService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        var existing = new Customer("Lucas", "lucas@email.com");
        _repoMock.Setup(r => r.GetByEmailAsync("lucas@email.com")).ReturnsAsync(existing);

        var act = async () => await _service.CreateAsync("Lucas", "lucas@email.com");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*email*");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCustomer_WhenEmailIsNew()
    {
        _repoMock.Setup(r => r.GetByEmailAsync("new@email.com")).ReturnsAsync((Customer?)null);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Customer>())).Returns(Task.CompletedTask);

        var result = await _service.CreateAsync("Lucas", "new@email.com");

        result.Should().NotBeNull();
        result.Name.Should().Be("Lucas");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenCustomerNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Customer?)null);

        var act = async () => await _service.UpdateAsync(Guid.NewGuid(), "Lucas", "lucas@email.com");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenCustomerNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Customer?)null);

        var act = async () => await _service.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}