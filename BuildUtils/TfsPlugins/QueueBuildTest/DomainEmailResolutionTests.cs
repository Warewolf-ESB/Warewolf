using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QueueBuildTest
{
    [TestClass]
    public class DomainEmailResolutionTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ResolveDomainEmailAddress_GetEmail")]
        public void ResolveDomainEmailAddress_GetEmail_Ashley_ashleylewisAtdev2coza()
        {
            var resolveDomainEmailAddress = new DomainEmailResolver();
            //------------Execute Test---------------------------

            // Assert ashley.lewisAtdev2.co.za
        }
    }
}
