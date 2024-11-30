// -----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

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

    internal static bool IsDbTransaction(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol.Name == "DbTransaction")
        {
            return true;
        }

        var baseType = typeSymbol.BaseType;
        if (baseType == null)
        {
            return false;
        }

        return IsDbTransaction(baseType);
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

    internal static ITypeSymbol UnwrapTaskType(this ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedTypeSymbol)
        {
            if (type.Name == "Task" && namedTypeSymbol.IsGenericType)
            {
                return namedTypeSymbol.TypeArguments[0];
            }
        }

        return type;
    }

    internal static ITypeSymbol UnwrapNullableType(this ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedTypeSymbol)
        {
            if (type.Name == "Nullable")
            {
                return namedTypeSymbol.TypeArguments[0];
            }
        }

        return type;
    }

    internal static INamedTypeSymbol UnwrapNullableType(this INamedTypeSymbol namedTypeSymbol)
    {
        if (namedTypeSymbol.Name == "Nullable")
        {
            return (INamedTypeSymbol)namedTypeSymbol.TypeArguments[0];
        }

        return namedTypeSymbol;
    }

    internal static bool IsScalarType(ITypeSymbol returnType)
    {
        return returnType.SpecialType switch
        {
            SpecialType.System_String => true,
            SpecialType.System_Boolean => true,
            SpecialType.System_Byte => true,
            SpecialType.System_Int32 => true,
            SpecialType.System_Int64 => true,
            SpecialType.System_DateTime => true,
            SpecialType.System_Decimal => true,
            SpecialType.System_Double => true,
            _ => false,
        };
    }

    internal static bool IsTuple(ITypeSymbol returnType)
    {
        return returnType.Name == "Tuple" || returnType.Name == "ValueTuple";
    }

    internal static ITypeSymbol GetUnderlyingType(ITypeSymbol returnType)
    {
        if (returnType is INamedTypeSymbol namedTypeSymbol)
        {
            if (!namedTypeSymbol.IsGenericType || namedTypeSymbol.TypeArguments.Length != 1)
            {
                return returnType;
            }

            return namedTypeSymbol.TypeArguments[0];
        }

        return returnType;
    }

    internal static bool IsList(ITypeSymbol returnType) => returnType.Name == "IList" || returnType.Name == "List";

    internal static bool IsEnumerable(ITypeSymbol returnType) => returnType.Name == "IEnumerable";

    internal static ITypeSymbol UnwrapListItem(ITypeSymbol returnType)
    {
        if (returnType is INamedTypeSymbol namedTypeSymbol)
        {
            if (!IsList(returnType) && !IsEnumerable(returnType))
            {
                return returnType;
            }

            if (!namedTypeSymbol.IsGenericType || namedTypeSymbol.TypeArguments.Length != 1)
            {
                return returnType;
            }

            return namedTypeSymbol.TypeArguments[0];
        }

        return returnType;
    }

    internal static IPropertySymbol? FindIdMember(this ITypeSymbol returnType)
    {
        return returnType.GetMembers().OfType<IPropertySymbol>().FirstOrDefault(FindIdMember);
    }

    internal static bool FindIdMember(this IPropertySymbol propertySymbol)
    {
        return propertySymbol.Name == "Id";
    }

    internal static IPropertySymbol? FindMember(this ITypeSymbol returnType, string parameterName)
    {
        return returnType.GetMembers().OfType<IPropertySymbol>()
            .FirstOrDefault(propertySymbol => string.Equals(propertySymbol.Name, parameterName, System.StringComparison.InvariantCultureIgnoreCase));
    }
}
