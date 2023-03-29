using Microsoft.CodeAnalysis;

namespace TomLonghurst.Eventing.Mediator;

[Generator]
public class EventMediatorIncrementalSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            //System.Diagnostics.Debugger.Launch();
        }
#endif
        var eventMediatorTypes = context.SyntaxProvider
            .CreateSyntaxProvider(EventMediatorGenerator.IsEventMediator, EventMediatorGenerator.GetEventMediatorInterfaceTypeOrNull)
            .Where(type => type is not null)
            .Collect();
        
        context.RegisterSourceOutput(eventMediatorTypes, EventMediatorGenerator.GenerateEventMediatorCode);
        
        var eventSubscriberTypes = context.SyntaxProvider
            .CreateSyntaxProvider(EventSubscriberGenerator.IsEventSubscriber, EventSubscriberGenerator.GetEventSubscriberClassTypeOrNull)
            .Where(type => type is not null)
            .Collect();
        
        context.RegisterSourceOutput(eventSubscriberTypes, EventSubscriberGenerator.GenerateEventSubscriberCode);
    }
    
  
}