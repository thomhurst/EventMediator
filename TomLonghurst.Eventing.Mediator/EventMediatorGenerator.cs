using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomLonghurst.Eventing.Mediator.Extensions;
using TomLonghurst.Eventing.Mediator.SourceGenerator.Helpers;

namespace TomLonghurst.Eventing.Mediator;

public class EventMediatorGenerator
{
    internal static bool IsEventMediator(
        SyntaxNode syntaxNode,
        CancellationToken cancellationToken)
    {
        if (syntaxNode is not BaseTypeSyntax baseTypeSyntax)
        {
            return false;
        }

        var name = baseTypeSyntax.Type.ToString();

        return name is nameof(IEventMediator);
    }

    internal static ITypeSymbol? GetEventMediatorInterfaceTypeOrNull(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken)
    {
        var baseTypeSyntax = (BaseTypeSyntax) context.Node;
        
        if (baseTypeSyntax.Parent?.Parent is not InterfaceDeclarationSyntax interfaceDeclarationSyntax)
        {
            return null;
        }

        return context.SemanticModel.GetDeclaredSymbol(interfaceDeclarationSyntax) as ITypeSymbol;
    }
    
    internal static void GenerateEventMediatorCode(
        SourceProductionContext context,
        ImmutableArray<ITypeSymbol?> interfaces)
    {
        if (interfaces.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (var type in interfaces)
        {
            if (type is null)
            {
                continue;
            }
            
            var code = GenerateEventMediatorCode(type);
            
            var typeNamespace = type.ContainingNamespace.IsGlobalNamespace
                ? null
                : $"{type.ContainingNamespace}.";

            context.AddSource($"{typeNamespace}{type.Name}.Mediator.g.cs", code);
        }
    }
    
    private static string GenerateEventMediatorCode(ITypeSymbol typeSymbol)
    {
        var ns = typeSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : typeSymbol.ContainingNamespace.ToString();

        var codeWriter = new CodeGenerationTextWriter();
        
        codeWriter.WriteLine($"using {typeof(DependencyInjectionExtensions).Namespace};");
        codeWriter.WriteLine($"using {typeof(IEventMediator).Namespace};");
        codeWriter.WriteLine($"using {typeof(IEnumerable<>).Namespace};");
        codeWriter.WriteLine($"using {typeof(Enumerable).Namespace};");
        codeWriter.WriteLine($"using {typeof(string).Namespace};");
        codeWriter.WriteLine();

        var interfaceShortName = typeSymbol.ToDisplayString(SymbolDisplayFormats.GenericBase).Split('.').Last();
        var interfaceLongName = typeSymbol.ToDisplayString(SymbolDisplayFormats.NamespaceAndType);

        var guid = Guid.NewGuid().ToString("N");

        if (!string.IsNullOrEmpty(ns))
        {
            codeWriter.WriteLine($"namespace {ns}");
        }

        codeWriter.WriteLine("{");

        codeWriter.WriteLine($"public interface {interfaceShortName}EventHandlers");
        codeWriter.WriteLine("{");

        var methodsInInterface = typeSymbol.GetMembers().OfType<IMethodSymbol>().ToList();
        
        foreach (var methodSymbol in methodsInInterface)
        {
            var eventArgType = GeneratorHelpers.GetEventArgType(methodSymbol);
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

        codeWriter.WriteLine(
            $"internal class EventMediator_{interfaceShortName}_Impl_{guid} : {interfaceLongName}, {interfaceLongName}EventHandlers");
        codeWriter.WriteLine("{");

        codeWriter.WriteLine($"private readonly IEnumerable<{interfaceLongName}Subscriber> _subscribers;");

        codeWriter.WriteLine(
            $"public EventMediator_{interfaceShortName}_Impl_{guid}(IEnumerable<{interfaceLongName}Subscriber> subscribers)");
        codeWriter.WriteLine("{");

        codeWriter.WriteLine("_subscribers = subscribers;");

        codeWriter.WriteLine("foreach (var subscriber in subscribers)");
        codeWriter.WriteLine("{");
        codeWriter.WriteLine("subscriber.Subscribe(this);");
        codeWriter.WriteLine("}");

        codeWriter.WriteLine("}");
        codeWriter.WriteLine();

        foreach (var methodSymbol in methodsInInterface)
        {
            var eventArgType = GeneratorHelpers.GetEventArgType(methodSymbol);
            eventArgType = string.IsNullOrEmpty(eventArgType) ? string.Empty : $"<{eventArgType}>";

            codeWriter.WriteLine($"public event EventHandler{eventArgType} On{methodSymbol.Name};");
        }

        codeWriter.WriteLine();
        codeWriter.WriteLine("[System.Runtime.CompilerServices.ModuleInitializer]");
        codeWriter.WriteLine($"internal static void Register{Guid.NewGuid():N}()");
        codeWriter.WriteLine("{");
        codeWriter.WriteLine(
            $"{typeof(DependencyInjectionExtensions).Namespace}.{nameof(DependencyInjectionExtensions)}.{nameof(DependencyInjectionExtensions.Mediators)}.Add(typeof(EventMediator_{interfaceShortName}_Impl_{guid}));");
        codeWriter.WriteLine("}");
        codeWriter.WriteLine();

        foreach (var methodSymbol in methodsInInterface)
        {
            var eventArgType = GeneratorHelpers.GetEventArgType(methodSymbol);
            var eventArgParameterName = string.IsNullOrEmpty(eventArgType) ? string.Empty : " args";
            var eventArgToPass =
                string.IsNullOrEmpty(eventArgParameterName) ? " EventArgs.Empty" : eventArgParameterName;

            codeWriter.WriteLine($"public void {methodSymbol.Name}({eventArgType}{eventArgParameterName})");
            codeWriter.WriteLine("{");
            codeWriter.WriteLine($"On{methodSymbol.Name}?.Invoke(this,{eventArgToPass});");
            codeWriter.WriteLine("}");
            codeWriter.WriteLine();
        }

        codeWriter.WriteLine("}");

        codeWriter.WriteLine("}");
        codeWriter.WriteLine();

        return codeWriter.ToString();
    }
}