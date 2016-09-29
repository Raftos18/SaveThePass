using System;
using System.Text;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace Cryptocraphy
{
    static class Chipher
    {
        const int keyLength = 16;

        /// <summary>
        /// Takes a message and a key and returns the encrypted message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string EncryptMessage(string message, string key)
        {
            if (String.IsNullOrEmpty(message))
                return message;

            key = AdjustKey(key, keyLength);

            // AES, CBC mode with PKCS#7 padding to encrypt
            SymmetricKeyAlgorithmProvider aesCbcPkcs7 = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);

            // Convert the private key to binary 
            IBuffer keyMaterial = CryptographicBuffer.ConvertStringToBinary(key, BinaryStringEncoding.Utf8);

            // Create the private key
            CryptographicKey k = aesCbcPkcs7.CreateSymmetricKey(keyMaterial);

            // Convert the data to byte array
            byte[] plainText = Encoding.UTF8.GetBytes(message); // Data to encrypt 

            //  Encrypt data.
            IBuffer buff = CryptographicEngine.Encrypt(k, CryptographicBuffer.CreateFromByteArray(plainText), keyMaterial);

            // Return the encryptedstring base64 encoded
            return CryptographicBuffer.EncodeToBase64String(buff);
        }

        /// <summary>
        /// Takes a message and a key and returns the decrypted message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string DecryptMessage(string message, string key)
        {
            if (String.IsNullOrEmpty(message))
                return message;

            key = AdjustKey(key, keyLength);

            // Decode the base64 string provided to binary data
            IBuffer val = CryptographicBuffer.DecodeFromBase64String(message);

            // We'll be using AES, CBC mode with PKCS#7 padding to decrypt
            SymmetricKeyAlgorithmProvider aesCbcPkcs7 = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);

            // Convert the private key to binary 
            IBuffer keymaterial = CryptographicBuffer.ConvertStringToBinary(key, BinaryStringEncoding.Utf8);

            // Create the private key 
            CryptographicKey k = aesCbcPkcs7.CreateSymmetricKey(keymaterial);

            // Do the actual decryption 
            IBuffer buff = CryptographicEngine.Decrypt(k, val, keymaterial);

            // return the string as plain text 
            return CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, buff);
        }


        /// <summary>
        /// Converts the key of the user in the appropriate key to 
        /// be to used by the encryption alogorithm. REFACTOR IF WORKS!!!
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyLength"></param>
        /// <returns></returns>
        private static string AdjustKey(string key, int keyLength)
        {
            string finalKey = key;

            if (key.Length > keyLength)
                finalKey = key.Substring(0,16);
            else
            {
                int characterSelectorIndex = 0;
                while (finalKey.Length < keyLength)
                {
                    if (characterSelectorIndex == key.Length)
                        characterSelectorIndex = 0;

                    finalKey += key[characterSelectorIndex];
                    characterSelectorIndex++;
                }
            }
            return finalKey;
        }
    }
}