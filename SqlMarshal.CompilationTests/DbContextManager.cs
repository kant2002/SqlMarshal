// -----------------------------------------------------------------------
// <copyright file="DbContextManager.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal.CompilationTests;

using System.Collections.Generic;

internal partial class DbContextManager
{
    private readonly PersonDbContext context;

    public DbContextManager(PersonDbContext context)
    {
        this.context = context;
    }

    [SqlMarshal("persons_list")]
    public partial IList<PersonDbContext.Person> GetResult();

    [SqlMarshal("persons_list")]
    public partial IList<(int Id, string Name)> GetTupleResult();
}
