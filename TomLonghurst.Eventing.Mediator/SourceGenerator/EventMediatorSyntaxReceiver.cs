using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomLonghurst.Eventing.Mediator.Extensions;
using TomLonghurst.Eventing.Mediator.SourceGenerator.Attributes;
using TomLonghurst.Eventing.Mediator.SourceGenerator.Helpers;

namespace TomLonghurst.Eventing.Mediator.SourceGenerator;

internal class EventMediatorSyntaxReceiver : ISyntaxContextReceiver
{
    public EventMediatorSyntaxReceiver()
    {
#if DEBUG
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            //System.Diagnostics.Debugger.Launch();
        }
#endif    
    }
    
    public List<IdentifiedMediator> IdentifiedMediators { get; } = new();
    public List<IdentifiedSubscriber> IdentifiedSubscribers { get; } = new();
    public List<Diagnostic> DiagnosticErrors { get; } = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is InterfaceDeclarationSyntax interfaceDeclarationSyntax)
        {
            ProcessInterface(context, interfaceDeclarationSyntax);
        }
        
        if (context.Node is ClassDeclarationSyntax classDeclarationSyntax)
        {
            ProcessClass(context, classDeclarationSyntax);
        }
    }


    private void ProcessClass(GeneratorSyntaxContext context, ClassDeclarationSyntax classDeclarationSyntax)
    {
        var symbol = GetSymbol(context, classDeclarationSyntax);

        if (symbol is not ITypeSymbol { TypeKind: TypeKind.Class } classSymbol)
        {
            return;
        }

        var eventSubscriberAttributes = classSymbol.GetAttributes()
            .Where(x => x.AttributeClass?.ToDisplayString(SymbolDisplayFormats.GenericBase) == typeof(EventSubscriberAttribute<>).GetFullNameWithoutGenericArity())
            .ToList();

        if (eventSubscriberAttributes.Count == 0)
        {
            return;
        }

        var mediatorTypeFromAttribute = eventSubscriberAttributes.First().AttributeClass!.TypeArguments.First();

        CheckForEventMediatorAttribute(mediatorTypeFromAttribute);

        IdentifiedSubscribers.Add(new IdentifiedSubscriber
        {
            MediatorInterfaceType = mediatorTypeFromAttribute,
            ClassType = classSymbol
        });
    }

    private void CheckForEventMediatorAttribute(ISymbol mediatorTypeFromAttribute)
    {
        var eventMediatorAttributes = mediatorTypeFromAttribute.GetAttributes()
            .Where(x => x.AttributeClass?.ToDisplayString(SymbolDisplayFormats.NamespaceAndType) == typeof(EventMediatorAttribute).FullName)
            .ToList();
        
        if (eventMediatorAttributes.Count == 0)
        {
            DiagnosticErrors.Add(Diagnostic.Create(Mediator.DiagnosticErrors.MissingEventMediatorAttribute, mediatorTypeFromAttribute.Locations.FirstOrDefault() ?? Location.None, mediatorTypeFromAttribute.Name));
        }
    }

    private void ProcessInterface(GeneratorSyntaxContext context, InterfaceDeclarationSyntax interfaceDeclarationSyntax)
    {
        var symbol = GetSymbol(context, interfaceDeclarationSyntax);

        if (symbol is not ITypeSymbol { TypeKind: TypeKind.Interface } interfaceSymbol)
        {
            return;
        }
        
        var eventMediatorAttributes = interfaceSymbol.GetAttributes()
            .Where(x => x.AttributeClass?.ToDisplayString(SymbolDisplayFormats.NamespaceAndType) == typeof(EventMediatorAttribute).FullName)
            .ToList();

        if (eventMediatorAttributes.Count == 0)
        {
            return;
        }

        var methods = GetMethods(interfaceSymbol);

        IdentifiedMediators.Add(new IdentifiedMediator
        {
            InterfaceType = interfaceSymbol,
            MethodsInInterface = methods
        });
    }

    private ISymbol? GetSymbol(GeneratorSyntaxContext context, SyntaxNode syntaxNode)
    {
        return context.SemanticModel.GetDeclaredSymbol(syntaxNode) ?? context.SemanticModel.GetSymbolInfo(syntaxNode).Symbol;
    }

    private List<IMethodSymbol> GetMethods(ITypeSymbol interfaceSymbol)
    {
        var interfaceMembers = interfaceSymbol.GetMembers();

        var methods = interfaceMembers
            .OfType<IMethodSymbol>()
            .ToList();

        DiagnosticErrors.AddRange(methods.Where(x => !x.ReturnsVoid).Select(x => Diagnostic.Create(Mediator.DiagnosticErrors.InvalidReturnType, x.Locations.FirstOrDefault() ?? Location.None, x.Name)));
        DiagnosticErrors.AddRange(methods.Where(x => x.IsVararg || x.Parameters.Length > 1).Select(x => Diagnostic.Create(Mediator.DiagnosticErrors.InvalidEventArguments, x.Locations.FirstOrDefault() ?? Location.None, x.Name)));

        return methods;
    }
}