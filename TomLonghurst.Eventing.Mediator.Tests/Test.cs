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
            .AddSingleton<SingletonService>()
            .AddEventMediator<IMyEvents>()
            .AddEventSubscriber<IMyEvents, MySubscriber1>()
            .AddEventSubscriber<IMyEvents, MySubscriber2>()
            .AddEventSubscriber<IMyEvents, TransientSubscriber>()
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

    private int _count;
    [Repeat(100)]
    [Test]
    public void ArgsCounting()
    {
        var mediator = _services.GetRequiredService<IMyEvents>();

        mediator.DidSomethingWithArgs(new Args { Foo = (++_count).ToString() });

        Assert.That(_textWriter.ToString().Trim(), Is.EqualTo(_count.ToString()));
    }
    
    [Repeat(100)]
    [Test]
    public void CountingWithoutArgs()
    {
        var mediator = _services.GetRequiredService<IMyEvents>();

        mediator.DidSomeCounting();

        Assert.That(_textWriter.ToString().Trim(), Is.EqualTo("0"));
    }
    
    private int _serviceCount;
    [Repeat(100)]
    [Test]
    public void ServiceCounting()
    {
        var mediator = _services.GetRequiredService<IMyEvents>();

        mediator.DidSomethingWithService();

        Assert.That(_textWriter.ToString().Trim(), Is.EqualTo((++_serviceCount).ToString()));
    }

    [SetUp]
    public void Clear()
    {
        _textWriter.GetStringBuilder().Clear();
    }
}