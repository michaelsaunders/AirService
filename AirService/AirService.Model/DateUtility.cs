using System;
using System.Text.RegularExpressions;

namespace AirService.Model
{
    public static class DateUtility
    {
        private static readonly Regex iso8061DateFormatRegex;

        static DateUtility()
        {
            iso8061DateFormatRegex =
                new Regex(
                    @"(?<y>[\d]{4})-(?<m>[\d]{1,2})-(?<d>[\d]{1,2})T(?<hh>[\d]{1,2})[:\.](?<mm>[\d]{1,2})([:\.](?<ss>[\d]{1,2}))?Z");
        }

        public static string ToIso8061DateString(this DateTime localTime)
        {
            return localTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        /// <summary>
        /// From ISO8061 UTC date to Local date/time
        /// </summary> 
        public static DateTime FromIso8061FormattedDateString(string formattedString)
        {
            Match match = iso8061DateFormatRegex.Match(formattedString);
            if (match.Success)
            {
                Group secondGroup = match.Groups["ss"];
                return new DateTime(
                    int.Parse(match.Groups["y"].Value),
                    int.Parse(match.Groups["m"].Value),
                    int.Parse(match.Groups["d"].Value),
                    int.Parse(match.Groups["hh"].Value),
                    int.Parse(match.Groups["mm"].Value),
                    secondGroup.Success ? int.Parse(secondGroup.Value) : 0, DateTimeKind.Utc
                    ).ToLocalTime();
            }

            // if need to support timezone offset, modify expression.
            throw new FormatException("Expected ISO8061 formatted date string.");
        }
    }
}