
// ============================================================================
// BASIC THREADING EXAMPLES - Building Blocks
// ============================================================================

// 1. Creating Your First Thread
using System;
using System.Collections.Concurrent;
using System.Threading;

public class BasicThreadingExamples
{
    public static void CreateFirstThread()
    {
        // Create a thread with a method
        Thread workerThread = new Thread(new ThreadStart(DoWork));
        workerThread.Name = "WorkerThread";
        workerThread.Start();

        // Wait for the thread to complete
        workerThread.Join();

        Console.WriteLine("Main thread continues...");
    }

    static void DoWork()
    {
        for (int i = 0; i < 5; i++)
        {
            Console.WriteLine($"Worker thread: {i}");
            Thread.Sleep(1000); // Simulate work
        }
    }

    // 2. Parameterized Threading
    public static void ParameterizedThreadExample()
    {
        Thread thread1 = new Thread(PrintNumbers);
        Thread thread2 = new Thread(PrintNumbers);

        thread1.Start(5);  // Pass parameter
        thread2.Start(10); // Pass different parameter

        thread1.Join();
        thread2.Join();
    }

    static void PrintNumbers(object max)
    {
        int maxNumber = (int)max;
        for (int i = 1; i <= maxNumber; i++)
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}: {i}");
            Thread.Sleep(500);
        }
    }
}

// ============================================================================
// THREAD POOL EXAMPLES
// ============================================================================

public class ThreadPoolExamples
{
    public static void BasicThreadPoolUsage()
    {
        // Queue work to thread pool
        for (int i = 0; i < 5; i++)
        {
            int workerId = i;
            ThreadPool.QueueUserWorkItem(state =>
            {
                Console.WriteLine($"Worker {workerId} starting on thread {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(2000); // Simulate work
                Console.WriteLine($"Worker {workerId} completed");
            });
        }

        // Wait for all work to complete (simplified)
        Thread.Sleep(5000);
    }

    public static void ThreadPoolWithWaitHandle()
    {
        const int workItems = 5;
        var doneEvents = new ManualResetEvent[workItems];

        for (int i = 0; i < workItems; i++)
        {
            doneEvents[i] = new ManualResetEvent(false);
            var workObject = new WorkObject(i, doneEvents[i]);
            ThreadPool.QueueUserWorkItem(workObject.DoWork);
        }

        // Wait for all work items to complete
        WaitHandle.WaitAll(doneEvents);
        Console.WriteLine("All work completed!");
    }
}

public class WorkObject
{
    private readonly int _id;
    private readonly ManualResetEvent _doneEvent;

    public WorkObject(int id, ManualResetEvent doneEvent)
    {
        _id = id;
        _doneEvent = doneEvent;
    }

    public void DoWork(object state)
    {
        Console.WriteLine($"Work item {_id} started");
        Thread.Sleep(1000); // Simulate work
        Console.WriteLine($"Work item {_id} completed");
        _doneEvent.Set(); // Signal completion
    }
}

// ============================================================================
// SYNCHRONIZATION EXAMPLES
// ============================================================================

public class SynchronizationExamples
{
    private static int _counter = 0;
    private static readonly object _lockObject = new object();

    // Race Condition Example
    public static void RaceConditionExample()
    {
        const int threadCount = 5;
        const int incrementsPerThread = 1000;
        var threads = new Thread[threadCount];

        // Reset counter
        _counter = 0;

        // Create threads that increment counter unsafely
        for (int i = 0; i < threadCount; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < incrementsPerThread; j++)
                {
                    _counter++; // UNSAFE! Race condition possible
                }
            });
        }

        // Start all threads
        foreach (var thread in threads)
            thread.Start();

        // Wait for all to complete
        foreach (var thread in threads)
            thread.Join();

        Console.WriteLine($"Expected: {threadCount * incrementsPerThread}");
        Console.WriteLine($"Actual: {_counter}");
        Console.WriteLine($"Lost increments: {(threadCount * incrementsPerThread) - _counter}");
    }

    // Thread-Safe Version
    public static void ThreadSafeExample()
    {
        const int threadCount = 5;
        const int incrementsPerThread = 1000;
        var threads = new Thread[threadCount];

        // Reset counter
        _counter = 0;

        // Create threads that increment counter safely
        for (int i = 0; i < threadCount; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < incrementsPerThread; j++)
                {
                    lock (_lockObject)
                    {
                        _counter++; // SAFE! Protected by lock
                    }
                }
            });
        }

        // Start all threads
        foreach (var thread in threads)
            thread.Start();

        // Wait for all to complete
        foreach (var thread in threads)
            thread.Join();

        Console.WriteLine($"Expected: {threadCount * incrementsPerThread}");
        Console.WriteLine($"Actual: {_counter}");
        Console.WriteLine("Result is thread-safe!");
    }
}

