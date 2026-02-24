/*
 * Copyright (c) 2023, NCQA and contributors
 * See the file CONTRIBUTORS for details.
 *
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-cql-sdk/main/LICENSE
 */

using Hl7.Cql.Runtime;
using Ncqa.Hedis.Core;
using System.Reflection;

namespace Hl7.Cql.Hedis;

/// <summary>
/// Type resolver for HEDIS Core DTOs.
/// Maps ELM/FHIR type specifiers to the lightweight HEDIS DTO types.
/// </summary>
public class HedisTypeResolver : BaseTypeResolver
{
    /// <summary>
    /// Default singleton instance.
    /// </summary>
    public static readonly HedisTypeResolver Default = new();

    private readonly Dictionary<Type, string> _typeSpecifiers = new();

    /// <summary>
    /// Creates a new HedisTypeResolver and registers all HEDIS DTO types.
    /// </summary>
    public HedisTypeResolver()
    {
        RegisterHedisTypes();
    }

    /// <inheritdoc/>
    internal override IEnumerable<Assembly> ModelAssemblies => new[] { typeof(Patient).Assembly };

    /// <inheritdoc/>
    internal override IEnumerable<string> ModelNamespaces => new[] { "Ncqa.Hedis.Core" };

    /// <inheritdoc/>
    internal override IEnumerable<(string alias, string type)> Aliases => base.Aliases
        .Concat(new[]
        {
            ("Range", typeof(Ncqa.Hedis.Core.Range).FullName!),
        });

    /// <inheritdoc/>
    internal override Type? PatientType => typeof(Patient);

    /// <inheritdoc/>
    internal override PropertyInfo? PatientBirthDateProperty => typeof(Patient).GetProperty(nameof(Patient.BirthDate));

    /// <inheritdoc/>
    internal override PropertyInfo? GetPrimaryCodePath(string typeSpecifier)
    {
        // Return the primary code path for common resource types
        var type = ResolveType(typeSpecifier);
        if (type == null) return null;

        // Map common FHIR resource types to their primary code properties
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

    /// <inheritdoc/>
    protected override PropertyInfo? GetPropertyCore(Type type, string propertyName)
    {
        // For HEDIS DTOs, we use simple property name matching
        // First try exact match
        var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop != null) return prop;

        // Handle FHIR "value" property for primitive-like types
        if (propertyName == "value")
        {
            // For string-based date types, the value IS the string itself
            if (type == typeof(string)) return null;
        }

        return null;
    }

    /// <inheritdoc/>
    internal override bool ShouldUseSourceObject(Type type, string propertyName)
    {
        // For HEDIS DTOs, dates are already strings, so no special handling needed
        return false;
    }

    /// <summary>
    /// Gets the type specifier for a given .NET type.
    /// </summary>
    public string? GetTypeSpecifier(Type type)
    {
        return _typeSpecifiers.TryGetValue(type, out var spec) ? spec : null;
    }

    private void RegisterHedisTypes()
    {
        const string fhirPrefix = "{http://hl7.org/fhir}";

        // Register all HEDIS DTO types from the assembly
        var hedisAssembly = typeof(Patient).Assembly;
        var hedisTypes = hedisAssembly.GetExportedTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Namespace == "Ncqa.Hedis.Core");

        foreach (var type in hedisTypes)
        {
            var typeSpec = fhirPrefix + type.Name;
            Types.TryAdd(typeSpec, type);
            _typeSpecifiers.TryAdd(type, typeSpec);

            // Also register nested component types
            foreach (var nestedType in type.GetNestedTypes().Where(t => t.IsPublic))
            {
                var nestedSpec = fhirPrefix + type.Name + "." + nestedType.Name.Replace("Component", "");
                Types.TryAdd(nestedSpec, nestedType);
                _typeSpecifiers.TryAdd(nestedType, nestedSpec);
            }
        }

        // Add common FHIR type aliases
        Types.TryAdd("{http://hl7.org/fhir}positiveInt", typeof(int?));
        Types.TryAdd("{http://hl7.org/fhir}unsignedInt", typeof(int?));
        Types.TryAdd("{http://hl7.org/fhir}string", typeof(string));
        Types.TryAdd("{http://hl7.org/fhir}boolean", typeof(bool?));
        Types.TryAdd("{http://hl7.org/fhir}integer", typeof(int?));
        Types.TryAdd("{http://hl7.org/fhir}decimal", typeof(decimal?));
        Types.TryAdd("{http://hl7.org/fhir}uri", typeof(string));
        Types.TryAdd("{http://hl7.org/fhir}url", typeof(string));
        Types.TryAdd("{http://hl7.org/fhir}canonical", typeof(string));
        Types.TryAdd("{http://hl7.org/fhir}base64Binary", typeof(string));
        Types.TryAdd("{http://hl7.org/fhir}instant", typeof(string));
        Types.TryAdd("{http://hl7.org/fhir}date", typeof(string));
        Types.TryAdd("{http://hl7.org/fhir}dateTime", typeof(string));
        Types.TryAdd("{http://hl7.org/fhir}time", typeof(string));
        Types.TryAdd("{http://hl7.org/fhir}code", typeof(string));
        Types.TryAdd("{http://hl7.org/fhir}oid", typeof(string));
        Types.TryAdd("{http://hl7.org/fhir}id", typeof(string));
        Types.TryAdd("{http://hl7.org/fhir}markdown", typeof(string));
        Types.TryAdd("{http://hl7.org/fhir}xhtml", typeof(string));

        // SimpleQuantity and MoneyQuantity map to Quantity
        if (Types.TryGetValue("{http://hl7.org/fhir}Quantity", out var quantityType))
        {
            Types.TryAdd("{http://hl7.org/fhir}SimpleQuantity", quantityType);
            Types.TryAdd("{http://hl7.org/fhir}MoneyQuantity", quantityType);
        }
    }
}
