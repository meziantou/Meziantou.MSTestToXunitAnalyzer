using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Meziantou.MsTestToXunitAnalyzer;

internal static class Extensions
{
    public static T CopyTriviaFrom<T>(this T node, SyntaxNode triviaToCopy) where T : SyntaxNode
    {
        return node
            .WithLeadingTrivia(triviaToCopy.GetLeadingTrivia())
            .WithTrailingTrivia(triviaToCopy.GetTrailingTrivia());
    }

    public static bool Equals(this ITypeSymbol symbol, Compilation compilation, string type)
    {
        if (symbol is null)
            return false;

        return symbol.Equals(compilation.GetTypeByMetadataName(type), SymbolEqualityComparer.Default);
    }

    public static bool IsOrImplements(this ITypeSymbol symbol, Compilation compilation, string type)
    {
        if (symbol is null)
            return false;

        var otherSymbol = compilation.GetTypeByMetadataName(type);
        return IsOrImplements(symbol, otherSymbol);
    }

    public static bool IsOrImplements(this ITypeSymbol symbol, ITypeSymbol type)
    {
        if (symbol is null)
            return false;

        if (symbol.Equals(type, SymbolEqualityComparer.Default))
            return true;

        foreach (var s in symbol.AllInterfaces)
        {
            if (s.OriginalDefinition.Equals(type, SymbolEqualityComparer.Default))
                return true;
        }

        return false;
    }

    public static bool IsOrInheritsFrom(this ITypeSymbol symbol, Compilation compilation, string type)
    {
        if (symbol is null)
            return false;

        var otherSymbol = compilation.GetTypeByMetadataName(type);
        if (otherSymbol is null)
            return false;

        do
        {
            if (symbol.Equals(otherSymbol, SymbolEqualityComparer.Default))
                return true;

            symbol = symbol.BaseType;
        } while (symbol is not null);

        return false;
    }

    public static bool IsOrInheritsFrom(this ITypeSymbol symbol, ITypeSymbol baseSymbol)
    {
        if (symbol is null || baseSymbol is null)
            return false;

        do
        {
            if (symbol.Equals(baseSymbol, SymbolEqualityComparer.Default))
                return true;

            symbol = symbol.BaseType;
        } while (symbol is not null);

        return false;
    }

    public static IOperation RemoveImplicitConversion(this IOperation operation)
    {
        while (operation is IConversionOperation conversionOperation && conversionOperation.IsImplicit)
        {
            operation = conversionOperation.Operand;
        }

        return operation;
    }

    public static AttributeData GetAttribute(this ISymbol symbol, ITypeSymbol attributeType, bool inherits = true)
    {
        if (attributeType is null)
            return null;

        if (attributeType.IsSealed)
        {
            inherits = false;
        }

        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass is null)
                continue;

            if (inherits)
            {
                if (attribute.AttributeClass.IsOrInheritsFrom(attributeType))
                    return attribute;
            }
            else
            {
                if (SymbolEqualityComparer.Default.Equals(attributeType, attribute.AttributeClass))
                    return attribute;
            }
        }

        return null;
    }

    public static bool HasAttribute(this ISymbol symbol, ITypeSymbol attributeType, bool inherits = true)
    {
        return GetAttribute(symbol, attributeType, inherits) is not null;
    }
}
