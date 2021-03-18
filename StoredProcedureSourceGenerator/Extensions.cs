// -----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace StoredProcedureSourceGenerator
{
    using Microsoft.CodeAnalysis;

    internal static class Extensions
    {
        public static bool CanHaveNullValue(this ITypeSymbol typeSymbol, bool hasNullableAnnotations)
        {
            var requireParameterNullCheck = !hasNullableAnnotations
                && !(typeSymbol.IsValueType
                    && typeSymbol.NullableAnnotation == NullableAnnotation.NotAnnotated);
            return requireParameterNullCheck;
        }
    }
}
