using TomLonghurst.Eventing.Mediator.SourceGenerator.Attributes;

namespace TomLonghurst.Eventing.Mediator.Tests;

[EventSubscriber<IMyEvents>]
public partial class MySubscriber1
{
    public void Subscribe(IMyEventsEventHandlers eventHandlers)
    {
        eventHandlers.OnDidSomethingWithArgs += (sender, args) =>
        {
            // Do something
        };
    }
}