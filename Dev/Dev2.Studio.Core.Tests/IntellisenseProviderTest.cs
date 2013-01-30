using System;
using Dev2.Data.Binary_Objects;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.Core.ViewModels.DataList;
using Dev2.Studio.InterfaceImplementors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// 2013.01.30: Ashley Lewis - Naming is consistant with [FunctionName]_With_[InputConditions]_Expected_[OutputConditions]
// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests
{
    [TestClass]
    public class IntellisenseProviderTest
    {
        private IResourceModel _resourceModel;

        #region Test Initialization

        [TestInitialize]
        public void Init()
        {
            var _testEnvironmentModel = new Mock<IEnvironmentModel>();
            _testEnvironmentModel.Setup(model => model.DsfChannel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("");

            _resourceModel = new ResourceModel(_testEnvironmentModel.Object);
            _resourceModel.ResourceName = "test";
            _resourceModel.ResourceType = ResourceType.Service;
            _resourceModel.DataList = @"
            <DataList>
                    <Scalar/>
                    <Country/>
                    <State />
                    <City>
                        <Name/>
                        <GeoLocation />
                    </City>
             </DataList>
            ";
        }

        #endregion Test Initialization

        #region DefaultIntellisenseProvider

        #region GetIntellisenseResults

        [TestMethod]
        public void GetIntellisenseResults_With_OpenRegion_Expected_AllVarsInResults()
        {
            IDataListViewModel setupDatalist = new DataListViewModel();
            DataListSingleton.SetDataList(setupDatalist);
            DataListSingleton.ActiveDataList.InitializeDataListViewModel(_resourceModel);
            var context = new IntellisenseProviderContext { CaretPosition = 2, InputText = "[[", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City()]]", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[5].ToString());
        }

        [TestMethod]
        public void GetIntellisenseResults_With_OpenRegion_AndInRecSetIndex_Expected_AllVarsInResults()
        {
            IDataListViewModel setupDatalist = new DataListViewModel();
            DataListSingleton.SetDataList(setupDatalist);
            DataListSingleton.ActiveDataList.InitializeDataListViewModel(_resourceModel);
            var context = new IntellisenseProviderContext { CaretPosition = 9, InputText = "[[City([[", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            //Assert.AreEqual("[[City()]]", getResults[3].ToString());
            //Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            //Assert.AreEqual("[[City().GeoLocation]]", getResults[5].ToString());
        }

        [TestMethod]
        public void GetIntellisenseResults_With_OpenRegion_AndInRecSetIndex_AndNoParentRegion_Expected_AllVarsInResults()
        {
            IDataListViewModel setupDatalist = new DataListViewModel();
            DataListSingleton.SetDataList(setupDatalist);
            DataListSingleton.ActiveDataList.InitializeDataListViewModel(_resourceModel);
            var context = new IntellisenseProviderContext { CaretPosition = 7, InputText = "City([[", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City()]]", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[5].ToString());
        }

        [TestMethod]
        public void GetIntellisenseResults_With_OpenRegion_AndInRecSetIndex_AndWithField_Expected_AllVarsInResults()
        {
            IDataListViewModel setupDatalist = new DataListViewModel();
            DataListSingleton.SetDataList(setupDatalist);
            DataListSingleton.ActiveDataList.InitializeDataListViewModel(_resourceModel);
            var context = new IntellisenseProviderContext { CaretPosition = 9, InputText = "[[City([[).Name]]", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City()]]", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[5].ToString());
        }

        [TestMethod]
        public void GetIntellisenseResults_With_InRecSetIndex_AndWithField_Expected_AllVarsInResults()
        {
            IDataListViewModel setupDatalist = new DataListViewModel();
            DataListSingleton.SetDataList(setupDatalist);
            DataListSingleton.ActiveDataList.InitializeDataListViewModel(_resourceModel);
            var context = new IntellisenseProviderContext { CaretPosition = 7, InputText = "City([[).Name", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City()]]", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[5].ToString());
        }

        [TestMethod]
        public void GetIntellisenseResults_With_OpenRegion_AndInRecSetIndex_AndWithField_Expected_ScalarVarInResults()
        {
            IDataListViewModel setupDatalist = new DataListViewModel();
            DataListSingleton.SetDataList(setupDatalist);
            DataListSingleton.ActiveDataList.InitializeDataListViewModel(_resourceModel);
            var context = new IntellisenseProviderContext { CaretPosition = 12, InputText = "[[City([[sca).Name]]", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
        }

        [TestMethod]
        public void GetIntellisenseResults_With_InRecSetIndex_AndWithField_Expected_ScalarVarInResults()
        {
            IDataListViewModel setupDatalist = new DataListViewModel();
            DataListSingleton.SetDataList(setupDatalist);
            DataListSingleton.ActiveDataList.InitializeDataListViewModel(_resourceModel);
            var context = new IntellisenseProviderContext { CaretPosition = 10, InputText = "City([[sca).Name]]", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
        }

        [TestMethod]
        public void GetIntellisenseResults_With_OpenRegion_AndInRecSetIndex_AndWithField_Expected_RecSetVarInResults()
        {
            IDataListViewModel setupDatalist = new DataListViewModel();
            DataListSingleton.SetDataList(setupDatalist);
            DataListSingleton.ActiveDataList.InitializeDataListViewModel(_resourceModel);
            var context = new IntellisenseProviderContext { CaretPosition = 12, InputText = "[[City([[Cit).Name]]", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[City(", getResults[0].ToString());
            Assert.AreEqual("[[City(*)]]", getResults[1].ToString());
            Assert.AreEqual("[[City()]]", getResults[2].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[3].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[4].ToString());
        }

        [TestMethod]
        public void GetIntellisenseResults_With_InRecSetIndex_AndWithField_Expected_RecSetVarInResults()
        {
            IDataListViewModel setupDatalist = new DataListViewModel();
            DataListSingleton.SetDataList(setupDatalist);
            DataListSingleton.ActiveDataList.InitializeDataListViewModel(_resourceModel);
            var context = new IntellisenseProviderContext { CaretPosition = 10, InputText = "City([[Cit).Name]]", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[City(", getResults[0].ToString());
            Assert.AreEqual("[[City(*)]]", getResults[1].ToString());
            Assert.AreEqual("[[City()]]", getResults[2].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[3].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[4].ToString());
        }

        #endregion

        #region PerformResultInsertion

        [TestMethod]
        public void PerformResultInsertion_With_PartialScalar_AndRegion_Expected_ResultReplacesText()
        {
            var context = new IntellisenseProviderContext { CaretPosition = 5, InputText = "[[sca", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            Assert.AreEqual("[[scalar]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[scalar]]", context));
        }

        [TestMethod]
        public void PerformResultInsertion_With_PartialRecSet_AndRegion_Expected_ResultReplacesText()
        {
            var context = new IntellisenseProviderContext { CaretPosition = 5, InputText = "[[rec", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            Assert.AreEqual("[[recset().field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset().field]]", context));
        }

        [TestMethod]
        public void PerformResultInsertion_With_PartialField_AndRegion_Expected_ResultReplacesText()
        {
            var context = new IntellisenseProviderContext { CaretPosition = 5, InputText = "[[fie", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            Assert.AreEqual("[[recset().field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset().field]]", context));
        }

        [TestMethod]
        public void PerformResultInsertion_With_PartialScalar_AndNoRegion_Expected_ResultReplacesText()
        {
            var context = new IntellisenseProviderContext { CaretPosition = 4, InputText = "scal", DesiredResultSet = IntellisenseDesiredResultSet.Default, State = true };
            Assert.AreEqual("[[scalar]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[scalar]]", context));
        }

        [TestMethod]
        public void PerformResultInsertion_With_PartialRecSet_AndNoRegion_Expected_ResultReplacesText()
        {
            var context = new IntellisenseProviderContext { CaretPosition = 3, InputText = "rec", DesiredResultSet = IntellisenseDesiredResultSet.Default, State = true };
            Assert.AreEqual("[[recset().field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset().field]]", context));
        }
        
        //2013.01.24: Ashley Lewis - Bug 8105
        [TestMethod]
        public void PerformResultInsertion_With_PartialField_AndNoRegion_Expected_ResultReplacesText()
        {
            var context = new IntellisenseProviderContext { CaretPosition = 3, InputText = "fie", DesiredResultSet = IntellisenseDesiredResultSet.Default, State = true };
            Assert.AreEqual("[[recset().field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset().field]]", context));
        }
        [TestMethod]
        public void PerformResultInsertion_With_PartialRecset_AndNoRegion_AndMatchOnRecsetName_Expected_ResultReplacesText()
        {
            var context = new IntellisenseProviderContext { CaretPosition = 3, InputText = "rec", DesiredResultSet = IntellisenseDesiredResultSet.Default, State = true };
            Assert.AreEqual("[[recset().recField]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset().recField]]", context));
        }

        //Bug 8437
        [TestMethod]
        public void NoFieldResultInsertion_AndMatchOnMiddleOfRecsetName_Expected_ResultReplacesText()
        {
            var context = new IntellisenseProviderContext { CaretPosition = 3, InputText = "set", DesiredResultSet = IntellisenseDesiredResultSet.Default, State = true };
            Assert.AreEqual("[[recset()]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset()]]", context));
        }
        [TestMethod]
        public void NoFieldStarResultInsertion_AndMatchOnRecsetName_AndRegion_Expected_ResultReplacesText()
        {
            var context = new IntellisenseProviderContext { CaretPosition = 5, InputText = "[[rec", DesiredResultSet = IntellisenseDesiredResultSet.Default, State = true };
            Assert.AreEqual("[[recset(*)]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset(*)]]", context));
        }
        [TestMethod]
        public void PerformResultInsertion_With_PartialScalar_AndRegion_Expected_ResultAppendsText()
        {
            var context = new IntellisenseProviderContext { CaretPosition = 14, InputText = "[[recset([[sca", DesiredResultSet = 0, State = true};
            Assert.AreEqual("[[recset([[scalar]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[scalar]]", context));
        }
         
        //Bug 6103
        [TestMethod]
        public void PerformResultInsertion_With_PartialScalar_AndRegion_Expected_ResultInsertsText()
        {
            var context = new IntellisenseProviderContext { CaretPosition = 14, InputText = "[[recset([[sca).field]]", DesiredResultSet = 0, State = true };
            Assert.AreEqual("[[recset([[scalar]]).field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[scalar]]", context));
        }

        [TestMethod]
        public void PerformResultInsertion_With_PartialRecset_AndRegion_Expected_ResultInsertsText()
        {
            var context = new IntellisenseProviderContext { CaretPosition = 14, InputText = "[[recset([[ano).field]]", DesiredResultSet = 0, State = true };
            Assert.AreEqual("[[recset([[anotherRecset().newfield]]).field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[anotherRecset().newfield]]", context));
        }

        [TestMethod]
        public void PerformResultInsertion_With_PartialScalar_AndRegion_AndDeepWithinExtaIndex_Expected_ResultInsertsText()
        {
            var context = new IntellisenseProviderContext { CaretPosition = 23, InputText = "[[recset([[recset([[sca).field]]).field]]", DesiredResultSet = 0, State = true };
            Assert.AreEqual("[[recset([[recset([[scalar]]).field]]).field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[scalar]]", context));
        }
        #endregion

        #endregion
    }
}
        // ReSharper restore InconsistentNaming
