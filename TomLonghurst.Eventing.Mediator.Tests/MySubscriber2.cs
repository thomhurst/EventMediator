namespace TomLonghurst.Eventing.Mediator.Tests;

public partial class MySubscriber2 : IEventSubscriber<IMyEvents>
{
    public void Subscribe(IMyEventsEventHandlers eventHandlers)
    {
        eventHandlers.OnDidSomething += (sender, args) =>
        {
            Console.WriteLine("Empty args");
        };
    }
}