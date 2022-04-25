// -----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal;

using Microsoft.CodeAnalysis;

internal static class Extensions
{
    public static bool CanHaveNullValue(this ITypeSymbol typeSymbol, bool hasNullableAnnotations)
    {
        if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return true;
        }

        var requireParameterNullCheck = !hasNullableAnnotations && !typeSymbol.IsValueType;
        return requireParameterNullCheck;
    }

    internal static bool IsDbConnection(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol.Name == "DbConnection")
        {
            return true;
        }

        var baseType = typeSymbol.BaseType;
        if (baseType == null)
        {
            return false;
        }

        return IsDbConnection(baseType);
    }

    internal static bool IsDbContext(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol.Name == "DbContext")
        {
            return true;
        }

        var baseType = typeSymbol.BaseType;
        if (baseType == null)
        {
            return false;
        }

        return IsDbContext(baseType);
    }

    internal static bool IsCancellationToken(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol.Name == "CancellationToken")
        {
            return true;
        }

        return false;
    }
}