// ============================================================================
// SYNCHRONIZATION MECHANISMS
// ============================================================================
class ThreadSafetyExamples
{
    //different synchronization mechanisms
    private static readonly object lockObject = new object();
    private static readonly Mutex mutex = new Mutex();
    private static readonly Semaphore semaphore = new Semaphore(2, 2); //allow 2 concurrent threads
    private static readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();

    private static int sharedCounter = 0;
    private static string sharedData = "Initial Data";

    public static void Main()
    {
        Console.WriteLine("Thread Safety Examples");

        //lock statement example
        LockExample();

        //Mutex example
        MutexExample();

        //Semaphore example
        SemaphoreExample();

        //ReaderWriterLock example
        ReaderWriterLockExample();

        //Concurrent collections example
        ConcurrentCollectionExample();
    }

    static void LockExample()
    {
        Console.WriteLine("\n=== Lock Statement Example ===");

        Task[] tasks = new Task[5];

        for (int i = 0; i < tasks.Length; i++)
        {
            int taskId = i;
            tasks[i] = Task.Run(() => IncrementWithLock(taskId));
        }

        Task.WaitAll(tasks);
        Console.WriteLine($"Final counter value: {sharedCounter}");
    }

    static void IncrementWithLock(int taskId)
    {
        for (int i = 0; i < 1000; i++)
        {
            lock (lockObject)
            {
                sharedCounter++;
                if (i % 200 == 0)
                {
                    Console.WriteLine($"Task {taskId}: Counter = {sharedCounter}");
                }
            }
        }
    }

    static void MutexExample()
    {
        Console.WriteLine("\n=== Mutex Example ===");

        Task[] tasks = new Task[3];

        for (int i = 0; i < tasks.Length; i++)
        {
            int taskId = i;
            tasks[i] = Task.Run(() => ProcessWithMutex(taskId));
        }

        Task.WaitAll(tasks);
    }

