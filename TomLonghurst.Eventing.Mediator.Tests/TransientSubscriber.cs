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
        eventHandlers.OnDidSomeCounting += (_, _) =>
        {
            Console.WriteLine(_count);
        };

        eventHandlers.OnDidSomethingWithService += (_, _) =>
        {
            Console.WriteLine(_singletonService.Count);
        };
    }
}