using OrderApi.Domain.Entities;
using OrderApi.Domain.Interfaces.Repositories;

namespace OrderApi.src.Application.Services;
public class CustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await _customerRepository.GetAllAsync();
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        return await _customerRepository.GetByIdAsync(id);
    }

    public async Task<Customer> CreateAsync(string name, string email)
    {
        var existing = await _customerRepository.GetByEmailAsync(email);

        if (existing is not null)
            throw new InvalidOperationException("Já existe um cliente com esse email.");

        var customer = new Customer(name, email);
        await _customerRepository.AddAsync(customer);
        return customer;
    }

    public async Task UpdateAsync(Guid id, string name, string email)
    {
        var customer = await _customerRepository.GetByIdAsync(id);

        if (customer is null)
            throw new KeyNotFoundException("Cliente não encontrado.");

        customer.Update(name, email);
        await _customerRepository.UpdateAsync(customer);
    }

    public async Task DeleteAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);

        if (customer is null)
            throw new KeyNotFoundException("Cliente não encontrado.");

        await _customerRepository.DeleteAsync(customer.Id);
    }
}