// -----------------------------------------------------------------------
// <copyright file="ConnectionExtensions.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal.CompilationTests
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;

    internal static partial class ConnectionExtensions
    {
        [SqlMarshal("persons_list")]
        public static partial IList<PersonInformation> GetResult(this DbConnection connection);

        [SqlMarshal("persons_list")]
        public static partial Task<IList<PersonInformation>> GetResultAsync(this DbConnection connection, CancellationToken cancellationToken);

        [SqlMarshal("persons_by_page")]
        public static partial IList<PersonInformation> GetResultByPage(this DbConnection connection, int pageNo, out int totalCount);

        [SqlMarshal("persons_list")]
        public static partial DbDataReader GetResultReader(this DbConnection connection);
    }
}
