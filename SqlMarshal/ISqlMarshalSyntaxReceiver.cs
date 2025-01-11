// -----------------------------------------------------------------------
// <copyright file="ISqlMarshalSyntaxReceiver.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

/// <summary>
/// Language specific interface for syntax context receiver which is used to collect information about methods.
/// </summary>
internal interface ISqlMarshalSyntaxReceiver : ISyntaxContextReceiver
{
    /// <summary>
    /// Gets list of collected methods.
    /// </summary>
    List<IMethodSymbol> Methods { get; }
}
