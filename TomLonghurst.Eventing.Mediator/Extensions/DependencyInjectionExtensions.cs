using System.Data;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TomLonghurst.Eventing.Mediator.SourceGenerator.Attributes;

namespace TomLonghurst.Eventing.Mediator.Extensions;

public static class DependencyInjectionExtensions
{
    private static List<Type> _mediators = new();
    
    public static List<Type> Mediators => _mediators;

    public static IServiceCollection AddEventMediator<TEventMediatorInterface>(this IServiceCollection services)
    {
        if (services.IsReadOnly)
        {
            throw new ReadOnlyException($"{nameof(services)} is read only");
        }

        if (!typeof(TEventMediatorInterface).IsInterface)
        {
            throw new ArgumentException($"{typeof(TEventMediatorInterface).Name} must be an interface");
        }

        if (typeof(TEventMediatorInterface).GetCustomAttribute<EventMediatorAttribute>() == null)
        {
            throw new ArgumentException($"{typeof(TEventMediatorInterface).Name} must have the attribute '{nameof(EventMediatorAttribute)}'");
        }

        var mediators = Interlocked.Exchange(ref _mediators, new List<Type>());
        
        foreach (var mediator in mediators)
        {
            var interfaceType = mediator.GetInterfaces().First();
            services.TryAddSingleton(interfaceType, mediator);
        }

        return services;
    }
    
    public static IServiceCollection AddEventSubscriber<TEventSubscriber>(this IServiceCollection services) where TEventSubscriber : class
    {
        if (services.IsReadOnly)
        {
            throw new ReadOnlyException($"{nameof(services)} is read only");
        }

        var subscriberType = typeof(TEventSubscriber);
        
        if (!subscriberType.IsClass)
        {
            throw new ArgumentException($"{subscriberType.Name} must be a class");
        }

        if (subscriberType.GetCustomAttributes().All(x => x.GetType().GetFullNameWithoutGenericArity() != typeof(EventSubscriberAttribute<>).GetFullNameWithoutGenericArity()))
        {
            throw new ArgumentException($"{subscriberType.Name} must have the attribute '{nameof(EventSubscriberAttribute<object>)}'");
        }
        
        var interfaces = subscriberType.GetInterfaces();
            
        if (interfaces.Length != 1)
        {
            throw new ArgumentException($"{subscriberType.Name} must have a single interface");
        }
            
        var interfaceType = interfaces.First();

        if (!services.Any(x => x.ServiceType == interfaceType && x.ImplementationType == subscriberType))
        {
            services.AddSingleton(interfaceType, subscriberType);   
        }

        return services;
    }
}