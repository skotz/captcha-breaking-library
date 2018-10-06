using System;
using System.IO;
using System.IO.Compression;

namespace ScottClayton.Utility
{
    /// <summary>
    /// Provides GZip compression methods for byte arrays
    /// </summary>
    public static class Compressor
    {
        /// <summary>
        /// Compressed an array of bytes using GZip
        /// </summary>
        public static byte[] Compress(byte[] input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    zip.Write(input, 0, input.Length);
                }

                ms.Position = 0;

                byte[] compressed = new byte[ms.Length];
                ms.Read(compressed, 0, compressed.Length);

                byte[] gzBuffer = new byte[compressed.Length + 4];
                Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
                Buffer.BlockCopy(BitConverter.GetBytes(input.Length), 0, gzBuffer, 0, 4);

                return gzBuffer;
            }
        }

        /// <summary>
        /// Decompress data that was compressed with GZip
        /// </summary>
        public static byte[] Decompress(byte[] input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                int msgLength = BitConverter.ToInt32(input, 0);
                ms.Write(input, 4, input.Length - 4);

                byte[] buffer = new byte[msgLength];

                ms.Position = 0;

                using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);

                    return buffer;
                }
            }
        }
    }
}