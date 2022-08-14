// -----------------------------------------------------------------------
// <copyright file="ConnectionManager.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal.CompilationTests;

using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.SqlClient;

internal partial class ConnectionManager
{
    private readonly DbConnection connection;

    public ConnectionManager(SqlConnection connection)
    {
        this.connection = connection;
    }

    [SqlMarshal("persons_list")]
    public partial IList<PersonInformation> GetResult();

    [SqlMarshal("persons_list")]
    public partial IList<(int Id, string Name)> GetTupleResult();

    [SqlMarshal("persons_by_page")]
    public partial IList<PersonInformation> GetResultByPage(int pageNo, out int totalCount);

    [SqlMarshal("")]
    public partial IList<PersonInformation> GetResultFromSql([RawSql]string sql, int maxId);

    [SqlMarshal("persons_by_id")]
    public partial PersonInformation GetPersonById(int personId);

    [SqlMarshal("persons_list")]
    public partial IList<PersonInformation> GetResult(DbTransaction tran);
}
