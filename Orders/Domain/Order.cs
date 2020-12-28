using System;

namespace Orders.Domain
{
    public class Order
    {
        public Order()
        {
            Id = Guid.NewGuid();
            Status = EOrderStatus.Placed;
        }
        
        public Guid Id { get; set; }
        public EOrderStatus Status { get; set; }
    }

    public enum EOrderStatus
    {
        Placed,
        Billed,
        Shipped,
        Delivered,
        OutOfStock,
        Refunded
    }
}