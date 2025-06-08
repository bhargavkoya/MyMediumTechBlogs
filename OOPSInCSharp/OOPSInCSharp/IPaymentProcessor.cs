using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOPSInCSharp
{
    public interface IPaymentProcessor
    {
        Task<PaymentResult> ProcessPaymentAsync(decimal amount, PaymentDetails details);
        bool SupportsRefunds { get; }
        Task<RefundResult> RefundPaymentAsync(string transactionId, decimal amount);
    }
    public record PaymentResult(bool Success, string TransactionId, string Message);
    public record RefundResult(bool Success, string Message);
    public abstract record PaymentDetails(decimal Amount);

    // Specific payment details
    public record CreditCardDetails(decimal Amount, string CardNumber, string CVV, DateTime Expiry)
        : PaymentDetails(Amount);
    public record PayPalDetails(decimal Amount, string Email)
        : PaymentDetails(Amount);
    public record CryptoDetails(decimal Amount, string WalletAddress, string Currency)
        : PaymentDetails(Amount);

    // Implementations
    public class CreditCardProcessor : IPaymentProcessor
    {
        public bool SupportsRefunds => true;

        public async Task<PaymentResult> ProcessPaymentAsync(decimal amount, PaymentDetails details)
        {
            if (details is not CreditCardDetails cardDetails)
                return new PaymentResult(false, "", "Invalid payment details");

            // Simulate processing
            await Task.Delay(1000);

            var transactionId = $"CC_{Guid.NewGuid():N}";
            return new PaymentResult(true, transactionId, $"Charged ${amount} to card ending in {cardDetails.CardNumber[^4..]}");
        }

        public async Task<RefundResult> RefundPaymentAsync(string transactionId, decimal amount)
        {
            await Task.Delay(500);
            return new RefundResult(true, $"Refunded ${amount} to original card");
        }
    }

    public class PayPalProcessor : IPaymentProcessor
    {
        public bool SupportsRefunds => true;

        public async Task<PaymentResult> ProcessPaymentAsync(decimal amount, PaymentDetails details)
        {
            if (details is not PayPalDetails paypalDetails)
                return new PaymentResult(false, "", "Invalid payment details");

            await Task.Delay(800);

            var transactionId = $"PP_{Guid.NewGuid():N}";
            return new PaymentResult(true, transactionId, $"Paid ${amount} via PayPal ({paypalDetails.Email})");
        }

        public async Task<RefundResult> RefundPaymentAsync(string transactionId, decimal amount)
        {
            await Task.Delay(600);
            return new RefundResult(true, $"Refunded ${amount} to PayPal account");
        }
    }

    public class CryptoProcessor : IPaymentProcessor
    {
        public bool SupportsRefunds => false; // Crypto transactions are typically irreversible

        public async Task<PaymentResult> ProcessPaymentAsync(decimal amount, PaymentDetails details)
        {
            if (details is not CryptoDetails cryptoDetails)
                return new PaymentResult(false, "", "Invalid payment details");

            await Task.Delay(2000); // Blockchain processing takes longer

            var transactionId = $"CRYPTO_{Guid.NewGuid():N}";
            return new PaymentResult(true, transactionId,
                $"Sent ${amount} {cryptoDetails.Currency} to {cryptoDetails.WalletAddress}");
        }

        public async Task<RefundResult> RefundPaymentAsync(string transactionId, decimal amount)
        {
            return new RefundResult(false, "Cryptocurrency transactions cannot be refunded automatically");
        }
    }

    // Payment Service - Now flexible and extensible!
    public class PaymentService
    {
        private readonly Dictionary<string, IPaymentProcessor> _processors;

        public PaymentService()
        {
            _processors = new Dictionary<string, IPaymentProcessor>
            {
                ["CreditCard"] = new CreditCardProcessor(),
                ["PayPal"] = new PayPalProcessor(),
                ["Crypto"] = new CryptoProcessor()
            };
        }

        // Adding new processors is easy - no code changes needed!
        public void RegisterProcessor(string name, IPaymentProcessor processor)
        {
            _processors[name] = processor;
        }

        public async Task<PaymentResult> ProcessPaymentAsync(string processorType, PaymentDetails details)
        {
            if (!_processors.TryGetValue(processorType, out var processor))
                return new PaymentResult(false, "", "Payment processor not supported");

            return await processor.ProcessPaymentAsync(details.Amount, details);
        }

        public async Task<RefundResult> RefundPaymentAsync(string processorType, string transactionId, decimal amount)
        {
            if (!_processors.TryGetValue(processorType, out var processor))
                return new RefundResult(false, "Payment processor not found");

            if (!processor.SupportsRefunds)
                return new RefundResult(false, "This payment method doesn't support refunds");

            return await processor.RefundPaymentAsync(transactionId, amount);
        }
    }
}
