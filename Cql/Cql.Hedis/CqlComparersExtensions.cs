/*
 * Copyright (c) 2023, NCQA and contributors
 * See the file CONTRIBUTORS for details.
 *
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-cql-sdk/main/LICENSE
 */

using Hl7.Cql.Comparers;
using Hl7.Cql.Hedis.Comparers;
using Ncqa.Hedis.Core;

namespace Hl7.Cql.Hedis;

/// <summary>
/// Extension methods for registering HEDIS-specific comparers.
/// </summary>
internal static class CqlComparersExtensions
{
    /// <summary>
    /// Adds HEDIS DTO comparers to the CqlComparers instance.
    /// </summary>
    public static CqlComparers AddHedisComparers(this CqlComparers comparers)
    {
        comparers.Register(typeof(Coding), new CodingComparer());
        comparers.Register(typeof(CodeableConcept), new CodeableConceptComparer());
        comparers.Register(typeof(Identifier), new IdentifierComparer());

        return comparers;
    }
}
