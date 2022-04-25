// -----------------------------------------------------------------------
// <copyright file="PersonDbContext.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal.CompilationTests
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.EntityFrameworkCore;

    internal class PersonDbContext : DbContext
    {
        public PersonDbContext(DbContextOptions<PersonDbContext> options)
            : base(options)
        {
        }

        public DbSet<Person> Persons { get; set; } = null!;

        internal class Person
        {
            [Column("person_id")]
            public int PersonId { get; set; }

            [Column("person_name")]
            public string? PersonName { get; set; }
        }
    }
}
