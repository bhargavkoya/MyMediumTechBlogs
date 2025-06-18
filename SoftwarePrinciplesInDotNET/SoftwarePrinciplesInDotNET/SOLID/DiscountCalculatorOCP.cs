using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwarePrinciplesInDotNET.SOLID
{
    //Abstract base class (abstraction)
    public abstract class DiscountStrategy
    {
        public abstract decimal CalculateDiscount(decimal amount);
    }

    //Concrete implementations
    public class RegularCustomerDiscount : DiscountStrategy
    {
        public override decimal CalculateDiscount(decimal amount)
        {
            return amount * 0.05m;
        }
    }

    public class PremiumCustomerDiscount : DiscountStrategy
    {
        public override decimal CalculateDiscount(decimal amount)
        {
            return amount * 0.10m;
        }
    }

    //Client class that uses the strategy
    public class DiscountCalculatorOCP
    {
        public decimal CalculateDiscount(DiscountStrategy strategy, decimal amount)
        {
            return strategy.CalculateDiscount(amount);
        }
    }
}
