/*
 * Copyright (c) 2023, NCQA and contributors
 * See the file CONTRIBUTORS for details.
 *
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-cql-sdk/main/LICENSE
 */

using Hl7.Cql.Operators;
using Hl7.Cql.Runtime;
using Hl7.Cql.ValueSets;

namespace Hl7.Cql.Hedis;

/// <summary>
/// Factory methods to initialize a <see cref="CqlContext"/> that uses HEDIS DTOs as the model binding.
/// </summary>
public static class HedisCqlContext
{
    /// <summary>
    /// Creates a CQL context with the given data source.
    /// </summary>
    /// <param name="dataSource">The data source providing FHIR resources.</param>
    /// <param name="parameters">Optional parameters for CQL execution.</param>
    /// <param name="valueSets">Value set dictionary for terminology operations.</param>
    /// <param name="now">The current date/time for CQL operations.</param>
    /// <param name="delegates">Pre-compiled CQL definitions.</param>
    /// <returns>A configured CqlContext.</returns>
    public static CqlContext WithDataSource(
        IDataSource? dataSource = null,
        IDictionary<string, object>? parameters = null,
        IValueSetDictionary? valueSets = null,
        DateTimeOffset? now = null,
        DefinitionDictionary<Delegate>? delegates = null)
    {
        return new CqlContext(
            new HedisModelBindingSetup(dataSource, valueSets, now).Operators,
            parameters,
            delegates);
    }

    /// <summary>
    /// Creates a CQL context with the given collection of resources.
    /// </summary>
    /// <param name="resources">The HEDIS DTO resources.</param>
    /// <param name="parameters">Optional parameters for CQL execution.</param>
    /// <param name="valueSets">Value set dictionary for terminology operations.</param>
    /// <param name="now">The current date/time for CQL operations.</param>
    /// <param name="delegates">Pre-compiled CQL definitions.</param>
    /// <returns>A configured CqlContext.</returns>
    public static CqlContext ForResources(
        IEnumerable<object> resources,
        IDictionary<string, object>? parameters = null,
        IValueSetDictionary? valueSets = null,
        DateTimeOffset? now = null,
        DefinitionDictionary<Delegate>? delegates = null)
    {
        var valueSetDict = valueSets ?? new HashValueSetDictionary();
        var dataSource = new HedisDataSource(resources, valueSetDict);
        return WithDataSource(dataSource, parameters, valueSetDict, now, delegates);
    }

    /// <summary>
    /// Creates a CQL context without any data source (for expression evaluation only).
    /// </summary>
    /// <param name="parameters">Optional parameters for CQL execution.</param>
    /// <param name="valueSets">Value set dictionary for terminology operations.</param>
    /// <param name="now">The current date/time for CQL operations.</param>
    /// <param name="delegates">Pre-compiled CQL definitions.</param>
    /// <returns>A configured CqlContext.</returns>
    public static CqlContext Create(
        IDictionary<string, object>? parameters = null,
        IValueSetDictionary? valueSets = null,
        DateTimeOffset? now = null,
        DefinitionDictionary<Delegate>? delegates = null)
    {
        return WithDataSource(null, parameters, valueSets, now, delegates);
    }
}
