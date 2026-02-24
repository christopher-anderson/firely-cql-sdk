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
/// Compares <see cref="Coding"/> instances for CQL operations.
/// </summary>
internal class CodingComparer : CqlComparerBase<Coding>
{
    private readonly StringComparer _stringComparer = StringComparer.OrdinalIgnoreCase;

    public override int? Compare(Coding? x, Coding? y, string? precision)
    {
        if (x == null || y == null) return null;

        // Compare by system first, then code
        var systemCompare = _stringComparer.Compare(x.System ?? "", y.System ?? "");
        if (systemCompare != 0) return systemCompare;

        return _stringComparer.Compare(x.Code ?? "", y.Code ?? "");
    }

    public override bool Equivalent(Coding? x, Coding? y, string? precision)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;

        return _stringComparer.Equals(x.System, y.System) &&
               _stringComparer.Equals(x.Code, y.Code);
    }

    public override int GetHashCode(Coding x)
    {
        if (x == null) return typeof(Coding).GetHashCode();

        var hash = typeof(Coding).GetHashCode();
        if (x.System != null) hash ^= _stringComparer.GetHashCode(x.System);
        if (x.Code != null) hash ^= _stringComparer.GetHashCode(x.Code);
        return hash;
    }
}
