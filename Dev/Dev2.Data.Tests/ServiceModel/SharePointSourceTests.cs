using System.IO;
using System.Xml.Linq;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.ServiceModel
{
    [TestClass]
    public class SharePointSourceTests
    {
        [TestMethod]
        public void SharePointSource_ShouldHaveConstructorAndSetDefaultValues()
        {
            var sharepointSource = new SharepointSource();
            Assert.IsNotNull(sharepointSource);
            Assert.AreEqual("SharepointServerSource", sharepointSource.ResourceType);
            sharepointSource.AuthenticationType = AuthenticationType.User;
            var xElement = sharepointSource.ToXml();
            Assert.IsNotNull(xElement);
        }

        [TestMethod]
        public void GivenNoURL_SharePointSource_TestConnection_ShouldReturn()
        {
            var sharepointSource = new SharepointSource();
            Assert.IsNotNull(sharepointSource);
            Assert.AreEqual("SharepointServerSource", sharepointSource.ResourceType);
            sharepointSource.AuthenticationType = AuthenticationType.User;
            var testConnection = sharepointSource.TestConnection();
            Assert.IsTrue(testConnection.Contains("Test Failed"));
        }

        [TestMethod]
        public void GivenXelemt_SharePointSource_ShouldHaveConstructorAndSetDefaultValues()
        {
            const string conStr = @"<Source ID=""2aa3fdba-e0c3-47dd-8dd5-e6f24aaf5c7a"" Name=""test server"" Type=""Dev2Server"" ConnectionString=""AppServerUri=http://178.63.172.163:3142/dsf;WebServerPort=3142;AuthenticationType=Public;UserName=;Password="" Version=""1.0"" ResourceType=""Server"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"">
  <TypeOf>Dev2Server</TypeOf>
  <DisplayName>test server</DisplayName>
  <Category>WAREWOLF SERVERS</Category>
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>1ia51dqx+BIMQ4QgLt+DuKtTBUk=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>Wqd39EqkFE66XVETuuAqZveoTk3JiWtAk8m1m4QykeqY4/xQmdqRRSaEfYBr7EHsycI3STuILCjsz4OZgYQ2QL41jorbwULO3NxAEhu4nrb2EolpoNSJkahfL/N9X5CvLNwpburD4/bPMG2jYegVublIxE50yF6ZZWG5XiB6SF8=</SignatureValue>
  </Signature>
</Source>";

            var xElement = XElement.Parse(conStr);                        
            var sharepointSource = new SharepointSource(xElement);
            Assert.IsNotNull(sharepointSource);
            Assert.IsNotNull(sharepointSource.ResourceID);
            Assert.AreEqual("SharepointServerSource", sharepointSource.ResourceType);
        }
    }
}
