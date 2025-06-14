using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace DelegatesEventsCallbacksDemo
{
    // ======= DELEGATE TYPES DEMONSTRATION =======

    // 1. Custom Delegates
    public delegate decimal TaxCalculationDelegate(decimal amount, string region);
    public delegate decimal DiscountCalculationDelegate(decimal amount, CustomerType customerType);
    public delegate void OrderOperationDelegate(OrderInfo order);
    public delegate bool ValidationDelegate<T>(T item);

    // 2. Event Arguments Classes
    public class OrderProcessedEventArgs : EventArgs
    {
        public string OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime ProcessedAt { get; set; }
        public OrderStatus Status { get; set; }
    }

    public class PaymentEventArgs : EventArgs
    {
        public string PaymentId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public bool IsSuccessful { get; set; }
    }

    // ======= ENUMS AND VALUE TYPES =======
    public enum CustomerType { Regular, Premium, VIP }
    public enum OrderStatus { Pending, Processing, Completed, Failed }
    public enum PaymentMethod { CreditCard, PayPal, BankTransfer }

    // ======= DATA MODELS =======
    public class OrderInfo
    {
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public CustomerType CustomerType { get; set; }
        public string Region { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
    }

    public class Customer
    {
        public string CustomerId { get; set; }
        public string Name { get; set; }
        public CustomerType Type { get; set; }
        public string Region { get; set; }
    }

    // ======= WEAK EVENT IMPLEMENTATION =======
    public class WeakEventSubscription
    {
        private readonly List<WeakReference> _subscribers = new();

        public void Subscribe(EventHandler<OrderProcessedEventArgs> handler)
        {
            _subscribers.Add(new WeakReference(handler));
        }

        public void Unsubscribe(EventHandler<OrderProcessedEventArgs> handler)
        {
            _subscribers.RemoveAll(wr => !wr.IsAlive || wr.Target == handler);
        }

        public void Raise(object sender, OrderProcessedEventArgs e)
        {
            var toRemove = new List<WeakReference>();

            foreach (var wr in _subscribers)
            {
                if (wr.IsAlive && wr.Target is EventHandler<OrderProcessedEventArgs> handler)
                {
                    try
                    {
                        handler(sender, e);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in weak event handler: {ex.Message}");
                    }
                }
                else
                {
                    toRemove.Add(wr);
                }
            }

            foreach (var wr in toRemove)
                _subscribers.Remove(wr);
        }
    }

    // ======= MAIN ORDER PROCESSOR CLASS =======
    public class OrderProcessor
    {
        // Custom delegate properties for flexible calculations
        public TaxCalculationDelegate TaxCalculator { get; set; }
        public DiscountCalculationDelegate DiscountCalculator { get; set; }

        // Multicast delegate for order operations
        public OrderOperationDelegate OrderOperations { get; set; }

        // Various event types demonstration
        public event EventHandler<OrderProcessedEventArgs> OrderProcessed;
        public event EventHandler<PaymentEventArgs> PaymentProcessed;
        public event Action<string> StatusUpdated;
        public event Func<OrderInfo, bool> OrderValidating;

        // Weak event implementation
        private readonly WeakEventSubscription _weakOrderProcessed = new();

        // Generic delegates for validation
        public Predicate<OrderInfo> OrderValidator { get; set; }
        public Func<OrderInfo, string> OrderFormatter { get; set; }
        public Action<Exception> ErrorHandler { get; set; }

        public OrderProcessor()
        {
            // Default implementations
            TaxCalculator = CalculateStandardTax;
            DiscountCalculator = CalculateStandardDiscount;
            OrderValidator = DefaultOrderValidation;
            OrderFormatter = DefaultOrderFormatter;
            ErrorHandler = DefaultErrorHandler;
        }

        // ======= SYNCHRONOUS PROCESSING WITH DELEGATES =======
        public OrderInfo ProcessOrder(OrderInfo order)
        {
            try
            {
                StatusUpdated?.Invoke($"Starting order processing for {order.OrderId}");

                // Validation using Predicate delegate
                if (!OrderValidator(order))
                {
                    order.Status = OrderStatus.Failed;
                    StatusUpdated?.Invoke($"Order {order.OrderId} failed validation");
                    return order;
                }

                // Event-based validation with return values
                var validationResults = OrderValidating?.GetInvocationList()
                    .Cast<Func<OrderInfo, bool>>()
                    .Select(validator => validator(order))
                    .ToArray();

                if (validationResults?.Any(r => !r) == true)
                {
                    order.Status = OrderStatus.Failed;
                    return order;
                }

                // Apply calculations using custom delegates
                decimal tax = TaxCalculator?.Invoke(order.Amount, order.Region) ?? 0;
                decimal discount = DiscountCalculator?.Invoke(order.Amount, order.CustomerType) ?? 0;

                order.Amount = order.Amount + tax - discount;
                order.Status = OrderStatus.Processing;

                // Execute multicast delegate operations
                OrderOperations?.Invoke(order);

                order.Status = OrderStatus.Completed;

                // Raise events
                OnOrderProcessed(new OrderProcessedEventArgs
                {
                    OrderId = order.OrderId,
                    TotalAmount = order.Amount,
                    ProcessedAt = DateTime.UtcNow,
                    Status = order.Status
                });

                StatusUpdated?.Invoke($"Order {order.OrderId} completed successfully");
                return order;
            }
            catch (Exception ex)
            {
                ErrorHandler?.Invoke(ex);
                order.Status = OrderStatus.Failed;
                return order;
            }
        }

        // ======= ASYNCHRONOUS PROCESSING WITH CALLBACKS =======
        public async Task<OrderInfo> ProcessOrderAsync(OrderInfo order,
            Func<OrderInfo, Task> onSuccess = null,
            Action<Exception> onError = null,
            Action<string> progressCallback = null)
        {
            try
            {
                progressCallback?.Invoke("Starting async order processing...");

                // Simulate async validation
                await Task.Delay(100);
                progressCallback?.Invoke("Validation completed");

                if (!OrderValidator(order))
                {
                    var exception = new InvalidOperationException("Order validation failed");
                    onError?.Invoke(exception);
                    return order;
                }

                // Simulate async tax calculation
                await Task.Delay(200);
                progressCallback?.Invoke("Tax calculation completed");

                decimal tax = TaxCalculator?.Invoke(order.Amount, order.Region) ?? 0;
                decimal discount = DiscountCalculator?.Invoke(order.Amount, order.CustomerType) ?? 0;

                order.Amount = order.Amount + tax - discount;
                order.Status = OrderStatus.Completed;

                // Success callback
                if (onSuccess != null)
                {
                    await onSuccess(order);
                }

                progressCallback?.Invoke("Order processing completed");
                return order;
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                order.Status = OrderStatus.Failed;
                return order;
            }
        }

        // ======= EVENT HANDLING METHODS =======
        protected virtual void OnOrderProcessed(OrderProcessedEventArgs e)
        {
            OrderProcessed?.Invoke(this, e);
            _weakOrderProcessed.Raise(this, e);
        }

        public void SubscribeToWeakEvent(EventHandler<OrderProcessedEventArgs> handler)
        {
            _weakOrderProcessed.Subscribe(handler);
        }

        // ======= DEFAULT IMPLEMENTATIONS =======
        private decimal CalculateStandardTax(decimal amount, string region)
        {
            return region.ToUpper() switch
            {
                "US" => amount * 0.08m,
                "EU" => amount * 0.20m,
                "CA" => amount * 0.12m,
                _ => amount * 0.05m
            };
        }

        private decimal CalculateStandardDiscount(decimal amount, CustomerType customerType)
        {
            return customerType switch
            {
                CustomerType.VIP => amount * 0.15m,
                CustomerType.Premium => amount * 0.10m,
                CustomerType.Regular => amount * 0.05m,
                _ => 0
            };
        }

        private bool DefaultOrderValidation(OrderInfo order)
        {
            return order != null &&
                   !string.IsNullOrEmpty(order.OrderId) &&
                   order.Amount > 0;
        }

        private string DefaultOrderFormatter(OrderInfo order)
        {
            return $"Order {order.OrderId}: {order.Amount:C} ({order.Status})";
        }

        private void DefaultErrorHandler(Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    // ======= ASYNC CALLBACK SERVICE =======
    public class AsyncCallbackService
    {
        // Synchronous callbacks
        public void ProcessWithCallbacks(string data,
            Action<string> onSuccess,
            Action<Exception> onError)
        {
            try
            {
                string result = data.ToUpper();
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
        }

        // Asynchronous callbacks with Task
        public async Task ProcessAsyncWithCallbacks(string data,
            Func<string, Task> onSuccess,
            Func<Exception, Task> onError)
        {
            try
            {
                await Task.Delay(1000); // Simulate async work
                string result = data.ToUpper();
                if (onSuccess != null)
                    await onSuccess(result);
            }
            catch (Exception ex)
            {
                if (onError != null)
                    await onError(ex);
            }
        }

        // Continuation-based callbacks
        public Task<T> ProcessWithContinuation<T>(T data, Func<T, T> processor)
        {
            return Task.FromResult(data)
                .ContinueWith(t => processor(t.Result))
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        Console.WriteLine($"Processing failed: {t.Exception?.GetBaseException().Message}");
                    return t.Result;
                });
        }
    }

    // ======= EVENT SUBSCRIBER CLASS =======
    public class OrderEventSubscriber : IDisposable
    {
        private readonly OrderProcessor _processor;
        private bool _disposed = false;

        public OrderEventSubscriber(OrderProcessor processor)
        {
            _processor = processor;
            Subscribe();
        }

        private void Subscribe()
        {
            _processor.OrderProcessed += OnOrderProcessed;
            _processor.PaymentProcessed += OnPaymentProcessed;
            _processor.StatusUpdated += OnStatusUpdated;
            _processor.OrderValidating += OnOrderValidating;
        }

        private void OnOrderProcessed(object sender, OrderProcessedEventArgs e)
        {
            Console.WriteLine($"✓ Order processed: {e.OrderId} - {e.TotalAmount:C} at {e.ProcessedAt}");
        }

        private void OnPaymentProcessed(object sender, PaymentEventArgs e)
        {
            Console.WriteLine($"💳 Payment {e.PaymentId}: {e.Amount:C} via {e.Method} - {(e.IsSuccessful ? "Success" : "Failed")}");
        }

        private void OnStatusUpdated(string status)
        {
            Console.WriteLine($"📊 Status: {status}");
        }

        private bool OnOrderValidating(OrderInfo order)
        {
            bool isValid = order.Amount >= 10; // Minimum order amount
            Console.WriteLine($"🔍 Validating order {order.OrderId}: {(isValid ? "Valid" : "Invalid")}");
            return isValid;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _processor.OrderProcessed -= OnOrderProcessed;
                _processor.PaymentProcessed -= OnPaymentProcessed;
                _processor.StatusUpdated -= OnStatusUpdated;
                _processor.OrderValidating -= OnOrderValidating;
                _disposed = true;
            }
        }
    }

    // ======= DEMONSTRATION PROGRAM =======
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== Delegates, Events, and Callbacks Comprehensive Demo ===\n");

            // Create processor and subscriber
            var processor = new OrderProcessor();
            using var subscriber = new OrderEventSubscriber(processor);

            // Demonstrate custom delegate assignments
            processor.TaxCalculator = (amount, region) =>
            {
                Console.WriteLine($"💰 Calculating tax for {region}: {amount * 0.1m:C}");
                return amount * 0.1m;
            };

            processor.DiscountCalculator = (amount, customerType) =>
            {
                decimal discount = customerType == CustomerType.VIP ? amount * 0.2m : amount * 0.05m;
                Console.WriteLine($"🎯 Applying {customerType} discount: {discount:C}");
                return discount;
            };

            // Multicast delegate operations
            processor.OrderOperations += order => Console.WriteLine($"📦 Packaging order {order.OrderId}");
            processor.OrderOperations += order => Console.WriteLine($"🚚 Scheduling delivery for {order.OrderId}");
            processor.OrderOperations += order => Console.WriteLine($"📧 Sending confirmation email for {order.OrderId}");

            // Test orders
            var orders = new[]
            {
                new OrderInfo { OrderId = "ORD001", Amount = 100m, CustomerType = CustomerType.VIP, Region = "US" },
                new OrderInfo { OrderId = "ORD002", Amount = 50m, CustomerType = CustomerType.Premium, Region = "EU" },
                new OrderInfo { OrderId = "ORD003", Amount = 5m, CustomerType = CustomerType.Regular, Region = "CA" } // This will fail validation
            };

            Console.WriteLine("--- Synchronous Processing with Delegates ---");
            foreach (var order in orders)
            {
                Console.WriteLine($"\nProcessing {order.OrderId}:");
                var result = processor.ProcessOrder(order);
                Console.WriteLine($"Final result: {processor.OrderFormatter(result)}");
            }

            Console.WriteLine("\n--- Asynchronous Processing with Callbacks ---");
            var asyncOrder = new OrderInfo { OrderId = "ASYNC001", Amount = 200m, CustomerType = CustomerType.VIP, Region = "US" };

            await processor.ProcessOrderAsync(asyncOrder,
                onSuccess: async order =>
                {
                    Console.WriteLine($"✅ Async success callback: Order {order.OrderId} completed with amount {order.Amount:C}");
                    await Task.Delay(100);
                },
                onError: ex => Console.WriteLine($"❌ Async error callback: {ex.Message}"),
                progressCallback: progress => Console.WriteLine($"⏳ Progress: {progress}")
            );

            Console.WriteLine("\n--- Generic Delegate Examples ---");

            // Func delegate examples
            Func<int, int, int> calculator = (x, y) => x + y;
            Console.WriteLine($"Calculator result: {calculator(5, 3)}");

            // Action delegate examples
            Action<string> logger = message => Console.WriteLine($"[LOG] {message}");
            logger("This is a log message via Action delegate");

            // Predicate delegate examples
            Predicate<int> isEven = number => number % 2 == 0;
            Console.WriteLine($"Is 4 even? {isEven(4)}");
            Console.WriteLine($"Is 5 even? {isEven(5)}");

            Console.WriteLine("\n--- Callback Service Demo ---");
            var callbackService = new AsyncCallbackService();

            // Synchronous callbacks
            callbackService.ProcessWithCallbacks("hello world",
                onSuccess: result => Console.WriteLine($"Sync callback success: {result}"),
                onError: ex => Console.WriteLine($"Sync callback error: {ex.Message}")
            );

            // Asynchronous callbacks
            await callbackService.ProcessAsyncWithCallbacks("async hello",
                onSuccess: async result =>
                {
                    Console.WriteLine($"Async callback success: {result}");
                    await Task.Delay(100);
                },
                onError: async ex =>
                {
                    Console.WriteLine($"Async callback error: {ex.Message}");
                    await Task.Delay(100);
                }
            );

            Console.WriteLine("\n--- Multicast Delegate Error Handling ---");
            DemonstrateMulticastErrorHandling();

            Console.WriteLine("\n--- Performance Comparison ---");
            PerformanceComparison();

            Console.WriteLine("\n=== Demo completed successfully! ===");
        }

        private static void DemonstrateMulticastErrorHandling()
        {
            Action<string> multicastAction = message => Console.WriteLine($"Handler 1: {message}");
            multicastAction += message => throw new Exception("Handler 2 failed!");
            multicastAction += message => Console.WriteLine($"Handler 3: {message}");

            // Safe multicast invocation
            var handlers = multicastAction.GetInvocationList();
            foreach (Action<string> handler in handlers)
            {
                try
                {
                    handler("Test message");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Handler failed: {ex.Message}");
                }
            }
        }

        private static void PerformanceComparison()
        {
            const int iterations = 1000000;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Direct method call
            stopwatch.Restart();
            for (int i = 0; i < iterations; i++)
            {
                DirectMethod(i);
            }
            stopwatch.Stop();
            Console.WriteLine($"Direct method: {stopwatch.ElapsedTicks} ticks");

            // Delegate call
            Action<int> delegateMethod = DirectMethod;
            stopwatch.Restart();
            for (int i = 0; i < iterations; i++)
            {
                delegateMethod(i);
            }
            stopwatch.Stop();
            Console.WriteLine($"Delegate call: {stopwatch.ElapsedTicks} ticks");

            // Func delegate
            Func<int, int> funcDelegate = x => x * 2;
            stopwatch.Restart();
            for (int i = 0; i < iterations; i++)
            {
                funcDelegate(i);
            }
            stopwatch.Stop();
            Console.WriteLine($"Func delegate: {stopwatch.ElapsedTicks} ticks");
        }

        private static void DirectMethod(int value)
        {
            // Simple operation for performance testing
            var result = value * 2;
        }
    }
}