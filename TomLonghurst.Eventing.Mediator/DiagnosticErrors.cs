using Microsoft.CodeAnalysis;

namespace TomLonghurst.Eventing.Mediator;

public static class DiagnosticErrors
{
    public static readonly DiagnosticDescriptor InvalidReturnType = new DiagnosticDescriptor(id: "TLEM001",
        title: "Event Mediator methods must return void",
        messageFormat: "Event Mediator method '{0}' must return void",
        category: "EventMediator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
        
    public static readonly DiagnosticDescriptor InvalidEventArguments = new DiagnosticDescriptor(id: "TLEM002",
            title: "Event Mediator methods must have 1 or less parameters",
            messageFormat: "Event Mediator method '{0}' must have 1 or less parameters",
            category: "EventMediator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    
    public static readonly DiagnosticDescriptor MissingEventMediatorAttribute = new DiagnosticDescriptor(id: "TLEM003",
        title: "Are you missing an 'EventMediator' attribute?",
        messageFormat: "EventMediatorAttribute should be added to '{0}'",
        category: "EventMediator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}