namespace DependencyInjectionInNETCore.Interfaces
{
    public interface IPaymentProcessor
    {
        Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
    }

    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentMethod { get; set; } // e.g., "CreditCard", "PayPal"
        public string CustomerEmail { get; set; }
        // Other payment properties can be added here
    }
    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string TransactionId { get; set; }
        public string ErrorMessage { get; set; } // Optional, for error details
        // Other result properties can be added here
    }
}
