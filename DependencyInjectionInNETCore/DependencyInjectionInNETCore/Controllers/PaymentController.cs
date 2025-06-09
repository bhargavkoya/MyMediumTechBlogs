using DependencyInjectionInNETCore.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DependencyInjectionInNETCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        [HttpPost("stripe")]
        public async Task<IActionResult> ProcessStripePayment(
        [FromKeyedServices("stripe")] IPaymentProcessor paymentProcessor,
        PaymentRequest request)
        {
            var result = await paymentProcessor.ProcessPaymentAsync(request);
            return Ok(result);
        }

        [HttpPost("paypal")]
        public async Task<IActionResult> ProcessPayPalPayment(
            [FromKeyedServices("paypal")] IPaymentProcessor paymentProcessor,
            PaymentRequest request)
        {
            var result = await paymentProcessor.ProcessPaymentAsync(request);
            return Ok(result);
        }
    }
}
