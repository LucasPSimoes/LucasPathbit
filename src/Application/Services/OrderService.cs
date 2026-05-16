using OrderApi.Domain.Entities;
using OrderApi.Domain.Interfaces.Repositories;
using System.Net.Http;
using System.Text.Json;

public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly HttpClient _httpClient;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        ICustomerRepository customerRepository,
        HttpClient httpClient)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
        => await _orderRepository.GetAllAsync();

    public async Task<Order?> GetByIdAsync(Guid id)
        => await _orderRepository.GetByIdAsync(id);

    public async Task<Order> CreateAsync(Guid customerId, Guid productId, int quantity, string zipCode, string deliveryAddress)
    {
        
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer is null)
            throw new KeyNotFoundException("Cliente não encontrado.");

        
        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null)
            throw new KeyNotFoundException("Produto não encontrado.");

        
        var cepLimpo = zipCode.Replace("-", "").Trim();
        var response = await _httpClient.GetAsync($"https://viacep.com.br/ws/{cepLimpo}/json/");
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("CEP inválido ou não encontrado.");

        var json = await response.Content.ReadAsStringAsync();
        var cepData = JsonSerializer.Deserialize<JsonElement>(json);
        if (cepData.TryGetProperty("erro", out _))
            throw new InvalidOperationException("CEP não encontrado.");

        
        var logradouro = cepData.GetProperty("logradouro").GetString();
        var bairro = cepData.GetProperty("bairro").GetString();
        var cidade = cepData.GetProperty("localidade").GetString();
        var estado = cepData.GetProperty("uf").GetString();
        var enderecoCompleto = $"{logradouro}, {bairro}, {cidade} - {estado}";

        
        product.DecreaseQuantity(quantity);

        var order = new Order(customerId, productId, quantity, product.Price, zipCode, enderecoCompleto);

        await _productRepository.UpdateAsync(product);
        await _orderRepository.AddAsync(order);

        return order;
    }
}