using System.IO;
using System.Text;
using Dev2.Common.Common;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.Security
{
    [TestClass]
    public class DecryptAllPasswordsTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("FetchResourceDefinition_DecryptAllPasswords")]
        public void FetchResourceDefinition_DecryptAllPasswords_WithGENDEVDbSourceResource_ConnectionStringHasBeenDecrypted()
        {
            //------------Setup for test--------------------------
            var fetchResourceDefinition = new FetchResourceDefinition();
            var resourceDefinition = File.ReadAllText(@"XML\GenDev.xml");
            var stringBuilder = new StringBuilder(resourceDefinition);

            //------------Execute Test---------------------------
            var plaintextResourceDefinition = fetchResourceDefinition.DecryptAllPasswords(stringBuilder);

            //------------Assert Results-------------------------
            Assert.IsTrue(plaintextResourceDefinition.Contains("rsaklfsvrgendev"), "Cannot decrypt resource definition.");
        }
    }
}
