// -----------------------------------------------------------------------
// <copyright file="ConnectionExtensions.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal.CompilationTests
{
    using System.Collections.Generic;
    using System.Data.Common;

    internal static partial class ConnectionExtensions
    {
        [SqlMarshal("persons_list")]
        public static partial IList<PersonInformation> GetResult(this DbConnection connection);

        [SqlMarshal("persons_by_page")]
        public static partial IList<PersonInformation> GetResultByPage(this DbConnection connection, int pageNo, out int totalCount);
    }
}
