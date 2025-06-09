using DependencyInjectionInNETCore.Interfaces;

namespace DependencyInjectionInNETCore.Services
{
    public class OrderService : IOrderService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public OrderService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task CreateOrderAsync(OrderRequest orderRequest)
        {
            _logger.LogInformation($"Creating new order by {orderRequest.CustomerName} with price {orderRequest.Price} and quantity{orderRequest.Quantity}");
            //implementation details to create order
        }
    }
}
