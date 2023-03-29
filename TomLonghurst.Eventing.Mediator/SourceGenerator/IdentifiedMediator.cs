using Microsoft.CodeAnalysis;

namespace TomLonghurst.Eventing.Mediator.SourceGenerator;

public record IdentifiedMediator
{
    public ITypeSymbol InterfaceType { get; set; } = null!;

    public List<IMethodSymbol> MethodsInInterface { get; set; } = null!;
}