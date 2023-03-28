using Microsoft.CodeAnalysis;

namespace TomLonghurst.Eventing.Mediator.SourceGenerator;

public record IdentifiedSubscriber
{
    public ITypeSymbol ClassType { get; set; } = null!;

    public ITypeSymbol MediatorInterfaceType { get; set; } = null!;
}