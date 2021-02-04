﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
    [TestCategory("Runtime WebServer")]
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
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.View, It.IsAny<IResource>())).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Execute, It.IsAny<IResource>())).Returns(true);
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
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.View, It.IsAny<IResource>())).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Execute, It.IsAny<IResource>())).Returns(true);
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
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.View, It.IsAny<IResource>())).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Execute, It.IsAny<IResource>())).Returns(true);
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

            var resource1 = new Resource { ResourceID = unAuthortizedResourceID, ResourceName = "Execution Engine Test", FilePath = EnvironmentVariables.ResourcePath + "\\Acceptance Testing Resources\\Execution Engine\\Execution Engine Test.xml", ResourceType = "WorkflowService" };
            var resource2 = new Resource { ResourceID = authorizedResource1, ResourceName = "Hello World", FilePath = EnvironmentVariables.ResourcePath + "\\Hello World.xml", ResourceType = "WorkflowService" };
            var resource3 = new Resource { ResourceID = authorizedResource2, ResourceName = "9139Local", FilePath = EnvironmentVariables.ResourcePath + "\\Acceptance Testing Resources\\9139Local.xml", ResourceType = "WorkflowService" };

            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.View, resource1)).Returns(false);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Execute, resource1)).Returns(false);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.View, resource2)).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Execute, resource2)).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.View, resource3)).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Execute, resource3)).Returns(true);
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var resources = new List<IResource>();

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

            var resource1 = new Resource { ResourceID = publicAuthorizedResourceID, ResourceName = "Execution Engine Test", FilePath = EnvironmentVariables.ResourcePath + "\\Acceptance Testing Resources\\Execution Engine\\Execution Engine Test.xml", ResourceType = "WorkflowService" };
            var resource2 = new Resource { ResourceID = authorizedResource1, ResourceName = "Hello World", FilePath = EnvironmentVariables.ResourcePath + "\\Hello World.xml", ResourceType = "WorkflowService" };
            var resource3 = new Resource { ResourceID = authorizedResource2, ResourceName = "9139Local", FilePath = EnvironmentVariables.ResourcePath + "\\Acceptance Testing Resources\\9139Local.xml", ResourceType = "WorkflowService" };

            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.View, resource1)).Returns(false);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Execute, resource1)).Returns(false);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.View, resource2)).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Execute, resource2)).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.View, resource3)).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.Execute, resource3)).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(GlobalConstants.GenericPrincipal, AuthorizationContext.View, resource1)).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(GlobalConstants.GenericPrincipal, AuthorizationContext.Execute, resource1)).Returns(true);
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var resources = new List<IResource>();

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
            Assert.IsInstanceOfType(apiJson.GetHashCode(), typeof(int), "ApisJson object did not hash.");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        public void ApisJsonBuilder_Include_Equals()
        {
            var apiJson = new ApisJson
            {
                Include = new List<IncludeApi>()
                {
                    new IncludeApi()
                    {
                        Name = "Included Api",
                        Url = "Included Url"
                    }
                }
            };
            var differentApiJson = new ApisJson
            {
                Include = new List<IncludeApi>()
                {
                    new IncludeApi()
                    {
                        Name = "Different Included Api",
                        Url = "Different Included Url"
                    }
                }
            };
            Assert.IsFalse(differentApiJson.Include == apiJson.Include, "ApisJson object cannot compare Included Apis.");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        public void ApisJsonBuilder_Maintainers_Equals()
        {
            var apiJson = new ApisJson
            {
                Maintainers = new List<MaintainerApi>()
                {
                    new MaintainerApi()
                    {
                        Fn = "Ashley Lewis",
                        Email = "ashley.lewis@dev2.co.za",
                        Url = "https://warewolf.io",
                        Org = "https://dev2.co.za",
                        Adr = "Bellevue, Kloof",
                        Tel = "9139",
                        XTwitter = "@warewolf",
                        XGithub = "Warewolf-ESB/Warewolf",
                        Photo = "https://warewolf.io/images/logo.png",
                        VCard = "39A03A58-978F-4CFB-B1D1-3EFA6C55E380"
                    }
                }
            };
            var differentApiJson = new ApisJson
            {
                Maintainers = new List<MaintainerApi>()
                {
                    new MaintainerApi()
                    {
                        Fn = "A Totally Different Ashley Lewis",
                        Email = "not.ashley.lewis@dev2.co.za"
                    }
                }
            };
            Assert.IsFalse(differentApiJson.Maintainers == apiJson.Maintainers, "ApisJson object cannot compare Maintainers.");
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
            var openAPI1 = new PropertyApi
            {
                Type = "OpenAPI",
                Value = EnvironmentVariables.PublicWebServerUri + "secure/Hello World.api"
            };
            singleApi1.Properties.Add(openAPI1);
            
            var singleApi2 = new SingleApi
            {
                Name = "Execution Engine Test",
                Description = "",
                BaseUrl = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/Execution Engine/Execution Engine Test.json",
                Properties = new List<PropertyApi>()
            };
            var openAPI2 = new PropertyApi
            {
                Type = "OpenAPI",
                Value = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/Execution Engine/Execution Engine Test.api"
            };
            singleApi2.Properties.Add(openAPI2);
            var singleApi3 = new SingleApi
            {
                Name = "9139Local",
                Description = "",
                BaseUrl = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/9139Local.json",
                Properties = new List<PropertyApi>()
            };
            var openAPI3 = new PropertyApi
            {
                Type = "OpenAPI",
                Value = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/9139Local.api"
            };
            singleApi3.Properties.Add(openAPI3);
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
            var openAPI1 = new PropertyApi
            {
                Type = "OpenAPI",
                Value = EnvironmentVariables.PublicWebServerUri + "secure/Hello World.api"
            };
            singleApi1.Properties.Add(openAPI1);
            
            var singleApi3 = new SingleApi
            {
                Name = "9139Local",
                Description = "",
                BaseUrl = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/9139Local.json",
                Properties = new List<PropertyApi>()
            };
            var openAPI3 = new PropertyApi
            {
                Type = "OpenAPI",
                Value = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/9139Local.api"
            };
            singleApi3.Properties.Add(openAPI3);
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
            var openAPI2 = new PropertyApi
            {
                Type = "OpenAPI",
                Value = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/Execution Engine/Execution Engine Test.api"
            };
            singleApi2.Properties.Add(openAPI2);
            var singleApi3 = new SingleApi
            {
                Name = "9139Local",
                Description = "",
                BaseUrl = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/9139Local.json",
                Properties = new List<PropertyApi>()
            };
            var openAPI3 = new PropertyApi
            {
                Type = "OpenAPI",
                Value = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/9139Local.api"
            };
            singleApi3.Properties.Add(openAPI3);
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
                Properties = new List<PropertyApi>(),
                HumanUrl = "https://warewolf.io",
                Image = "https://warewolf.io/images/logo.png",
                Version = "1.0",
                Tags = new List<string>()
            };
            var openAPI2 = new PropertyApi
            {
                Type = "OpenAPI",
                Value = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/Execution Engine/Execution Engine Test.api"
            };
            singleApi2.Properties.Add(openAPI2);
            
            exceptedApisJsonForServerNoSecurity.Apis.Add(singleApi2);
            return exceptedApisJsonForServerNoSecurity;
        }
    }
}
