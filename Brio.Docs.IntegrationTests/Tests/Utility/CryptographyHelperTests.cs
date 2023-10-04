using System;
using Brio.Docs.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brio.Docs.Tests.Utility
{
    [TestClass]
    public class CryptographyHelperTests
    {
        private static CryptographyHelper helper;

        [ClassInitialize]
        public static void Setup(TestContext unused)
        {
            helper = new CryptographyHelper();
        }

        [TestMethod]
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void VerifyPasswordHash_PasswordIsInvalid_ReturnsFalse(string pass)
        {
            var result = helper.VerifyPasswordHash(pass, new byte[1], new byte[1]);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void VerifyPasswordHash_HashIsEmpty_ReturnsFalse()
        {
            var pass = "pass";

            var result = helper.VerifyPasswordHash(pass, Array.Empty<byte>(), new byte[1]);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void VerifyPasswordHash_SaltIsEmpty_ReturnsFalse()
        {
            var pass = "pass";

            var result = helper.VerifyPasswordHash(pass, new byte[1], Array.Empty<byte>());

            Assert.IsFalse(result);
        }

        [TestMethod]
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        [ExpectedException(typeof(ArgumentException))]
        public void CreatePasswordHash_PasswordIsInvalid_RaisesArgumentException(string pass)
        {
            helper.CreatePasswordHash(pass, out _, out var _);

            Assert.Fail();
        }

        [TestMethod]
        public void CreatePasswordHash_PasswordIsValid_CreatesHashAndSalt()
        {
            string pass = "pass";

            helper.CreatePasswordHash(pass, out var hash, out var salt);

            Assert.IsNotNull(hash);
            Assert.IsNotNull(salt);
            Assert.IsTrue(hash.Length > 0);
            Assert.IsTrue(salt.Length > 0);
        }
    }
}
