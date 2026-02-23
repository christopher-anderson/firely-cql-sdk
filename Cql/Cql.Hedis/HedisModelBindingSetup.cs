/*
 * Copyright (c) 2023, NCQA and contributors
 * See the file CONTRIBUTORS for details.
 *
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-cql-sdk/main/LICENSE
 */

using Hl7.Cql.Abstractions;
using Hl7.Cql.Comparers;
using Hl7.Cql.Conversion;
using Hl7.Cql.Iso8601;
using Hl7.Cql.Operators;
using Hl7.Cql.Primitives;
using Hl7.Cql.Runtime;
using Hl7.Cql.ValueSets;

namespace Hl7.Cql.Hedis;

/// <summary>
/// Supplies binding information to the CQL engine on how to use HEDIS DTOs as data model.
/// </summary>
internal class HedisModelBindingSetup : ModelBindingSetup
{
    /// <summary>
    /// Creates a model binding for HEDIS DTOs.
    /// </summary>
    /// <param name="dataSource">The data source providing FHIR resources.</param>
    /// <param name="valuesets">Value set dictionary for terminology operations.</param>
    /// <param name="now">The current date/time for CQL operations.</param>
    public HedisModelBindingSetup(
        IDataSource? dataSource,
        IValueSetDictionary? valuesets,
        DateTimeOffset? now)
    {
        Comparers = new CqlComparers();
        Operators = CqlOperators.Create(
            TypeResolver,
            TypeConverter,
            dataSource,
            Comparers,
            valuesets,
            UnitConverter,
            now is not null
                ? new DateTimeIso8601(now.Value, DateTimePrecision.Millisecond)
                : null,
            null);

        Comparers
            .AddIntervalComparisons(Operators)
            .AddHedisComparers();
    }

    /// <inheritdoc/>
    public override TypeResolver TypeResolver => HedisTypeResolver.Default;

    /// <inheritdoc/>
    public override TypeConverter TypeConverter => HedisTypeConverter.Default;

    /// <inheritdoc/>
    public override CqlComparers Comparers { get; }

    /// <inheritdoc/>
    public override IUnitConverter UnitConverter => new UnitConverter();

    /// <inheritdoc/>
    public override ICqlOperators Operators { get; }
}
