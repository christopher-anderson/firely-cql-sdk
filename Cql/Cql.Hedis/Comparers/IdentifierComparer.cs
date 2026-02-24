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
/// Compares <see cref="Identifier"/> instances for CQL operations.
/// </summary>
internal class IdentifierComparer : CqlComparerBase<Identifier>
{
    private readonly StringComparer _stringComparer = StringComparer.OrdinalIgnoreCase;

    public override int? Compare(Identifier? x, Identifier? y, string? precision)
    {
        if (x == null || y == null) return null;

        // Compare by system first, then value
        var systemCompare = _stringComparer.Compare(x.System ?? "", y.System ?? "");
        if (systemCompare != 0) return systemCompare;

        return _stringComparer.Compare(x.Value ?? "", y.Value ?? "");
    }

    public override bool Equivalent(Identifier? x, Identifier? y, string? precision)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;

        return _stringComparer.Equals(x.System, y.System) &&
               _stringComparer.Equals(x.Value, y.Value);
    }

    public override int GetHashCode(Identifier x)
    {
        if (x == null) return typeof(Identifier).GetHashCode();

        var hash = typeof(Identifier).GetHashCode();
        if (x.System != null) hash ^= _stringComparer.GetHashCode(x.System);
        if (x.Value != null) hash ^= _stringComparer.GetHashCode(x.Value);
        return hash;
    }
}
