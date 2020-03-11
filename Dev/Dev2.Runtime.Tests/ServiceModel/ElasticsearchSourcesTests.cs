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
        [Depends(Depends.ContainerType.Elasticsearch)]
        public void ElasticsearchSources_Test_With_ValidHost_AuthenticationType_Anonymous_Expected_ValidValidationResult()
        {
            try
            { 
                var dependency = new Depends(Depends.ContainerType.Elasticsearch);
                var hostName = "http://" + dependency.Container.IP;
                var source = new ElasticsearchSource
                {
                    HostName = hostName,
                    Port = dependency.Container.Port,
                    AuthenticationType = Dev2.Runtime.ServiceModel.Data.AuthenticationType.Anonymous
                }.ToString();

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
        [Depends(Depends.ContainerType.Elasticsearch)]
        public void ElasticsearchSources_Test_With_InvalidHost_AuthenticationType_Anonymous_Expected_InvalidValidationResult()
        {
            var source = new ElasticsearchSource
            {
                HostName = "http://ddd",
                AuthenticationType = Dev2.Runtime.ServiceModel.Data.AuthenticationType.Anonymous,
                Port = "9200"
            }.ToString();
            try
            {
                var handler = new ElasticsearchSources();
                var result = handler.Test(source);
                Assert.IsFalse(result.IsValid);
                Assert.AreEqual("could not connect to elasticsearch Instance",result.ErrorMessage);
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
        [Depends(Depends.ContainerType.Elasticsearch)]
        public void ElasticsearchSources_Test_With_ValidHost_AuthenticationType_Password_Expected_ValidValidationResult()
        {
            var dependency = new Depends(Depends.ContainerType.Elasticsearch);
            var hostName = "http://" + dependency.Container.IP;
            var source = new ElasticsearchSource
            {
                HostName = hostName,
                Port = dependency.Container.Port,
                AuthenticationType = Dev2.Runtime.ServiceModel.Data.AuthenticationType.Password,
                Password = "test",
                Username =  "test"
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
            Assert.AreEqual("could not connect to elasticsearch Instance", result.ErrorMessage);
        }
    }
}
