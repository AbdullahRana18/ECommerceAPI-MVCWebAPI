namespace ECommerceAPI.Dtos
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Har order ke multiple items honge
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDtos
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
