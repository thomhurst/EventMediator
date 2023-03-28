using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using TomLonghurst.Eventing.Mediator.Extensions;
using TomLonghurst.Eventing.Mediator.SourceGenerator.Attributes;
using TomLonghurst.Eventing.Mediator.SourceGenerator.Helpers;

namespace TomLonghurst.Eventing.Mediator.SourceGenerator;

[Generator]
public class EventMediatorGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new EventMediatorSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not EventMediatorSyntaxReceiver syntaxReciever)
        {
            return;
        }
        
#if DEBUG
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            //System.Diagnostics.Debugger.Launch();
        }
#endif

        if (syntaxReciever.DiagnosticErrors.Any())
        {
            foreach (var diagnosticError in syntaxReciever.DiagnosticErrors)
            {
                context.ReportDiagnostic(diagnosticError);
            }

            return;
        }

        var mediatorSource = GenerateMediatorSource(context, syntaxReciever);
        context.AddSource("EventMediator_Mediators.generated", SourceText.From(mediatorSource, Encoding.UTF8));
        
        var subscriberSource = GenerateSubscriberSource(context, syntaxReciever);
        context.AddSource("EventMediator_Subscribers.generated", SourceText.From(subscriberSource, Encoding.UTF8));
    }
    
    private string GenerateMediatorSource(GeneratorExecutionContext context, EventMediatorSyntaxReceiver syntaxReciever)
    {
        var codeWriter = new CodeGenerationTextWriter();
        
        codeWriter.WriteLine(context.GetUsingStatementsForTypes(
            syntaxReciever.IdentifiedMediators.Select(d => d.InterfaceType),
            typeof(DependencyInjectionExtensions),
            typeof(EventMediatorAttribute),
            typeof(EventSubscriberAttribute<>),
            typeof(IEnumerable<>),
            typeof(Enumerable),
            typeof(string),
            typeof(EventHandler),
            typeof(EventHandler<>),
            typeof(EventArgs)
        ));   
        codeWriter.WriteLine();

        
        foreach (var identifiedMediator in syntaxReciever.IdentifiedMediators.DistinctBy(d => d.InterfaceType))
        {
            var typeSymbol = identifiedMediator.InterfaceType;
            
            var interfaceShortName = typeSymbol.ToDisplayString(SymbolDisplayFormats.GenericBase).Split('.').Last();
            var interfaceLongName = typeSymbol.ToDisplayString(SymbolDisplayFormats.NamespaceAndType);

            var guid = Guid.NewGuid().ToString("N");

            codeWriter.WriteLine($"namespace {typeSymbol.ContainingNamespace}");
            codeWriter.WriteLine("{");
            
            codeWriter.WriteLine($"public interface {interfaceShortName}EventHandlers");
            codeWriter.WriteLine("{");
            foreach (var methodSymbol in identifiedMediator.MethodsInInterface)
            {
                var eventArgType = GetEventArgType(methodSymbol);
                eventArgType = string.IsNullOrEmpty(eventArgType) ? string.Empty : $"<{eventArgType}>";
                
                codeWriter.WriteLine($"public event EventHandler{eventArgType} On{methodSymbol.Name};");
            }
            codeWriter.WriteLine("}");
            codeWriter.WriteLine();
            
            codeWriter.WriteLine($"public interface {interfaceShortName}Subscriber");
            codeWriter.WriteLine("{");
            codeWriter.WriteLine($"void Subscribe({interfaceLongName}EventHandlers eventHandlers);");
            codeWriter.WriteLine("}");
            codeWriter.WriteLine();
            
            codeWriter.WriteLine($"internal class EventMediator_{interfaceShortName}_Impl_{guid} : {interfaceLongName}, {interfaceLongName}EventHandlers");
            codeWriter.WriteLine("{");
            
            codeWriter.WriteLine($"private readonly IEnumerable<{interfaceLongName}Subscriber> _subscribers;");
            
            codeWriter.WriteLine($"public EventMediator_{interfaceShortName}_Impl_{guid}(IEnumerable<{interfaceLongName}Subscriber> subscribers)");
            codeWriter.WriteLine("{");
            
            codeWriter.WriteLine("_subscribers = subscribers;");

            codeWriter.WriteLine("foreach (var subscriber in subscribers)");
            codeWriter.WriteLine("{");
            codeWriter.WriteLine("subscriber.Subscribe(this);");
            codeWriter.WriteLine("}");
            
            codeWriter.WriteLine("}");
            codeWriter.WriteLine();
            
            foreach (var methodSymbol in identifiedMediator.MethodsInInterface)
            {
                var eventArgType = GetEventArgType(methodSymbol);
                eventArgType = string.IsNullOrEmpty(eventArgType) ? string.Empty : $"<{eventArgType}>";
                
                codeWriter.WriteLine($"public event EventHandler{eventArgType} On{methodSymbol.Name};");
            }
            
            codeWriter.WriteLine();
            codeWriter.WriteLine("[System.Runtime.CompilerServices.ModuleInitializer]");
            codeWriter.WriteLine($"internal static void Register{Guid.NewGuid():N}()");
            codeWriter.WriteLine("{");
            codeWriter.WriteLine($"{typeof(DependencyInjectionExtensions).Namespace}.{nameof(DependencyInjectionExtensions)}.{nameof(DependencyInjectionExtensions.Mediators)}.Add(typeof(EventMediator_{interfaceShortName}_Impl_{guid}));");
            codeWriter.WriteLine("}");
            codeWriter.WriteLine();
            
            foreach (var methodSymbol in identifiedMediator.MethodsInInterface)
            {
                var eventArgType = GetEventArgType(methodSymbol);
                var eventArgParameterName = string.IsNullOrEmpty(eventArgType) ? string.Empty : " args";
                var eventArgToPass = string.IsNullOrEmpty(eventArgParameterName) ? " EventArgs.Empty" : eventArgParameterName;
                
                codeWriter.WriteLine($"public void {methodSymbol.Name}({eventArgType}{eventArgParameterName})");
                codeWriter.WriteLine("{");
                codeWriter.WriteLine($"On{methodSymbol.Name}?.Invoke(this,{eventArgToPass});");
                codeWriter.WriteLine("}");
                codeWriter.WriteLine();
            }

            codeWriter.WriteLine("}");

            codeWriter.WriteLine("}");
            codeWriter.WriteLine();
        }

        return codeWriter.ToString();
    }
    
    private string GenerateSubscriberSource(GeneratorExecutionContext context, EventMediatorSyntaxReceiver syntaxReceiver)
    {
        var codeWriter = new CodeGenerationTextWriter();
        
        codeWriter.WriteLine(context.GetUsingStatementsForTypes(
            syntaxReceiver.IdentifiedMediators.Select(d => d.InterfaceType).Concat(syntaxReceiver.IdentifiedSubscribers.Select(d => d.ClassType)),
            typeof(DependencyInjectionExtensions),
            typeof(EventMediatorAttribute),
            typeof(EventSubscriberAttribute<>),
            typeof(IEnumerable<>),
            typeof(Enumerable),
            typeof(string),
            typeof(EventHandler),
            typeof(EventHandler<>),
            typeof(EventArgs)
        ));   
        codeWriter.WriteLine();


        foreach (var subscriber in syntaxReceiver.IdentifiedSubscribers.DistinctBy(d => d.ClassType))
        {
            var interfaceType = subscriber.MediatorInterfaceType;
            
            var interfaceLongName = interfaceType.ToDisplayString(SymbolDisplayFormats.NamespaceAndType);
            
            codeWriter.WriteLine($"namespace {subscriber.ClassType.ContainingNamespace}");
            codeWriter.WriteLine("{");
            
            codeWriter.WriteLine($"public partial class {subscriber.ClassType.Name} : {interfaceLongName}Subscriber");
            codeWriter.WriteLine("{");
            codeWriter.WriteLine("}");

            codeWriter.WriteLine("}");
            codeWriter.WriteLine();
        }

        return codeWriter.ToString();
    }

    private static string GetEventArgType(IMethodSymbol methodSymbol)
    {
        return methodSymbol.Parameters.Any()
            ? methodSymbol.Parameters.First().Type.ToDisplayString(SymbolDisplayFormats.NamespaceAndType)
            : string.Empty;
    }
}