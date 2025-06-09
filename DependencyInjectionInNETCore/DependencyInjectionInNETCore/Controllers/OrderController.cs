using DependencyInjectionInNETCore.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DependencyInjectionInNETCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IOrderService _orderService;

        public OrderController(IEmailService emailService, IOrderService orderService)
        {
            _emailService = emailService;
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(OrderRequest request)
        {
            await _orderService.CreateOrderAsync(request);
            await _emailService.SendEmailAsync(request.Email, "Order Confirmation", "Thank you!");
            return Ok("order");
        }
    }
}
