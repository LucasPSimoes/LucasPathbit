using OrderApi.Domain.Enums;

namespace OrderApi.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }

    public Guid CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    public Guid ProductId { get; private set; }
    public Product? Product { get; private set; }

    public int Quantity { get; private set; }
    public decimal Price { get; private set; }
    public string ZipCode { get; private set; }
    public string DeliveryAddress { get; private set; }

    protected Order()
    {
        ZipCode = null!;
        DeliveryAddress = null!;
        Customer = null!;
        Product = null!;
    }

    public Order(Guid customerId, Guid productId, int quantity, decimal price, string zipCode, string deliveryAddress)
    {
        Id = Guid.NewGuid();
        OrderDate = DateTime.UtcNow;
        Status = OrderStatus.Sent;
        CustomerId = customerId;
        ProductId = productId;
        Quantity = quantity;
        Price = price;
        ZipCode = zipCode;
        DeliveryAddress = deliveryAddress;
    }
}


