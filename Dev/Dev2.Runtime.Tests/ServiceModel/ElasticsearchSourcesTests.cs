﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Castle.Components.DictionaryAdapter.Xml;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UnitTestAttributes;

namespace Dev2.Tests.Runtime.ServiceModel
{
    [TestClass]
    public class ElasticsearchSourcesTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSources))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ElasticsearchSources_ConstructorWithNullResourceCatalogExpectedThrowsArgumentNullException() => new ElasticsearchSources(null);

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSources))]
        public void ElasticsearchSources_TestWithValidArgs_Expected_Valid_ValidationResult()
        {
            var handler = new ElasticsearchSources();
            var elasticsearchSource = new ElasticsearchSource();
            var dependency = new Depends(Depends.ContainerType.AnonymousElasticsearch);
            var hostName = "http://" + dependency.Container.IP;
            elasticsearchSource.HostName = hostName;
            elasticsearchSource.Port = dependency.Container.Port;
            elasticsearchSource.Username = "test";
            elasticsearchSource.Password = "test123";
            elasticsearchSource.AuthenticationType = Dev2.Runtime.ServiceModel.Data.AuthenticationType.Password;
            var result = handler.Test(elasticsearchSource);
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSources))]
        public void ElasticsearchSources_Test_With_InValidArgs_Expected_Valid_InValidationResult()
        {
            var handler = new ElasticsearchSources();
            var result = handler.Test("asd");
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSources))]
        public void ElasticsearchSources_Test_With_ValidHost_AuthenticationType_Password_Expected_ValidValidationResult()
        {
            var dependency = new Depends(Depends.ContainerType.AnonymousElasticsearch);
            var hostName = "http://" + dependency.Container.IP;
            var source = new ElasticsearchSource
            {
                HostName = hostName,
                Port = dependency.Container.Port,
                AuthenticationType = Dev2.Runtime.ServiceModel.Data.AuthenticationType.Password,
                Username = "test",
                Password = "test123"
            }.ToString();

            try
            {
                var handler = new ElasticsearchSources();
                var result = handler.Test(source);
                Assert.IsTrue(result.IsValid, result.ErrorMessage);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("could not connect to elasticsearch Instance"))
                {
                    Assert.Inconclusive(e.Message);
                }
                else
                {
                    throw;
                }
            }
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSources))]
        public void ElasticsearchSources_Test_With_InvalidHost_AuthenticationType_Password_Expected_InvalidValidationResult()
        {
            
            var source = new ElasticsearchSource
            {
                HostName = "http://ddd",
                Port = "9300",
                AuthenticationType = Dev2.Runtime.ServiceModel.Data.AuthenticationType.Password,
                Password = "test",
                Username =  "test"
            }.ToString();

            var handler = new ElasticsearchSources();
            var result = handler.Test(source);
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.ErrorMessage.StartsWith("Unsuccessful () low level call on HEAD: /\r\n Exception: The remote name could not be resolved: 'ddd'. Call: Status code unknown from: HEAD /\r\n\r\n# Audit trail of this API call:\r\n - [1] BadResponse: Node: http://ddd:9300/ Took: "));
        }
    }
}
