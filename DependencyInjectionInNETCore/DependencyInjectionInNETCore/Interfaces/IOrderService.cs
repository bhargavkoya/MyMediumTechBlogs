namespace DependencyInjectionInNETCore.Interfaces
{
    public interface IOrderService
    {
        Task CreateOrderAsync(OrderRequest orderRequest);
    }

    public class OrderRequest
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        // Other order properties can be added here
    }
}
