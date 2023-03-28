# Event Mediator

A mediator for eventing - Publish a message and it'll make sure all your subscribers get it

This uses source code generation to build publishers for you based off of an interface.
Subscribers can be implemented by you exactly how you'd like, you just have to hook into the events given to you.

Install via Nuget
`Install-Package TomLonghurst.Eventing.Mediator`

## Usage

1.  Create an interface, defining methods that represent events, and add the [EventMediator] attribute

```csharp
[EventMediator]
public interface IMyEvents
{
    void DidSomething();
    void DidSomethingWithArgs(Args args);
}
```

2.  Create a partial class for your Subscriber, with an `[EventSubscriber<TEventInterface>]` attribute, where `TEventInterface` is the interface you created in step 1

```csharp
[EventSubscriber<IMyEvents>]
public partial class MySubscriber { }
```

3.  You'll be required by the compiler to implement a `Subscribe` method. Here you'll be passed an object containing `event` hooks that you can choose to opt into or not. Your subscriber may care about all or some. You choose.
    You can inject in any dependencies you want, to this class. The freedom is yours.

```csharp
[EventSubscriber<IMyEvents>]
public partial class MySubscriber
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

4.  (Optional) - Repeat Step 3 for as many subscribers as you want.

5.  Hook it all up to your ServiceCollection, and enjoy. Simply inject in your interface from Step 1 when you want to publish an event, and this'll take care of the rest.

```csharp
_services
    .AddEventMediator<IMyEvents>()
    .AddEventSubscriber<MySubscriber1>()
    .AddEventSubscriber<MySubscriber2>()
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
