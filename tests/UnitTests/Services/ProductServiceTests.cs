using FluentAssertions;
using Moq;
using OrderApi.Domain.Entities;
using OrderApi.Domain.Interfaces.Repositories;
using OrderApi.src.Application.Services;

namespace UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repoMock = new();
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _service = new ProductService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnProduct()
    {
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

        var result = await _service.CreateAsync("Whey Protein", 150.00m, 100);

        result.Should().NotBeNull();
        result.Name.Should().Be("Whey Protein");
        result.Price.Should().Be(150.00m);
        result.AvailableQuantity.Should().Be(100);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenProductNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var act = async () => await _service.UpdateAsync(Guid.NewGuid(), "Novo", 100m, 10);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenProductNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var act = async () => await _service.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}