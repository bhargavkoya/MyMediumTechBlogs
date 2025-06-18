using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwarePrinciplesInDotNET.RealWorldUsecases
{
    //Interfaces (abstractions)
    public interface IOrderValidator
    {
        bool Validate(Order order);
    }

    public interface IPriceCalculator
    {
        decimal CalculatePrice(Order order);
    }

    public interface IPaymentGateway
    {
        bool ProcessPayment(Order order, decimal amount);
    }

    public interface INotificationService
    {
        void SendConfirmation(Order order);
    }

    //Implementations
    public class OrderValidator : IOrderValidator
    {
        public bool Validate(Order order)
        {
            //Validation logic
            return order != null && order.Items.Count > 0 && order.Customer != null;
        }
    }

    public class PriceCalculator : IPriceCalculator
    {
        public decimal CalculatePrice(Order order)
        {
            //Price calculation logic
            decimal total = 0;
            foreach (var item in order.Items)
            {
                total += item.Price * item.Quantity;
            }
            return total;
        }
    }

    public class StripePaymentGateway : IPaymentGateway
    {
        public bool ProcessPayment(Order order, decimal amount)
        {
            //Payment processing logic
            Console.WriteLine($"Processing payment of {amount:C} via Stripe");
            return true;
        }
    }

    public class EmailNotificationService : INotificationService
    {
        public void SendConfirmation(Order order)
        {
            //Email sending logic
            Console.WriteLine($"Sending order confirmation to {order.Customer.Email}");
        }
    }

    //High-level component that orchestrates the workflow
    public class OrderService
    {
        private readonly IOrderValidator _validator;
        private readonly IPriceCalculator _priceCalculator;
        private readonly IPaymentGateway _paymentGateway;
        private readonly INotificationService _notificationService;

        public OrderService(
            IOrderValidator validator,
            IPriceCalculator priceCalculator,
            IPaymentGateway paymentGateway,
            INotificationService notificationService)
        {
            _validator = validator;
            _priceCalculator = priceCalculator;
            _paymentGateway = paymentGateway;
            _notificationService = notificationService;
        }

        public bool ProcessOrder(Order order)
        {
            //Single responsibility for orchestration
            if (!_validator.Validate(order))
            {
                return false;
            }

            var price = _priceCalculator.CalculatePrice(order);

            if (!_paymentGateway.ProcessPayment(order, price))
            {
                return false;
            }

            _notificationService.SendConfirmation(order);

            return true;
        }
    }

    //domain models
    public class Order
    {
        public Customer Customer { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    public class Customer
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class OrderItem
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
