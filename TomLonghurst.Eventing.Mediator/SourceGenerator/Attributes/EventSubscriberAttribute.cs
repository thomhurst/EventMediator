namespace TomLonghurst.Eventing.Mediator.SourceGenerator.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class EventSubscriberAttribute<T> : Attribute
{
    public Type Type => typeof(T);
}