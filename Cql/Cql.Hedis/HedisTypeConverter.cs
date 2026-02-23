/*
 * Copyright (c) 2023, NCQA and contributors
 * See the file CONTRIBUTORS for details.
 *
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-cql-sdk/main/LICENSE
 */

using Hl7.Cql.Conversion;
using Hl7.Cql.Primitives;
using Ncqa.Hedis.Core._2025;
using System.Text;
using H = Ncqa.Hedis.Core._2025;

namespace Hl7.Cql.Hedis;

/// <summary>
/// Type converter for HEDIS Core DTOs.
/// Handles conversions between HEDIS DTOs and CQL primitive types.
/// </summary>
public static class HedisTypeConverter
{
    /// <summary>
    /// Default singleton TypeConverter configured for HEDIS DTOs.
    /// </summary>
    public static readonly TypeConverter Default = Create();

    /// <summary>
    /// Creates a new TypeConverter configured for HEDIS DTOs.
    /// </summary>
    public static TypeConverter Create()
    {
        return TypeConverter
            .Create()
            .ConvertSystemTypes()
            .ConvertHedisToCqlPrimitives()
            .ConvertCqlPrimitivesToHedis();
    }

    internal static TypeConverter ConvertSystemTypes(this TypeConverter converter)
    {
        converter.AddConversion<byte[], string>(binary => Encoding.UTF8.GetString(binary));
        converter.AddConversion<DateTimeOffset?, CqlDateTime?>(dto => dto == null ? null : new CqlDateTime(dto.Value, Iso8601.DateTimePrecision.Millisecond));
        converter.AddConversion<string, CqlDate?>(str =>
        {
            if (string.IsNullOrEmpty(str)) return null;
            if (CqlDate.TryParse(str, out var date))
                return date!;
            return null;
        });
        converter.AddConversion<string, CqlDateTime?>(str =>
        {
            if (string.IsNullOrEmpty(str)) return null;
            if (CqlDateTime.TryParse(str, out var dateTime))
                return dateTime;
            return null;
        });
        converter.AddConversion<string, CqlTime?>(str =>
        {
            if (string.IsNullOrEmpty(str)) return null;
            if (CqlTime.TryParse(str, out var time))
                return time;
            return null;
        });
        converter.AddConversion<DateTimeOffset, CqlDateTime>(dto => new CqlDateTime(dto, Iso8601.DateTimePrecision.Millisecond));

        return converter;
    }

    internal static TypeConverter ConvertHedisToCqlPrimitives(this TypeConverter converter)
    {
        // Quantity conversions
        converter.AddConversion((H.Quantity q) => q.Value.HasValue ? new CqlQuantity(q.Value.Value, q.Unit) : null);
        converter.AddConversion((H.Quantity q) => q.Value);
        converter.AddConversion((H.Quantity q) => (int?)q.Value);

        // Period to CqlInterval conversions
        converter.AddConversion((H.Period p) =>
        {
            CqlDateTime? start = null;
            CqlDateTime? end = null;

            if (!string.IsNullOrEmpty(p.Start) && CqlDateTime.TryParse(p.Start, out var s))
                start = s;
            if (!string.IsNullOrEmpty(p.End) && CqlDateTime.TryParse(p.End, out var e))
                end = e;

            return new CqlInterval<CqlDateTime>(start!, end!, lowClosed: true, highClosed: true);
        });

        converter.AddConversion((H.Period p) =>
        {
            CqlDate? start = null;
            CqlDate? end = null;

            if (!string.IsNullOrEmpty(p.Start) && CqlDate.TryParse(p.Start, out var s))
                start = s;
            if (!string.IsNullOrEmpty(p.End) && CqlDate.TryParse(p.End, out var e))
                end = e;

            return new CqlInterval<CqlDate>(start!, end!, lowClosed: true, highClosed: true);
        });

        // Range to CqlInterval conversions
        converter.AddConversion((H.Range r) => new CqlInterval<CqlQuantity>(
            converter.Convert<CqlQuantity>(r.Low),
            converter.Convert<CqlQuantity>(r.High),
            lowClosed: true, highClosed: true));

        converter.AddConversion((H.Range r) => new CqlInterval<decimal?>(
            converter.Convert<decimal?>(r.Low),
            converter.Convert<decimal?>(r.High),
            lowClosed: true, highClosed: true));

        converter.AddConversion((H.Range r) => new CqlInterval<int?>(
            converter.Convert<int?>(r.Low),
            converter.Convert<int?>(r.High),
            lowClosed: true, highClosed: true));

        // Coding to CqlCode
        converter.AddConversion((H.Coding c) => new CqlCode(c.Code, c.System, c.Version, c.Display));

        // CodeableConcept to CqlConcept
        converter.AddConversion((H.CodeableConcept cc) =>
        {
            var codes = cc.Coding?
                .Select(c => new CqlCode(c.Code, c.System, c.Version, c.Display))
                .ToArray() ?? Array.Empty<CqlCode>();
            return new CqlConcept(codes, cc.Text);
        });

        // Ratio to CqlRatio
        converter.AddConversion((H.Ratio r) =>
        {
            var num = r.Numerator != null ? converter.Convert<CqlQuantity>(r.Numerator) : null;
            var denom = r.Denominator != null ? converter.Convert<CqlQuantity>(r.Denominator) : null;
            return new CqlRatio(num, denom);
        });

        return converter;
    }

