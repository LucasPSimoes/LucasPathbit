namespace OrderApi.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public int AvailableQuantity { get; private set; }

    protected Product()
    {
        Name = null!;
    }

    public Product(string name, decimal price, int availableQuantity)
    {
        Id = Guid.NewGuid();
        Name = name;
        Price = price;
        AvailableQuantity = availableQuantity;
    }

    public void Update(string name, decimal price, int availableQuantity)
    {
        Name = name;
        Price = price;
        AvailableQuantity = availableQuantity;
    }

    // Chamado quando um pedido é feito
    public void DecreaseQuantity(int quantity)
    {
        if (quantity > AvailableQuantity)
            throw new InvalidOperationException("Quantidade insuficiente em estoque.");

        AvailableQuantity -= quantity;
    }
}