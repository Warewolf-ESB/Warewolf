
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Webs.Callbacks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Webs
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class WebsiteCallbackHandlerTests
    {
        private IResourceModel _resourceModel;

        #region Test Initialization

        [TestInitialize]
        public void Init()
        {
            Monitor.Enter(DataListSingletonTest.DataListSingletonTestGuard);

            var testEnvironmentModel = ResourceModelTest.CreateMockEnvironment();

            _resourceModel = new ResourceModel(testEnvironmentModel.Object)
            {
                ResourceName = "test",
                ResourceType = ResourceType.Service,
                DataList = @"
            <DataList>
                    <Scalar/>
                    <Country/>
                    <State />
                    <City>
                        <Name/>
                        <GeoLocation />
                    </City>
             </DataList>
            "
            };

            IDataListViewModel setupDatalist = new DataListViewModel();
            DataListSingleton.SetDataList(setupDatalist);
            DataListSingleton.ActiveDataList.InitializeDataListViewModel(_resourceModel);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Monitor.Exit(DataListSingletonTest.DataListSingletonTestGuard);
        }

        #endregion Test Initialization

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("WebsiteCallbackHandler_GetJsonIntellisenseResults")]
        public void WebsiteCallbackHandler_GetJsonIntellisenseResults_SearchTermIsEmpty_EmptyJsonArray()
        {
            var results = WebsiteCallbackHandler.GetJsonIntellisenseResults("", 0);
            Assert.IsNotNull(results);
            Assert.AreEqual("[]", results);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("WebsiteCallbackHandler_GetJsonIntellisenseResults")]
        public void WebsiteCallbackHandler_GetJsonIntellisenseResults_SearchTermIsNull_EmptyJsonArray()
        {
            var results = WebsiteCallbackHandler.GetJsonIntellisenseResults(null, 0);
            Assert.IsNotNull(results);
            Assert.AreEqual("[]", results);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("WebsiteCallbackHandler_GetJsonIntellisenseResults")]
        public void WebsiteCallbackHandler_GetJsonIntellisenseResults_SearchTermIsOpeningTagsAndCaretPositionIsZero_EmptyJsonArray()
        {
            var results = WebsiteCallbackHandler.GetJsonIntellisenseResults("", 0);
            Assert.IsNotNull(results);
            Assert.AreEqual("[]", results);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("WebsiteCallbackHandler_GetJsonIntellisenseResults")]
        public void WebsiteCallbackHandler_GetJsonIntellisenseResults_SearchTermIsOpeningTagsAndCaretPositionIsTwo_JsonArrayWithAllVariables()
        {
            var results = WebsiteCallbackHandler.GetJsonIntellisenseResults("[[", 2);
            Assert.IsNotNull(results);
            Assert.AreEqual("[\"[[Scalar]]\",\"[[Country]]\",\"[[State]]\",\"[[City(\",\"[[City().Name]]\",\"[[City(*).Name]]\",\"[[City().GeoLocation]]\",\"[[City(*).GeoLocation]]\"]", results);
        }
    }
}
