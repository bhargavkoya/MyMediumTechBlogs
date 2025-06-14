using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreadingInDotnet
{
    //Real-world use case: Document Processing System
    public class DocumentProcessor
    {
        private readonly int _maxDegreeOfParallelism;
        private readonly ConcurrentQueue<string> _processingQueue;
        private readonly ConcurrentBag<ProcessingResult> _results;

        public DocumentProcessor(int maxThreads)
        {
            _maxDegreeOfParallelism = maxThreads == 0 ? Environment.ProcessorCount : maxThreads;
            _processingQueue = new ConcurrentQueue<string>();
            _results = new ConcurrentBag<ProcessingResult>();
        }

        //Overload constructor to provide default behavior  
        public DocumentProcessor() : this(Environment.ProcessorCount)
        {
        }

        //Basic Thread Creation - The Old Way
        public void ProcessFilesWithBasicThreads(string[] filePaths)
        {
            var threads = new List<Thread>();

            foreach (var filePath in filePaths)
            {
                var thread = new Thread(() => ProcessSingleFile(filePath));
                thread.Name = $"ProcessingThread-{Path.GetFileName(filePath)}";
                threads.Add(thread);
                thread.Start();
            }

            // Wait for all threads to complete
            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

        //ThreadPool Approach - Better Resource Management
        public void ProcessFilesWithThreadPool(string[] filePaths)
        {
            var countdown = new CountdownEvent(filePaths.Length);

            foreach (var filePath in filePaths)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        ProcessSingleFile(filePath);
                    }
                    finally
                    {
                        countdown.Signal(); //decrement counter
                    }
                });
            }

            countdown.Wait(); //Wait for all work items to complete
        }

        //Modern Task-Based Approach Recommended
        public async Task ProcessFilesWithTasksAsync(string[] filePaths)
        {
            var tasks = filePaths.Select(async filePath =>
            {
                return await Task.Run(() => ProcessSingleFile(filePath));
            });

            await Task.WhenAll(tasks);
        }

        //Parallel Processing with Controlled Concurrency
        public void ProcessFilesInBatches(string[] filePaths, int batchSize = 10)
        {
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = _maxDegreeOfParallelism
            };

            Parallel.ForEach(filePaths, parallelOptions, filePath =>
            {
                ProcessSingleFile(filePath);
            });
        }

        //Producer-Consumer Pattern Implementation
        public void ProcessFilesWithProducerConsumer(string[] filePaths)
        {
            //Producer: Enqueue all files
            var producer = Task.Run(() =>
            {
                foreach (var filePath in filePaths)
                {
                    _processingQueue.Enqueue(filePath);
                }
            });

            //Consumers: Process files from queue
            var consumers = Enumerable.Range(0, _maxDegreeOfParallelism)
                .Select(i => Task.Run(() =>
                {
                    while (_processingQueue.TryDequeue(out string filePath))
                    {
                        ProcessSingleFile(filePath);
                        Thread.Sleep(10); //simulate processing time
                    }
                }))
                .ToArray();

            //Wait for producer to finish, then consumers
            producer.Wait();
            Task.WaitAll(consumers);
        }

        //Demonstrate Thread Safety Issues - Race Condition Example
        private static int _unsafeCounter = 0;
        private static int _safeCounter = 0;
        private static readonly object _lockObject = new object();

        public void DemonstrateRaceCondition()
        {
            const int threadCount = 10;
            const int incrementsPerThread = 1000;

            var threads = new Thread[threadCount];

            // Unsafe counter increment
            for (int i = 0; i < threadCount; i++)
            {
                threads[i] = new Thread(() =>
                {
                    for (int j = 0; j < incrementsPerThread; j++)
                    {
                        _unsafeCounter++; // Race condition!
                    }
                });
                threads[i].Start();
            }

            foreach (var thread in threads)
                thread.Join();

            Console.WriteLine($"Expected: {threadCount * incrementsPerThread}");
            Console.WriteLine($"Unsafe Result: {_unsafeCounter}");

            // Safe counter increment with lock
            for (int i = 0; i < threadCount; i++)
            {
                threads[i] = new Thread(() =>
                {
                    for (int j = 0; j < incrementsPerThread; j++)
                    {
                        lock (_lockObject)
                        {
                            _safeCounter++; // Thread-safe
                        }
                    }
                });
                threads[i].Start();
            }

            foreach (var thread in threads)
                thread.Join();

            Console.WriteLine($"Safe Result: {_safeCounter}");
        }

        //Deadlock Example - The Classic Two-Lock Problem
        private static readonly object _lock1 = new object();
        private static readonly object _lock2 = new object();

        public void DemonstrateDeadlock()
        {
            var thread1 = new Thread(() =>
            {
                lock (_lock1)
                {
                    Console.WriteLine("Thread 1: Acquired lock1");
                    Thread.Sleep(100); // Simulate work

                    Console.WriteLine("Thread 1: Waiting for lock2");
                    lock (_lock2) // This will cause deadlock
                    {
                        Console.WriteLine("Thread 1: Acquired lock2");
                    }
                }
            });

            var thread2 = new Thread(() =>
            {
                lock (_lock2)
                {
                    Console.WriteLine("Thread 2: Acquired lock2");
                    Thread.Sleep(100); // Simulate work

                    Console.WriteLine("Thread 2: Waiting for lock1");
                    lock (_lock1) // This will cause deadlock
                    {
                        Console.WriteLine("Thread 2: Acquired lock1");
                    }
                }
            });

            thread1.Start();
            thread2.Start();

            // These threads will never complete due to deadlock
            // In real code, you'd implement timeout mechanisms
        }

        // Deadlock Prevention with Timeout
        public void PreventDeadlockWithTimeout()
        {
            var thread1 = new Thread(() =>
            {
                if (Monitor.TryEnter(_lock1, TimeSpan.FromSeconds(5)))
                {
                    try
                    {
                        Console.WriteLine("Thread 1: Acquired lock1");
                        Thread.Sleep(100);

                        if (Monitor.TryEnter(_lock2, TimeSpan.FromSeconds(1)))
                        {
                            try
                            {
                                Console.WriteLine("Thread 1: Acquired both locks");
                            }
                            finally
                            {
                                Monitor.Exit(_lock2);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Thread 1: Couldn't acquire lock2, avoiding deadlock");
                        }
                    }
                    finally
                    {
                        Monitor.Exit(_lock1);
                    }
                }
            });

            thread1.Start();
            thread1.Join();
        }

        private ProcessingResult ProcessSingleFile(string filePath)
        {
            try
            {
                // Simulate file processing work
                var fileInfo = new FileInfo(filePath);
                var content = File.ReadAllText(filePath);
                var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

                var result = new ProcessingResult
                {
                    FileName = fileInfo.Name,
                    FileSize = fileInfo.Length,
                    WordCount = wordCount,
                    ProcessedBy = Thread.CurrentThread.Name ?? $"Thread-{Thread.CurrentThread.ManagedThreadId}",
                    ProcessingTime = DateTime.Now
                };

                _results.Add(result);
                Console.WriteLine($"Processed {fileInfo.Name} on {result.ProcessedBy}");

                // Simulate processing time
                Thread.Sleep(Random.Shared.Next(100, 500));

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing {filePath}: {ex.Message}");
                return null;
            }
        }
    }

    public class ProcessingResult
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public int WordCount { get; set; }
        public string ProcessedBy { get; set; }
        public DateTime ProcessingTime { get; set; }
    }

    // Performance Comparison Example
    public class PerformanceComparison
    {
        public static void CompareApproaches(string[] filePaths)
        {
            var processor = new DocumentProcessor();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Sequential Processing
            stopwatch.Restart();
            foreach (var file in filePaths)
            {
                // Process files one by one
            }
            var sequentialTime = stopwatch.ElapsedMilliseconds;

            // Multithreaded Processing
            stopwatch.Restart();
            processor.ProcessFilesInBatches(filePaths);
            var parallelTime = stopwatch.ElapsedMilliseconds;

            Console.WriteLine($"Sequential Time: {sequentialTime}ms");
            Console.WriteLine($"Parallel Time: {parallelTime}ms");
            Console.WriteLine($"Performance Improvement: {(double)sequentialTime / parallelTime:F2}x");
        }
    }
}