    internal static TypeConverter ConvertCqlPrimitivesToHedis(this TypeConverter converter)
    {
        // CqlDate/DateTime to string (HEDIS DTOs use strings for dates)
        converter.AddConversion((CqlDate d) => d.ToString());
        converter.AddConversion((CqlDateTime dt) => dt.ToString());
        converter.AddConversion((CqlTime t) => t.ToString());

        // CqlQuantity to Quantity
        converter.AddConversion((CqlQuantity q) => q.value.HasValue
            ? new H.Quantity { Value = q.value.Value, Unit = q.unit ?? "1" }
            : null);

        // CqlInterval<CqlDateTime> to Period
        converter.AddConversion((CqlInterval<CqlDateTime>? interval) =>
        {
            if (interval == null) return null;
            return new H.Period
            {
                Start = interval.low?.ToString(),
                End = interval.high?.ToString()
            };
        });

        // CqlInterval<CqlDate> to Period
        converter.AddConversion((CqlInterval<CqlDate>? interval) =>
        {
            if (interval == null) return null;
            return new H.Period
            {
                Start = interval.low?.ToString(),
                End = interval.high?.ToString()
            };
        });

        // CqlInterval<CqlQuantity> to Range
        converter.AddConversion((CqlInterval<CqlQuantity>? interval) =>
        {
            if (interval == null) return null;
            return new H.Range
            {
                Low = interval.low?.value.HasValue == true
                    ? new H.Quantity { Value = interval.low.value!.Value, Unit = interval.low.unit ?? "1" }
                    : null,
                High = interval.high?.value.HasValue == true
                    ? new H.Quantity { Value = interval.high.value!.Value, Unit = interval.high.unit ?? "1" }
                    : null
            };
        });

        // CqlCode to Coding
        converter.AddConversion((CqlCode c) => new H.Coding
        {
            Code = c.code,
            System = c.system,
            Version = c.version,
            Display = c.display
        });

        // CqlConcept to CodeableConcept
        converter.AddConversion((CqlConcept cc) => new H.CodeableConcept
        {
            Text = cc.display,
            Coding = cc.codes?.Select(c => new H.Coding
            {
                Code = c.code,
                System = c.system,
                Version = c.version,
                Display = c.display
            }).ToList()
        });

        // CqlRatio to Ratio
        converter.AddConversion((CqlRatio r) =>
        {
            return new H.Ratio
            {
                Numerator = r.numerator?.value.HasValue == true
                    ? new H.Quantity { Value = r.numerator.value!.Value, Unit = r.numerator.unit ?? "1" }
                    : null,
                Denominator = r.denominator?.value.HasValue == true
                    ? new H.Quantity { Value = r.denominator.value!.Value, Unit = r.denominator.unit ?? "1" }
                    : null
            };
        });

        return converter;
    }
}
