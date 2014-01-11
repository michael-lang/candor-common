using NUnit.Framework;

namespace Candor.Security.Cryptography
{
    [TestFixture]
    public class SHA2HashProviderTests
    {
        [Test]
        public void GetSaltLength16()
        {
            var salt = new SHA2HashProvider("x").GetSalt(16);

            Assert.AreEqual(16, salt.Length);
        }
        [Test]
        public void GetSaltLength32()
        {
            var salt = new SHA2HashProvider("x").GetSalt(32);

            Assert.AreEqual(32, salt.Length);
        }
    }
}
