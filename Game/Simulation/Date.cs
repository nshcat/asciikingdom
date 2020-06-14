using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Game.Simulation
{
    /// <summary>
    /// JSON converter for dates
    /// </summary>
    public class DateConverter : JsonConverter<Date>
    {
        public override Date Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new Date
            {
                Weeks = long.Parse(reader.GetString())
            };
        }

        public override void Write(Utf8JsonWriter writer, Date value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Weeks.ToString());
        }
    }
    
    /// <summary>
    /// Represents the current date in the simulation state
    /// </summary>
    /// <remarks>
    /// For simplicity reasons, each month has exactly four weeks (we even disregard leap years).
    /// </remarks>
    [JsonConverter(typeof(DateConverter))]
    public class Date
    {
        /// <summary>
        /// All months, sorted
        /// </summary>
        private static Month[] _months = (Month[])Enum.GetValues(typeof(Month));

        /// <summary>
        /// Short month names
        /// </summary>
        private static string[] _monthShortNames =
        {
            "Jan",
            "Feb",
            "Apr",
            "Mar",
            "May",
            "Jun",
            "Jul",
            "Aug",
            "Sep",
            "Oct",
            "Nov",
            "Dec"
        };

        /// <summary>
        /// The elapsed days in a month, to be indexed by the current week index (in [0, 4))
        /// </summary>
        private static int[] _daysInMonth = { 1, 8, 15, 22 };

        /// <summary>
        /// The starting year all date calculations are based on
        /// </summary>
        public static long StartYear = 500;
        
        /// <summary>
        /// The current number of elapsed weeks since the <see cref="StartYear"/>
        /// </summary>
        public long Weeks { get; set; }
        
        /// <summary>
        /// The current day in the month
        /// </summary>
        public int Day => _daysInMonth[Weeks % 4];

        /// <summary>
        /// The current month in the year
        /// </summary>
        public Month Month => _months[(Weeks / 4) % 12];

        /// <summary>
        /// Short month name
        /// </summary>
        public string MonthShort => _monthShortNames[(Weeks / 4) % 12];

        /// <summary>
        /// The current year
        /// </summary>
        public long Year => (Weeks / 48) + StartYear;

        /// <summary>
        /// Day postfix
        /// </summary>
        private string DayPostfix => (Day == 1) ? "st" : "th";
        
        /// <summary>
        /// Format date
        /// </summary>
        public override string ToString()
        {
            var day = $"{this.Day}{this.DayPostfix}";
            return $"{day,4} {this.MonthShort} {this.Year}";
        }
    }
}