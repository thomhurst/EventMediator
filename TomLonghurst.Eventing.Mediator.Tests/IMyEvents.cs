namespace TomLonghurst.Eventing.Mediator.Tests;

public interface IMyEvents : IEventMediator
{
    void DidSomething();
    
    void DidSomethingWithArgs(Args args);

    void DidSomeCounting();

    void DidSomethingWithService();
}