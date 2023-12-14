using System;
using System.Collections.Generic;
using System.Threading;

public class Operation
{
    public DateTime Timestamp { get; }
    public int ThreadId { get; }
    public string Action { get; }

    public Operation(int threadId, string action)
    {
        Timestamp = DateTime.Now;
        ThreadId = threadId;
        Action = action;
    }
}

public class ConflictLogger
{
    private readonly List<Operation> operationLog = new List<Operation>();
    private readonly object lockObject = new object();

    public void LogOperation(Operation operation)
    {
        lock (lockObject)
        {
            operationLog.Add(operation);
        }
    }

    public void DetectAndResolveConflicts()
    {
        lock (lockObject)
        {
            // Detect conflicts and resolve them
            for (int i = 0; i < operationLog.Count - 1; i++)
            {
                for (int j = i + 1; j < operationLog.Count; j++)
                {
                    if (IsConflict(operationLog[i], operationLog[j]))
                    {
                        Console.WriteLine($"Conflict detected between Thread {operationLog[i].ThreadId} and Thread {operationLog[j].ThreadId}");
                        ResolveConflict(operationLog[i], operationLog[j]);
                    }
                }
            }
        }
    }

    private bool IsConflict(Operation operation1, Operation operation2)
    {
        
        return Math.Abs((operation1.Timestamp - operation2.Timestamp).TotalMilliseconds) < 100
            && operation1.ThreadId != operation2.ThreadId;
    }

    private void ResolveConflict(Operation operation1, Operation operation2)
    {
        
        if (operation1.ThreadId < operation2.ThreadId)
        {
            operationLog.Remove(operation2);
            Console.WriteLine($"Resolved conflict by keeping operation from Thread {operation1.ThreadId}");
        }
        else
        {
            operationLog.Remove(operation1);
            Console.WriteLine($"Resolved conflict by keeping operation from Thread {operation2.ThreadId}");
        }
    }

    public void PrintOperationLog()
    {
        lock (lockObject)
        {
            Console.WriteLine("Operation Log:");
            foreach (var operation in operationLog)
            {
                Console.WriteLine($"{operation.Timestamp} - Thread {operation.ThreadId}: {operation.Action}");
            }
            Console.WriteLine();
        }
    }
}

class Program
{
    static void Main()
    {
        ConflictLogger conflictLogger = new ConflictLogger();
        Random random = new Random();

        for (int i = 1; i <= 5; i++)
        {
            int threadId = i;
            Thread thread = new Thread(() =>
            {
                for (int j = 0; j < 3; j++)
                {
                    string action = $"Operation {j + 1}";
                    conflictLogger.LogOperation(new Operation(threadId, action));
                    Thread.Sleep(random.Next(100)); // Simulate some processing time
                }
            });
            thread.Start();
        }

        Thread.Sleep(3000);

        
        conflictLogger.DetectAndResolveConflicts();

        
        conflictLogger.PrintOperationLog();
    }
}
