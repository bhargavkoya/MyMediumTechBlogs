using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionsAndLinqInDotNet
{
    public class CollectionsExamples
    {
        public static void ListExample()
        {
            List<string> names = new List<string> { "Alice", "Bob", "Charlie" };
            names.Add("David");
            names.Remove("Bob");
            Console.WriteLine(string.Join(", ", names));
        }
        public static void DictionaryExample()
        {
            Dictionary<int, string> students = new Dictionary<int, string>
            {
                { 1, "Alice" },
                { 2, "Bob" }
            };
            students[3] = "Charlie";
            Console.WriteLine(string.Join(", ", students.Select(kv => $"{kv.Key}: {kv.Value}")));
        }
        public static void HashSetExample()
        {
            HashSet<int> numbers = new HashSet<int> { 1, 2, 3 };
            numbers.Add(4);
            numbers.Remove(2);
            Console.WriteLine(string.Join(", ", numbers));
        }
        public static void QueueExample()
        {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue("Alice");
            queue.Enqueue("Bob");
            Console.WriteLine(queue.Dequeue()); // Outputs: Alice
            Console.WriteLine(string.Join(", ", queue)); // Outputs: Bob
        }
        public static void StackExample()
        {
            Stack<string> stack = new Stack<string>();
            stack.Push("Alice");
            stack.Push("Bob");
            Console.WriteLine(stack.Pop()); // Outputs: Bob
            Console.WriteLine(string.Join(", ", stack)); // Outputs: Alice
        }
        public static void LinkedListExample()
        {
            LinkedList<string> linkedList = new LinkedList<string>();
            linkedList.AddLast("Alice");
            linkedList.AddLast("Bob");
            linkedList.AddFirst("Charlie");
            Console.WriteLine(string.Join(", ", linkedList)); // Outputs: Charlie, Alice, Bob
        }
        public static void SortedListExample()
        {
            SortedList<int, string> sortedList = new SortedList<int, string>
            {
                { 2, "Bob" },
                { 1, "Alice" }
            };
            sortedList.Add(3, "Charlie");
            Console.WriteLine(string.Join(", ", sortedList.Select(kv => $"{kv.Key}: {kv.Value}")));
        }
        public static void SortedDictionaryExample()
        {
            SortedDictionary<int, string> sortedDictionary = new SortedDictionary<int, string>
            {
                { 2, "Bob" },
                { 1, "Alice" }
            };
            sortedDictionary.Add(3, "Charlie");
            Console.WriteLine(string.Join(", ", sortedDictionary.Select(kv => $"{kv.Key}: {kv.Value}")));
        }

        public static void SortedSetExample()
        {
            var sortedSet = new SortedSet<int> { 5, 1, 10, 3 };
            foreach (var item in sortedSet)
                Console.WriteLine(item); // Output: 1 3 5 10

            sortedSet.Add(7);
            sortedSet.Remove(3);
            Console.WriteLine(string.Join(", ", sortedSet)); // Output: 1, 5, 7, 10
        }

        public static void ObservableCollectionExample()
        {
            var observableCollection = new System.Collections.ObjectModel.ObservableCollection<string>();
            observableCollection.CollectionChanged += (sender, e) =>
            {
                Console.WriteLine($"Collection changed: {e.Action}");
            };
            observableCollection.Add("Alice");
            observableCollection.Add("Bob");
            observableCollection.Remove("Alice");
        }
        public static void ConcurrentBagExample()
        {
            var concurrentBag = new System.Collections.Concurrent.ConcurrentBag<string>();
            concurrentBag.Add("Alice");
            concurrentBag.Add("Bob");
            Console.WriteLine(string.Join(", ", concurrentBag));
        }
        public static void ConcurrentQueueExample()
        {
            var concurrentQueue = new System.Collections.Concurrent.ConcurrentQueue<string>();
            concurrentQueue.Enqueue("Alice");
            concurrentQueue.Enqueue("Bob");
            Console.WriteLine(concurrentQueue.TryDequeue(out string result) ? result : "Queue is empty"); // Outputs: Alice
            Console.WriteLine(string.Join(", ", concurrentQueue)); // Outputs: Bob
        }
        public static void ConcurrentStackExample()
        {
            var concurrentStack = new System.Collections.Concurrent.ConcurrentStack<string>();
            concurrentStack.Push("Alice");
            concurrentStack.Push("Bob");
            Console.WriteLine(concurrentStack.TryPop(out string result) ? result : "Stack is empty"); // Outputs: Bob
            Console.WriteLine(string.Join(", ", concurrentStack)); // Outputs: Alice
        }
        public static void ImmutableListExample()
        {
            var immutableList = System.Collections.Immutable.ImmutableList.Create("Alice", "Bob");
            var newImmutableList = immutableList.Add("Charlie");
            Console.WriteLine(string.Join(", ", newImmutableList)); // Outputs: Alice, Bob, Charlie
        }
        public static void ImmutableDictionaryExample()
        {
            var immutableDictionary = System.Collections.Immutable.ImmutableDictionary.Create<int, string>()
                .Add(1, "Alice")
                .Add(2, "Bob");
            var newImmutableDictionary = immutableDictionary.Add(3, "Charlie");
            Console.WriteLine(string.Join(", ", newImmutableDictionary.Select(kv => $"{kv.Key}: {kv.Value}"))); // Outputs: 1: Alice, 2: Bob, 3: Charlie
        }
        public static void ImmutableHashSetExample()
        {
            var immutableHashSet = System.Collections.Immutable.ImmutableHashSet.Create(1, 2, 3);
            var newImmutableHashSet = immutableHashSet.Add(4);
            Console.WriteLine(string.Join(", ", newImmutableHashSet)); // Outputs: 1, 2, 3, 4
        }
        public static void ImmutableQueueExample()
        {
            var immutableQueue = System.Collections.Immutable.ImmutableQueue.Create<string>();
            immutableQueue = immutableQueue.Enqueue("Alice");
            immutableQueue = immutableQueue.Enqueue("Bob");
            Console.WriteLine(string.Join(", ", immutableQueue)); // Outputs: Alice, Bob
        }
        public static void ImmutableStackExample()
        {
            var immutableStack = System.Collections.Immutable.ImmutableStack.Create<string>();
            immutableStack = immutableStack.Push("Alice");
            immutableStack = immutableStack.Push("Bob");
            Console.WriteLine(string.Join(", ", immutableStack)); // Outputs: Bob, Alice
        }
        public static void ImmutableSortedSetExample()
        {
            var immutableSortedSet = System.Collections.Immutable.ImmutableSortedSet.Create<int>();
            immutableSortedSet = immutableSortedSet.Add(3);
            immutableSortedSet = immutableSortedSet.Add(1);
            immutableSortedSet = immutableSortedSet.Add(2);
            Console.WriteLine(string.Join(", ", immutableSortedSet)); // Outputs: 1, 2, 3
        }
        public static void ImmutableSortedDictionaryExample()
        {
            var immutableSortedDictionary = System.Collections.Immutable.ImmutableSortedDictionary.Create<int, string>();
            immutableSortedDictionary = immutableSortedDictionary.Add(2, "Bob");
            immutableSortedDictionary = immutableSortedDictionary.Add(1, "Alice");
            immutableSortedDictionary = immutableSortedDictionary.Add(3, "Charlie");
            Console.WriteLine(string.Join(", ", immutableSortedDictionary.Select(kv => $"{kv.Key}: {kv.Value}"))); // Outputs: 1: Alice, 2: Bob, 3: Charlie
        }

        public static void ExecuteCollectionsCodeSamples()
        {
            ListExample();
            DictionaryExample();
            HashSetExample();
            QueueExample();
            StackExample();
            LinkedListExample();
            SortedListExample();
            SortedDictionaryExample();
            ObservableCollectionExample();
            ConcurrentBagExample();
            ConcurrentQueueExample();
            ConcurrentStackExample();
            ImmutableListExample();
            ImmutableDictionaryExample();
            ImmutableHashSetExample();
            ImmutableQueueExample();
            ImmutableStackExample();
            ImmutableSortedSetExample();
            ImmutableSortedDictionaryExample();
        }
    }
}
