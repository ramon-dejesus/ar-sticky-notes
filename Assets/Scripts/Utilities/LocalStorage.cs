using UnityEngine;
using System.IO;
using System;

namespace ARStickyNotes.Utilities
{
    /// <summary>
    /// Provides a local storage mechanism for saving and loading data using the file system or PlayerPrefs,
    /// with optional encryption support.
    /// </summary>
    public class LocalStorage
    {
        /// <summary>
        /// The root directory path used for file-based storage.
        /// </summary>
        private string StoragePath { get; set; } = "";

        /// <summary>
        /// The encryption password used for securing stored data.
        /// </summary>
        private string EncryptionPassword { get; set; } = "";
        
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalStorage"/> class.
        /// </summary>
        /// <param name="locationPath">The file system path for data storage.</param>
        /// <param name="password">An optional encryption password.</param>
        public LocalStorage(string locationPath = "", string password = "")
        {
            StoragePath = locationPath;
            EncryptionPassword = password;
        }

        /// <summary>
        /// Constructs the full file path for a given key.
        /// </summary>
        /// <param name="key">The key to use as a filename.</param>
        /// <returns>Full file path including extension.</returns>
        /// <exception cref="Exception">Thrown if the storage path or key is empty.</exception>
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

        /// <summary>
        /// Saves an object to persistent storage, using file system if available or PlayerPrefs otherwise.
        /// Data can be encrypted if a password is provided.
        /// </summary>
        /// <param name="key">The key or filename identifier.</param>
        /// <param name="value">The object to serialize and store.</param>
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

        /// <summary>
        /// Saves a raw value (object or string) into PlayerPrefs with optional encryption.
        /// </summary>
        /// <param name="key">The PlayerPrefs key name.</param>
        /// <param name="value">The value to save. If null, the key is deleted.</param>
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

        /// <summary>
        /// Deletes a stored object from the file system or PlayerPrefs.
        /// </summary>
        /// <param name="key">The key or filename of the object to delete.</param>
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

        /// <summary>
        /// Deletes a stored key from PlayerPrefs.
        /// </summary>
        /// <param name="key">The PlayerPrefs key to delete.</param>
        public void DeleteValue(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new Exception("The storage key is empty.");
            }
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Retrieves and deserializes an object from the file system or PlayerPrefs.
        /// </summary>
        /// <typeparam name="T">The type to deserialize into.</typeparam>
        /// <param name="key">The key or filename identifier.</param>
        /// <returns>The deserialized object, or default value of type <typeparamref name="T"/> if not found.</returns>
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

        /// <summary>
        /// Retrieves a stored value from PlayerPrefs, with optional decryption and deserialization.
        /// </summary>
        /// <typeparam name="T">The expected return type.</typeparam>
        /// <param name="key">The PlayerPrefs key to retrieve.</param>
        /// <returns>The value as type <typeparamref name="T"/>.</returns>
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