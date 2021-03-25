// -----------------------------------------------------------------------
// <copyright file="NameMapper.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal
{
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides mapping name support between parameter names in C# to Stored procedures parameter names.
    /// </summary>
    public static class NameMapper
    {
        /// <summary>
        /// Maps parameter name to the stored procedure name.
        /// </summary>
        /// <param name="parameterName">Name of the parameter to map.</param>
        /// <returns>Corresponding stored procedure parameter name.</returns>
        public static string MapName(string parameterName)
        {
            var firstname = Regex.Match(parameterName, "[^A-Z]*").Value;
            var matches = Regex.Matches(parameterName, "[A-Z][^A-Z]*").Cast<Match>().Select(_ => _.Value.ToLower());
            return string.Join("_", new string[] { firstname }.Union(matches));
        }
    }
}
