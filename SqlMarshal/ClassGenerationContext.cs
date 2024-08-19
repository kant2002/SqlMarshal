// -----------------------------------------------------------------------
// <copyright file="ClassGenerationContext.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using static SqlMarshal.Extensions;

internal class ClassGenerationContext
{
    public ClassGenerationContext(
        INamedTypeSymbol classSymbol,
        List<IMethodSymbol> methods,
        ISymbol attributeSymbol,
        NullableContextOptions nullableContextOptions)
    {
        this.ClassSymbol = classSymbol;
        this.Methods = methods.Select(_ => new MethodGenerationContext(this, _)).ToList();
        this.AttributeSymbol = attributeSymbol;
        this.NullableContextOptions = nullableContextOptions;

        this.ConnectionField = GetConnectionField(classSymbol);
        this.DbContextField = GetContextField(classSymbol);
    }

    public INamedTypeSymbol ClassSymbol { get; }

    public List<MethodGenerationContext> Methods { get; }

    public ISymbol AttributeSymbol { get; }

    public NullableContextOptions NullableContextOptions { get; }

    public bool HasNullableAnnotations => this.NullableContextOptions != NullableContextOptions.Disable;

    public IFieldSymbol? ConnectionField { get; }

    public IFieldSymbol? DbContextField { get; }

    public string DbContextName => this.DbContextField?.Name ?? "dbContext";

    public bool HasEfCore => this.ConnectionField == null && this.Methods.All(_ => _.ConnectionParameter == null);

    public bool HasCollections => !this.HasEfCore || this.Methods.Any(_ => (_.IsList || _.IsEnumerable) && (IsScalarType(_.ItemType) || IsTuple(_.ItemType)));

    private static IFieldSymbol? GetConnectionField(INamedTypeSymbol classSymbol)
    {
        var fieldSymbols = classSymbol.GetMembers().OfType<IFieldSymbol>();
        foreach (var fieldSymbol in fieldSymbols)
        {
            if (fieldSymbol.Type.IsDbConnection())
            {
                return fieldSymbol;
            }
        }

        if (classSymbol.BaseType != null)
        {
            return GetConnectionField(classSymbol.BaseType);
        }

        return null;
    }

    private static IFieldSymbol? GetContextField(INamedTypeSymbol classSymbol)
    {
        var fieldSymbols = classSymbol.GetMembers().OfType<IFieldSymbol>();
        foreach (var fieldSymbol in fieldSymbols)
        {
            if (fieldSymbol.Type.IsDbContext())
            {
                return fieldSymbol;
            }
        }

        return null;
    }
}
