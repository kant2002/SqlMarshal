// -----------------------------------------------------------------------
// <copyright file="DbContextManager.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal.CompilationTests;

using System.Collections.Generic;
using System.Threading.Tasks;

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

    [SqlMarshal("persons_list")]
    public partial Task<IList<PersonDbContext.Person>> GetResultAsync();

    [SqlMarshal("persons_list")]
    public partial Task<IList<(int Id, string Name)>> GetTupleResultAsync();

    [SqlMarshal("persons_list")]
    public partial Task<PersonDbContext.Person?> GetFirstOrDefaultAsync();
}
