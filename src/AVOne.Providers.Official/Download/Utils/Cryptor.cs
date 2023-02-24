// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download.Utils
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;

    internal class Cryptor
    {
        public static byte[] HexToBytes(string str)
        {
            if (str.ToLower().StartsWith("0x"))
            {
                str = str.Remove(0, 2);
            }

            var bytes = new byte[str.Length / 2];
            for (var i = 0; i < str.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
            }

            return bytes;
        }

        public async Task AES128Decrypt(Stream input, string key, string iv,
            Stream output, CancellationToken token = default)
        {
            using var aes = Aes.Create();
            aes.BlockSize = 128;
            aes.KeySize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Convert.FromBase64String(key);
            aes.IV = HexToBytes(iv);
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var stream = new CryptoStream(input, decryptor, CryptoStreamMode.Read);
            await stream.CopyToAsync(output, 4096, token);
        }
    }
}
