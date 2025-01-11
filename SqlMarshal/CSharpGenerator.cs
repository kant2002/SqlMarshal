﻿// -----------------------------------------------------------------------
// <copyright file="CSharpGenerator.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal;

using Microsoft.CodeAnalysis;

/// <summary>
/// Stored procedures generator for C#.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class CSharpGenerator : AbstractGenerator
{
    private const string CSharpAttributeSource = @"// <auto-generated>
// Code generated by SqlMarshal Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable

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

[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple=false)]
internal sealed class RepositoryAttribute: System.Attribute
{
    public RepositoryAttribute(global::System.Type entityType)
    {
        EntityType = entityType;
    }

    public global::System.Type EntityType { get; }
}
";

    /// <inheritdoc/>
    public override void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization((pi) =>
        {
            pi.AddSource("SqlMarshalAttribute.cs", CSharpAttributeSource);
        });
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }
}
