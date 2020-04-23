/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using System;
using System.Xml.Linq;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.ServiceModel
{
    [TestClass]
    public class ElasticsearchSourceTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSource))]
        public void ElasticsearchSource_Validate_DefaultValues()
        {
            var elasticsearchSource = new ElasticsearchSource();
            Assert.IsTrue(elasticsearchSource.IsSource);
            Assert.IsFalse(elasticsearchSource.IsService);
            Assert.IsFalse(elasticsearchSource.IsFolder);
            Assert.IsFalse(elasticsearchSource.IsReservedService);
            Assert.IsFalse(elasticsearchSource.IsServer);
            Assert.IsFalse(elasticsearchSource.IsResourceVersion);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSource))]
        public void ElasticsearchSource_Constructor_Validate_DefaultValues()
        {
            var elasticsearchSource = new ElasticsearchSource();
            Assert.IsNotNull(elasticsearchSource.ResourceID);
            Assert.AreEqual(Guid.Empty, elasticsearchSource.ResourceID);
            Assert.AreEqual(nameof(ElasticsearchSource), elasticsearchSource.ResourceType);
            Assert.AreEqual("9200", elasticsearchSource.Port);
            Assert.AreEqual(AuthenticationType.Anonymous, elasticsearchSource.AuthenticationType);

        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSource))]
        public void ElasticsearchSource_Validate_ToXml_AuthenticationType_Anonymous()
        {
            const string xmlString = @"<Source ID=""1a82a341-b678-4992-a25a-39cdd57198d4"" Name=""Example Elasticsearch Source"" ResourceType=""ElasticsearchSource"" IsValid=""false"" 
                                               ConnectionString=""HostName=localhost;Port=9200;UserName=;Password=;AuthenticationType=Anonymous"" Type=""ElasticsearchSource"" ServerVersion=""1.4.1.27"" ServerID=""693ca20d-fb17-4044-985a-df3051d6bac7"">
                                          <DisplayName>Example Elasticsearch Source</DisplayName>
                                          <AuthorRoles>
                                          </AuthorRoles>
                                          <ErrorMessages />
                                          <TypeOf>ElasticsearchSource</TypeOf>
                                          <VersionInfo DateTimeStamp=""2017-05-26T14:21:24.3247847+02:00"" Reason="""" User=""NT AUTHORITY\SYSTEM"" VersionNumber=""3"" ResourceId=""1a82a341-b678-4992-a25a-39cdd57198d4"" VersionId=""b1a6de00-3cac-41cd-b0ed-9fac9bb61266"" />
                                        </Source>";

            var xElement = XElement.Parse(xmlString);
            var elasticsearchSource = new ElasticsearchSource(xElement);
            var result = elasticsearchSource.ToXml();

            var elasticsearchSourceWithXml = new ElasticsearchSource(result);
            Assert.AreEqual(nameof(ElasticsearchSource), elasticsearchSourceWithXml.ResourceType);
            Assert.AreEqual("9200", elasticsearchSourceWithXml.Port);
            Assert.AreEqual("localhost", elasticsearchSourceWithXml.HostName);
            Assert.AreEqual("", elasticsearchSourceWithXml.Password);
            Assert.AreEqual("", elasticsearchSourceWithXml.Username);
            Assert.AreEqual(AuthenticationType.Anonymous, elasticsearchSourceWithXml.AuthenticationType);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSource))]
        public void ElasticsearchSource_Validate_ToXml_AuthenticationType_Password()
        {
            const string xmlString = @"<Source ID=""1a82a341-b678-4992-a25a-39cdd57198d4"" Name=""Example Elasticsearch Source"" ResourceType=""ElasticsearchSource"" IsValid=""false"" 
                                               ConnectionString=""HostName=localhost;Port=9200;UserName=test;Password=test;AuthenticationType=Password"" Type=""ElasticsearchSource"" ServerVersion=""1.4.1.27"" ServerID=""693ca20d-fb17-4044-985a-df3051d6bac7"">
                                          <DisplayName>Example Elasticsearch Source</DisplayName>
                                          <AuthorRoles>
                                          </AuthorRoles>
                                          <ErrorMessages />
                                          <TypeOf>ElasticsearchSource</TypeOf>
                                          <VersionInfo DateTimeStamp=""2017-05-26T14:21:24.3247847+02:00"" Reason="""" User=""NT AUTHORITY\SYSTEM"" VersionNumber=""3"" ResourceId=""1a82a341-b678-4992-a25a-39cdd57198d4"" VersionId=""b1a6de00-3cac-41cd-b0ed-9fac9bb61266"" />
                                        </Source>";

            var xElement = XElement.Parse(xmlString);
            var elasticsearchSource = new ElasticsearchSource(xElement);
            var result = elasticsearchSource.ToXml();

            var elasticsearchSourceWithXml = new ElasticsearchSource(result);
            Assert.AreEqual(nameof(ElasticsearchSource), elasticsearchSourceWithXml.ResourceType);
            Assert.AreEqual("9200", elasticsearchSourceWithXml.Port);
            Assert.AreEqual("localhost", elasticsearchSourceWithXml.HostName);
            Assert.AreEqual("test", elasticsearchSourceWithXml.Password);
            Assert.AreEqual("test", elasticsearchSourceWithXml.Username);
            Assert.AreEqual(AuthenticationType.Password, elasticsearchSourceWithXml.AuthenticationType);
        }
    }
}
