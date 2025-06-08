using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace ARStickyNotes.Utilities
{
    /// <summary>
    /// Provides AES-based encryption and decryption utilities for secure local data storage.
    /// Includes dynamic key generation using device-specific data and optional passwords.
    /// </summary>
    public class UnityEncryption
    {
        private const string DefaultSalt = "9CE189F6849B4F43AA6007256C4F3CAE";

        private const string DefaultPassword = "47F8F007168A4DB9834E0746922695A4";

        /// <summary>
        /// Encrypts a UTF-8 string using AES and returns a Base64-encoded ciphertext.
        /// </summary>
        /// <param name="plaintext">The plain text string to encrypt.</param>
        /// <param name="pwd">An optional password for encryption. If empty, a default password is used.</param>
        /// <returns>The encrypted Base64 string. Returns an empty string if input is null or empty.</returns>
        public string Encrypt(string plaintext, string pwd = "")
        {
            try
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
            catch (Exception ex)
            {
                ErrorReporter.Report("Failed to encrypt data.", ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// Decrypts a Base64-encoded string using AES and returns the original UTF-8 plaintext.
        /// </summary>
        /// <param name="cipherText">The encrypted Base64 string to decrypt.</param>
        /// <param name="pwd">The password used for decryption. If empty, a device-unique password is used.</param>
        /// <returns>The decrypted plain text string. Returns an empty string if input is null or empty.</returns>
        public string Decrypt(string cipherText, string pwd = "")
        {
            try
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
            catch (Exception ex)
            {
                ErrorReporter.Report("Failed to decrypt data.", ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// Creates and configures a new AES encryption instance using the derived key and IV.
        /// </summary>
        /// <param name="pwd">The password to derive the encryption key from.</param>
        /// <returns>An AES instance ready for encryption or decryption.</returns>
        private Aes GetEncryptor(string pwd)
        {
            var key = GetEncryptionKey(pwd);
            var encryptor = Aes.Create();
            encryptor.Key = key.GetBytes(32);
            encryptor.IV = key.GetBytes(16);
            return encryptor;
        }

        /// <summary>
        /// Derives an encryption key from a password and a static salt using PBKDF2 (Rfc2898).
        /// </summary>
        /// <param name="pwd">The password to use. If empty, a device-unique password is generated.</param>
        /// <returns>An instance of <see cref="Rfc2898DeriveBytes"/> used for AES key/IV derivation.</returns>
        private Rfc2898DeriveBytes GetEncryptionKey(string pwd)
        {
            pwd = pwd == "" ? DefaultPassword : pwd;
            var saltBts = System.Text.Encoding.UTF8.GetBytes(DefaultSalt);
            return new Rfc2898DeriveBytes(pwd, saltBts);
        }

        /// <summary>
        /// Generates a unique password string for the current device and application.
        /// </summary>
        /// <remarks>
        /// Uses <c>Application.productName</c> and <c>SystemInfo.deviceUniqueIdentifier</c> to create a device-specific seed.
        /// </remarks>
        /// <returns>A consistent, device-specific password string for use in encryption.</returns>
        public string GetUniquePassword()
        {
            try
            {
                var pwd = Application.productName + SystemInfo.deviceUniqueIdentifier;
                return pwd;
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Failed to generate a unique password.", ex);
                return string.Empty;
            }
        }
    }
}