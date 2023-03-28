using TomLonghurst.Eventing.Mediator.SourceGenerator.Attributes;

namespace TomLonghurst.Eventing.Mediator.Tests;

[EventMediator]
public interface IMyEvents
{
    void DidSomething();
    void DidSomethingWithArgs(Args args);
}