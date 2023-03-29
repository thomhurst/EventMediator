using Microsoft.CodeAnalysis;

namespace TomLonghurst.Eventing.Mediator.SourceGenerator.Helpers;

public static class GeneratorHelpers
{
    internal static string GetEventArgType(IMethodSymbol methodSymbol)
    {
        return methodSymbol.Parameters.Any()
            ? methodSymbol.Parameters.First().Type.ToDisplayString(SymbolDisplayFormats.NamespaceAndType)
            : string.Empty;
    }
}