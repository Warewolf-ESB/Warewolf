using Dev2.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev2.Infrastructure.Tests.Services
{
    [TestClass]
    public class ResourceDefinationCleanerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetResourceDefinition_CorrectServiceDef_MessageHasNoErrors()
        {
            //------------Setup for test--------------------------
            var model = new ResourceDefinationCleaner();
            var a = XmlResource.Fetch("Utility - Assign");
            Dev2JsonSerializer dev2JsonSerializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            Assert.IsNotNull(model);
            var cleanDef = model.GetResourceDefinition(false, Guid.NewGuid(), new System.Text.StringBuilder(a.ToString()));
            var message = dev2JsonSerializer.Deserialize<ExecuteMessage>(cleanDef);
            //------------Assert Results-------------------------
            Assert.IsNotNull(cleanDef);
            Assert.IsNotNull(message);
            var hasError = message.HasError == false;
            Assert.IsTrue(hasError);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DecryptAllPasswords_PassThrough()
        {
            //------------Setup for test--------------------------
            var model = new ResourceDefinationCleaner();
            var a = XmlResource.Fetch("Utility - Assign");
            Dev2JsonSerializer dev2JsonSerializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            Assert.IsNotNull(model);
            var cleanDef = model.DecryptAllPasswords(new System.Text.StringBuilder(a.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            //------------Assert Results-------------------------
            Assert.IsNotNull(cleanDef);           
        }
    }
}
