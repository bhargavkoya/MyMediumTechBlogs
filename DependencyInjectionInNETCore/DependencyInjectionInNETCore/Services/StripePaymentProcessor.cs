using DependencyInjectionInNETCore.Interfaces;

namespace DependencyInjectionInNETCore.Services
{
    public class StripePaymentProcessor : IPaymentProcessor
    {
        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            //stripe implementation
            return new PaymentResult { IsSuccess = true, TransactionId = "stripe_123" };
        }
    }
}
