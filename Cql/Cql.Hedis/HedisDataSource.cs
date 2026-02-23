/*
 * Copyright (c) 2023, NCQA and contributors
 * See the file CONTRIBUTORS for details.
 *
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-cql-sdk/main/LICENSE
 */

using Hl7.Cql.Abstractions;
using Hl7.Cql.Comparers;
using Hl7.Cql.Operators;
using Hl7.Cql.Primitives;
using Hl7.Cql.ValueSets;
using Ncqa.Hedis.Core._2025;
using System.Reflection;

namespace Hl7.Cql.Hedis;

/// <summary>
/// A data source implementation for HEDIS DTOs.
/// </summary>
public class HedisDataSource : IDataSource
{
    private readonly Dictionary<Type, List<object>> _byType = new();
    private readonly IValueSetDictionary _valueSets;
    private readonly ICqlComparer _codeComparer;
    private readonly ICqlComparer _systemComparer;

    /// <summary>
    /// Creates a new HedisDataSource from a collection of resources.
    /// </summary>
    /// <param name="resources">The resources to include in the data source.</param>
    /// <param name="valueSets">The value set dictionary for terminology operations.</param>
    /// <param name="codeComparer">Optional comparer for codes.</param>
    /// <param name="systemComparer">Optional comparer for systems.</param>
    public HedisDataSource(
        IEnumerable<object> resources,
        IValueSetDictionary valueSets,
        ICqlComparer? codeComparer = null,
        ICqlComparer? systemComparer = null)
    {
        _valueSets = valueSets ?? throw new ArgumentNullException(nameof(valueSets));
        _codeComparer = codeComparer ?? new StringCqlComparer(StringComparer.OrdinalIgnoreCase);
        _systemComparer = systemComparer ?? new StringCqlComparer(StringComparer.OrdinalIgnoreCase);

        IndexResources(resources);
    }

    private void IndexResources(IEnumerable<object> resources)
    {
        foreach (var resource in resources)
        {
            var type = resource.GetType();
            while (type != typeof(object) && type != null)
            {
                if (!_byType.TryGetValue(type, out var list))
                {
                    list = new List<object>();
                    _byType.Add(type, list);
                }
                list.Add(resource);
                type = type.BaseType;
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<T> RetrieveByCodes<T>(IEnumerable<CqlCode?>? allowedCodes = null, PropertyInfo? codeProperty = null) where T : class
    {
        if (allowedCodes is not null)
        {
            Predicate<Coding> filter = allowedCodes is IValueSetFacade valueSet
                ? c => valueSet.IsCodeInValueSet(c.Code, c.System) == true
                : ListFilter;

            return ExecuteFilter<T>(filter, codeProperty);

            bool ListFilter(Coding coding) => allowedCodes.Any(allowed =>
                allowed is not null &&
                _systemComparer.Equivalent(coding.System, allowed.system, null) &&
                _codeComparer.Equivalent(coding.Code, allowed.code, null));
        }
        else
        {
            return FilterByType<T>();
        }
    }

    /// <inheritdoc/>
    public IEnumerable<T> RetrieveByValueSet<T>(CqlValueSet? valueSet = null, PropertyInfo? codeProperty = null) where T : class
    {
        if (valueSet != null && valueSet.id != null)
        {
            return ExecuteFilter<T>(c => _valueSets.IsCodeInValueSet(valueSet.id, c.Code ?? "", c.System ?? ""), codeProperty);
        }
        else
        {
            return FilterByType<T>();
        }
    }

    private IEnumerable<T> FilterByType<T>() where T : class
    {
        if (_byType.TryGetValue(typeof(T), out var resources))
            return resources.Cast<T>();
        return Enumerable.Empty<T>();
    }

    private IEnumerable<T> ExecuteFilter<T>(Predicate<Coding> filter, PropertyInfo? codeProperty) where T : class
    {
        var candidates = FilterByType<T>();

        if (codeProperty == null)
        {
            // Use convention-based code path detection
            codeProperty = GetDefaultCodeProperty(typeof(T));
        }

        if (codeProperty == null)
        {
            // No code property, return all
            foreach (var candidate in candidates)
                yield return candidate;
            yield break;
        }

        foreach (var candidate in candidates)
        {
            var codings = GetCodings(candidate, codeProperty);
            foreach (var coding in codings)
            {
                if (filter(coding))
                {
                    yield return candidate;
                    break;
                }
            }
        }
    }

    private static PropertyInfo? GetDefaultCodeProperty(Type type)
    {
        // Convention-based code property detection for common FHIR resource types
        return type.Name switch
        {
            "Condition" => type.GetProperty("Code"),
            "Procedure" => type.GetProperty("Code"),
            "Observation" => type.GetProperty("Code"),
            "MedicationRequest" => type.GetProperty("Medication"),
            "Encounter" => type.GetProperty("Type"),
            "DiagnosticReport" => type.GetProperty("Code"),
            "Immunization" => type.GetProperty("VaccineCode"),
            "AllergyIntolerance" => type.GetProperty("Code"),
            "ServiceRequest" => type.GetProperty("Code"),
            _ => null
        };
    }

    private static IEnumerable<Coding> GetCodings(object resource, PropertyInfo codeProperty)
    {
        var value = codeProperty.GetValue(resource);
        return value switch
        {
            CodeableConcept cc => cc.Coding ?? Enumerable.Empty<Coding>(),
            Coding coding => new[] { coding },
            IEnumerable<CodeableConcept> concepts => concepts.SelectMany(c => c.Coding ?? Enumerable.Empty<Coding>()),
            IEnumerable<Coding> codings => codings,
            _ => Enumerable.Empty<Coding>()
        };
    }
}
