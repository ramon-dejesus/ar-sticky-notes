using UnityEngine;
using System.IO;
using System;

public class LocalStorage
{
    private string StoragePath { get; set; } = "";
    public LocalStorage(string directoryName = "")
    {
        StoragePath = Application.persistentDataPath;
        if (string.IsNullOrEmpty(StoragePath))
        {
            throw new Exception("The persistent data path is empty.");
        }
        if (directoryName != "")
        {
            StoragePath += @"\" + directoryName;
        }
    }
    private string GetFullPath(string key)
    {
        var fileName = key + ".dat";
        return !string.IsNullOrEmpty(StoragePath) ? Path.Combine(StoragePath, fileName) : "";
    }
    public void SaveObject(string key, object value)
    {
        DeleteObject(key);
        if (value != null)
        {
            var js = JsonUtility.ToJson(value);
            var pth = GetFullPath(key);
            if (pth != "")
            {
                File.WriteAllText(pth, js);
            }
            else
            {
                SaveValue(key, js);
            }
        }
    }
    public void SaveValue(string key, object value)
    {
        DeleteValue(key);
        if (value != null)
        {
            PlayerPrefs.SetString(key, value.ToString());
            PlayerPrefs.Save();
        }
    }
    public void DeleteObject(string key)
    {
        var pth = GetFullPath(key);
        if (pth != "")
        {
            File.Delete(pth);
        }
        else
        {
            DeleteValue(key);
        }
    }
    public void DeleteValue(string key)
    {
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
    }
    public T GetObject<T>(string key)
    {
        var pth = GetFullPath(key);
        if (pth != "")
        {
            return JsonUtility.FromJson<T>(File.ReadAllText(pth));
        }
        else
        {
            return JsonUtility.FromJson<T>(GetValue(key));
        }
    }
    public string GetValue(string key)
    {
        return PlayerPrefs.GetString(key);
    }
}
