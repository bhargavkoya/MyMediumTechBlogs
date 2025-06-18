using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwarePrinciplesInDotNET.SOLID
{
    public abstract class Shape
    {
        public abstract int CalculateArea();
        public abstract string GetShapeType();
    }

    public class Rectangle : Shape
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public override int CalculateArea()
        {
            return Width * Height;
        }

        public override string GetShapeType()
        {
            return "Rectangle";
        }
    }

    public class Square : Shape
    {
        public int SideLength { get; set; }

        public override int CalculateArea()
        {
            return SideLength * SideLength;
        }

        public override string GetShapeType()
        {
            return "Square";
        }
    }

    public class AreaCalculatorLSP
    {
        //This method now works correctly for both Rectangle and Square
        //because both classes inherit from Shape and implement the required methods.
        //This adheres to the Liskov Substitution Principle (LSP).

        //Now this works correctly for both shapes
        public void CalculateShapeAreas(List<Shape> shapes)
        {
            foreach (Shape shape in shapes)
            {
                Console.WriteLine($"{shape.GetShapeType()}: Area = {shape.CalculateArea()}");
            }
        }
    }
}
