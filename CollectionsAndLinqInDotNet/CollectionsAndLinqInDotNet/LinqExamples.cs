using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionsAndLinqInDotNet
{
    public class LinqExamples
    {
        public static void LinqQueryExample()
        {
            List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6 };
            var evenNumbers = from n in numbers
                              where n % 2 == 0
                              select n;
            Console.WriteLine(string.Join(", ", evenNumbers)); // Outputs: 2, 4, 6
        }
        public static void LinqMethodSyntaxExample()
        {
            List<string> names = new List<string> { "Alice", "Bob", "Charlie", "David" };
            var filteredNames = names.Where(n => n.Length > 3).OrderBy(n => n);
            Console.WriteLine(string.Join(", ", filteredNames)); // Outputs: Alice, Charlie, David
        }
        public static void LinqAggregationExample()
        {
            List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };
            int sum = numbers.Sum();
            Console.WriteLine($"Sum: {sum}"); // Outputs: Sum: 15
        }
        public static void LinqGroupingExample()
        {
            List<string> fruits = new List<string> { "Apple", "Banana", "Cherry", "Date", "Elderberry" };
            var groupedFruits = fruits.GroupBy(f => f.Length);
            foreach (var group in groupedFruits)
            {
                Console.WriteLine($"Length {group.Key}: {string.Join(", ", group)}");
            }
            // Outputs:
            // Length 5: Apple, Banana
            // Length 6: Cherry, Elderberry
            // Length 4: Date
        }
        public static void LinqGroupByExample()
        {
            List<string> words = new List<string> { "apple", "banana", "cherry", "date", "fig", "grape" };
            var groupedWords = words.GroupBy(w => w.Length);
            foreach (var group in groupedWords)
            {
                Console.WriteLine($"Length {group.Key}: {string.Join(", ", group)}");
            }
            // Outputs:
            // Length 3: fig
            // Length 4: date
            // Length 5: apple, grape
            // Length 6: banana, cherry
        }
        public static void LinqJoinExample()
        {
            List<string> names = new List<string> { "Alice", "Bob", "Charlie" };
            List<int> ages = new List<int> { 25, 30, 35 };
            var joined = names.Zip(ages, (name, age) => $"{name} is {age} years old");
            Console.WriteLine(string.Join(", ", joined)); // Outputs: Alice is 25 years old, Bob is 30 years old, Charlie is 35 years old
        }
        public static void LinqDistinctExample()
        {
            List<int> numbers = new List<int> { 1, 2, 2, 3, 4, 4, 5 };
            var distinctNumbers = numbers.Distinct();
            Console.WriteLine(string.Join(", ", distinctNumbers)); // Outputs: 1, 2, 3, 4, 5
        }
        public static void LinqSelectExample()
        {
            List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };
            var squaredNumbers = numbers.Select(n => n * n);
            Console.WriteLine(string.Join(", ", squaredNumbers)); // Outputs: 1, 4, 9, 16, 25
        }
        public static void LinqFirstOrDefaultExample()
        {
            List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };
            int firstEven = numbers.FirstOrDefault(n => n % 2 == 0);
            Console.WriteLine(firstEven); // Outputs: 2
        }
        public static void LinqAnyExample()
        {
            List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };
            bool hasEven = numbers.Any(n => n % 2 == 0);
            Console.WriteLine(hasEven); // Outputs: True
        }
        public static void LinqAllExample()
        {
            List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };
            bool allGreaterThanZero = numbers.All(n => n > 0);
            Console.WriteLine(allGreaterThanZero); // Outputs: True
        }

        public static void ExecuteLinqCodeSamples()
        {
            LinqQueryExample();
            LinqMethodSyntaxExample();
            LinqAggregationExample();
            LinqGroupingExample();
            LinqGroupByExample();
            LinqJoinExample();
            LinqDistinctExample();
            LinqSelectExample();
            LinqFirstOrDefaultExample();
            LinqAnyExample();
            LinqAllExample();
        }
    }
}
