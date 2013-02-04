using System;
using Dev2.Composition;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.ViewModels.DataList;
using Dev2.Studio.InterfaceImplementors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class IntellisenseProviderTest
    {
        private IResourceModel _resourceModel;
// ReSharper disable InconsistentNaming
        private static readonly object _testGuard = new object();
// ReSharper restore InconsistentNaming

        #region Test Initialization

        [TestInitialize]
        public void Init()
        {
            Monitor.Enter(_testGuard);

            ImportService.CurrentContext = CompositionInitializer.InitializeForMeflessBaseViewModel();

// ReSharper disable InconsistentNaming
            var _testEnvironmentModel = new Mock<IEnvironmentModel>();
// ReSharper restore InconsistentNaming
            _testEnvironmentModel.Setup(model => model.DsfChannel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("");

            _resourceModel = new ResourceModel(_testEnvironmentModel.Object) { ResourceName = "test", ResourceType = ResourceType.Service, DataList = @"
            <DataList>
                    <Scalar/>
                    <Country/>
                    <State />
                    <City>
                        <Name/>
                        <GeoLocation />
                    </City>
             </DataList>
            " };

            IDataListViewModel setupDatalist = new DataListViewModel();
            DataListSingleton.SetDataList(setupDatalist);
            DataListSingleton.ActiveDataList.InitializeDataListViewModel(_resourceModel);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Monitor.Exit(_testGuard);
        }

        #endregion Test Initialization

        #region DefaultIntellisenseProvider

        #region GetIntellisenseResults

        [TestMethod]
// ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_OpenRegion_Expected_AllVarsInResults()
// ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 2, InputText = "[[", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City()]]", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[5].ToString());
            foreach(var result in getResults) Assert.IsFalse(result.IsError);
        }

        [TestMethod]
// ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_OpenRegion_AndInRecSetIndex_Expected_AllVarsInResults()
// ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 9, InputText = "[[City([[", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            //Assert.AreEqual("[[City()]]", getResults[3].ToString());
            //Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            //Assert.AreEqual("[[City().GeoLocation]]", getResults[5].ToString());
            foreach (var result in getResults) Assert.IsFalse(result.IsError);
        }

        [TestMethod]
// ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_OpenRegion_AndInRecSetIndex_AndNoParentRegion_Expected_AllVarsInResults()
// ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 7, InputText = "City([[", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City()]]", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[5].ToString());
            foreach (var result in getResults) Assert.IsFalse(result.IsError);
        }

        [TestMethod]
// ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_OpenRegion_AndInRecSetIndex_AndWithField_Expected_AllVarsInResults()
// ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 9, InputText = "[[City([[).Name]]", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City()]]", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[5].ToString());
            foreach (var result in getResults) Assert.IsFalse(result.IsError);
        }

        [TestMethod]
// ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_InRecSetIndex_AndWithField_Expected_AllVarsInResults()
// ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 7, InputText = "City([[).Name", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City()]]", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[5].ToString());
            foreach (var result in getResults) Assert.IsFalse(result.IsError);
        }

        [TestMethod]
// ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_OpenRegion_AndInRecSetIndex_AndWithField_Expected_ScalarVarInResults()
// ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 12, InputText = "[[City([[sca).Name]]", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            foreach (var result in getResults) Assert.IsFalse(result.IsError);
        }

        [TestMethod]
// ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_InRecSetIndex_AndWithField_Expected_ScalarVarInResults()
// ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 10, InputText = "City([[sca).Name]]", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            foreach (var result in getResults) Assert.IsFalse(result.IsError);
        }

        [TestMethod]
// ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_OpenRegion_AndInRecSetIndex_AndWithField_Expected_RecSetVarInResults()
// ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 12, InputText = "[[City([[Cit).Name]]", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[City(", getResults[0].ToString());
            Assert.AreEqual("[[City(*)]]", getResults[1].ToString());
            Assert.AreEqual("[[City()]]", getResults[2].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[3].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[4].ToString());
            foreach (var result in getResults) Assert.IsFalse(result.IsError);
        }

        [TestMethod]
// ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_InRecSetIndex_AndWithField_Expected_RecSetVarInResults()
// ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 10, InputText = "City([[Cit).Name]]", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[City(", getResults[0].ToString());
            Assert.AreEqual("[[City(*)]]", getResults[1].ToString());
            Assert.AreEqual("[[City()]]", getResults[2].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[3].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[4].ToString());
            foreach (var result in getResults) Assert.IsFalse(result.IsError);
        }

        [TestMethod]
// ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_Expression_Expected_NoResults()
// ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 10, InputText = "{{var r  = \"[[xpath('//result/node()')]]\";if(r.indexOf(\"success\") == -1){var s = \"No\";}else{var s =\"Yes\";}}}", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual(0, getResults.Count);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_CommaSeperatedRegions_Expected_AllVarsInResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 13, InputText = "[[Scalar]],[[", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City()]]", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[5].ToString());
            foreach (var result in getResults) Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_CommaSeperatedRegions_AndWithinIndex_Expected_AllVarsInResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 20, InputText = "[[Scalar]],[[City([[).Name]],[[Country]]", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City()]]", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[5].ToString());
            foreach (var result in getResults) Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_CommaSeperatedRegions_AndBeforeFirstComma_Expected_AllVarsInResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 2, InputText = "[[,[[City([[Scalar]]).Name]],[[Country]]", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City()]]", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[5].ToString());
            foreach (var result in getResults) Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_CommaSeperatedRegions_AndAfterLastComma_Expected_AllVarsInResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 40, InputText = "[[City([[Scalar]]).Name]],[[Country]],[[", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City()]]", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[5].ToString());
            foreach (var result in getResults) Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_Sum_Expected_AllVarsInResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 6, InputText = "Sum([[", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City()]]", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[5].ToString());
            foreach (var result in getResults) Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_Sum_AndAfterComma_Expected_AllVarsInResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 17, InputText = "Sum([[Scalar]],[[", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City()]]", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[5].ToString());
            foreach (var result in getResults) Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_Sum_AndBeforeComma_Expected_AllVarsInResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 6, InputText = "Sum([[,[[Scalar]])", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City()]]", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[5].ToString());
            foreach (var result in getResults) Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_Sum_AndWithinCommas_Expected_AllVarsInResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 16, InputText = "Sum([[State]],[[,[[Scalar]])", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City()]]", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[5].ToString());
            foreach (var result in getResults) Assert.IsFalse(result.IsError);
        }

        #endregion

        #region PerformResultInsertion

        [TestMethod]
// ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialScalar_AndRegion_Expected_ResultReplacesText()
// ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 5, InputText = "[[sca", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            Assert.AreEqual("[[scalar]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[scalar]]", context));
        }

        [TestMethod]
// ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialRecSet_AndRegion_Expected_ResultReplacesText()
// ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 5, InputText = "[[rec", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            Assert.AreEqual("[[recset().field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset().field]]", context));
        }

        [TestMethod]
// ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialField_AndRegion_Expected_ResultReplacesText()
// ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 5, InputText = "[[fie", DesiredResultSet = IntellisenseDesiredResultSet.Default };
            Assert.AreEqual("[[recset().field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset().field]]", context));
        }

        [TestMethod]
// ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialScalar_AndNoRegion_Expected_ResultReplacesText()
// ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 4, InputText = "scal", DesiredResultSet = IntellisenseDesiredResultSet.Default, State = true };
            Assert.AreEqual("[[scalar]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[scalar]]", context));
        }

        [TestMethod]
// ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialRecSet_AndNoRegion_Expected_ResultReplacesText()
// ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 3, InputText = "rec", DesiredResultSet = IntellisenseDesiredResultSet.Default, State = true };
            Assert.AreEqual("[[recset().field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset().field]]", context));
        }
        
        //2013.01.24: Ashley Lewis - Bug 8105
        [TestMethod]
// ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialField_AndNoRegion_Expected_ResultReplacesText()
// ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 3, InputText = "fie", DesiredResultSet = IntellisenseDesiredResultSet.Default, State = true };
            Assert.AreEqual("[[recset().field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset().field]]", context));
        }
        [TestMethod]
// ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialRecset_AndNoRegion_AndMatchOnRecsetName_Expected_ResultReplacesText()
// ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 3, InputText = "rec", DesiredResultSet = IntellisenseDesiredResultSet.Default, State = true };
            Assert.AreEqual("[[recset().recField]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset().recField]]", context));
        }

        //Bug 8437
        [TestMethod]
// ReSharper disable InconsistentNaming
        public void NoFieldResultInsertion_AndMatchOnMiddleOfRecsetName_Expected_ResultReplacesText()
// ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 3, InputText = "set", DesiredResultSet = IntellisenseDesiredResultSet.Default, State = true };
            Assert.AreEqual("[[recset()]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset()]]", context));
        }
        [TestMethod]
// ReSharper disable InconsistentNaming
        public void NoFieldStarResultInsertion_AndMatchOnRecsetName_AndRegion_Expected_ResultReplacesText()
// ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 5, InputText = "[[rec", DesiredResultSet = IntellisenseDesiredResultSet.Default, State = true };
            Assert.AreEqual("[[recset(*)]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset(*)]]", context));
        }
        [TestMethod]
// ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialScalar_AndRegion_Expected_ResultAppendsText()
// ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 14, InputText = "[[recset([[sca", DesiredResultSet = 0, State = true};
            Assert.AreEqual("[[recset([[scalar]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[scalar]]", context));
        }
         
        //Bug 6103
        [TestMethod]
// ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialScalar_AndRegion_Expected_ResultInsertsText()
// ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext { CaretPosition = 14, InputText = "[[recset([[sca).field]]", DesiredResultSet = 0, State = true };
            Assert.AreEqual("[[recset([[scalar]]).field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[scalar]]", context));
        }

        [TestMethod]
// ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialRecset_AndRegion_Expected_ResultInsertsText()
// ReSharper restore InconsistentNaming
        {
            Assert.AreEqual("[[recset([[anotherRecset().newfield]]).field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[anotherRecset().newfield]]", new IntellisenseProviderContext { CaretPosition = 14, InputText = "[[recset([[ano).field]]", DesiredResultSet = 0, State = true }));
        }

        [TestMethod]
// ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialScalar_AndRegion_AtDeepWithinExtaIndex_Expected_ResultInsertsText()
// ReSharper restore InconsistentNaming
        {
            Assert.AreEqual("[[recset([[recset([[scalar]]).field]]).field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[scalar]]", new IntellisenseProviderContext { CaretPosition = 23, InputText = "[[recset([[recset([[sca).field]]).field]]", DesiredResultSet = 0, State = true }));
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialScalar_AndRegion_AndWithinPluses_Expected_ResultInsertsText()
        // ReSharper restore InconsistentNaming
        {
            Assert.AreEqual("[[recset().field]]+[[Scalar]]+[[Car]]+[[fail]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[Car]]", new IntellisenseProviderContext { CaretPosition = 32, InputText = "[[recset().field]]+[[Scalar]]+[[+[[fail]]", DesiredResultSet = 0}));
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialScalar_AndRegion_AndAfterPluses_Expected_ResultInsertsText()
        // ReSharper restore InconsistentNaming
        {
            Assert.AreEqual("[[recset().field]]+[[Scalar]]+[[Car]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[Car]]", new IntellisenseProviderContext { CaretPosition = 32, InputText = "[[recset().field]]+[[Scalar]]+[[", DesiredResultSet = 0 }));
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialScalar_AndRegion_AndAfterSum_Expected_ResultInsertsText()
        // ReSharper restore InconsistentNaming
        {
            Assert.AreEqual("Sum([[Scalar]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[Scalar]]", new IntellisenseProviderContext { CaretPosition = 9, InputText = "Sum([[Sca", DesiredResultSet = 0 }));
        }
        #endregion

        #endregion
    }
}
