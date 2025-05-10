using UnityEngine;
using System.IO;
using System;

namespace ARStickyNotes.Utilities
{
    public class LocalStorage
    {
        private string StoragePath { get; set; } = "";
        private string EncryptionPassword { get; set; } = "";
        public LocalStorage(string locationPath = "", string password = "")
        {
            StoragePath = locationPath;
            EncryptionPassword = password;
        }
        private string GetFullPath(string key)
        {
            if (string.IsNullOrEmpty(StoragePath))
            {
                throw new Exception("The storage path is empty.");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new Exception("The storage key is empty.");
            }
            var fileName = key + ".dat";
            return Path.Combine(StoragePath, fileName);
        }
        public void SaveObject(string key, object value)
        {
            if (value != null)
            {
                var js = new UnityConverter().ConvertObjectToJson(value);
                var pth = GetFullPath(key);
                if (pth != "")
                {
                    if (EncryptionPassword != "")
                    {
                        js = new UnityEncryption().Encrypt(js);
                    }
                    File.WriteAllText(pth, js);
                }
                else
                {
                    SaveValue(key, js);
                }
            }
            else
            {
                DeleteObject(key);
            }
        }
        public void SaveValue(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new Exception("The storage key is empty.");
            }
            if (value != null)
            {
                var tmp = value.GetType().Name.ToLower() != "string" ? new UnityConverter().ConvertObjectToJson(value) : value.ToString();
                if (EncryptionPassword != "")
                {
                    tmp = new UnityEncryption().Encrypt(tmp, EncryptionPassword);
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
                if (File.Exists(pth))
                {
                    File.Delete(pth);
                }
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
        public T GetObject<T>(string key)
        {
            var pth = GetFullPath(key);
            if (pth != "")
            {
                if (File.Exists(pth))
                {
                    var js = File.ReadAllText(pth);
                    if (EncryptionPassword != "")
                    {
                        js = new UnityEncryption().Decrypt(js, EncryptionPassword);
                    }
                    return new UnityConverter().ConvertJsonToObject<T>(js);
                }
            }
            else
            {
                return GetValue<T>(key);
            }
            return default;
        }
        public T GetValue<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new Exception("The storage key is empty.");
            }
            object tmp = PlayerPrefs.GetString(key);
            if (EncryptionPassword != "")
            {
                tmp = new UnityEncryption().Decrypt(tmp.ToString(), EncryptionPassword);
            }
            if (typeof(T).Name.ToLower() == "string")
            {
                return (T)tmp;
            }
            return new UnityConverter().ConvertJsonToObject<T>(tmp.ToString());
        }
    }
}