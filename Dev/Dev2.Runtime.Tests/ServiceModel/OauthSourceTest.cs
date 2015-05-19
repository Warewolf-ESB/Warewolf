using System.Xml.Linq;
using Dev2.Data.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ServiceModel
{
    [TestClass]
    public class OauthSourceTest
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OauthSource_Ctor")]
        public void OauthSource_Ctor_Construct_ExpectSource()
        {
            //------------Setup for test--------------------------
            var oauthSource = new OauthSource();
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual(oauthSource.Secret,"");
            Assert.AreEqual(oauthSource.Key,"");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OauthSource_Ctor")]
        public void OauthSource_Ctor_FromXML_Construct_ExpectSource()
        {
            //------------Setup for test--------------------------
            var oauthSource = new OauthSource(XElement.Parse(@"<Source ID=""00000000-0000-0000-0000-000000000000"" Name="""" ResourceType=""OauthSource"" IsValid=""false"" ConnectionString=""Secret=secret;Key=key"" Type=""OauthSource"">
  <DisplayName></DisplayName>
  <Category></Category>
  <AuthorRoles></AuthorRoles>
  <ErrorMessages />
  <TypeOf>OauthSource</TypeOf>
</Source>"));

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual(oauthSource.Secret, "secret");
            Assert.AreEqual(oauthSource.Key, "key");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OauthSource_ToXML")]
        public void OauthSource_ToXML_Construct_ExpectPropertiesSet()
        {
            //------------Setup for test--------------------------
            var oauthSource = new OauthSource(){Key = "key",Secret = "secret"};

            //------------Execute Test---------------------------
            var outxml = oauthSource.ToXml();
            //------------Assert Results-------------------------
            Assert.AreEqual(outxml.ToString(), @"<Source ID=""00000000-0000-0000-0000-000000000000"" Name="""" ResourceType=""OauthSource"" IsValid=""false"" ConnectionString=""Secret=secret;Key=key"" Type=""OauthSource"">
  <DisplayName></DisplayName>
  <Category></Category>
  <AuthorRoles></AuthorRoles>
  <ErrorMessages />
  <TypeOf>OauthSource</TypeOf>
</Source>");
        }

    }
}