    static void ProcessWithMutex(int taskId)
    {
        Console.WriteLine($"Task {taskId}: Waiting for mutex...");

        try
        {
            mutex.WaitOne();
            Console.WriteLine($"Task {taskId}: Acquired mutex, processing...");

            // Simulate exclusive work
            Thread.Sleep(2000);

            Console.WriteLine($"Task {taskId}: Work completed, releasing mutex");
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }

    static void SemaphoreExample()
    {
        Console.WriteLine("\n=== Semaphore Example ===");

        Task[] tasks = new Task[6];

        for (int i = 0; i < tasks.Length; i++)
        {
            int taskId = i;
            tasks[i] = Task.Run(() => ProcessWithSemaphore(taskId));
        }

        Task.WaitAll(tasks);
    }

    static void ProcessWithSemaphore(int taskId)
    {
        Console.WriteLine($"Task {taskId}: Waiting for semaphore...");

        try
        {
            semaphore.WaitOne();
            Console.WriteLine($"Task {taskId}: Acquired semaphore, processing...");

            // Simulate work (max 2 threads can do this simultaneously)
            Thread.Sleep(3000);

            Console.WriteLine($"Task {taskId}: Work completed, releasing semaphore");
        }
        finally
        {
            semaphore.Release();
        }
    }

    static void ReaderWriterLockExample()
    {
        Console.WriteLine("\n=== ReaderWriterLock Example ===");

        // Start multiple reader and writer tasks
        Task[] tasks = new Task[8];

        for (int i = 0; i < 6; i++) // 6 readers
        {
            int taskId = i;
            tasks[i] = Task.Run(() => ReadData(taskId));
        }

        for (int i = 6; i < 8; i++) // 2 writers
        {
            int taskId = i;
            tasks[i] = Task.Run(() => WriteData(taskId));
        }

        Task.WaitAll(tasks);
    }

    static void ReadData(int readerId)
    {
        for (int i = 0; i < 3; i++)
        {
            try
            {
                readerWriterLock.EnterReadLock();
                Console.WriteLine($"Reader {readerId}: Reading data - '{sharedData}'");
                Thread.Sleep(1000); // Simulate reading time
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }

            Thread.Sleep(500); // Pause between reads
        }
    }

    static void WriteData(int writerId)
    {
        for (int i = 0; i < 2; i++)
        {
            try
            {
                readerWriterLock.EnterWriteLock();
                string newData = $"Data updated by Writer {writerId} at {DateTime.Now:HH:mm:ss}";
                sharedData = newData;
                Console.WriteLine($"Writer {writerId}: Updated data to '{newData}'");
                Thread.Sleep(2000); // Simulate writing time
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }

            Thread.Sleep(1000); // Pause between writes
        }
    }

    static void ConcurrentCollectionExample()
    {
        Console.WriteLine("\n=== Concurrent Collections Example ===");

        var concurrentDict = new ConcurrentDictionary<string, int>();
        var concurrentQueue = new ConcurrentQueue<string>();
        var concurrentBag = new ConcurrentBag<int>();

        // Start multiple tasks to work with concurrent collections
        Task[] tasks = new Task[4];

        tasks[0] = Task.Run(() => PopulateDictionary(concurrentDict));
        tasks[1] = Task.Run(() => PopulateQueue(concurrentQueue));
        tasks[2] = Task.Run(() => PopulateBag(concurrentBag));
        tasks[3] = Task.Run(() => ConsumeCollections(concurrentDict, concurrentQueue, concurrentBag));

        Task.WaitAll(tasks);

        Console.WriteLine($"Final dictionary count: {concurrentDict.Count}");
        Console.WriteLine($"Final queue count: {concurrentQueue.Count}");
        Console.WriteLine($"Final bag count: {concurrentBag.Count}");
    }

    static void PopulateDictionary(ConcurrentDictionary<string, int> dict)
    {
        for (int i = 0; i < 10; i++)
        {
            string key = $"Key{i}";
            dict.TryAdd(key, i * 10);
            Console.WriteLine($"Dictionary: Added {key} = {i * 10}");
            Thread.Sleep(100);
        }
    }

    static void PopulateQueue(ConcurrentQueue<string> queue)
    {
        for (int i = 0; i < 10; i++)
        {
            string item = $"QueueItem{i}";
            queue.Enqueue(item);
            Console.WriteLine($"Queue: Enqueued {item}");
            Thread.Sleep(150);
        }
    }

    static void PopulateBag(ConcurrentBag<int> bag)
    {
        for (int i = 0; i < 10; i++)
        {
            bag.Add(i * i);
            Console.WriteLine($"Bag: Added {i * i}");
            Thread.Sleep(120);
        }
    }

    static void ConsumeCollections(ConcurrentDictionary<string, int> dict,
                                 ConcurrentQueue<string> queue,
                                 ConcurrentBag<int> bag)
    {
        Thread.Sleep(500); // Let other tasks populate first

        // Try to consume from collections
        for (int i = 0; i < 5; i++)
        {
            // Try to get from dictionary
            if (dict.TryRemove($"Key{i}", out int value))
            {
                Console.WriteLine($"Consumer: Removed Key{i} = {value} from dictionary");
            }

            // Try to dequeue from queue
            if (queue.TryDequeue(out string queueItem))
            {
                Console.WriteLine($"Consumer: Dequeued {queueItem} from queue");
            }

            // Try to take from bag
            if (bag.TryTake(out int bagItem))
            {
                Console.WriteLine($"Consumer: Took {bagItem} from bag");
            }

            Thread.Sleep(300);
        }
    }
}


// ============================================================================
// DEADLOCK PREVENTION EXAMPLES
// ============================================================================

public class DeadlockExamples
{
    private static readonly object _resourceA = new object();
    private static readonly object _resourceB = new object();

