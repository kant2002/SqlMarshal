// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal.CompilationTests;

using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using static System.Console;
using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;

internal class Program
{
    private const string ConnectionString = "server=(localdb)\\mssqllocaldb;database=sqlmarshal_sample;integrated security=true";

    private static async Task Main(string[] args)
    {
        Console.WriteLine("SqlMarshal sample application!");

        TestSqlClient();
        TestDbContext();
        await TestDbContextAsync();
    }

    private static void TestSqlClient()
    {
        Console.WriteLine("**** Testing Sql client! ****");

        using var sqlConnection = new SqlConnection(ConnectionString);
        var connectionManager = new ConnectionManager(sqlConnection);
        try
        {
            sqlConnection.Open();
            Console.WriteLine("**** Testing Connection Manager ! ****");
            TestConnectionManager(connectionManager);
            Console.WriteLine("**** Testing Extension methods ! ****");
            TestExtensionMethods(sqlConnection);
            Console.WriteLine("**** Testing Data Reader methods ! ****");
            TestDataReaderMethods(sqlConnection);
        }
        catch (DbException ex)
        {
            WriteLine("SQL Exception happens. Create sqlmarshal_sample database on the LocalDB instance.");
            WriteLine(ex.Message);
        }
    }

    private static void TestExtensionMethods(DbConnection sqlConnection)
    {
        var persons = sqlConnection.GetResult();
        WriteLine("Print first 10 rows from persons_list SP");
        foreach (var personInfo in persons.Take(10))
        {
            WritePerson(personInfo);
        }
    }

    private static void TestDataReaderMethods(DbConnection sqlConnection)
    {
        using var reader = sqlConnection.GetResultReader();
        WriteLine("Print first 10 rows from persons_list SP");
        int i = 0;
        while (reader.Read() && i < 10)
        {
            var person = new PersonInformation()
            {
                PersonId = reader.GetInt32(0),
                PersonName = reader.GetString(1),
            };
            WritePerson(person);
            i++;
        }
    }

    private static void TestConnectionManager(ConnectionManager connectionManager)
    {
        var persons = connectionManager.GetResult();
        WriteLine("Print first 10 rows from persons_list SP");
        foreach (var personInfo in persons.Take(10))
        {
            WritePerson(personInfo);
        }

        var persons2 = connectionManager.GetResultByPage(2, out var totalCount);
        WriteLine("Print results of persons_by_page SP");
        foreach (var personInfo in persons2)
        {
            WritePerson(personInfo);
        }

        WriteLine($"Total count of persons: {totalCount}");

        var persons3 = connectionManager.GetResultFromSql(
            "SELECT * FROM person WHERE person_id < @max_id",
            2);
        WriteLine("Print results of SQL");
        foreach (var personInfo in persons3)
        {
            WritePerson(personInfo);
        }

        var persons4 = connectionManager.GetTupleResult();
        WriteLine("Print first 10 rows from persons_list SP using tuples");
        foreach (var personInfo in persons4.Take(10))
        {
            WriteLine($"Name: {personInfo.Name} (#{personInfo.Id})");
        }
    }

    private static void TestDbContext()
    {
        Console.WriteLine("**** Testing Db Context! ****");
        var options = new DbContextOptionsBuilder<PersonDbContext>().UseSqlServer(ConnectionString).Options;
        PersonDbContext context = new PersonDbContext(options);
        var manager = new DbContextManager(context);

        var persons = manager.GetResult();
        WriteLine("Print first 10 rows from persons_list SP");
        foreach (var personInfo in persons.Take(10))
        {
            WritePerson(personInfo);
        }

        var persons4 = manager.GetTupleResult();
        WriteLine("Print first 10 rows from persons_list SP using tuples");
        foreach (var personInfo in persons4.Take(10))
        {
            WriteLine($"Name: {personInfo.Name} (#{personInfo.Id})");
        }

        Console.WriteLine("**** Testing Transactions methods ! ****");
        using (var transaction = context.Database.BeginTransaction())
        {
            var persons5 = manager.GetTupleResult();
            WriteLine("Print first 10 rows from persons_list SP using tuples");
            foreach (var personInfo in persons5.Take(10))
            {
                WriteLine($"Name: {personInfo.Name} (#{personInfo.Id})");
            }

            transaction.Commit();
        }
    }

    private static async Task TestDbContextAsync()
    {
        Console.WriteLine("**** Testing Db Context Async! ****");
        var options = new DbContextOptionsBuilder<PersonDbContext>().UseSqlServer(ConnectionString).Options;
        PersonDbContext context = new PersonDbContext(options);
        var manager = new DbContextManager(context);

        var persons = await manager.GetResultAsync();
        WriteLine("Print first 10 rows from persons_list SP");
        foreach (var personInfo in persons.Take(10))
        {
            WritePerson(personInfo);
        }

        var persons4 = await manager.GetTupleResultAsync();
        WriteLine("Print first 10 rows from persons_list SP using tuples");
        foreach (var personInfo in persons4.Take(10))
        {
            WriteLine($"Name: {personInfo.Name} (#{personInfo.Id})");
        }

        Console.WriteLine("**** Testing Transactions methods ! ****");
        using (var transaction = context.Database.BeginTransaction())
        {
            var persons5 = await manager.GetTupleResultAsync();
            WriteLine("Print first 10 rows from persons_list SP using tuples");
            foreach (var personInfo in persons5.Take(10))
            {
                WriteLine($"Name: {personInfo.Name} (#{personInfo.Id})");
            }

            transaction.Commit();
        }
    }

    private static void WritePerson(PersonInformation personInfo)
    {
        WriteLine($"Name: {personInfo.PersonName} (#{personInfo.PersonId})");
    }

    private static void WritePerson(PersonDbContext.Person personInfo)
    {
        WriteLine($"Name: {personInfo.PersonName} (#{personInfo.PersonId})");
    }
}
