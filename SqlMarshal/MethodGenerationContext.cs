// -----------------------------------------------------------------------
// <copyright file="MethodGenerationContext.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal;

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using static SqlMarshal.Extensions;

internal class MethodGenerationContext
{
    internal MethodGenerationContext(ClassGenerationContext classGenerationContext, IMethodSymbol methodSymbol)
    {
        this.ClassGenerationContext = classGenerationContext;
        this.MethodSymbol = methodSymbol;

        this.ConnectionParameter = GetConnectionParameter(methodSymbol);
        this.DbContextParameter = GetDbContextParameter(methodSymbol);
        this.CustomSqlParameter = GetCustomSqlParameter(methodSymbol);
        this.CancellationTokenParameter = GetCancellationTokenParameter(methodSymbol);
        var parameters = methodSymbol.Parameters;
        if (this.ConnectionParameter != null)
        {
            parameters = parameters.Remove(this.ConnectionParameter);
        }

        if (this.DbContextParameter != null)
        {
            parameters = parameters.Remove(this.DbContextParameter);
        }

        if (this.CustomSqlParameter != null)
        {
            parameters = parameters.Remove(this.CustomSqlParameter);
        }

        if (this.CancellationTokenParameter != null)
        {
            parameters = parameters.Remove(this.CancellationTokenParameter);
        }

        this.SqlParameters = parameters;
    }

    internal IMethodSymbol MethodSymbol { get; }

    internal ClassGenerationContext ClassGenerationContext { get; }

    internal bool UseDbConnection => this.ClassGenerationContext.ConnectionField != null || this.ConnectionParameter != null;

    internal IParameterSymbol? ConnectionParameter { get; }

    internal IParameterSymbol? DbContextParameter { get; }

    internal IParameterSymbol? CustomSqlParameter { get; }

    internal IParameterSymbol? CancellationTokenParameter { get; }

    internal ImmutableArray<IParameterSymbol> SqlParameters { get; }

    internal bool IsTask => this.MethodSymbol.ReturnType.Name == "Task";

    internal bool IsDataReader => this.MethodSymbol.ReturnType.Name == "DbDataReader";

    internal ITypeSymbol ReturnType => this.MethodSymbol.ReturnType.UnwrapTaskType();

    internal bool IsList => this.ItemType != this.ReturnType;

    internal ITypeSymbol ItemType => UnwrapListItem(this.ReturnType);

    private static IParameterSymbol? GetConnectionParameter(IMethodSymbol methodSymbol)
    {
        foreach (var parameterSymbol in methodSymbol.Parameters)
        {
            if (parameterSymbol.Type.IsDbConnection())
            {
                return parameterSymbol;
            }
        }

        return null;
    }

    private static IParameterSymbol? GetDbContextParameter(IMethodSymbol methodSymbol)
    {
        foreach (var parameterSymbol in methodSymbol.Parameters)
        {
            if (parameterSymbol.Type.IsDbContext())
            {
                return parameterSymbol;
            }
        }

        return null;
    }

    private static IParameterSymbol? GetCancellationTokenParameter(IMethodSymbol methodSymbol)
    {
        foreach (var parameterSymbol in methodSymbol.Parameters)
        {
            if (parameterSymbol.Type.IsCancellationToken())
            {
                return parameterSymbol;
            }
        }

        return null;
    }

    private static IParameterSymbol? GetCustomSqlParameter(IMethodSymbol methodSymbol)
    {
        foreach (var parameterSymbol in methodSymbol.Parameters)
        {
            var customSqlAttributeCandidate = parameterSymbol.GetAttributes()
                .FirstOrDefault(_ => _.AttributeClass?.Name == "RawSqlAttribute");
            if (customSqlAttributeCandidate != null)
            {
                return parameterSymbol;
            }
        }

        return null;
    }
}