/*
 * Copyright (c) 2023, NCQA and contributors
 * See the file CONTRIBUTORS for details.
 *
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-cql-sdk/main/LICENSE
 */

using Hl7.Cql.Abstractions;
using Hl7.Cql.Comparers;
using Ncqa.Hedis.Core;

namespace Hl7.Cql.Hedis.Comparers;

/// <summary>
/// Compares <see cref="CodeableConcept"/> instances for CQL operations.
/// </summary>
internal class CodeableConceptComparer : CqlComparerBase<CodeableConcept>
{
    private readonly CodingComparer _codingComparer = new();

    public override int? Compare(CodeableConcept? x, CodeableConcept? y, string? precision)
    {
        if (x == null || y == null) return null;

        // Compare by first coding if available
        var xCoding = x.Coding?.FirstOrDefault();
        var yCoding = y.Coding?.FirstOrDefault();

        if (xCoding == null && yCoding == null) return 0;
        if (xCoding == null) return -1;
        if (yCoding == null) return 1;

        return _codingComparer.Compare(xCoding, yCoding, precision);
    }

    public override bool Equivalent(CodeableConcept? x, CodeableConcept? y, string? precision)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;

        // Two CodeableConcepts are equivalent if any of their codings match
        if (x.Coding == null || y.Coding == null)
            return x.Coding == null && y.Coding == null;

        foreach (var xCoding in x.Coding)
        {
            foreach (var yCoding in y.Coding)
            {
                if (_codingComparer.Equivalent(xCoding, yCoding, precision))
                    return true;
            }
        }

        return false;
    }

    public override int GetHashCode(CodeableConcept x)
    {
        if (x == null) return typeof(CodeableConcept).GetHashCode();

        var hash = typeof(CodeableConcept).GetHashCode();
        if (x.Coding?.FirstOrDefault() is { } firstCoding)
        {
            hash ^= _codingComparer.GetHashCode(firstCoding);
        }
        return hash;
    }
}
