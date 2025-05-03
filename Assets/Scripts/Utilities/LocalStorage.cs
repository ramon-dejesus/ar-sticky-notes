using UnityEngine;
using System.IO;
using System;
public class LocalStorage
{
    private string StoragePath { get; set; } = "";
    public LocalStorage()
    {
        StoragePath = Application.persistentDataPath;
    }
    private string GetFullPath(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new Exception("The storage key is empty.");
        }
        var fileName = key + ".dat";
        return !string.IsNullOrEmpty(StoragePath) ? Path.Combine(StoragePath, fileName) : "";
    }
    public void SaveObject(string key, object value, bool encrypted = true)
    {
        if (value != null)
        {
            var js = JsonUtility.ToJson(value);
            var pth = GetFullPath(key);
            if (pth != "")
            {
                if (encrypted)
                {
                    js = new UnityEncryption().Encrypt(js);
                }
                File.WriteAllText(pth, js);
            }
            else
            {
                SaveValue(key, js, encrypted);
            }
        }
        else
        {
            DeleteObject(key);
        }
    }
    public void SaveValue(string key, object value, bool encrypted = true)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new Exception("The storage key is empty.");
        }
        if (value != null)
        {
            var tmp = value.GetType().Name.ToLower() != "string" ? JsonUtility.ToJson(value) : value.ToString();
            if (encrypted)
            {
                tmp = new UnityEncryption().Encrypt(tmp);
            }
            PlayerPrefs.SetString(key, tmp);
            PlayerPrefs.Save();
        }
        else
        {
            DeleteValue(key);
        }
    }
    public void DeleteObject(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new Exception("The storage key is empty.");
        }
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
        if (string.IsNullOrEmpty(key))
        {
            throw new Exception("The storage key is empty.");
        }
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
    }
    public T GetObject<T>(string key, bool encrypted = true)
    {
        var pth = GetFullPath(key);
        if (pth != "")
        {
            var js = File.ReadAllText(pth);
            if (encrypted)
            {
                js = new UnityEncryption().Decrypt(js);
            }
            return JsonUtility.FromJson<T>(js);
        }
        else
        {
            return GetValue<T>(key);
        }
    }
    public T GetValue<T>(string key, bool encrypted = true)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new Exception("The storage key is empty.");
        }
        object tmp = PlayerPrefs.GetString(key);
        if (encrypted)
        {
            tmp = new UnityEncryption().Decrypt(tmp.ToString());
        }
        if (typeof(T).Name.ToLower() == "string")
        {
            return (T)tmp;
        }
        return JsonUtility.FromJson<T>(tmp.ToString());
    }
}
