using ECommerceWithRoutingAndMiddleware.Entities;

namespace ECommerceWithRoutingAndMiddleware.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(CreateOrderRequest request, string? userId);
        Task<PagedResult<Order>> GetUserOrdersAsync(string? userId, int page, int pageSize);
        Task<SalesAnalytics> GetSalesAnalyticsAsync(DateTime? from, DateTime? to);
    }

    public class OrderService : IOrderService
    {
        public Task<Order> CreateOrderAsync(CreateOrderRequest request, string? userId)
        {
            throw new NotImplementedException();
        }

        public Task<SalesAnalytics> GetSalesAnalyticsAsync(DateTime? from, DateTime? to)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<Order>> GetUserOrdersAsync(string? userId, int page, int pageSize)
        {
            throw new NotImplementedException();
        }
    }

    public record CreateOrderRequest(List<OrderItemRequest> Items, string ShippingAddress);
    public record OrderItemRequest(int ProductId, int Quantity);
    public record SalesAnalytics(decimal TotalRevenue, int TotalOrders, decimal AverageOrderValue);
}
