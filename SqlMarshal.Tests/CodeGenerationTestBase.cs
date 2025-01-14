// -----------------------------------------------------------------------
// <copyright file="CodeGenerationTestBase.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal.Tests;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyMSTest;

public class CodeGenerationTestBase : VerifyBase
{
    static CodeGenerationTestBase()
    {
        // To disable Visual Studio popping up on every test execution. Also make tests work on .NET 6
        Environment.SetEnvironmentVariable("DiffEngine_Disabled", "true");
        Environment.SetEnvironmentVariable("Verify_DisableClipboard", "true");
    }

    protected static Task VerifyCSharp(string source, NullableContextOptions nullableContextOptions)
    {
        return Verifier.Verify(GetCSharpGeneratedOutput(source, nullableContextOptions)).UseDirectory("Snapshots");
    }

    protected static string GetCSharpGeneratedOutput(string source, NullableContextOptions nullableContextOptions)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText("using SqlMarshal.Annotations;\r\n" + source);

        var references = new List<MetadataReference>();
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            if (!assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            {
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

        references.Add(MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.Schema.ColumnAttribute).Assembly.Location));

        var compilation = CSharpCompilation.Create(
            "foo",
            new SyntaxTree[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: nullableContextOptions, usings: new[] { "SqlMarshal.Annotations" }));

        // var compileDiagnostics = compilation.GetDiagnostics();
        // Assert.IsFalse(compileDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + compileDiagnostics.FirstOrDefault()?.GetMessage());
        ISourceGenerator generator = new CSharpGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);
        Assert.IsFalse(generateDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + generateDiagnostics.FirstOrDefault()?.GetMessage());

        string output = outputCompilation.SyntaxTrees.Last().ToString();

        Console.WriteLine(output);

        return output;
    }

    protected static string GetVisualBasicGeneratedOutput(string source)
    {
        var syntaxTree = VisualBasicSyntaxTree.ParseText("Imports SqlMarshal.Annotations\r\n" + source);

        var references = new List<MetadataReference>();
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            if (!assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            {
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

        references.Add(MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.Schema.ColumnAttribute).Assembly.Location));

        var compilation = VisualBasicCompilation.Create(
            "foo",
            new SyntaxTree[] { syntaxTree },
            references,
            new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // var compileDiagnostics = compilation.GetDiagnostics();
        // Assert.IsFalse(compileDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + compileDiagnostics.FirstOrDefault()?.GetMessage());
        ISourceGenerator generator = new VisualBasicGenerator();

        var driver = VisualBasicGeneratorDriver.Create(ImmutableArray.Create(generator));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);
        Assert.IsFalse(generateDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + generateDiagnostics.FirstOrDefault()?.GetMessage());

        string output = outputCompilation.SyntaxTrees.Last().ToString();

        Console.WriteLine(output);

        return output;
    }
}
