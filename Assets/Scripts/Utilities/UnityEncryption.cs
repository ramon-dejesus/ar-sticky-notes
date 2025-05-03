using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public class UnityEncryption
{
    public string Encrypt(string plaintext, string pwd = "")
    {
        if (string.IsNullOrEmpty(plaintext))
        {
            return "";
        }
        var bts = System.Text.Encoding.UTF8.GetBytes(plaintext);
        var encryptor = GetEncryptor(pwd);
        using (var ms = new MemoryStream())
        {
            using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(bts, 0, bts.Length);
            }
            return Convert.ToBase64String(ms.ToArray());
        }
    }

    public string Decrypt(string cipherText, string pwd = "")
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            return "";
        }
        var bts = Convert.FromBase64String(cipherText);
        var encryptor = GetEncryptor(pwd);
        using (var ms = new MemoryStream())
        {
            using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(bts, 0, bts.Length);
            }
            return System.Text.Encoding.UTF8.GetString(ms.ToArray());
        }
    }
    private Aes GetEncryptor(string pwd)
    {
        var key = GetEncryptionKey(pwd);
        var encryptor = Aes.Create();
        encryptor.Key = key.GetBytes(32);
        encryptor.IV = key.GetBytes(16);
        return encryptor;
    }
    private Rfc2898DeriveBytes GetEncryptionKey(string pwd)
    {
        pwd = pwd == "" ? GetUniquePassword() : pwd;
        return new Rfc2898DeriveBytes(pwd, 20);
    }
    private string GetUniquePassword()
    {
        var appName = Application.productName;
        var deviceName = SystemInfo.deviceUniqueIdentifier;
        var pwd = appName + deviceName;
        return pwd;
    }
}
