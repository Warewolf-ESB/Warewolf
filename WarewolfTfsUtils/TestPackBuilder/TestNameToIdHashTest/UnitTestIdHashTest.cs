using System;
using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class UnitTestIdHashTest
    {
        [TestMethod]
        public void TestNameHash_GuidIsSameAsOneFromATRXFile()
        {
            var testHash = UnitTestIdHash.GuidFromString("Dev2.Studio.UI.Tests.AdornerTests.ToolDesigners_MoveLargeView_TabOrderAndDestinationUserNameAndPassword_UiRepondingFine");
            Assert.AreEqual(Guid.Parse("ad5fe5f9-d176-59df-941a-1fe909602984"), testHash, "Hash algorithm does not work as expected");
        }

    }

    internal static class UnitTestIdHash
    {
        private static HashAlgorithm s_provider = new SHA1CryptoServiceProvider();

        internal static HashAlgorithm Provider
        {
            get { return s_provider; }
        }

        /// 

        /// Calculates a hash of the string and copies the first 128 bits of the hash
        /// to a new Guid.
        /// 

        internal static Guid GuidFromString(string data)
        {
            Debug.Assert(!String.IsNullOrEmpty(data));
            byte[] hash = Provider.ComputeHash(System.Text.Encoding.Unicode.GetBytes(data));

            // Guid is always 16 bytes
            Debug.Assert(Guid.Empty.ToByteArray().Length == 16, "Expected Guid to be 16 bytes");

            byte[] toGuid = new byte[16];
            Array.Copy(hash, toGuid, 16);

            return new Guid(toGuid);
        }
    }
}
