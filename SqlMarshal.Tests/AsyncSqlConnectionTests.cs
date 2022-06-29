﻿// -----------------------------------------------------------------------
// <copyright file="AsyncSqlConnectionTests.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal.Tests;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class AsyncSqlConnectionTests : CodeGenerationTestBase
{
    [TestMethod]
    public void ScalarResult()
    {
        string source = @"
namespace Foo
{
    class C
    {
        private DbConnection connection;

        [SqlMarshal(""sp_TestSP"")]
        public partial Task<int> M(int clientId, string? personId);
    }
}";
        string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

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
        public partial async Task<int> M(int clientId, string? personId)
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
            var result = await command.ExecuteScalarAsync().ConfigureAwait(false);
            return (int)result!;
        }
    }
}";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void ScalarResultWithIntOutput()
    {
        string source = @"
namespace Foo
{
    class C
    {
        private DbConnection connection;

        [SqlMarshal(""sp_TestSP"")]
        public partial Task<int> M(int clientId, out int personId);
    }
}";
        string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

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
        public partial async Task<int> M(int clientId, out int personId)
        {
            var connection = this.connection;
            using var command = connection.CreateCommand();

            var clientIdParameter = command.CreateParameter();
            clientIdParameter.ParameterName = ""@client_id"";
            clientIdParameter.Value = clientId;

            var personIdParameter = command.CreateParameter();
            personIdParameter.ParameterName = ""@person_id"";
            personIdParameter.DbType = System.Data.DbType.Int32;
            personIdParameter.Direction = System.Data.ParameterDirection.Output;

            var parameters = new DbParameter[]
            {
                clientIdParameter,
                personIdParameter,
            };

            var sqlQuery = @""sp_TestSP @client_id, @person_id OUTPUT"";
            command.CommandText = sqlQuery;
            command.Parameters.AddRange(parameters);
            var result = await command.ExecuteScalarAsync().ConfigureAwait(false);
            personId = (int)personIdParameter.Value;
            return (int)result!;
        }
    }
}";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void MapResultSetToProcedure()
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
        public partial Task<IList<Item>> M()
    }
}";
        string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

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
        public partial async Task<IList<Foo.Item>> M()
        {
            var connection = this.connection;
            using var command = connection.CreateCommand();

            var sqlQuery = @""sp_TestSP"";
            command.CommandText = sqlQuery;
            using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            var result = new List<Item>();
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var item = new Item();
                var value_0 = reader.GetValue(0);
                item.StringValue = value_0 == DBNull.Value ? (string?)null : (string)value_0;
                var value_1 = reader.GetValue(1);
                item.Int32Value = (int)value_1;
                var value_2 = reader.GetValue(2);
                item.NullableInt32Value = value_2 == DBNull.Value ? (int?)null : (int)value_2;
                result.Add(item);
            }

            await reader.CloseAsync().ConfigureAwait(false);
            return result;
        }
    }
}";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void MapSingleObjectToProcedureFromDbContext()
    {
        string source = @"
namespace Foo
{
    class C
    {
        [SqlMarshal(""sp_TestSP"")]
        public partial Task<Item> M()
    }
}";
        string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

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
        public partial async Task<Item?> M()
        {
            var connection = this.dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            var sqlQuery = @""sp_TestSP"";
            Item result = null!;
            var asyncEnumerable = this.dbContext.Items.FromSqlRaw(sqlQuery).AsAsyncEnumerable();
            await foreach (var current in asyncEnumerable)
            {
                result = current;
                break;
            }
            return result;
        }
    }
}";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void MapSingleObjectToProcedureConnection()
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
        public partial Task<Item> M()
    }
}";
        string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

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
        public partial async Task<Foo.Item?> M()
        {
            var connection = this.connection;
            using var command = connection.CreateCommand();

            var sqlQuery = @""sp_TestSP"";
            command.CommandText = sqlQuery;
            using var reader = await command.ExecuteReaderAsync(System.Data.CommandBehavior.SingleResult | System.Data.CommandBehavior.SingleRow).ConfigureAwait(false);
            if (!(await reader.ReadAsync().ConfigureAwait(false)))
            {
                return null;
            }

            var result = new Item();
            var value_0 = reader.GetValue(0);
            result.StringValue = value_0 == DBNull.Value ? (string?)null : (string)value_0;
            var value_1 = reader.GetValue(1);
            result.Int32Value = (int)value_1;
            var value_2 = reader.GetValue(2);
            result.NullableInt32Value = value_2 == DBNull.Value ? (int?)null : (int)value_2;
            await reader.CloseAsync().ConfigureAwait(false);
            return result;
        }
    }
}";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void MapListFromDbContext()
    {
        string source = @"
namespace Foo
{
    class C
    {
        [SqlMarshal(""sp_TestSP"")]
        public partial Task<IList<Item>> M(int clientId, out int? personId)
    }
}";
        string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

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
        public partial async Task<IList<Item>> M(int clientId, out int? personId)
        {
            var connection = this.dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            var clientIdParameter = command.CreateParameter();
            clientIdParameter.ParameterName = ""@client_id"";
            clientIdParameter.Value = clientId;

            var personIdParameter = command.CreateParameter();
            personIdParameter.ParameterName = ""@person_id"";
            personIdParameter.DbType = System.Data.DbType.Int32;
            personIdParameter.Direction = System.Data.ParameterDirection.Output;

            var parameters = new DbParameter[]
            {
                clientIdParameter,
                personIdParameter,
            };

            var sqlQuery = @""sp_TestSP @client_id, @person_id OUTPUT"";
            var result = await this.dbContext.Items.FromSqlRaw(sqlQuery, parameters).ToListAsync().ConfigureAwait(false);
            personId = personIdParameter.Value == DBNull.Value ? (int?)null : (int)personIdParameter.Value;
            return result;
        }
    }
}";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void NoResults()
    {
        string source = @"
namespace Foo
{
    class C
    {
        private DbConnection connection;

        [SqlMarshal(""sp_TestSP"")]
        public partial Task M(int clientId, string? personId);
    }
}";
        string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

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
        public partial async Task M(int clientId, string? personId)
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
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}";
        Assert.AreEqual(expectedOutput, output);
    }
}
