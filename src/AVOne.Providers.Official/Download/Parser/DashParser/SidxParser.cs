// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download.Parser.DashParser
{
    using System.IO;
    using System.Text;

    internal class SidxParser
    {
        // Refer to:
        // https://www.jianshu.com/p/d0815ed1b7e6
        // https://blog.csdn.net/u014508743/article/details/102668705
        // https://www.cnblogs.com/gardenofhu/p/10044853.html
        public Sidx Parse(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                var size = (ulong)reader.ReadUInt32();
                var value = reader.ReadInt32();
                var type = "";
                for (var i = 0; i < 4; i++)
                {
                    type += char.ConvertFromUtf32((value >> (i * 8)) & 0x000000ff);
                }

                type = type.Trim();
                var largeSize = size == 1 ? reader.ReadUInt64() : 0;
                var version = reader.ReadByte();
                var flags = reader.ReadBytes(3);
                var referenceID = reader.ReadUInt32();
                var timescale = reader.ReadUInt32();
                var earliestPresentationTime = (ulong)0;
                var firstOffset = (ulong)0;
                if (version == 0)
                {
                    earliestPresentationTime = reader.ReadUInt32();
                    firstOffset = reader.ReadUInt32();
                }
                if (version == 1)
                {
                    earliestPresentationTime = reader.ReadUInt64();
                    firstOffset = reader.ReadUInt64();
                }
                var reserved = reader.ReadUInt16();
                var referenceCount = reader.ReadUInt16();

                var bytes = (uint)0;
                for (var i = 0; i < referenceCount; i++)
                {
                    bytes = reader.ReadUInt32();
                    var referenceType = (bytes >> 31) & 1;
                    var referencedSize = bytes & 0x7fffffff;
                    var subsegmentDuration = reader.ReadUInt32();
                    bytes = reader.ReadUInt32();
                    var startsWithSAP = (bytes >> 31) & 1;
                    var SAP_Type = (bytes >> 29) & 7;
                    var SAP_DeltaTime = bytes & 0x0fffffff;
                }
            }
            return new Sidx();
        }
    }
}
