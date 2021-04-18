using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace Game.Maths
{
    /// <summary>
    /// JSON converter for <see cref="FloatRange"/> instances
    /// </summary>
    public class RangeConverter : JsonConverter<FloatRange>
    {
        public override FloatRange Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";

            var strValue = reader.GetString();
            var components = strValue.Split(';');

            if (components.Length != 2)
                throw new JsonException($"Could not read FloatRange: \"{strValue}\" is not a valid value");

            float min, max;

            if (!float.TryParse(components[0], NumberStyles.Any, ci, out min))
                throw new JsonException($"Could not parse FloatRange minimum: \"{components[0]}\" is not a valid floating point number");

            if (!float.TryParse(components[1], NumberStyles.Any, ci, out max))
                throw new JsonException($"Could not parse FloatRange maximum: \"{components[1]}\" is not a valid floating point number");

            return new FloatRange(min, max);
        }

        public override void Write(Utf8JsonWriter writer, FloatRange value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    /// <summary>
    /// Represents a floating point range.
    /// </summary>
    [JsonConverter(typeof(RangeConverter))]
    public struct FloatRange
    {
        /// <summary>
        /// The lower bound (inclusive)
        /// </summary>
        public float Minimum { get; }
        
        /// <summary>
        /// The upper bound (inclusive)
        /// </summary>
        public float Maximum { get; }

        /// <summary>
        /// Construct a new range instance from given bounds.
        /// </summary>
        public FloatRange(float minimum, float maximum)
        {
            this.Maximum = maximum;
            this.Minimum = minimum;
        }
        
        /// <summary>
        /// Determine whether given value lies within the range defined by this instance.
        /// </summary>
        public bool IsInside(float value) => (value >= this.Minimum) && (value <= this.Maximum);

        /// <summary>
        /// The unit [0, 1] range.
        /// </summary>
        public static FloatRange UnitRange => new FloatRange(0.0f, 1.0f);

        /// <summary>
        /// Return a flipped range
        /// </summary>
        public FloatRange Flipped => new FloatRange(this.Maximum, this.Minimum);

        /// <summary>
        /// Convert range to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{this.Minimum};{this.Maximum}";
        }
    }
}