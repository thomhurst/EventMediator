using Microsoft.Extensions.DependencyInjection;
using TomLonghurst.Eventing.Mediator.Extensions;

namespace TomLonghurst.Eventing.Mediator.Tests;

[NonParallelizable]
public class Test
{
    private readonly ServiceProvider _services;
    private readonly StringWriter _textWriter;

    public Test()
    {
        _textWriter = new StringWriter();

        _services = new ServiceCollection()
            .AddEventMediator<IMyEvents>()
            .AddEventSubscriber<MySubscriber1>()
            .AddEventSubscriber<MySubscriber2>()
            .BuildServiceProvider();

        Console.SetOut(_textWriter);
    }

    [Test]
    public void EmptyArgs()
    {
        var mediator = _services.GetRequiredService<IMyEvents>();

        mediator.DidSomething();

        Assert.That(_textWriter.ToString().Trim(), Is.EqualTo("Empty args"));
    }
    
    
    [Test]
    public void Args()
    {
        var mediator = _services.GetRequiredService<IMyEvents>();

        mediator.DidSomethingWithArgs(new Args { Foo =  "Bar" });

        Assert.That(_textWriter.ToString().Trim(), Is.EqualTo("Bar"));
    }

    [SetUp]
    public void Clear()
    {
        _textWriter.GetStringBuilder().Clear();
    }
}

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