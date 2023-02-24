// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download.Extensions
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public static class ImageExtension
    {
        // https://stackoverflow.com/questions/210650/validate-image-from-file-in-c-sharp/9446045#9446045
        // https://gist.github.com/ChuckSavage/dc079e21563ba1402cf6c907d81ac1ca

        private static readonly byte[] png =
            new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        public static bool IsPng(this Stream stream)
        {
            var size = 8;
            if (stream.Length >= size)
            {
                var bytes = new byte[size];
                _ = stream.Read(bytes, 0, bytes.Length);
                _ = stream.Seek(-size, SeekOrigin.Current);
                for (var i = 0; i < size; i++)
                {
                    if (bytes[i] != png[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static async Task<Stream> TrySkipPngHeaderAsync(
            this Stream stream, CancellationToken token = default)
        {
            if (!stream.IsPng())
            {
                return stream;
            }

            var size = 1024;
            var bytes = new byte[size];
            _ = await stream.ReadAsync(bytes, 0, bytes.Length, token);
            _ = stream.Seek(-size, SeekOrigin.Current);

            var skip = 0;
            for (var i = 0; i < size - 188; i++)
            {
                if (bytes[i] == 0x47 && bytes[i + 188] == 0x47)
                {
                    skip = i;
                    break;
                }
            }
            if (skip == 0)
            {
                throw new Exception("Not match MPEG-TS.");
            }

            _ = stream.Seek(skip, SeekOrigin.Current);
            return stream;
        }
    }
}
