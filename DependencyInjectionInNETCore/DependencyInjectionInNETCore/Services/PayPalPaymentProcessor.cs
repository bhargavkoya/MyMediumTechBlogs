using DependencyInjectionInNETCore.Interfaces;

namespace DependencyInjectionInNETCore.Services
{
    public class PayPalPaymentProcessor: IPaymentProcessor
    {
        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            // PayPal implementation
            return new PaymentResult { IsSuccess = true, TransactionId = "paypal_123" };
        }
    }
}
