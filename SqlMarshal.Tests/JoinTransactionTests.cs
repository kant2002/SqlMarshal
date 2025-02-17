﻿// -----------------------------------------------------------------------
// <copyright file="JoinTransactionTests.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal.Tests;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class JoinTransactionTests : CodeGenerationTestBase
{
    [TestMethod]
    public void DbConectionCanJoinTransactions()
    {
        string source = @"
namespace Foo
{
    class C
    {
        private DbConnection connection;

        [SqlMarshal(""sp_TestSP"")]
        public partial int M(DbTransaction transaction, int clientId, string? personId);
    }
}";
        string output = GetCSharpGeneratedOutput(source, NullableContextOptions.Disable);

        Assert.IsNotNull(output);

        var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;

    partial class C
    {
        public partial int M(DbTransaction transaction, int clientId, string? personId)
        {
            var connection = this.connection;
            using var command = connection.CreateCommand();

            var clientIdParameter = command.CreateParameter();
            clientIdParameter.ParameterName = ""@client_id"";
            clientIdParameter.Value = clientId;

            var personIdParameter = command.CreateParameter();
            personIdParameter.ParameterName = ""@person_id"";
            personIdParameter.Value = personId == null ? (object)DBNull.Value : personId;

            var parameters = new DbParameter[]
            {
                clientIdParameter,
                personIdParameter,
            };

            var sqlQuery = @""sp_TestSP @client_id, @person_id"";
            command.CommandText = sqlQuery;
            command.Parameters.AddRange(parameters);
            command.Transaction = transaction;
            var __result = command.ExecuteScalar();
            return (int)__result!;
        }
    }
}";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void DbContextCanJoinTransactions()
    {
        string source = @"
namespace Foo
{
    class C
    {
        [SqlMarshal(""sp_TestSP"")]
        public partial IList<Item> M(DbTransaction transaction)
    }
}";
        string output = GetCSharpGeneratedOutput(source, NullableContextOptions.Disable);

        Assert.IsNotNull(output);

        var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;

    partial class C
    {
        public partial IList<Item> M(DbTransaction transaction)
        {
            var connection = this.dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            var sqlQuery = @""sp_TestSP"";
            this.dbContext.Database.UseTransaction(transaction);
            var __result = this.dbContext.Items.FromSqlRaw(sqlQuery).ToList();
            return __result;
        }
    }
}";
        Assert.AreEqual(expectedOutput, output);
    }
}
