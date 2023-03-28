using Microsoft.CodeAnalysis;

namespace TomLonghurst.Eventing.Mediator;

public class ErroredSymbol
{
    public ISymbol Symbol { get; }
    public Diagnostic Diagnostic { get; }

    public ErroredSymbol(ISymbol symbol, Diagnostic diagnostic)
    {
        Symbol = symbol;
        Diagnostic = diagnostic;
    }
}