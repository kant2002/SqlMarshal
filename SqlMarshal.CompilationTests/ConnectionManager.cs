// -----------------------------------------------------------------------
// <copyright file="ConnectionManager.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal.CompilationTests
{
    using System.Collections.Generic;
    using System.Data.Common;

    internal partial class ConnectionManager
    {
        private readonly DbConnection connection;

        public ConnectionManager(DbConnection connection)
        {
            this.connection = connection;
        }

        [StoredProcedureGenerated("persons_list")]
        public partial IList<PersonInformation> GetResult();

        [StoredProcedureGenerated("persons_by_page")]
        public partial IList<PersonInformation> GetResultByPage(int pageNo, out int totalCount);
    }
}
