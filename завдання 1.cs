using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class DistributedSystemNode
{
    public event EventHandler<string> MessageReceived;
    public event EventHandler<bool> NodeStatusChanged;

    private readonly int nodeId;
    private bool isActive;

    public DistributedSystemNode(int nodeId)
    {
        this.nodeId = nodeId;
        isActive = true;
    }

    public async Task SendMessageAsync(int targetNodeId, string message)
    {
        
        await Task.Delay(TimeSpan.FromSeconds(1));

        OnMessageReceived(new MessageEventArgs(targetNodeId, message));
    }

    public void StartProcessingMessages()
    {
        /
        Task.Run(async () =>
        {
            while (isActive)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

               
                ProcessMessages();
            }
        });
    }

    private void ProcessMessages()
    {
        
        Console.WriteLine($"Node {nodeId} is processing messages...");

       
        if (new Random().Next(10) < 2)
        {
            ChangeNodeStatus(!isActive);
        }
    }

    private void OnMessageReceived(MessageEventArgs e)
    {
        MessageReceived?.Invoke(this, $"Node {nodeId} received message from Node {e.TargetNodeId}: {e.Message}");
    }

    private void ChangeNodeStatus(bool newStatus)
    {
        if (isActive != newStatus)
        {
            isActive = newStatus;
            NodeStatusChanged?.Invoke(this, isActive);
            Console.WriteLine($"Node {nodeId} is now {(isActive ? "active" : "inactive")}");
        }
    }
}

public class MessageEventArgs : EventArgs
{
    public int TargetNodeId { get; }
    public string Message { get; }

    public MessageEventArgs(int targetNodeId, string message)
    {
        TargetNodeId = targetNodeId;
        Message = message;
    }
}

class Program
{
    static async Task Main()
    {
        var node1 = new DistributedSystemNode(1);
        var node2 = new DistributedSystemNode(2);

        node1.MessageReceived += (sender, message) => Console.WriteLine(message);
        node1.NodeStatusChanged += (sender, status) => Console.WriteLine($"Node 1 status changed: {status}");

        node2.MessageReceived += (sender, message) => Console.WriteLine(message);
        node2.NodeStatusChanged += (sender, status) => Console.WriteLine($"Node 2 status changed: {status}");
        node1.StartProcessingMessages();
        node2.StartProcessingMessages();

        await node1.SendMessageAsync(2, "Hello from Node 1 to Node 2");
        await node2.SendMessageAsync(1, "Hello from Node 2 to Node 1");

        
        await Task.Delay(TimeSpan.FromSeconds(5));

        node1.ChangeNodeStatus(false);
        node2.ChangeNodeStatus(false);

        Console.ReadLine();
    }
}
