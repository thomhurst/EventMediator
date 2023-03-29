# Event Mediator

A mediator for eventing - Source Generated publishers from your interfaces 

Publish a message and it'll make sure all your subscribers get it

This uses source code generation to build publishers for you based off of an interface.
Subscribers can be implemented by you exactly how you'd like, you just have to hook into the events given to you.

Install via Nuget
`Install-Package TomLonghurst.Eventing.Mediator`

## Usage

1.  Create an interface, defining methods that represent events, and add the `IEventMediator` interface

```csharp
public interface IMyEvents : IEventMediator
{
    void DidSomething();
    void DidSomethingWithArgs(Args args);
}
```

2.  Create a partial class for your Subscriber, with an `IEventSubscriber<TEventMediator>` interface, where `TEventMediator` is the interface you created in step 1

```csharp
public partial class MySubscriber : IEventSubscriber<IMyEvents> { }
```

3.  You'll be required by the compiler to implement a `Subscribe` method. Here you'll be passed an object containing `event` hooks that you can choose to opt into or not. Your subscriber may care about all or some. You choose.
    You can inject in any dependencies you want, to this class. The freedom is yours.

```csharp
public partial class MySubscriber : IEventSubscriber<IMyEvents>
{
    public void Subscribe(IMyEventsEventHandlers eventHandlers)
    {
        eventHandlers.OnDidSomethingWithArgs += (sender, args) =>
        {
            // Do something
        };
    }
}
```

4.  (Optional) - Repeat Steps 2 + 3 for as many subscribers as you want.

5.  Hook it all up to your ServiceCollection, and enjoy. Simply inject in your interface from Step 1 when you want to publish an event, and this'll take care of the rest.

```csharp
_services
    .AddEventMediator<IMyEvents>()
    .AddEventSubscriber<IMyEvents, MySubscriber1>()
    .AddEventSubscriber<IMyEvents, MySubscriber2>()
```

```csharp
public class MyService
{
    private readonly IMyEvents _myEvents;

    public MyService(IMyEvents myEvents)
    {
        _myEvents = myEvents;
    }

    public async Task DoSomething()
    {
        // Do Something
        await Task.Delay(1000);
        
        // Publish Event
        _myEvents.DidSomething();
    }
}
```

## FAQ
Q: What Lifetimes do my Publisher/Subscriber classes have?
A: Transient - They are reconstructed (and the subscribers resubscribe) on each new Mediator injection
