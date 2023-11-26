using System;
using System.Threading;

public class Resource
{
    public string Name { get; }

    public Resource(string name)
    {
        Name = name;
    }
}

public class ResourceManager
{
    private Semaphore resourceSemaphore;
    private Mutex priorityMutex;
    private int priorityCount;

    public ResourceManager(int resourceCount)
    {
        resourceSemaphore = new Semaphore(resourceCount, resourceCount);
        priorityMutex = new Mutex();
        priorityCount = 0;
    }

    public void RequestResource(Resource resource, bool isHighPriority)
    {
        Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} requesting {resource.Name}...");

        if (isHighPriority)
        {
            priorityMutex.WaitOne();
            priorityCount++;
            priorityMutex.ReleaseMutex();
        }

        resourceSemaphore.WaitOne();

        Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} got {resource.Name}.");

        if (isHighPriority)
        {
            priorityMutex.WaitOne();
            priorityCount--;
            if (priorityCount == 0)
            {
                priorityMutex.ReleaseMutex();
            }
            else
            {
                priorityMutex.ReleaseMutex();
                resourceSemaphore.Release();
            }
        }
    }

    public void ReleaseResource(Resource resource)
    {
        Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} releasing {resource.Name}.");
        resourceSemaphore.Release();
    }
}

class Program
{
    static void Main()
    {
        Resource cpu = new Resource("CPU");
        Resource ram = new Resource("RAM");
        Resource disk = new Resource("Disk");

        ResourceManager resourceManager = new ResourceManager(2);

        Thread highPriorityThread = new Thread(() =>
        {
            resourceManager.RequestResource(cpu, true);
            Thread.Sleep(2000); // Simulate using CPU
            resourceManager.ReleaseResource(cpu);

            resourceManager.RequestResource(ram, true);
            Thread.Sleep(2000); // Simulate using RAM
            resourceManager.ReleaseResource(ram);
        });

        Thread lowPriorityThread = new Thread(() =>
        {
            resourceManager.RequestResource(disk, false);
            Thread.Sleep(2000); // Simulate using Disk
            resourceManager.ReleaseResource(disk);
        });

        highPriorityThread.Start();
        lowPriorityThread.Start();

        highPriorityThread.Join();
        lowPriorityThread.Join();
    }
}
