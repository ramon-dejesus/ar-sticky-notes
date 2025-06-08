using System;
using System.Globalization;
using UnityEngine;

namespace ARStickyNotes.Utilities
{
    /// <summary>
    /// Provides helper methods for converting between common Unity types and formats,
    /// including JSON serialization, DateTime handling, and Base64 encoding.
    /// </summary>
    public class UnityConverter
    {
        /// <summary>
        /// Serializes an object into a JSON string using Unity's <c>JsonUtility</c>.
        /// </summary>
        /// <param name="item">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public string ConvertObjectToJson(object item)
        {
            try
            {
                return JsonUtility.ToJson(item);
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Failed to convert object to JSON.", ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// Deserializes a JSON string into an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to deserialize into.</typeparam>
        /// <param name="value">The JSON string to convert.</param>
        /// <returns>An object of type <typeparamref name="T"/>.</returns>
        public T ConvertJsonToObject<T>(string value)
        {
            try
            {
                return JsonUtility.FromJson<T>(value);
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Failed to convert JSON to object.", ex);
                return default;
            }
        }

        /// <summary>
        /// Converts an ISO-8601 formatted string into a <see cref="DateTime"/> object.
        /// </summary>
        /// <param name="value">The date-time string to parse.</param>
        /// <param name="datetimeFormat">The format pattern to use for parsing (default: "yyyy-MM-DDThh:mm:ss").</param>
        /// <returns>
        /// A <see cref="DateTime"/> if parsing is successful; otherwise, <see cref="DateTime.MinValue"/>.
        /// </returns>
        public DateTime ConvertStringToDateTime(string value, string datetimeFormat = "yyyy-MM-DDThh:mm:ss")
        {
            try
            {
                if (DateTime.TryParseExact(value, datetimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dtm))
                {
                    return dtm;
                }
                return DateTime.MinValue;
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Failed to convert string to DateTime.", ex);
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> object into a string using the specified format.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> to convert.</param>
        /// <param name="datetimeFormat">The output format pattern (default: "yyyy-MM-DDThh:mm:ss").</param>
        /// <returns>A formatted date-time string.</returns>
        public string ConvertDateTimeToString(DateTime value, string datetimeFormat = "yyyy-MM-DDThh:mm:ss")
        {
            try
            {
                return value.ToString(datetimeFormat);
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Failed to convert DateTime to string.", ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// Encodes a plain string into Base64 format using UTF-8 encoding.
        /// </summary>
        /// <param name="value">The plain text string to encode.</param>
        /// <returns>A Base64 encoded string, or empty string if input is null or empty.</returns>
        public string ConvertStringToBase64(string value)
        {
            try
            {
                return !string.IsNullOrEmpty(value) ? Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value)) : "";
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Failed to convert string to Base64.", ex);
                return string.Empty;
            }
        }
    }
}