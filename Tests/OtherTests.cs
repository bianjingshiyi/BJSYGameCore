using NUnit.Framework;
using System.IO;
namespace BJSYGameCore.Tests
{
    public class OtherTests
    {
        [Test]
        public void getDirTest()
        {
            string path = "A/B/C";
            Assert.AreEqual("C", new DirectoryInfo(path).Name);
        }
    }
}
