using FluentAssertions;
using Moq;
using Moq.Protected;
using OrderApi.Domain.Entities;
using OrderApi.Domain.Interfaces.Repositories;
using OrderApi.src.Application.Services;
using System.Net;
using System.Net.Http;

namespace UnitTests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<ICustomerRepository> _customerRepoMock = new();

    private HttpClient CreateHttpClient(string json, HttpStatusCode code = HttpStatusCode.OK)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = code,
                Content = new StringContent(json)
            });
        return new HttpClient(handlerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenCustomerNotFound()
    {
        _customerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Customer?)null);
        var service = new OrderService(_orderRepoMock.Object, _productRepoMock.Object, _customerRepoMock.Object, CreateHttpClient("{}"));

        var act = async () => await service.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), 1, "01310100", "Rua X");

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*Cliente*");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenProductNotFound()
    {
        var customer = new Customer("Lucas", "lucas@email.com");
        _customerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(customer);
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);
        var service = new OrderService(_orderRepoMock.Object, _productRepoMock.Object, _customerRepoMock.Object, CreateHttpClient("{}"));

        var act = async () => await service.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), 1, "01310100", "Rua X");

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*Produto*");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenCepIsInvalid()
    {
        var customer = new Customer("Lucas", "lucas@email.com");
        var product = new Product("Whey", 100m, 10);
        _customerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(customer);
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(product);

        var service = new OrderService(_orderRepoMock.Object, _productRepoMock.Object, _customerRepoMock.Object,
            CreateHttpClient("{\"erro\":true}"));

        var act = async () => await service.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), 1, "00000000", "Rua X");

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*CEP*");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenStockIsInsufficient()
    {
        var customer = new Customer("Lucas", "lucas@email.com");
        var product = new Product("Whey", 100m, 1);
        _customerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(customer);
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(product);

        var validCepJson = "{\"logradouro\":\"Av Paulista\",\"bairro\":\"Bela Vista\",\"localidade\":\"São Paulo\",\"uf\":\"SP\"}";
        var service = new OrderService(_orderRepoMock.Object, _productRepoMock.Object, _customerRepoMock.Object,
            CreateHttpClient(validCepJson));

        var act = async () => await service.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), 5, "01310100", "Rua X");

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*estoque*");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnOrder_WhenValid()
    {
        var customer = new Customer("Lucas", "lucas@email.com");
        var product = new Product("Whey", 100m, 10);
        _customerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(customer);
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
        _orderRepoMock.Setup(r => r.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);

        var validCepJson = "{\"logradouro\":\"Av Paulista\",\"bairro\":\"Bela Vista\",\"localidade\":\"São Paulo\",\"uf\":\"SP\"}";
        var service = new OrderService(_orderRepoMock.Object, _productRepoMock.Object, _customerRepoMock.Object,
            CreateHttpClient(validCepJson));

        var result = await service.CreateAsync(customer.Id, product.Id, 2, "01310100", "Av Paulista");

        result.Should().NotBeNull();
        result.Quantity.Should().Be(2);
        product.AvailableQuantity.Should().Be(8);
    }
}