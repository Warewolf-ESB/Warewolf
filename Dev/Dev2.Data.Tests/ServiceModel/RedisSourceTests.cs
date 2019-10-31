/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
    public class RedisSourceTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisSource))]
        public void RedisSource_Validate_DefaultValues()
        {
            var redisSource = new RedisSource();
            Assert.IsTrue(redisSource.IsSource);
            Assert.IsFalse(redisSource.IsService);
            Assert.IsFalse(redisSource.IsFolder);
            Assert.IsFalse(redisSource.IsReservedService);
            Assert.IsFalse(redisSource.IsServer);
            Assert.IsFalse(redisSource.IsResourceVersion);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisSource))]
        public void RedisSource_Constructor_Validate_DefaultValues()
        {
            var redisSource = new RedisSource();
            Assert.IsNotNull(redisSource.ResourceID);
            Assert.AreEqual(Guid.Empty, redisSource.ResourceID);
            Assert.AreEqual(nameof(RedisSource), redisSource.ResourceType);
            Assert.AreEqual("6379", redisSource.Port);
            Assert.AreEqual(AuthenticationType.Anonymous, redisSource.AuthenticationType);

        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisSource))]
        public void RedisSource_Validate_ToXml_AuthenticationType_Anonymous()
        {
            const string xmlString = @"<Source ID=""1a82a341-b678-4992-a25a-39cdd57198d4"" Name=""Example Redis Source"" ResourceType=""RedisSource"" IsValid=""false"" 
                                               ConnectionString=""HostName=localhost;Port=6379;UserName=warewolf;Password=;AuthenticationType=Anonymous"" Type=""RedisSource"" ServerVersion=""1.4.1.27"" ServerID=""693ca20d-fb17-4044-985a-df3051d6bac7"">
                                          <DisplayName>Example Redis Source</DisplayName>
                                          <AuthorRoles>
                                          </AuthorRoles>
                                          <ErrorMessages />
                                          <TypeOf>RedisSource</TypeOf>
                                          <VersionInfo DateTimeStamp=""2017-05-26T14:21:24.3247847+02:00"" Reason="""" User=""NT AUTHORITY\SYSTEM"" VersionNumber=""3"" ResourceId=""1a82a341-b678-4992-a25a-39cdd57198d4"" VersionId=""b1a6de00-3cac-41cd-b0ed-9fac9bb61266"" />
                                        </Source>";

            var xElement = XElement.Parse(xmlString);
            var redisSource = new RedisSource(xElement);
            var result = redisSource.ToXml();

            var redisSourceWithXml = new RedisSource(result);
            Assert.AreEqual(nameof(RedisSource), redisSourceWithXml.ResourceType);
            Assert.AreEqual("6379", redisSourceWithXml.Port);
            Assert.AreEqual("localhost", redisSourceWithXml.HostName);
            Assert.AreEqual("", redisSourceWithXml.Password);
            Assert.AreEqual(AuthenticationType.Anonymous, redisSourceWithXml.AuthenticationType);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisSource))]
        public void RedisSource_Validate_ToXml_AuthenticationType_Password()
        {
            const string xmlString = @"<Source ID=""1a82a341-b678-4992-a25a-39cdd57198d4"" Name=""Example Redis Source"" ResourceType=""RedisSource"" IsValid=""false"" 
                                               ConnectionString=""HostName=localhost;Port=6379;UserName=warewolf;Password=test123;AuthenticationType=Password"" Type=""RedisSource"" ServerVersion=""1.4.1.27"" ServerID=""693ca20d-fb17-4044-985a-df3051d6bac7"">
                                          <DisplayName>Example Redis Source</DisplayName>
                                          <AuthorRoles>
                                          </AuthorRoles>
                                          <ErrorMessages />
                                          <TypeOf>RedisSource</TypeOf>
                                          <VersionInfo DateTimeStamp=""2017-05-26T14:21:24.3247847+02:00"" Reason="""" User=""NT AUTHORITY\SYSTEM"" VersionNumber=""3"" ResourceId=""1a82a341-b678-4992-a25a-39cdd57198d4"" VersionId=""b1a6de00-3cac-41cd-b0ed-9fac9bb61266"" />
                                        </Source>";

            var xElement = XElement.Parse(xmlString);
            var redisSource = new RedisSource(xElement);
            var result = redisSource.ToXml();

            var redisSourceWithXml = new RedisSource(result);
            Assert.AreEqual(nameof(RedisSource), redisSourceWithXml.ResourceType);
            Assert.AreEqual("6379", redisSourceWithXml.Port);
            Assert.AreEqual("localhost", redisSourceWithXml.HostName);
            Assert.AreEqual("test123", redisSourceWithXml.Password);
            Assert.AreEqual(AuthenticationType.Password, redisSourceWithXml.AuthenticationType);
        }
    }
}
