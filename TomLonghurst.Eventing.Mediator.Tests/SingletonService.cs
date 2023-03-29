namespace TomLonghurst.Eventing.Mediator.Tests;

public class SingletonService
{
    private int _count;

    public int Count => Interlocked.Increment(ref _count);
}