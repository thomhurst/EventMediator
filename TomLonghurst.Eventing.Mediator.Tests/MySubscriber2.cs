using TomLonghurst.Eventing.Mediator.SourceGenerator.Attributes;

namespace TomLonghurst.Eventing.Mediator.Tests;

[EventSubscriber<IMyEvents>]
public partial class MySubscriber2
{
    public void Subscribe(IMyEventsEventHandlers eventHandlers)
    {
        eventHandlers.OnDidSomething += (sender, args) =>
        {
            Console.WriteLine("Empty args");
        };
    }
}