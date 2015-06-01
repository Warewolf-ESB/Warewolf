using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class TestGetDllListings
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetDllListings_Execute")]
        public void GetDllListings_Execute_NoUserNameNoPasswordNoDomain_ShouldReturnLocalResults()
        {
            //------------Setup for test--------------------------
            var getDllListings = new GetDllListings();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var result = getDllListings.Execute(new Dictionary<string, StringBuilder>(), new Mock<IWorkspace>().Object);
            //------------Assert Results-------------------------
            var msg = serializer.Deserialize<ExecuteMessage>(result);
            Assert.IsNotNull(msg);
            var listing = serializer.Deserialize<List<DllListing>>(msg.Message);
            Assert.IsNotNull(listing);
            Assert.IsTrue(listing.Count>0);
        }
    }
}
