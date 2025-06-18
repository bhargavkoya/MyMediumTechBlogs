using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwarePrinciplesInDotNET.OtherPrinciples
{
    public static class CircleCalculatorDRYKISS
    {
        private const double PI = Math.PI;

        public static double Area(double radius)
        {
            return PI * Math.Pow(radius, 2);
        }

        public static double Circumference(double radius)
        {
            return 2 * PI * radius;
        }

        public static double SphereVolume(double radius)
        {
            return 4.0 / 3.0 * PI * Math.Pow(radius, 3);
        }

        //Complex approach
        public static bool IsNumericComplex(string input)
        {
            foreach (char c in input)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }

        //Simple approach using KISS principle
        public static bool IsNumericSimple(string input)
        {
            return input.All(char.IsDigit);
        }
    }
}
