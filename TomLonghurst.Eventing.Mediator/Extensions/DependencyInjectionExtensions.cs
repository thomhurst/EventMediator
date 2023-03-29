using System.Data;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TomLonghurst.Eventing.Mediator.Extensions;

public static class DependencyInjectionExtensions
{
    private static List<Type> _mediators = new();
    
    public static List<Type> Mediators => _mediators;

    public static IServiceCollection AddEventMediator<TEventMediatorInterface>(this IServiceCollection services) where TEventMediatorInterface : IEventMediator
    {
        if (services.IsReadOnly)
        {
            throw new ReadOnlyException($"{nameof(services)} is read only");
        }

        if (!typeof(TEventMediatorInterface).IsInterface)
        {
            throw new ArgumentException($"{typeof(TEventMediatorInterface).Name} must be an interface");
        }

        var mediators = Interlocked.Exchange(ref _mediators, new List<Type>());
        
        foreach (var mediator in mediators)
        {
            var interfaceType = mediator.GetInterfaces().First();
            services.TryAddTransient(interfaceType, mediator);
        }

        return services;
    }
    
    public static IServiceCollection AddEventSubscriber<TEventMediator, TEventSubscriber>(this IServiceCollection services) 
        where TEventMediator : IEventMediator
        where TEventSubscriber : class, IEventSubscriber<TEventMediator>
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

        var interfaces = subscriberType.GetInterfaces()
            .Where(type => !(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEventSubscriber<>)))
            .ToArray();
            
        if (interfaces.Length != 1)
        {
            throw new ArgumentException($"{subscriberType.Name} must have a single interface");
        }
            
        var interfaceType = interfaces.First();

        if (!services.Any(x => x.ServiceType == interfaceType && x.ImplementationType == subscriberType))
        {
            services.AddTransient(interfaceType, subscriberType);   
        }

        return services;
    }
}