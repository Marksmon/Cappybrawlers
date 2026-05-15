using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Capybrawlers.Core
{
    public class LocalJsonSaveProvider : ISaveProvider
    {
        // IV is prepended to the ciphertext; key is 16 bytes (AES-128).
        // TODO: Move key to a platform-appropriate secure store before shipping.
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("CpyBrwlr2024AESI");

        private static string SavePath => Path.Combine(Application.persistentDataPath, "profile.dat");

        public PlayerProfile Load()
        {
            if (!File.Exists(SavePath))
                return null;

            try
            {
                var encrypted = File.ReadAllBytes(SavePath);
                var json = Decrypt(encrypted);
                return JsonUtility.FromJson<PlayerProfile>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveProvider] Failed to load profile, creating new. Reason: {e.Message}");
                return null;
            }
        }

        public void Save(PlayerProfile profile)
        {
            var json = JsonUtility.ToJson(profile, prettyPrint: false);
            var encrypted = Encrypt(json);
            File.WriteAllBytes(SavePath, encrypted);
        }

        private static byte[] Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.GenerateIV();
            using var enc = aes.CreateEncryptor();
            var plain = Encoding.UTF8.GetBytes(plainText);
            var cipher = enc.TransformFinalBlock(plain, 0, plain.Length);
            var result = new byte[aes.IV.Length + cipher.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipher, 0, result, aes.IV.Length, cipher.Length);
            return result;
        }

        private static string Decrypt(byte[] data)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            var iv = new byte[aes.BlockSize / 8];
            Buffer.BlockCopy(data, 0, iv, 0, iv.Length);
            aes.IV = iv;
            using var dec = aes.CreateDecryptor();
            var cipher = new byte[data.Length - iv.Length];
            Buffer.BlockCopy(data, iv.Length, cipher, 0, cipher.Length);
            var plain = dec.TransformFinalBlock(cipher, 0, cipher.Length);
            return Encoding.UTF8.GetString(plain);
        }
    }
}
