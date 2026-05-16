using OrderApi.Domain.Entities;
using OrderApi.Domain.Interfaces.Repositories;

namespace OrderApi.src.Application.Services;
public class ProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _productRepository.GetAllAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _productRepository.GetByIdAsync(id);
    }

    public async Task<Product> CreateAsync(string name, decimal price, int availableQuantity)
    {
        var Product = new Product(name, price, availableQuantity);
        await _productRepository.AddAsync(Product);
        return Product;
    }

    public async Task UpdateAsync(Guid id, string name, decimal price, int availableQuantity)
    {
        var Product = await _productRepository.GetByIdAsync(id);

        if (Product is null)
            throw new KeyNotFoundException("Suplemento não encontrado.");

        Product.Update(name, price, availableQuantity);
        await _productRepository.UpdateAsync(Product);
    }

    public async Task DeleteAsync(Guid id)
    {
        var Product = await _productRepository.GetByIdAsync(id);

        if (Product is null)
            throw new KeyNotFoundException("Suplemento não encontrado.");

        await _productRepository.DeleteAsync(Product.Id);
    }
}