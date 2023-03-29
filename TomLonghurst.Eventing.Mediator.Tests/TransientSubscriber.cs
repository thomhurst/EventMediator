namespace TomLonghurst.Eventing.Mediator.Tests;

public partial class TransientSubscriber : IEventSubscriber<IMyEvents>
{
    private readonly SingletonService _singletonService;
    private int _count;

    public TransientSubscriber(SingletonService singletonService)
    {
        _singletonService = singletonService;
    }
    
    public void Subscribe(IMyEventsEventHandlers eventHandlers)
    {
        eventHandlers.OnDidSomeCounting += (sender, args) =>
        {
            Console.WriteLine(_count);
        };

        eventHandlers.OnDidSomethingWithService += (sender, args) =>
        {
            Console.WriteLine(_singletonService.Count);
        };
    }
}