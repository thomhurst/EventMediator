using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomLonghurst.Eventing.Mediator.Extensions;
using TomLonghurst.Eventing.Mediator.SourceGenerator.Helpers;

namespace TomLonghurst.Eventing.Mediator;

public static class EventSubscriberGenerator
{
    internal static bool IsEventSubscriber(
        SyntaxNode syntaxNode,
        CancellationToken cancellationToken)
    {
        if (syntaxNode is not BaseTypeSyntax baseTypeSyntax)
        {
            return false;
        }
        
        if (baseTypeSyntax.Type is not GenericNameSyntax genericNameSyntax)
        {
            return false;
        }

        var name = genericNameSyntax.Identifier.Text;

        return name is nameof(IEventSubscriber<IEventMediator>);
    }

    internal static ITypeSymbol? GetEventSubscriberClassTypeOrNull(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken)
    {
        var baseTypeSyntax = (BaseTypeSyntax) context.Node;
        
        if (baseTypeSyntax.Parent?.Parent is not ClassDeclarationSyntax classDeclarationSyntax)
        {
            return null;
        }

        return context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) as ITypeSymbol;
    }
    
    internal static void GenerateEventSubscriberCode(
        SourceProductionContext context,
        ImmutableArray<ITypeSymbol?> classes)
    {
        if (classes.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (var type in classes)
        {
            if (type is null)
            {
                continue;
            }

            var code = GenerateEventSubscriberCode(type);
            
            if (string.IsNullOrEmpty(code))
            {
                return;
            }
            
            var typeNamespace = type.ContainingNamespace.IsGlobalNamespace
                ? null
                : $"{type.ContainingNamespace}.";

            context.AddSource($"{typeNamespace}{type.Name}.Subscriber.g.cs", code);
        }
    }
    
    private static string GenerateEventSubscriberCode(ITypeSymbol classType)
    {
        var codeWriter = new CodeGenerationTextWriter();
        
        codeWriter.WriteLine($"using {typeof(DependencyInjectionExtensions).Namespace};");
        codeWriter.WriteLine($"using {typeof(IEventMediator).Namespace};");
        codeWriter.WriteLine($"using {typeof(IEnumerable<>).Namespace};");
        codeWriter.WriteLine($"using {typeof(Enumerable).Namespace};");
        codeWriter.WriteLine($"using {typeof(string).Namespace};");
        codeWriter.WriteLine();

        var eventSubscriberInterface = classType.Interfaces.First();

        if (eventSubscriberInterface.Name != nameof(IEventSubscriber<IEventMediator>))
        {
            return string.Empty;
        }

        var typeOfMediator = eventSubscriberInterface.TypeArguments.First();

        var interfaceLongName = typeOfMediator.ToDisplayString(SymbolDisplayFormats.NamespaceAndType);

        codeWriter.WriteLine($"namespace {classType.ContainingNamespace}");
        codeWriter.WriteLine("{");

        codeWriter.WriteLine($"public partial class {classType.Name} : {interfaceLongName}Subscriber");
        codeWriter.WriteLine("{");
        codeWriter.WriteLine("}");

        codeWriter.WriteLine("}");
        codeWriter.WriteLine();

        return codeWriter.ToString();
    }
}