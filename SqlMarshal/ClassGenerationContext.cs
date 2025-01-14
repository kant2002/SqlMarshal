// -----------------------------------------------------------------------
// <copyright file="ClassGenerationContext.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using static SqlMarshal.Extensions;

internal class ClassGenerationContext
{
    public ClassGenerationContext(
        INamedTypeSymbol classSymbol,
        List<IMethodSymbol> methods,
        ISymbol attributeSymbol,
        ISymbol repositoryAttributeSymbol,
        GeneratorExecutionContext context)
    {
        this.ClassSymbol = classSymbol;
        this.Methods = methods.Select(_ => new MethodGenerationContext(this, _)).ToList();
        this.AttributeSymbol = attributeSymbol;
        this.RepositoryAttributeSymbol = repositoryAttributeSymbol;
        this.GeneratorExecutionContext = context;
        this.NullableContextOptions = context.Compilation.Options.NullableContextOptions;

        this.ConnectionField = GetConnectionField(classSymbol);
        this.DbContextField = GetContextField(classSymbol);
    }

    public INamedTypeSymbol ClassSymbol { get; }

    public List<MethodGenerationContext> Methods { get; }

    public ISymbol AttributeSymbol { get; }

    public ISymbol RepositoryAttributeSymbol { get; }

    public GeneratorExecutionContext GeneratorExecutionContext { get; }

    public NullableContextOptions NullableContextOptions { get; }

    public bool HasNullableAnnotations => this.NullableContextOptions != NullableContextOptions.Disable;

    public IFieldSymbol? ConnectionField { get; }

    public IFieldSymbol? DbContextField { get; }

    public string DbContextName => this.DbContextField?.Name ?? "dbContext";

    public bool IsRepository => this.ClassSymbol.GetAttributes().Any(ad => ad.AttributeClass!.Equals(this.RepositoryAttributeSymbol, SymbolEqualityComparer.Default));

    public ITypeSymbol? RepositoryEntityType => (ITypeSymbol?)this.ClassSymbol.GetAttributes().Single(ad => ad.AttributeClass!.Equals(this.RepositoryAttributeSymbol, SymbolEqualityComparer.Default)).ConstructorArguments.ElementAtOrDefault(0).Value;

    public bool HasEfCore => this.ConnectionField == null && this.Methods.All(_ => _.ConnectionParameter == null);

    public bool HasCollections => !this.HasEfCore || this.Methods.Any(_ => (_.IsList || _.IsEnumerable) && (IsScalarType(_.ItemType) || IsTuple(_.ItemType)));

    public INamedTypeSymbol CreateTaskType(ITypeSymbol nestedType)
    {
        var taskType = this.GeneratorExecutionContext.Compilation.GetTypeByMetadataName($"System.Threading.Tasks.Task`1")!;
        var taskedType = taskType.Construct(ImmutableArray.Create(nestedType), ImmutableArray.Create(nestedType.NullableAnnotation == NullableAnnotation.None ? NullableAnnotation.Annotated : nestedType.NullableAnnotation));
        return taskedType;
    }

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
