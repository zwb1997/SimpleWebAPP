namespace DBTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    public class TestWordGeneration
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestGUIDGeneration()
        {
            TestContext.WriteLine($"GUID=[{RandomNumberGenerator.GetString(new ReadOnlySpan<char>(
                new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p', 'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'z', 'x', 'c', 'v', 'b', 'n', 'm' })
                , 210)}]");
        }
    }
}
