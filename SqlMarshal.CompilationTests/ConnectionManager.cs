// -----------------------------------------------------------------------
// <copyright file="ConnectionManager.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal.CompilationTests
{
    using System.Collections.Generic;
    using Microsoft.Data.SqlClient;

    internal partial class ConnectionManager
    {
        private readonly SqlConnection connection;

        public ConnectionManager(SqlConnection connection)
        {
            this.connection = connection;
        }

        [StoredProcedureGenerated("persons_list")]
        public partial IList<PersonInformation> GetResult();

        [StoredProcedureGenerated("persons_by_page")]
        public partial IList<PersonInformation> GetResultByPage(int pageNo, out int totalCount);

        [StoredProcedureGenerated("")]
        public partial IList<PersonInformation> GetResultFromSql([CustomSql]string sql, int maxId);
    }
}
