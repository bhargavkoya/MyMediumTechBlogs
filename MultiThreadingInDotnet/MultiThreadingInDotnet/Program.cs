Console.WriteLine("=============================================");
Console.WriteLine("Welcome to the MultiThreading in .NET");
Console.WriteLine("=========================================");


Console.WriteLine("MultiThreading in .NET");

//basic thread sample execution
Console.WriteLine("=========================================");
Console.WriteLine("This is a simple example of multi-threading in .NET using C#.");
BasicThreadingExamples basicThreadingExamples = new BasicThreadingExamples();
BasicThreadingExamples.CreateFirstThread();
BasicThreadingExamples.ParameterizedThreadExample();
Console.WriteLine("sample thread execution completed.");
Console.WriteLine("=========================================");

//thread pool sample execution
Console.WriteLine("=========================================");
Console.WriteLine("This is a simple example of thread pool in .NET using C#.");
ThreadPoolExamples threadPoolExamples = new ThreadPoolExamples();
ThreadPoolExamples.BasicThreadPoolUsage();
ThreadPoolExamples.ThreadPoolWithWaitHandle();
Console.WriteLine("sample thread pool execution completed.");
Console.WriteLine("=========================================");

//race condition sample execution
Console.WriteLine("=========================================");
Console.WriteLine("This is simple example of race condition posibility in .NET using C#.");
SynchronizationExamples synchronizationExamples = new SynchronizationExamples();
SynchronizationExamples.RaceConditionExample();
SynchronizationExamples.ThreadSafeExample();
Console.WriteLine("sample race condition execution completed");
Console.WriteLine("=========================================");

//synchronization mechanisms sample execution
Console.WriteLine("=========================================");
Console.WriteLine("This is simple example of synchronization mechanisms in .NET using C#.");
ThreadSafetyExamples threadSafetyExamples=new ThreadSafetyExamples();
ThreadSafetyExamples.Main();
Console.WriteLine("sample synchronization mechanisms execution completed");
Console.WriteLine("=========================================");

//deadlock sample execution
Console.WriteLine("=========================================");
Console.WriteLine("This is a simple example of deadlock in .NET using C#.");
DeadlockExamples.DeadlockPrevention();
Console.WriteLine("sample deadlock execution completed");
Console.WriteLine("=========================================");

//async and await sample execution
Console.WriteLine("=========================================");
Console.WriteLine("This is a simple example of async and await in .NET using C#.");
await ModernTaskExamples.BasicTaskExamples();
await ModernTaskExamples.TaskContinuationExample();
await ModernTaskExamples.TaskCompositionExample();
Console.WriteLine("sample async and await execution completed");
Console.WriteLine("=========================================");

Console.ReadKey();


