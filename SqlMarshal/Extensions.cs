// -----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal
{
    using Microsoft.CodeAnalysis;

    internal static class Extensions
    {
        public static bool CanHaveNullValue(this ITypeSymbol typeSymbol, bool hasNullableAnnotations)
        {
            if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                return true;
            }

            var requireParameterNullCheck = !hasNullableAnnotations && !typeSymbol.IsValueType;
            return requireParameterNullCheck;
        }
    }
}
