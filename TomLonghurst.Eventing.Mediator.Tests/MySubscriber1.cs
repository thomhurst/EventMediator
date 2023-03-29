namespace TomLonghurst.Eventing.Mediator.Tests;

public partial class MySubscriber1 : IEventSubscriber<IMyEvents>
{
    public void Subscribe(IMyEventsEventHandlers eventHandlers)
    {
        eventHandlers.OnDidSomethingWithArgs += (sender, args) =>
        {
            Console.WriteLine(args.Foo);
        };
    }
}