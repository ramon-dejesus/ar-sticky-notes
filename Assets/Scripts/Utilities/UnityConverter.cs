using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class UnityConverter
{
    public string ConvertObjectToJson(object item)
    {
        return JsonUtility.ToJson(item);
    }
    public T ConvertJsonToObject<T>(string value)
    {
        return JsonUtility.FromJson<T>(value);
    }
    public DateTime ConvertStringToDateTime(string value, string datetimeFormat = "yyyy-MM-DDThh:mm:ss")
    {
        if (DateTime.TryParseExact(value, datetimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dtm))
        {
            return dtm;
        }
        return DateTime.MinValue;
    }
    public string ConvertDateTimeToString(DateTime value, string datetimeFormat = "yyyy-MM-DDThh:mm:ss")
    {
        return value.ToString(datetimeFormat);
    }
    public string ConvertStringToBase64(string value)
    {
        return !string.IsNullOrEmpty(value) ? Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value)) : "";
    }
}
