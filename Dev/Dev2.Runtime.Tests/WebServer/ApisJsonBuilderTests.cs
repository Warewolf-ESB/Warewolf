using System;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Enums;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.WebServer;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;


namespace Dev2.Tests.Runtime.WebServer
{
    [TestClass]
    public class ApisJsonBuilderTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ApisJsonBuilder_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApisJsonBuilder_Constructor_NullAuthorizationService_Exception()
        {
            //------------Setup for test--------------------------
            
            
            //------------Execute Test---------------------------
            
            new ApisJsonBuilder(null,new Mock<IResourceCatalog>().Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ApisJsonBuilder_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApisJsonBuilder_Constructor_NullResourceCatalog_Exception()
        {
            //------------Setup for test--------------------------
            
            
            //------------Execute Test---------------------------
            
            new ApisJsonBuilder(new Mock<IAuthorizationService>().Object,null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ApisJsonBuilder_Constructor")]
        public void ApisJsonBuilder_Constructor_AuthorizationService_PropertySet()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var builder = new ApisJsonBuilder(new Mock<IAuthorizationService>().Object,new Mock<IResourceCatalog>().Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(builder.AuthorizationService);
            Assert.IsNotNull(builder.ResourceCatalog);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ApisJsonBuilder_BuildForPath")]
        public void ApisJsonBuilder_BuildForPath_NullPath_ShouldBuildForWholeCatalog()
        {
            //------------Setup for test--------------------------
            EnvironmentVariables.DnsName = "http://localhost/";
            EnvironmentVariables.Port = 3142;
            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.View, It.IsAny<string>())).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Execute, It.IsAny<string>())).Returns(true);
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var resources = new List<IResource>();
            var resource1 = new Resource { ResourceName = "Execution Engine Test", FilePath = EnvironmentVariables.ResourcePath + "\\Acceptance Testing Resources\\Execution Engine\\Execution Engine Test.xml", ResourceType = "WorkflowService" };
            var resource2 = new Resource { ResourceName = "Hello World", FilePath = EnvironmentVariables.ResourcePath + "\\Hello World.xml", ResourceType = "WorkflowService" };
            var resource3 = new Resource { ResourceName = "9139Local", FilePath = EnvironmentVariables.ResourcePath + "\\Acceptance Testing Resources\\9139Local.xml", ResourceType = "WorkflowService" };
            
            resources.Add(resource1);
            resources.Add(resource2);
            resources.Add(resource3);
            mockResourceCatalog.Setup(catalog => catalog.GetResourceList(It.IsAny<Guid>())).Returns(resources);
            var apisJsonBuilder = new ApisJsonBuilder(mockAuthorizationService.Object,mockResourceCatalog.Object);
            var exceptedApisJson = GetExceptedApisJsonForServerNoSecurity();
            //------------Execute Test---------------------------
            var apisJson = apisJsonBuilder.BuildForPath(null,false);
            //------------Assert Results-------------------------
            Assert.IsNotNull(apisJson);
            Assert.AreEqual(exceptedApisJson,apisJson);
            Assert.AreEqual(exceptedApisJson.Apis.Count, apisJson.Apis.Count);
            Assert.AreEqual(exceptedApisJson.Apis[0].BaseUrl.Contains("secure"), apisJson.Apis[0].BaseUrl.Contains("secure"));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ApisJsonBuilder_BuildForPath")]
        public void ApisJsonBuilder_BuildForPath_WithPath_ShouldBuildForResourcesAtPath()
        {
            //------------Setup for test--------------------------
            EnvironmentVariables.DnsName = "http://localhost/";
            EnvironmentVariables.Port = 3142;
            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.View, It.IsAny<string>())).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Execute, It.IsAny<string>())).Returns(true);
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var resources = new List<IResource>();
            var resource1 = new Resource { ResourceName = "Execution Engine Test", FilePath = EnvironmentVariables.ResourcePath + "\\Acceptance Testing Resources\\Execution Engine\\Execution Engine Test.xml", ResourceType = "WorkflowService" };
            var resource2 = new Resource { ResourceName = "Hello World", FilePath = EnvironmentVariables.ResourcePath + "\\Hello World.xml", ResourceType = "WorkflowService" };
            var resource3 = new Resource { ResourceName = "9139Local", FilePath = EnvironmentVariables.ResourcePath + "\\Acceptance Testing Resources\\9139Local.xml", ResourceType = "WorkflowService" };
            
            resources.Add(resource1);
            resources.Add(resource2);
            resources.Add(resource3);
            mockResourceCatalog.Setup(catalog => catalog.GetResourceList(It.IsAny<Guid>())).Returns(resources);
            var apisJsonBuilder = new ApisJsonBuilder(mockAuthorizationService.Object,mockResourceCatalog.Object);
            var exceptedApisJson = GetExceptedApisJsonForServerPathWithNoSubDirectories();
            //------------Execute Test---------------------------
            var apisJson = apisJsonBuilder.BuildForPath("Acceptance Testing Resources\\Execution Engine",false);
            //------------Assert Results-------------------------
            Assert.IsNotNull(apisJson);
            Assert.AreEqual(exceptedApisJson,apisJson);
            Assert.AreEqual(exceptedApisJson.Apis.Count,apisJson.Apis.Count);
            Assert.AreEqual(exceptedApisJson.Apis[0].Name,apisJson.Apis[0].Name);
            Assert.AreEqual(exceptedApisJson.Apis[0].BaseUrl,apisJson.Apis[0].BaseUrl);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ApisJsonBuilder_BuildForPath")]
        public void ApisJsonBuilder_BuildForPath_WithPathHasSubDirectories_ShouldBuildForAllResourcesAtPath()
        {
            //------------Setup for test--------------------------
            EnvironmentVariables.DnsName = "http://localhost/";
            EnvironmentVariables.Port = 3142;
            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.View, It.IsAny<string>())).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Execute, It.IsAny<string>())).Returns(true);
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var resources = new List<IResource>();
            var resource1 = new Resource { ResourceName = "Execution Engine Test", FilePath = EnvironmentVariables.ResourcePath + "\\Acceptance Testing Resources\\Execution Engine\\Execution Engine Test.xml", ResourceType = "WorkflowService" };
            var resource2 = new Resource { ResourceName = "Hello World", FilePath = EnvironmentVariables.ResourcePath + "\\Hello World.xml", ResourceType = "WorkflowService" };
            var resource3 = new Resource { ResourceName = "9139Local", FilePath = EnvironmentVariables.ResourcePath + "\\Acceptance Testing Resources\\9139Local.xml", ResourceType = "WorkflowService" };
            
            resources.Add(resource1);
            resources.Add(resource2);
            resources.Add(resource3);
            mockResourceCatalog.Setup(catalog => catalog.GetResourceList(It.IsAny<Guid>())).Returns(resources);
            var apisJsonBuilder = new ApisJsonBuilder(mockAuthorizationService.Object,mockResourceCatalog.Object);
            var exceptedApisJson = GetExceptedApisJsonForServerWithSubDirectories();
            //------------Execute Test---------------------------
            var apisJson = apisJsonBuilder.BuildForPath("Acceptance Testing Resources",false);
            //------------Assert Results-------------------------
            Assert.IsNotNull(apisJson);
            Assert.AreEqual(exceptedApisJson,apisJson);
            Assert.AreEqual(exceptedApisJson.Apis.Count,apisJson.Apis.Count);
            Assert.AreEqual(exceptedApisJson.Apis[0].Name,apisJson.Apis[0].Name);
            Assert.AreEqual(exceptedApisJson.Apis[0].BaseUrl,apisJson.Apis[0].BaseUrl);
            Assert.AreEqual(exceptedApisJson.Apis[1].Name, apisJson.Apis[1].Name);
            Assert.AreEqual(exceptedApisJson.Apis[1].BaseUrl, apisJson.Apis[1].BaseUrl);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ApisJsonBuilder_BuildForPath")]
        public void ApisJsonBuilder_BuildForPath_OnlyAuthorized_ShouldBuildForWholeCatalog()
        {
            //------------Setup for test--------------------------
            EnvironmentVariables.DnsName = "http://localhost/";
            EnvironmentVariables.Port = 3142;
            var unAuthortizedResourceID = Guid.NewGuid();
            var authorizedResource1 = Guid.NewGuid();
            var authorizedResource2 = Guid.NewGuid();
            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.View, unAuthortizedResourceID.ToString())).Returns(false);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Execute, unAuthortizedResourceID.ToString())).Returns(false);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.View, authorizedResource1.ToString())).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Execute, authorizedResource1.ToString())).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.View, authorizedResource2.ToString())).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Execute, authorizedResource2.ToString())).Returns(true);
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var resources = new List<IResource>();
            var resource1 = new Resource { ResourceID = unAuthortizedResourceID, ResourceName = "Execution Engine Test", FilePath = EnvironmentVariables.ResourcePath + "\\Acceptance Testing Resources\\Execution Engine\\Execution Engine Test.xml", ResourceType = "WorkflowService" };
            var resource2 = new Resource { ResourceID = authorizedResource1, ResourceName = "Hello World", FilePath = EnvironmentVariables.ResourcePath + "\\Hello World.xml", ResourceType = "WorkflowService" };
            var resource3 = new Resource { ResourceID = authorizedResource2, ResourceName = "9139Local", FilePath = EnvironmentVariables.ResourcePath + "\\Acceptance Testing Resources\\9139Local.xml", ResourceType = "WorkflowService" };

            resources.Add(resource1);
            resources.Add(resource2);
            resources.Add(resource3);
            mockResourceCatalog.Setup(catalog => catalog.GetResourceList(It.IsAny<Guid>())).Returns(resources);
            var apisJsonBuilder = new ApisJsonBuilder(mockAuthorizationService.Object, mockResourceCatalog.Object);
            var exceptedApisJson = GetExceptedApisJsonForServerSecurity();
            //------------Execute Test---------------------------
            var apisJson = apisJsonBuilder.BuildForPath(null,false);
            //------------Assert Results-------------------------
            Assert.IsNotNull(apisJson);
            Assert.AreEqual(exceptedApisJson, apisJson);
            Assert.AreEqual(exceptedApisJson.Apis.Count, apisJson.Apis.Count);
            Assert.AreEqual(exceptedApisJson.Apis[0].BaseUrl, apisJson.Apis[0].BaseUrl);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ApisJsonBuilder_BuildForPath")]
        public void ApisJsonBuilder_BuildForPath_OnlyAuthorized_MixPublicWithSecure_ShouldBuildForWholeCatalog()
        {
            //------------Setup for test--------------------------
            EnvironmentVariables.DnsName = "http://localhost/";
            EnvironmentVariables.Port = 3142;
            var publicAuthorizedResourceID = Guid.NewGuid();
            var authorizedResource1 = Guid.NewGuid();
            var authorizedResource2 = Guid.NewGuid();
            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.View, publicAuthorizedResourceID.ToString())).Returns(false);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Execute, publicAuthorizedResourceID.ToString())).Returns(false);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.View, authorizedResource1.ToString())).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Execute, authorizedResource1.ToString())).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.View, authorizedResource2.ToString())).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Execute, authorizedResource2.ToString())).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(GlobalConstants.GenericPrincipal, AuthorizationContext.View, publicAuthorizedResourceID.ToString())).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(GlobalConstants.GenericPrincipal, AuthorizationContext.Execute, publicAuthorizedResourceID.ToString())).Returns(true);
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var resources = new List<IResource>();
            var resource1 = new Resource { ResourceID = publicAuthorizedResourceID, ResourceName = "Execution Engine Test", FilePath = EnvironmentVariables.ResourcePath + "\\Acceptance Testing Resources\\Execution Engine\\Execution Engine Test.xml", ResourceType = "WorkflowService" };
            var resource2 = new Resource { ResourceID = authorizedResource1, ResourceName = "Hello World", FilePath = EnvironmentVariables.ResourcePath + "\\Hello World.xml", ResourceType = "WorkflowService" };
            var resource3 = new Resource { ResourceID = authorizedResource2, ResourceName = "9139Local", FilePath = EnvironmentVariables.ResourcePath + "\\Acceptance Testing Resources\\9139Local.xml", ResourceType = "WorkflowService" };

            resources.Add(resource1);
            resources.Add(resource2);
            resources.Add(resource3);
            mockResourceCatalog.Setup(catalog => catalog.GetResourceList(It.IsAny<Guid>())).Returns(resources);
            var apisJsonBuilder = new ApisJsonBuilder(mockAuthorizationService.Object, mockResourceCatalog.Object);
            var exceptedApisJson = GetExceptedApisJsonForServerSecurity();
            //------------Execute Test---------------------------
            var apisJson = apisJsonBuilder.BuildForPath(null,false);
            //------------Assert Results-------------------------
            Assert.IsNotNull(apisJson);
            Assert.AreEqual(exceptedApisJson, apisJson);
            Assert.AreEqual(exceptedApisJson.Apis.Count, apisJson.Apis.Count);
            Assert.AreEqual(exceptedApisJson.Apis[0].BaseUrl, apisJson.Apis[0].BaseUrl);
            Assert.AreEqual(exceptedApisJson.Apis[1].BaseUrl, apisJson.Apis[1].BaseUrl);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        public void ApisJsonBuilder_GetHashCode()
        {
            var apiJson = new ApisJson
            {
                Name = EnvironmentVariables.PublicWebServerUri,
                Description = "",
                SpecificationVersion = "0.15",
                Apis = new List<SingleApi>()
            };
            Assert.IsInstanceOfType(apiJson.GetHashCode(), typeof(int), "ApisJson object did not hash to the expected hash code.");
        }

        static ApisJson GetExceptedApisJsonForServerNoSecurity()
        {
            var exceptedApisJsonForServerNoSecurity = new ApisJson
            {
                Name = EnvironmentVariables.PublicWebServerUri,
                Created = DateTime.Today.Date,
                Modified = DateTime.Today.Date,
                Description = "",
                Url = EnvironmentVariables.PublicWebServerUri + "apis.json",
                SpecificationVersion = "0.15",
                Apis = new List<SingleApi>()
            };
            var singleApi1 = new SingleApi
            {
                Name = "Hello World",
                Description = "",
                BaseUrl = EnvironmentVariables.PublicWebServerUri + "secure/Hello World.json",
                Properties = new List<PropertyApi>()
            };
            var swagger1 = new PropertyApi
            {
                Type = "Swagger",
                Value = EnvironmentVariables.PublicWebServerUri + "secure/Hello World.api"
            };
            singleApi1.Properties.Add(swagger1);
            
            var singleApi2 = new SingleApi
            {
                Name = "Execution Engine Test",
                Description = "",
                BaseUrl = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/Execution Engine/Execution Engine Test.json",
                Properties = new List<PropertyApi>()
            };
            var swagger2 = new PropertyApi
            {
                Type = "Swagger",
                Value = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/Execution Engine/Execution Engine Test.api"
            };
            singleApi2.Properties.Add(swagger2);
            var singleApi3 = new SingleApi
            {
                Name = "9139Local",
                Description = "",
                BaseUrl = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/9139Local.json",
                Properties = new List<PropertyApi>(),
                Contact = new List<MaintainerApi>()
                {
                    new MaintainerApi()
                    {
                        Fn = "Ashley Lewis",
                        Email = "ashley.lewis@dev2.co.za",
                        Url = "https://warewolf.io",
                        Org = "https://dev2.co.za",
                        Adr = "Bellevue, Kloof",
                        XTwitter = "@warewolf",
                        XGithub = "Warewolf-ESB/Warewolf",
                        Photo = "https://warewolf.io/images/logo.png",
                        VCard = "39A03A58-978F-4CFB-B1D1-3EFA6C55E380"
                    }
                }
            };
            var swagger3 = new PropertyApi
            {
                Type = "Swagger",
                Value = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/9139Local.api"
            };
            singleApi3.Properties.Add(swagger3);
            exceptedApisJsonForServerNoSecurity.Apis.Add(singleApi1);
            exceptedApisJsonForServerNoSecurity.Apis.Add(singleApi2);
            exceptedApisJsonForServerNoSecurity.Apis.Add(singleApi3);
            return exceptedApisJsonForServerNoSecurity;
        }

        static ApisJson GetExceptedApisJsonForServerSecurity()
        {
            var exceptedApisJsonForServerNoSecurity = new ApisJson
            {
                Name = EnvironmentVariables.PublicWebServerUri,
                Created = DateTime.Today.Date,
                Modified = DateTime.Today.Date,
                Description = "",
                Url = EnvironmentVariables.PublicWebServerUri + "apis.json",
                SpecificationVersion = "0.15",
                Apis = new List<SingleApi>()
            };
            var singleApi1 = new SingleApi
            {
                Name = "Hello World",
                Description = "",
                BaseUrl = EnvironmentVariables.PublicWebServerUri + "secure/Hello World.json",
                Properties = new List<PropertyApi>()
            };
            var swagger1 = new PropertyApi
            {
                Type = "Swagger",
                Value = EnvironmentVariables.PublicWebServerUri + "secure/Hello World.api"
            };
            singleApi1.Properties.Add(swagger1);
            
            var singleApi3 = new SingleApi
            {
                Name = "9139Local",
                Description = "",
                BaseUrl = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/9139Local.json",
                Properties = new List<PropertyApi>()
            };
            var swagger3 = new PropertyApi
            {
                Type = "Swagger",
                Value = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/9139Local.api"
            };
            singleApi3.Properties.Add(swagger3);
            exceptedApisJsonForServerNoSecurity.Apis.Add(singleApi1);
            exceptedApisJsonForServerNoSecurity.Apis.Add(singleApi3);
            return exceptedApisJsonForServerNoSecurity;
        }
        
        static ApisJson GetExceptedApisJsonForServerWithSubDirectories()
        {
            var exceptedApisJsonForServerNoSecurity = new ApisJson
            {
                Name = EnvironmentVariables.PublicWebServerUri,
                Created = DateTime.Today.Date,
                Modified = DateTime.Today.Date,
                Description = "",
                Url = EnvironmentVariables.PublicWebServerUri + "Acceptance Testing Resources/apis.json",
                SpecificationVersion = "0.15",
                Apis = new List<SingleApi>()
            };
           
            var singleApi2 = new SingleApi
            {
                Name = "Execution Engine Test",
                Description = "",
                BaseUrl = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/Execution Engine/Execution Engine Test.json",
                Properties = new List<PropertyApi>()
            };
            var swagger2 = new PropertyApi
            {
                Type = "Swagger",
                Value = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/Execution Engine/Execution Engine Test.api"
            };
            singleApi2.Properties.Add(swagger2);
            var singleApi3 = new SingleApi
            {
                Name = "9139Local",
                Description = "",
                BaseUrl = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/9139Local.json",
                Properties = new List<PropertyApi>()
            };
            var swagger3 = new PropertyApi
            {
                Type = "Swagger",
                Value = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/9139Local.api"
            };
            singleApi3.Properties.Add(swagger3);
            exceptedApisJsonForServerNoSecurity.Apis.Add(singleApi2);
            exceptedApisJsonForServerNoSecurity.Apis.Add(singleApi3);
            return exceptedApisJsonForServerNoSecurity;
        }

        static ApisJson GetExceptedApisJsonForServerPathWithNoSubDirectories()
        {
            var exceptedApisJsonForServerNoSecurity = new ApisJson
            {
                Name = EnvironmentVariables.PublicWebServerUri,
                Created = DateTime.Today.Date,
                Modified = DateTime.Today.Date,
                Description = "",
                Url = EnvironmentVariables.PublicWebServerUri + "Acceptance Testing Resources/Execution Engine/apis.json",
                SpecificationVersion = "0.15",
                Apis = new List<SingleApi>()
            };
            
            var singleApi2 = new SingleApi
            {
                Name = "Execution Engine Test",
                Description = "",
                BaseUrl = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/Execution Engine/Execution Engine Test.json",
                Properties = new List<PropertyApi>()
            };
            var swagger2 = new PropertyApi
            {
                Type = "Swagger",
                Value = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/Execution Engine/Execution Engine Test.api"
            };
            singleApi2.Properties.Add(swagger2);
            
            exceptedApisJsonForServerNoSecurity.Apis.Add(singleApi2);
            return exceptedApisJsonForServerNoSecurity;
        }
    }
}
