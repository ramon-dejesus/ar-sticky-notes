using System;
using System.Globalization;
using UnityEngine;

[Serializable]
public class BaseObject
{
    [SerializeField] public string Id = Guid.NewGuid().ToString("N");
    [SerializeField] private string _createdAt = new UnityConverter().ConvertDateTimeToString(DateTime.Now);
    public DateTime CreatedAt
    {
        //Datetime objects cannot be converted to json with JsonUtility. This is a workaround.
        get
        {
            return new UnityConverter().ConvertStringToDateTime(_createdAt);
        }
        set
        {
            _createdAt = new UnityConverter().ConvertDateTimeToString(value);
        }
    }
    [SerializeField] public string Title = "";
    [SerializeField] public string Description = "";
}
