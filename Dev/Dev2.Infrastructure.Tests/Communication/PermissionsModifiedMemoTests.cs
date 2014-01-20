using Dev2.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.Communication
{
    [TestClass]
    public class PermissionsModifiedMemoTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("PermissionsModifiedMemo_Constructor")]
        public void PermissionsModifiedMemo_Constructor_Initializes_Properties  ()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            var permissionsModifiedMemo = new PermissionsModifiedMemo();

            //------------Assert Results-------------------------
            Assert.IsNotNull(permissionsModifiedMemo.ModifiedPermissions);

        }
    }
}
