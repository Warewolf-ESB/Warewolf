/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using Dev2.Common.Interfaces;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{ 
    [TestClass]
    public class ElasticsearchSourceModelTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceModel))]
        public void ElasticsearchSourceModel_Valid_ExpectPropertiesSet()
        {
            //------------Setup for test--------------------------
            var updateManager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            var shellViewModel = new Mock<IShellViewModel>();

            //------------Execute Test---------------------------
            var elasticsearchSourceModel = new ElasticsearchSourceModel(updateManager.Object, queryManager.Object, shellViewModel.Object);
            //------------Assert Results-------------------------
            var p = new PrivateObject(elasticsearchSourceModel);

            Assert.IsNotNull(p.GetField("_updateRepository"));
            Assert.IsNotNull(p.GetField("_queryManager"));
            Assert.IsNotNull(p.GetField("_shellViewModel"));
        }
        
        [TestMethod,Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceModel))]
        public void ElasticsearchSourceModel_Retrieve_ExpectPassThrough()
        {
            //------------Setup for test--------------------------
            var updateManager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            var shellViewModel = new Mock<IShellViewModel>();
            var elasticsearchSourceModel = new ElasticsearchSourceModel(updateManager.Object, queryManager.Object, shellViewModel.Object);

            //------------Execute Test---------------------------
            elasticsearchSourceModel.RetrieveSources();
            //------------Assert Results-------------------------

            queryManager.Verify(a=>a.FetchElasticsearchServiceSources(),Times.Once);
        }

        [TestMethod,Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceModel))]
        public void ElasticsearchSourceModel_Edit_ExpectPassThrough()
        {
            //------------Setup for test--------------------------
            var updateManager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            var shellViewModel = new Mock<IShellViewModel>();
            var elasticsearchSourceModel = new ElasticsearchSourceModel(updateManager.Object, queryManager.Object, shellViewModel.Object);
            var src = new Mock<IElasticsearchSourceDefinition>();

            //------------Execute Test---------------------------
            elasticsearchSourceModel.EditSource(src.Object);
            //------------Assert Results-------------------------

            shellViewModel.Verify(a => a.EditResource(src.Object), Times.Once);
        }

        [TestMethod,Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceModel))]
        public void ElasticsearchSourceModel_New_ExpectPassThrough()
        {
            //------------Setup for test--------------------------
            var updateManager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            var shellViewModel = new Mock<IShellViewModel>();
            var elasticsearchSourceModel = new ElasticsearchSourceModel(updateManager.Object, queryManager.Object, shellViewModel.Object);
            var src = new Mock<IElasticsearchSourceDefinition>();

            //------------Execute Test---------------------------
            elasticsearchSourceModel.CreateNewSource();
            //------------Assert Results-------------------------

            shellViewModel.Verify(a => a.NewElasticsearchSource(""), Times.Once);
        }
        
        [TestMethod,Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceModel))]
        public void ElasticsearchSourceModel_Save_ExpectPassThrough()
        {
            var updateManager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            var shellViewModel = new Mock<IShellViewModel>();
            var elasticsearchSourceModel = new ElasticsearchSourceModel(updateManager.Object, queryManager.Object, shellViewModel.Object);
            var src = new Mock<IElasticsearchSourceDefinition>();
            elasticsearchSourceModel.Save(src.Object);
          
            updateManager.Verify(a => a.Save(src.Object),Times.Once);
        }
        
        [TestMethod,Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceModel))]
        public void ElasticsearchSourceModel_Test_ExpectPassThrough()
        {
            //------------Setup for test--------------------------
            var updateManager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            var shellViewModel = new Mock<IShellViewModel>();
            var elasticsearchSourceModel = new ElasticsearchSourceModel(updateManager.Object, queryManager.Object, shellViewModel.Object);
            var src = new Mock<IElasticsearchSourceDefinition>();
            updateManager.Setup(a => a.TestConnection(src.Object)).Returns("bob");
            //------------Execute Test---------------------------
            var res = elasticsearchSourceModel.TestConnection(src.Object);
            //------------Assert Results-------------------------

            Assert.AreEqual("bob",res);
        }       
    }
}