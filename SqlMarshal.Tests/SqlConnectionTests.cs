﻿// -----------------------------------------------------------------------
// <copyright file="SqlConnectionTests.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal.Tests;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class SqlConnectionTests : CodeGenerationTestBase
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
        public partial int M(int clientId, string? personId);
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
        public partial int M(int clientId, string? personId)
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
            var result = command.ExecuteScalar();
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
        public partial int M(int clientId, out int personId);
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
        public partial int M(int clientId, out int personId)
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
            var result = command.ExecuteScalar();
            personId = (int)personIdParameter.Value;
            return (int)result!;
        }
    }
}";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void ScalarResultWithIntOutputAndNullability()
    {
        string source = @"
namespace Foo
{
    class C
    {
        private DbConnection connection;

        [SqlMarshal(""sp_TestSP"")]
        public partial int M(int clientId, out int personId);
    }
}";
        string output = this.GetGeneratedOutput(source, NullableContextOptions.Enable);

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
        public partial int M(int clientId, out int personId)
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
            var result = command.ExecuteScalar();
            personId = (int)personIdParameter.Value!;
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
        public partial IList<Item> M()
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
        public partial IList<Foo.Item> M()
        {
            var connection = this.connection;
            using var command = connection.CreateCommand();

            var sqlQuery = @""sp_TestSP"";
            command.CommandText = sqlQuery;
            using var reader = command.ExecuteReader();
            var result = new List<Item>();
            while (reader.Read())
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

            reader.Close();
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
        public partial Item M()
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
        public partial Foo.Item? M()
        {
            var connection = this.connection;
            using var command = connection.CreateCommand();

            var sqlQuery = @""sp_TestSP"";
            command.CommandText = sqlQuery;
            using var reader = command.ExecuteReader(System.Data.CommandBehavior.SingleResult | System.Data.CommandBehavior.SingleRow);
            if (!reader.Read())
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
            reader.Close();
            return result;
        }
    }
}";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void MapSingleObjectToProcedureConnectionWithNullability()
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
        public partial Item? M()
    }
}";
        string output = this.GetGeneratedOutput(source, NullableContextOptions.Enable);

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
        public partial Foo.Item? M()
        {
            var connection = this.connection;
            using var command = connection.CreateCommand();

            var sqlQuery = @""sp_TestSP"";
            command.CommandText = sqlQuery;
            using var reader = command.ExecuteReader(System.Data.CommandBehavior.SingleResult | System.Data.CommandBehavior.SingleRow);
            if (!reader.Read())
            {
                return null;
            }

            var result = new Item();
            var value_0 = reader.GetValue(0);
            result.StringValue = (string)value_0;
            var value_1 = reader.GetValue(1);
            result.Int32Value = (int)value_1;
            var value_2 = reader.GetValue(2);
            result.NullableInt32Value = value_2 == DBNull.Value ? (int?)null : (int)value_2;
            reader.Close();
            return result;
        }
    }
}";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void MapSingleObjectToProcedureConnectionWithNullabilityAndNonNullReturnType()
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
        public partial Item M()
    }
}";
        string output = this.GetGeneratedOutput(source, NullableContextOptions.Enable);

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
        public partial Foo.Item M()
        {
            var connection = this.connection;
            using var command = connection.CreateCommand();

            var sqlQuery = @""sp_TestSP"";
            command.CommandText = sqlQuery;
            using var reader = command.ExecuteReader(System.Data.CommandBehavior.SingleResult | System.Data.CommandBehavior.SingleRow);
            if (!reader.Read())
            {
                throw new InvalidOperation(""No data returned from command."");
            }

            var result = new Item();
            var value_0 = reader.GetValue(0);
            result.StringValue = (string)value_0;
            var value_1 = reader.GetValue(1);
            result.Int32Value = (int)value_1;
            var value_2 = reader.GetValue(2);
            result.NullableInt32Value = value_2 == DBNull.Value ? (int?)null : (int)value_2;
            reader.Close();
            return result;
        }
    }
}";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void MapResultSetToProcedureWithNullability()
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
        public partial IList<Item> M()
    }
}";
        string output = this.GetGeneratedOutput(source, NullableContextOptions.Enable);

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
        public partial IList<Foo.Item> M()
        {
            var connection = this.connection;
            using var command = connection.CreateCommand();

            var sqlQuery = @""sp_TestSP"";
            command.CommandText = sqlQuery;
            using var reader = command.ExecuteReader();
            var result = new List<Item>();
            while (reader.Read())
            {
                var item = new Item();
                var value_0 = reader.GetValue(0);
                item.StringValue = (string)value_0;
                var value_1 = reader.GetValue(1);
                item.Int32Value = (int)value_1;
                var value_2 = reader.GetValue(2);
                item.NullableInt32Value = value_2 == DBNull.Value ? (int?)null : (int)value_2;
                result.Add(item);
            }

            reader.Close();
            return result;
        }
    }
}";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void SqlConnectionFromBaseClass()
    {
        string source = @"
namespace Foo
{
    class A
    {
        internal DbConnection connection;
    }

    class C : A
    {
        [SqlMarshal(""sp_TestSP"")]
        private partial int M(int clientId, string? personId);
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
        private partial int M(int clientId, string? personId)
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
            var result = command.ExecuteScalar();
            return (int)result!;
        }
    }
}";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void SqlConnectionFound()
    {
        string source = @"
namespace Foo
{
    class SqlConnection: DbConnection {}

    class C
    {
        internal SqlConnection connection;

        [SqlMarshal(""sp_TestSP"")]
        private partial int M(int clientId, string? personId);
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
        private partial int M(int clientId, string? personId)
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
            var result = command.ExecuteScalar();
            return (int)result!;
        }
    }
}";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void MapPrimitiveSequenceToProcedureWithNullability()
    {
        string source = @"
namespace Foo
{
    class C
    {
        private DbConnection connection;

        [SqlMarshal(""sp_TestSP"")]
        public partial IList<string> M()
    }
}";
        string output = this.GetGeneratedOutput(source, NullableContextOptions.Enable);

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
        public partial IList<string> M()
        {
            var connection = this.connection;
            using var command = connection.CreateCommand();

            var sqlQuery = @""sp_TestSP"";
            command.CommandText = sqlQuery;
            using var reader = command.ExecuteReader();
            var result = new List<String>();
            while (reader.Read())
            {
                var value_0 = reader.GetValue(0);
                var item = (string)value_0;
                result.Add(item);
            }

            reader.Close();
            return result;
        }
    }
}";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void MapTupleSequenceToProcedureWithNullability()
    {
        string source = @"
namespace Foo
{
    class C
    {
        private DbConnection connection;

        [SqlMarshal(""sp_TestSP"")]
        public partial IList<(string,int)> M()
    }
}";
        string output = this.GetGeneratedOutput(source, NullableContextOptions.Enable);

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
        public partial IList<(string, int)> M()
        {
            var connection = this.connection;
            using var command = connection.CreateCommand();

            var sqlQuery = @""sp_TestSP"";
            command.CommandText = sqlQuery;
            using var reader = command.ExecuteReader();
            var result = new List<(string, int)>();
            while (reader.Read())
            {
                var value_0 = reader.GetValue(0);
                var value_1 = reader.GetValue(1);
                result.Add((
                    (string)value_0,
                    (int)value_1
                ));
            }

            reader.Close();
            return result;
        }
    }
}";
        Assert.AreEqual(expectedOutput, output);
    }
}
