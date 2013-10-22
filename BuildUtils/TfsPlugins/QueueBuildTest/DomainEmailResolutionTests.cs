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
            Assert.AreEqual("Ashley.lewis@dev2.co.za", actual, "Domain email resolver cannot get email address for Ashley Lewis");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ResolveDomainEmailAddress_GetEmail")]
        public void ResolveDomainEmailAddress_GetEmail_Huggs_huggsAtdev2()
        {
            var resolveDomainEmailAddress = new DomainEmailAddressResolver();
            //------------Execute Test---------------------------
            var actual = resolveDomainEmailAddress.GetEmailAddress("Hagashen Naidu");

            // Assert ashley.lewisAtdev2.co.za
            Assert.AreEqual("Hagashen.Naidu@dev2.co.za", actual, "Domain email resolver cannot get email address for Hagashen Naidu");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ResolveDomainEmailAddress_GetEmail")]
        public void ResolveDomainEmailAddress_GetEmail_Trav_travAtdev2()
        {
            var resolveDomainEmailAddress = new DomainEmailAddressResolver();
            //------------Execute Test---------------------------
            var actual = resolveDomainEmailAddress.GetEmailAddress("Travis Frisinger");

            // Assert ashley.lewisAtdev2.co.za
            Assert.AreEqual("Travis.Frisinger@dev2.co.za", actual, "Domain email resolver cannot get email address for Travis Frisinger");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ResolveDomainEmailAddress_GetEmail")]
        public void ResolveDomainEmailAddress_GetEmail_Mo_moAtdev2()
        {
            var resolveDomainEmailAddress = new DomainEmailAddressResolver();
            //------------Execute Test---------------------------
            var actual = resolveDomainEmailAddress.GetEmailAddress("Massimo Guerrera");

            // Assert ashley.lewisAtdev2.co.za
            Assert.AreEqual("Massimo.Guerrera@dev2.co.za", actual, "Domain email resolver cannot get email address for Massimo Guerrera");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ResolveDomainEmailAddress_GetEmail")]
        public void ResolveDomainEmailAddress_GetEmail_Tshepo_TshepoAtdev2()
        {
            var resolveDomainEmailAddress = new DomainEmailAddressResolver();
            //------------Execute Test---------------------------
            var actual = resolveDomainEmailAddress.GetEmailAddress("Tshepo Ntlhokoa");

            // Assert ashley.lewisAtdev2.co.za
            Assert.AreEqual("Tshepo.Ntlhokoa@dev2.co.za", actual, "Domain email resolver cannot get email address for Tshepo Ntlhokoa");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ResolveDomainEmailAddress_GetEmail")]
        public void ResolveDomainEmailAddress_GetEmail_Trevor_TrevorAtdev2()
        {
            var resolveDomainEmailAddress = new DomainEmailAddressResolver();
            //------------Execute Test---------------------------
            var actual = resolveDomainEmailAddress.GetEmailAddress("Trevor Williams-Ros");

            // Assert ashley.lewisAtdev2.co.za
            Assert.AreEqual("Trevor.Williams-Ros@dev2.co.za", actual, "Domain email resolver cannot get email address for Trevor Williams-Ros");
        }
    }
}
