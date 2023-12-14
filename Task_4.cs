using System;
using System.Collections.Generic;
using System.Threading;

public class Event
{
    public int EventId { get; }
    public string EventMessage { get; }
    public int Timestamp { get; }

    public Event(int eventId, string eventMessage, int timestamp)
    {
        EventId = eventId;
        EventMessage = eventMessage;
        Timestamp = timestamp;
    }
}

public class EventNode
{
    private readonly List<Event> eventLog = new List<Event>();
    private int logicalClock = 0;
    private readonly object lockObject = new object();

    public int NodeId { get; }

    public EventNode(int nodeId)
    {
        NodeId = nodeId;
    }

    public void PublishEvent(string eventMessage)
    {
        lock (lockObject)
        {
            logicalClock++;
            Event newEvent = new Event(logicalClock, eventMessage, NodeId);
            eventLog.Add(newEvent);
            Console.WriteLine($"Node {NodeId} published event: {eventMessage}");
            NotifySubscribers(newEvent);
        }
    }

    public void Subscribe(EventNode publisher)
    {
        publisher.EventPublished += HandleEventPublished;
    }

    public void Unsubscribe(EventNode publisher)
    {
        publisher.EventPublished -= HandleEventPublished;
    }

    private void NotifySubscribers(Event newEvent)
    {
        EventPublished?.Invoke(this, newEvent);
    }

    private void HandleEventPublished(object sender, Event publishedEvent)
    {
        lock (lockObject)
        {
            logicalClock = Math.Max(logicalClock, publishedEvent.Timestamp) + 1;
            eventLog.Add(publishedEvent);
            Console.WriteLine($"Node {NodeId} received event: {publishedEvent.EventMessage}");
        }
    }

    public event EventHandler<Event> EventPublished;

    public void PrintEventLog()
    {
        lock (lockObject)
        {
            Console.WriteLine($"Event log for Node {NodeId}:");
            foreach (var ev in eventLog)
            {
                Console.WriteLine($"EventId: {ev.EventId}, Message: {ev.EventMessage}, Timestamp: {ev.Timestamp}");
            }
            Console.WriteLine();
        }
    }
}

class Program
{
    static void Main()
    {
        EventNode node1 = new EventNode(1);
        EventNode node2 = new EventNode(2);

        node1.Subscribe(node2);
        node2.Subscribe(node1);


        node1.PublishEvent("Event from Node 1");
        node2.PublishEvent("Event from Node 2");
        node1.PublishEvent("Another event from Node 1");


        node1.PrintEventLog();
        node2.PrintEventLog();
    }
}
