// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal.CompilationTests
{
    using System;
    using System.Linq;
    using Microsoft.Data.SqlClient;
    using static System.Console;

    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("SqlMarshal sample application!");

            using var sqlConnection = new SqlConnection("server=(localdb)\\mssqllocaldb;database=sqlmarshal_sample;integrated security=true");
            var connectionManager = new ConnectionManager(sqlConnection);
            try
            {
                sqlConnection.Open();
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
            }
            catch (SqlException ex)
            {
                WriteLine("SQL Exception happens. Create sqlmarshal_sample database on the LocalDB instance.");
                WriteLine(ex.Message);
            }
        }

        private static void WritePerson(PersonInformation personInfo)
        {
            WriteLine($"Name: {personInfo.PersonName} (#{personInfo.PersonId})");
        }
    }
}
