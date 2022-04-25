﻿// -----------------------------------------------------------------------
// <copyright file="Generator.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static SqlMarshal.Extensions;

/// <summary>
/// Stored procedures generator.
/// </summary>
[Generator]
public class Generator : ISourceGenerator
{
    private const string AttributeSource = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable disable

[System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple=true)]
internal sealed class SqlMarshalAttribute: System.Attribute
{
    public SqlMarshalAttribute()
        => (StoredProcedureName) = (string.Empty);

    public SqlMarshalAttribute(string name)
        => (StoredProcedureName) = (name);

    public string StoredProcedureName { get; }
}

[System.AttributeUsage(System.AttributeTargets.Parameter, AllowMultiple=false)]
internal sealed class RawSqlAttribute: System.Attribute
{
    public RawSqlAttribute() {}
}
";

    /// <inheritdoc/>
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization((pi) => pi.AddSource("SqlMarshalAttribute.cs", AttributeSource));
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    /// <inheritdoc/>
    public void Execute(GeneratorExecutionContext context)
    {
        // Retrieve the populated receiver
        if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver))
        {
            return;
        }

        INamedTypeSymbol? attributeSymbol = context.Compilation.GetTypeByMetadataName("SqlMarshalAttribute");
        if (attributeSymbol == null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("SP0001", "No stored procedure attribute", "Internal analyzer error.", "Internal", DiagnosticSeverity.Error, true),
                null));
            return;
        }

        var hasNullableAnnotations = context.Compilation.Options.NullableContextOptions != NullableContextOptions.Disable;

        // Group the fields by class, and generate the source
        foreach (IGrouping<ISymbol?, IMethodSymbol> group in receiver.Methods.GroupBy(f => f.ContainingType, SymbolEqualityComparer.Default))
        {
            var key = (INamedTypeSymbol)group.Key!;
            var generationContext = new ClassGenerationContext(
                (INamedTypeSymbol)group.Key!,
                group.ToList(),
                attributeSymbol,
                context.Compilation.Options.NullableContextOptions);
            var sourceCode = this.ProcessClass(
                generationContext,
                (INamedTypeSymbol)group.Key!,
                attributeSymbol,
                hasNullableAnnotations);
            if (sourceCode == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("SP0002", "No source code generated attribute", "Internal analyzer error.", "Internal", DiagnosticSeverity.Error, true),
                    null));
                continue;
            }

            context.AddSource($"{key.ToDisplayString().Replace(".", "_")}_sp.cs", SourceText.From(sourceCode, Encoding.UTF8));
        }
    }

    private static string GetAccessibility(Accessibility a)
    {
        return a switch
        {
            Accessibility.Public => "public",
            Accessibility.Friend => "internal",
            Accessibility.Private => "private",
            _ => string.Empty,
        };
    }

    private static ISymbol? GetDbSetField(IFieldSymbol? dbContextSymbol, ITypeSymbol itemTypeSymbol)
    {
        if (dbContextSymbol == null)
        {
            return null;
        }

        var members = dbContextSymbol.Type.GetMembers().OfType<IPropertySymbol>();
        foreach (var fieldSymbol in members)
        {
            var fieldType = fieldSymbol.Type;
            if (fieldType is INamedTypeSymbol namedTypeSymbol)
            {
                namedTypeSymbol = namedTypeSymbol.UnwrapNullableType();
                if (namedTypeSymbol.Name == "DbSet"
                    && namedTypeSymbol.TypeArguments.Length == 1
                    && namedTypeSymbol.TypeArguments[0].Name == itemTypeSymbol.Name)
                {
                    return fieldSymbol;
                }
            }
        }

        return null;
    }

    private static string GetParameterDeclaration(IMethodSymbol methodSymbol, IParameterSymbol parameter, int index)
    {
        if (parameter.RefKind == RefKind.Out)
        {
            return $"out {parameter.Type.ToDisplayString()} {parameter.Name}";
        }

        if (parameter.RefKind == RefKind.Ref)
        {
            return $"ref {parameter.Type.ToDisplayString()} {parameter.Name}";
        }

        if (methodSymbol.IsExtensionMethod && index == 0)
        {
            return $"this {parameter.Type.ToDisplayString()} {parameter.Name}";
        }

        return $"{parameter.Type.ToDisplayString()} {parameter.Name}";
    }

    private static string GetParameterPassing(IParameterSymbol parameter)
    {
        var parameterName = NameMapper.MapName(parameter.Name);
        if (parameter.RefKind == RefKind.Out || parameter.RefKind == RefKind.Ref)
        {
            return "@" + parameterName + " OUTPUT";
        }

        return "@" + parameterName;
    }

    private static ITypeSymbol UnwrapNullableType(ITypeSymbol type)
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

    private static string GetParameterSqlDbType(ITypeSymbol type)
    {
        type = UnwrapNullableType(type);
        switch (type.SpecialType)
        {
            case SpecialType.System_String:
                return "System.Data.DbType.String";
            case SpecialType.System_Byte:
                return "System.Data.DbType.Byte";
            case SpecialType.System_SByte:
                return "System.Data.DbType.SByte";
            case SpecialType.System_Int16:
                return "System.Data.DbType.Int16";
            case SpecialType.System_Int32:
                return "System.Data.DbType.Int32";
            case SpecialType.System_Int64:
                return "System.Data.DbType.Int64";
            case SpecialType.System_UInt16:
                return "System.Data.DbType.UInt16";
            case SpecialType.System_UInt32:
                return "System.Data.DbType.UInt32";
            case SpecialType.System_UInt64:
                return "System.Data.DbType.UInt64";
            case SpecialType.System_Single:
                return "System.Data.DbType.Single";
            case SpecialType.System_Double:
                return "System.Data.DbType.Double";
            case SpecialType.System_DateTime:
                return "System.Data.DbType.DateTime2";
            default:
                throw new System.NotImplementedException();
        }
    }

    private static string GetDataReaderMethod(ITypeSymbol type)
    {
        type = UnwrapNullableType(type);
        switch (type.SpecialType)
        {
            case SpecialType.System_String:
                return "GetString";
            case SpecialType.System_Byte:
                return "GetByte";
            case SpecialType.System_SByte:
                return "GetSByte";
            case SpecialType.System_Int16:
                return "GetInt16";
            case SpecialType.System_Int32:
                return "GetInt32";
            case SpecialType.System_Int64:
                return "GetInt64";
            case SpecialType.System_UInt16:
                return "GetUInt16";
            case SpecialType.System_UInt32:
                return "GetUInt32";
            case SpecialType.System_UInt64:
                return "GetUInt64";
            case SpecialType.System_Single:
                return "GetSingle";
            case SpecialType.System_Double:
                return "GetDouble";
            case SpecialType.System_DateTime:
                return "GetDateTime2";
            default:
                throw new System.NotImplementedException();
        }
    }

    private static void DeclareParameter(IndentedStringBuilder source, bool hasNullableAnnotations, IParameterSymbol parameter)
    {
        var requireParameterNullCheck = parameter.Type.CanHaveNullValue(hasNullableAnnotations);
        source.Append($@"var {parameter.Name}Parameter = command.CreateParameter();
            {parameter.Name}Parameter.ParameterName = ""@{NameMapper.MapName(parameter.Name)}"";
");
        if (parameter.RefKind == RefKind.Out || parameter.RefKind == RefKind.Ref)
        {
            var parameterSqlDbType = GetParameterSqlDbType(parameter.Type);
            source.AppendLine($@"{parameter.Name}Parameter.DbType = {parameterSqlDbType};");
            var direction = parameter.RefKind == RefKind.Out ? "System.Data.ParameterDirection.Output" : "System.Data.ParameterDirection.InputOutput";
            source.AppendLine($@"{parameter.Name}Parameter.Direction = {direction};");
            if (parameter.Type.SpecialType == SpecialType.System_String)
            {
                const int StringSize = 4000;
                source.AppendLine($@"{parameter.Name}Parameter.Size = {StringSize};");
            }
        }

        if (parameter.RefKind == RefKind.None || parameter.RefKind == RefKind.Ref)
        {
            if (requireParameterNullCheck)
            {
                source.AppendLine($@"{parameter.Name}Parameter.Value = {parameter.Name} == null ? (object)DBNull.Value : {parameter.Name};");
            }
            else
            {
                source.AppendLine($@"{parameter.Name}Parameter.Value = {parameter.Name};");
            }
        }
    }

    private static void MarshalOutputParameters(IndentedStringBuilder source, IEnumerable<IParameterSymbol> parameterSymbols, bool hasNullableAnnotations)
    {
        foreach (var parameter in parameterSymbols)
        {
            var requireReadOutput = parameter.RefKind == RefKind.Out || parameter.RefKind == RefKind.Ref;
            if (!requireReadOutput)
            {
                continue;
            }

            source.AppendLine($@"{parameter.Name} = {MarshalValue($"{parameter.Name}Parameter.Value" + (hasNullableAnnotations ? "!" : string.Empty), hasNullableAnnotations, parameter.Type)};");
        }
    }

    private static string MarshalValue(string identifier, bool hasNullableAnnotations, ITypeSymbol returnType)
    {
        if (returnType.CanHaveNullValue(hasNullableAnnotations))
        {
            var nonNullExpression = CastExpression(
                ParseTypeName(UnwrapNullableType(returnType).ToDisplayString()),
                IdentifierName(identifier));
            var nullExpression = CastExpression(
                ParseTypeName(returnType.ToDisplayString()),
                LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NullLiteralExpression));
            var mappingExpression = ConditionalExpression(
                BinaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.EqualsExpression, IdentifierName(identifier), MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleMemberAccessExpression, IdentifierName("DBNull"), IdentifierName("Value"))),
                nullExpression,
                nonNullExpression);

            // return mappingExpression.NormalizeWhitespace().ToFullString();
            var nullableReturnType = returnType.ToDisplayString();
            if (!hasNullableAnnotations && returnType.IsReferenceType && returnType.NullableAnnotation != NullableAnnotation.Annotated)
            {
                nullableReturnType += "?";
            }

            return $@"{identifier} == DBNull.Value ? ({nullableReturnType})null : ({UnwrapNullableType(returnType).ToDisplayString()}){identifier}";
        }
        else
        {
            return CastExpression(
                ParseTypeName(returnType.ToDisplayString()),
                IdentifierName(identifier)).NormalizeWhitespace().ToFullString();
        }
    }

    private static void ExecuteSimpleQuery(
        IndentedStringBuilder source,
        MethodGenerationContext methodGenerationContext,
        bool hasNullableAnnotations,
        bool isTask,
        ITypeSymbol returnType)
    {
        var hasResult = returnType.SpecialType != SpecialType.System_Void && returnType.Name != "Task";
        var cancellationToken = methodGenerationContext.CancellationTokenParameter?.Name ?? string.Empty;
        if (!hasResult)
        {
            if (isTask)
            {
                source.AppendLine($@"await command.ExecuteNonQueryAsync({cancellationToken}).ConfigureAwait(false);");
            }
            else
            {
                source.AppendLine($@"command.ExecuteNonQuery();");
            }
        }
        else
        {
            if (isTask)
            {
                source.AppendLine($@"var result = await command.ExecuteScalarAsync({cancellationToken}).ConfigureAwait(false);");
            }
            else
            {
                source.AppendLine($@"var result = command.ExecuteScalar();");
            }
        }

        MarshalOutputParameters(source, methodGenerationContext.MethodSymbol.Parameters, hasNullableAnnotations);

        if (hasResult)
        {
            source.AppendLine($@"return {MarshalValue("result", hasNullableAnnotations, returnType)};");
        }
    }

    private string? ProcessClass(
        ClassGenerationContext classGenerationContext,
        INamedTypeSymbol classSymbol,
        ISymbol attributeSymbol,
        bool hasNullableAnnotations)
    {
        if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
        {
            // TODO: issue a diagnostic that it must be top level
            return null;
        }

        string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

        var hasEfCore = classGenerationContext.HasEfCore;
        IndentedStringBuilder source = new IndentedStringBuilder($@"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace {namespaceName}
{{
");
        source.PushIndent();
        source.AppendLine("using System;");
        if (classGenerationContext.HasCollections)
        {
            source.AppendLine("using System.Collections.Generic;");
        }

        source.AppendLine("using System.Data.Common;");
        source.AppendLine("using System.Linq;");
        if (hasEfCore)
        {
            source.AppendLine("using Microsoft.EntityFrameworkCore;");
        }

        source.AppendLine();
        source.AppendLine($"{(classSymbol.IsStatic ? "static " : string.Empty)}partial class {classSymbol.Name}");
        source.AppendLine("{");

        // workaround.
        source.PopIndent();

        // Create properties for each field
        foreach (MethodGenerationContext methodGenerationContext in classGenerationContext.Methods)
        {
            this.ProcessMethod(
                source,
                methodGenerationContext,
                methodGenerationContext.MethodSymbol,
                attributeSymbol,
                hasNullableAnnotations);
        }

        // workaround.
        source.PushIndent();
        source.AppendLine("}");
        source.PopIndent();
        source.Append("}");
        return source.ToString();
    }

    private string GetConnectionStatement(MethodGenerationContext methodGenerationContext)
    {
        var connectionParameterSymbol = methodGenerationContext.ConnectionParameter;
        if (connectionParameterSymbol != null)
        {
            if (connectionParameterSymbol.Name == "connection")
            {
                return string.Empty;
            }

            return $"var connection = {connectionParameterSymbol.Name};";
        }

        var connectionSymbol = methodGenerationContext.ClassGenerationContext.ConnectionField;
        if (connectionSymbol != null)
        {
            return $"var connection = this.{connectionSymbol.Name};";
        }

        var dbContextParameterSymbol = methodGenerationContext.DbContextParameter;
        if (dbContextParameterSymbol != null)
        {
            return $"var connection = {dbContextParameterSymbol.Name}.Database.GetDbConnection();";
        }

        var dbContextSymbol = methodGenerationContext.ClassGenerationContext.DbContextField;
        var contextName = dbContextSymbol?.Name ?? "dbContext";
        return $"var connection = this.{contextName}.Database.GetDbConnection();";
    }

    private string GetOpenConnectionStatement(MethodGenerationContext methodGenerationContext)
    {
        var dbContextParameterSymbol = methodGenerationContext.DbContextParameter;
        if (dbContextParameterSymbol != null)
        {
            return $"{dbContextParameterSymbol.Name}.Database.OpenConnection();";
        }

        var connectionSymbol = methodGenerationContext.ClassGenerationContext.ConnectionField;
        if (connectionSymbol != null)
        {
            return $"this.{connectionSymbol.Name}.Open();";
        }

        var dbContextSymbol = methodGenerationContext.ClassGenerationContext.DbContextField;
        var contextName = dbContextSymbol?.Name ?? "dbContext";
        return $"this.{contextName}.Database.OpenConnection();";
    }

    private string GetCloseConnectionStatement(MethodGenerationContext methodGenerationContext)
    {
        var dbContextParameterSymbol = methodGenerationContext.DbContextParameter;
        if (dbContextParameterSymbol != null)
        {
            return $"{dbContextParameterSymbol.Name}.Database.CloseConnection();";
        }

        var connectionSymbol = methodGenerationContext.ClassGenerationContext.ConnectionField;
        if (connectionSymbol != null)
        {
            return $"this.{connectionSymbol.Name}.Close();";
        }

        var dbContextSymbol = methodGenerationContext.ClassGenerationContext.DbContextField;
        var contextName = dbContextSymbol?.Name ?? "dbContext";
        return $"this.{contextName}.Database.CloseConnection();";
    }

    private void MapResults(
        IndentedStringBuilder source,
        MethodGenerationContext methodGenerationContext,
        IMethodSymbol methodSymbol,
        System.Collections.Immutable.ImmutableArray<IParameterSymbol> parameters,
        ITypeSymbol itemType,
        bool hasNullableAnnotations,
        bool isList,
        bool isTask)
    {
        var useDbConnection = methodGenerationContext.UseDbConnection || (isList && (IsTuple(itemType) || IsScalarType(itemType)));
        var cancellationToken = methodGenerationContext.CancellationTokenParameter?.Name ?? string.Empty;
        if (useDbConnection)
        {
            string additionalReaderParameters = isList ? string.Empty : "System.Data.CommandBehavior.SingleResult | System.Data.CommandBehavior.SingleRow";
            if (isTask && !string.IsNullOrEmpty(cancellationToken))
            {
                if (string.IsNullOrEmpty(additionalReaderParameters))
                {
                    additionalReaderParameters = cancellationToken;
                }
                else
                {
                    additionalReaderParameters = additionalReaderParameters + "," + cancellationToken;
                }
            }

            if (isTask)
            {
                source.AppendLine($"using var reader = await command.ExecuteReaderAsync({additionalReaderParameters}).ConfigureAwait(false);");
            }
            else
            {
                source.AppendLine($"using var reader = command.ExecuteReader({additionalReaderParameters});");
            }

            if (isList)
            {
                source.AppendLine($@"var result = new List<{(IsTuple(itemType) ? itemType.ToDisplayString() : itemType.Name)}>();");
                if (isTask)
                {
                    source.AppendLine($"while (await reader.ReadAsync({cancellationToken}).ConfigureAwait(false))");
                }
                else
                {
                    source.AppendLine("while (reader.Read())");
                }

                source.AppendLine("{");
                source.PushIndent();
                if (IsScalarType(itemType))
                {
                    source.AppendLine($@"var value_0 = reader.GetValue(0);");
                    source.AppendLine($@"var item = {MarshalValue($"value_0", hasNullableAnnotations, itemType)};");
                    source.AppendLine("result.Add(item);");
                }
                else if (IsTuple(itemType))
                {
                    var types = ((INamedTypeSymbol)itemType).TypeArguments;
                    for (var i = 0; i < types.Length; i++)
                    {
                        source.AppendLine($@"var value_{i} = reader.GetValue({i});");
                    }

                    source.AppendLine("result.Add((");
                    source.PushIndent();
                    for (var i = 0; i < types.Length; i++)
                    {
                        if (i < types.Length - 1)
                        {
                            source.AppendLine($@"{MarshalValue($"value_{i}", hasNullableAnnotations, types[i])},");
                        }
                        else
                        {
                            source.AppendLine($@"{MarshalValue($"value_{i}", hasNullableAnnotations, types[i])}");
                        }
                    }

                    source.PopIndent();
                    source.AppendLine("));");
                }
                else
                {
                    source.AppendLine($@"var item = new {itemType.Name}();");
                    int i = 0;
                    foreach (var propertyName in itemType.GetMembers().OfType<IPropertySymbol>())
                    {
                        var dataReaderMethodName = GetDataReaderMethod(propertyName.Type);
                        source.AppendLine($@"var value_{i} = reader.GetValue({i});");
                        source.AppendLine($@"item.{propertyName.Name} = {MarshalValue($"value_{i}", hasNullableAnnotations, propertyName.Type)};");
                        i++;
                    }

                    source.AppendLine("result.Add(item);");
                }

                source.PopIndent();
                source.AppendLine("}");
                source.AppendLine();
                if (isTask)
                {
                    source.AppendLine($"await reader.CloseAsync({cancellationToken}).ConfigureAwait(false);");
                }
                else
                {
                    source.AppendLine("reader.Close();");
                }
            }
            else
            {
                if (isTask)
                {
                    source.AppendLine($"if (!(await reader.ReadAsync({cancellationToken}).ConfigureAwait(false)))");
                }
                else
                {
                    source.AppendLine("if (!reader.Read())");
                }

                source.AppendLine("{");
                source.PushIndent();
                if (hasNullableAnnotations && methodSymbol.ReturnType.NullableAnnotation != NullableAnnotation.Annotated)
                {
                    source.AppendLine("throw new InvalidOperation(\"No data returned from command.\");");
                }
                else
                {
                    source.AppendLine("return null;");
                }

                source.PopIndent();
                source.AppendLine("}");
                source.AppendLine();
                source.AppendLine($@"var result = new {itemType.Name}();");
                int i = 0;
                foreach (var propertyName in itemType.GetMembers().OfType<IPropertySymbol>())
                {
                    var dataReaderMethodName = GetDataReaderMethod(propertyName.Type);
                    source.AppendLine($@"var value_{i} = reader.GetValue({i});");
                    source.AppendLine($@"result.{propertyName.Name} = {MarshalValue($"value_{i}", hasNullableAnnotations, propertyName.Type)};");
                    i++;
                }

                if (isTask)
                {
                    source.AppendLine($"await reader.CloseAsync({cancellationToken}).ConfigureAwait(false);");
                }
                else
                {
                    source.AppendLine("reader.Close();");
                }
            }
        }
        else
        {
            var dbContextSymbol = methodGenerationContext.ClassGenerationContext.DbContextField;
            var contextName = dbContextSymbol?.Name ?? "dbContext";
            var itemTypeProperty = GetDbSetField(dbContextSymbol, itemType)?.Name ?? itemType.Name + "s";
            if (isTask)
            {
                source.AppendLine($"var result = await this.{contextName}.{itemTypeProperty}.FromSqlRaw(sqlQuery{(parameters.Length == 0 ? string.Empty : ", parameters")}).{(isList ? "ToListAsync" : "AsEnumerable().FirstOrDefaultAsync")}({cancellationToken}).ConfigureAwait(false);");
            }
            else
            {
                source.AppendLine($"var result = this.{contextName}.{itemTypeProperty}.FromSqlRaw(sqlQuery{(parameters.Length == 0 ? string.Empty : ", parameters")}).{(isList ? "ToList" : "AsEnumerable().FirstOrDefault")}();");
            }
        }
    }

    private void ProcessMethod(
        IndentedStringBuilder source,
        MethodGenerationContext methodGenerationContext,
        IMethodSymbol methodSymbol,
        ISymbol attributeSymbol,
        bool hasNullableAnnotations)
    {
        // get the name and type of the field
        string fieldName = methodSymbol.Name;
        ITypeSymbol returnType = methodGenerationContext.ReturnType;
        var symbol = (ISymbol)methodSymbol;
        var isTask = methodGenerationContext.IsTask;

        // get the AutoNotify attribute from the field, and any associated data
        AttributeData attributeData = methodSymbol.GetAttributes().Single(ad => ad.AttributeClass!.Equals(attributeSymbol, SymbolEqualityComparer.Default));
        TypedConstant overridenNameOpt = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "PropertyName").Value;
        var procedureNameConstraint = attributeData.ConstructorArguments.ElementAtOrDefault(0);
        object? procedureName = procedureNameConstraint.Value;
        var parameters = methodGenerationContext.SqlParameters;
        var originalParameters = methodSymbol.Parameters;

        bool hasCustomSql = methodGenerationContext.CustomSqlParameter != null;
        var signature = $"({string.Join(", ", originalParameters.Select((parameterSymbol, index) => GetParameterDeclaration(methodSymbol, parameterSymbol, index)))})";
        var itemType = methodGenerationContext.ItemType;
        var getConnection = this.GetConnectionStatement(methodGenerationContext);
        var isList = methodGenerationContext.IsList;
        var isScalarType = IsScalarType(UnwrapNullableType(returnType))
            || returnType.SpecialType == SpecialType.System_Void
            || returnType.Name == "Task";
        var returnTypeName = methodSymbol.ReturnType.ToString();
        if (!hasNullableAnnotations && methodSymbol.ReturnType.IsReferenceType && !isScalarType && !isList)
        {
            if (methodSymbol.ReturnType.Name == "Task")
            {
                returnTypeName = "Task<" + returnType + "?>";
            }
            else
            {
                returnTypeName += "?";
            }
        }

        source.Append($@"        {GetAccessibility(symbol.DeclaredAccessibility)} {(methodSymbol.IsStatic ? "static " : string.Empty)}partial {(isTask ? "async " : string.Empty)}{returnTypeName} {methodSymbol.Name}{signature}
        {{
            {getConnection}
            using var command = connection.CreateCommand();

");
        source.PushIndent();
        source.PushIndent();
        source.PushIndent();
        if (parameters.Length > 0)
        {
            foreach (var parameter in parameters)
            {
                DeclareParameter(source, hasNullableAnnotations, parameter);
                source.AppendLine();
            }

            source.Append($@"var parameters = new DbParameter[]
            {{
");
            source.PushIndent();
            foreach (var parameter in parameters)
            {
                source.AppendLine($"{parameter.Name}Parameter,");
            }

            source.PopIndent();
            source.AppendLine("};");
            source.AppendLine();
        }

        if (!hasCustomSql)
        {
            if (parameters.Length == 0)
            {
                source.AppendLine($@"var sqlQuery = @""{procedureName}"";");
            }
            else
            {
                string parametersList = string.Join(", ", parameters.Select(parameter => GetParameterPassing(parameter)));
                source.AppendLine($@"var sqlQuery = @""{procedureName} {parametersList}"";");
            }
        }

        bool useDbConnection = methodGenerationContext.UseDbConnection;
        var requireDbCommandParameters = isScalarType || useDbConnection || IsTuple(itemType) || IsScalarType(itemType);
        if (requireDbCommandParameters)
        {
            if (!hasCustomSql)
            {
                source.AppendLine($@"command.CommandText = sqlQuery;");
            }
            else
            {
                source.AppendLine($@"command.CommandText = {methodGenerationContext.CustomSqlParameter!.Name};");
            }

            if (parameters.Length > 0)
            {
                source.AppendLine($@"command.Parameters.AddRange(parameters);");
            }
        }

        if (isScalarType)
        {
            if (useDbConnection)
            {
                ExecuteSimpleQuery(source, methodGenerationContext, hasNullableAnnotations, isTask, returnType);
            }
            else
            {
                source.Append($@"{this.GetOpenConnectionStatement(methodGenerationContext)}
            try
            {{
");
                source.PushIndent();
                ExecuteSimpleQuery(source, methodGenerationContext, hasNullableAnnotations, isTask, returnType);

                source.PopIndent();
                source.Append($@"}}
            finally
            {{
                {this.GetCloseConnectionStatement(methodGenerationContext)}
            }}
");
            }
        }
        else
        {
            if (!methodGenerationContext.UseDbConnection && (isList && (IsTuple(itemType) || IsScalarType(itemType))))
            {
                source.Append($@"{this.GetOpenConnectionStatement(methodGenerationContext)}
            try
            {{
");
                source.PushIndent();
                this.MapResults(source, methodGenerationContext, methodSymbol, parameters, itemType, hasNullableAnnotations, isList, isTask);

                MarshalOutputParameters(source, parameters, hasNullableAnnotations);
                source.AppendLine(ReturnStatement(IdentifierName("result")).NormalizeWhitespace().ToFullString());
                source.PopIndent();
                source.Append($@"}}
            finally
            {{
                {this.GetCloseConnectionStatement(methodGenerationContext)}
            }}
");
            }
            else
            {
                this.MapResults(source, methodGenerationContext, methodSymbol, parameters, itemType, hasNullableAnnotations, isList, isTask);
                MarshalOutputParameters(source, parameters, hasNullableAnnotations);
                source.AppendLine(ReturnStatement(IdentifierName("result")).NormalizeWhitespace().ToFullString());
            }
        }

        source.PopIndent();
        source.PopIndent();
        source.PopIndent();
        source.AppendLine($@"        }}");
    }

    internal class SyntaxReceiver : ISyntaxContextReceiver
    {
        public List<IMethodSymbol> Methods { get; } = new List<IMethodSymbol>();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            // any field with at least one attribute is a candidate for property generation
            if (context.Node is MethodDeclarationSyntax methodDeclarationSyntax
                && methodDeclarationSyntax.AttributeLists.Count > 0)
            {
                // Get the symbol being declared by the field, and keep it if its annotated
                IMethodSymbol? methodSymbol = context.SemanticModel.GetDeclaredSymbol(context.Node) as IMethodSymbol;
                if (methodSymbol == null)
                {
                    return;
                }

                if (methodSymbol.GetAttributes().Any(ad => ad.AttributeClass?.ToDisplayString() == "SqlMarshalAttribute"))
                {
                    this.Methods.Add(methodSymbol);
                }
            }
        }
    }
}
