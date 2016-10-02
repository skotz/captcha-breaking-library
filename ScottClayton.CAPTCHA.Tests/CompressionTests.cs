using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScottClayton.Utility;
using System.Linq;

namespace ScottClayton.CAPTCHA.Tests
{
    [TestClass]
    public class CompressionTests
    {
        [TestMethod]
        public void TestCompressDecompress()
        {
            byte[] data = new byte[] { 0, 1, 50, 255 };
            byte[] compress = Compressor.Compress(data);
            byte[] decompress = Compressor.Decompress(compress);

            Assert.IsTrue(data.SequenceEqual(decompress));
        }
    }
}