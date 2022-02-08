using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CommonLibrary.Ciphers
{
    public class DES
    {
        private readonly DESCryptoServiceProvider _cipher;
        
        public DES(byte[] key)
        {
            Array.Resize(ref key, 8);
            _cipher = new DESCryptoServiceProvider
            {
                Mode = CipherMode.ECB,
                Key = key,
                Padding = PaddingMode.PKCS7
            };
            
        }
        
        public byte[] Encrypt(byte[] message)
        {
            var encryptedStream = new MemoryStream();
            var cryptoStream = new CryptoStream(encryptedStream, _cipher.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(message, 0, message.Length);
            return encryptedStream.ToArray();
        }

        public byte[] Decrypt(byte[] message)
        {
            var decryptedStream = new MemoryStream();
            var cryptoStream = new CryptoStream(decryptedStream, _cipher.CreateDecryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(message, 0, message.Length);
            return decryptedStream.ToArray();
        }
    }
}