    // BAD: Potential Deadlock
    public static void DeadlockScenario()
    {
        var thread1 = new Thread(() =>
        {
            lock (_resourceA)
            {
                Console.WriteLine("Thread 1: Locked Resource A");
                Thread.Sleep(100);

                Console.WriteLine("Thread 1: Attempting to lock Resource B");
                lock (_resourceB) // Potential deadlock here
                {
                    Console.WriteLine("Thread 1: Locked Resource B");
                }
            }
        });

        var thread2 = new Thread(() =>
        {
            lock (_resourceB)
            {
                Console.WriteLine("Thread 2: Locked Resource B");
                Thread.Sleep(100);

                Console.WriteLine("Thread 2: Attempting to lock Resource A");
                lock (_resourceA) // Potential deadlock here
                {
                    Console.WriteLine("Thread 2: Locked Resource A");
                }
            }
        });

        thread1.Start();
        thread2.Start();

        // These might never complete!
        thread1.Join();
        thread2.Join();
    }

    // GOOD: Deadlock Prevention with Ordered Locking
    public static void DeadlockPrevention()
    {
        var thread1 = new Thread(() =>
        {
            // Always lock in the same order: A then B
            lock (_resourceA)
            {
                Console.WriteLine("Thread 1: Locked Resource A");
                Thread.Sleep(100);

                lock (_resourceB)
                {
                    Console.WriteLine("Thread 1: Locked Resource B");
                }
            }
        });

        var thread2 = new Thread(() =>
        {
            // Always lock in the same order: A then B
            lock (_resourceA)
            {
                Console.WriteLine("Thread 2: Locked Resource A");
                Thread.Sleep(100);

                lock (_resourceB)
                {
                    Console.WriteLine("Thread 2: Locked Resource B");
                }
            }
        });

        thread1.Start();
        thread2.Start();

        thread1.Join();
        thread2.Join();

        Console.WriteLine("No deadlock occurred!");
    }
}

// ============================================================================
// MODERN TASK EXAMPLES
// ============================================================================

public class ModernTaskExamples
{
    public static async Task BasicTaskExamples()
    {
        // Simple task creation
        Task task1 = Task.Run(() =>
        {
            Console.WriteLine("Task 1 executing");
            Thread.Sleep(2000);
        });

        // Task with return value
        Task<int> task2 = Task.Run(() =>
        {
            Console.WriteLine("Task 2 calculating");
            Thread.Sleep(1000);
            return 42;
        });

        // Wait for tasks
        await task1;
        int result = await task2;

        Console.WriteLine($"Task 2 result: {result}");
    }

    public static async Task TaskContinuationExample()
    {
        var task = Task.Run(() =>
        {
            Console.WriteLine("Initial task running");
            Thread.Sleep(1000);
            return "Hello";
        });

        var continuationTask = task.ContinueWith(previousTask =>
        {
            Console.WriteLine($"Continuation task: {previousTask.Result} World!");
        });

        await continuationTask;
    }

    public static async Task TaskCompositionExample()
    {
        var tasks = new[]
        {
            Task.Run(() => CalculateSum(1, 100)),
            Task.Run(() => CalculateSum(101, 200)),
            Task.Run(() => CalculateSum(201, 300))
        };

        // Wait for all tasks and get results
        var results = await Task.WhenAll(tasks);
        var totalSum = results.Sum();

        Console.WriteLine($"Total sum: {totalSum}");
    }

    private static int CalculateSum(int start, int end)
    {
        int sum = 0;
        for (int i = start; i <= end; i++)
        {
            sum += i;
        }
        Thread.Sleep(500); // Simulate work
        return sum;
    }
}
