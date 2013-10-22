using Microsoft.VisualStudio.TestTools.UnitTesting;
using ResolveDomainEmailAddress;

namespace QueueBuildTest
{
    [TestClass]
    public class DomainEmailResolutionTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ResolveDomainEmailAddress_GetEmail")]
        public void ResolveDomainEmailAddress_GetEmail_Ashley_ashleylewisAtdev2()
        {
            var resolveDomainEmailAddress = new DomainEmailAddressResolver();
            //------------Execute Test---------------------------
            var actual = resolveDomainEmailAddress.GetEmailAddress("Ashley Lewis");

            // Assert ashley.lewisAtdev2.co.za
            Assert.AreEqual("ashley.lewis@dev2.co.za", actual, "Domain email resolver cannot get email address for Ashley Lewis");
        }
    }
}
