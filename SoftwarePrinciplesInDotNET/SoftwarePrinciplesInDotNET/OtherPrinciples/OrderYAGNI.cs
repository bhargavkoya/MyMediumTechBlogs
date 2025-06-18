using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwarePrinciplesInDotNET.OtherPrinciples
{
    //Without YAGNI - Overengineered
    public class Order
    {
        public decimal CalculateTotal(decimal discount, decimal tax, bool applyDiscount = true,
                                    bool includeTax = true, decimal additionalFees = 0)
        {
            decimal subtotal = GetSubtotal();

            if (applyDiscount)
            {
                subtotal -= discount;
            }

            if (includeTax)
            {
                subtotal += tax;
            }

            return subtotal + additionalFees;
        }
        private decimal GetSubtotal()
        {
            return 10;
        }
    }

    //With YAGNI - Simple and focused on current needs
    public class OrderYAGNI
    {
        public decimal CalculateTotal(decimal discount, decimal tax)
        {
            decimal subtotal = GetSubtotal();
            subtotal -= discount;
            return subtotal + tax;
        }

        private decimal GetSubtotal()
        {
            return 10;
        }
    }
}
