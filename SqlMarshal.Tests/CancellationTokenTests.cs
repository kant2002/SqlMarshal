// -----------------------------------------------------------------------
// <copyright file="CancellationTokenTests.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal.Tests;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

[TestClass]
public class CancellationTokenTests : CodeGenerationTestBase
{
    [TestMethod]
    public async Task ScalarResult()
    {
        string source = @"
namespace Foo
{
    using System.Threading;

    class C
    {
        private DbConnection connection;

        [SqlMarshal(""sp_TestSP"")]
        public partial Task<int> M(int clientId, string? personId, CancellationToken cancellationToken);
    }
}";
        await VerifyCSharp(source, NullableContextOptions.Disable);
    }

    [TestMethod]
    public async Task NoResults()
    {
        string source = @"
namespace Foo
{
    class C
    {
        private DbConnection connection;

        [SqlMarshal(""sp_TestSP"")]
        public partial Task M(int clientId, string? personId, CancellationToken cancellationToken);
    }
}";
        await VerifyCSharp(source, NullableContextOptions.Disable);
    }

    [TestMethod]
    public async Task MapResultSetToProcedure()
    {
        string source = @"
namespace Foo
{
    public class Item
    {
        public string StringValue { get; set; }
        public int Int32Value { get; set; }
        public int? NullableInt32Value { get; set; }
    }

    class C
    {
        private DbConnection connection;

        [SqlMarshal(""sp_TestSP"")]
        public partial Task<IList<Item>> M(CancellationToken cancellationToken)
    }
}";
        await VerifyCSharp(source, NullableContextOptions.Disable);
    }

    [TestMethod]
    public async Task MapListFromDbContext()
    {
        string source = @"
namespace Foo
{
    class C
    {
        [SqlMarshal(""sp_TestSP"")]
        public partial Task<IList<Item>> M(int clientId, out int? personId, CancellationToken cancellationToken)
    }
}";
        await VerifyCSharp(source, NullableContextOptions.Disable);
    }
}
