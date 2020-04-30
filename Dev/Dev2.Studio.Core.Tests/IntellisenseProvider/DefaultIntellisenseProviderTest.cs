/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Caliburn.Micro;
using Dev2.Common.Interfaces;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Parsers;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Models;
using Dev2.Studio.InterfaceImplementors;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Studio.Interfaces.Enums;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.IntellisenseProvider
{
    [TestClass]
    [TestCategory("Intellisense Provider Core")]
    [DoNotParallelize]
    public class DefaultIntellisenseProviderTest
    {

        IResourceModel _resourceModel;

        #region Test Initialization

        [TestInitialize]
        public void Init()
        {
            var p = new PrivateType(typeof(Dev2DataLanguageParser));
            var cache = p.GetStaticField("_expressionCache") as ConcurrentDictionary<string, IList<IIntellisenseResult>>;
            Assert.IsNotNull(cache);
            cache.Clear();
            var cache2 = p.GetStaticField("_payloadCache") as ConcurrentDictionary<Tuple<string, string>, IList<IIntellisenseResult>>;
            Assert.IsNotNull(cache2);
            cache2.Clear();

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
            DataListSingleton.ActiveDataList.UpdateDataListItems(_resourceModel, new List<IDataListVerifyPart>());
        }

        #endregion Test Initialization

        #region Constructor
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DefaultIntellisenseProvider_Construct")]
        public void DefaultIntellisenseProvider_Construct_DefaultPropertiesAreSet()
        {
            var provider = new DefaultIntellisenseProvider();

            Assert.IsTrue(provider.HandlesResultInsertion);
            Assert.AreEqual(IntellisenseProviderType.Default, provider.IntellisenseProviderType);
            Assert.IsFalse(provider.Optional);


        }
        #endregion

        #region GetIntellisenseResults


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("IntellisenseProvider_GetIntellisenseResults")]
        public void IntellisenseProvider_GetIntellisenseResults_CommaSeperatedListOfCompleteValuesAndCaretInMiddle_NoResults()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 13,
                InputText = "[[rec2().s]], [[rec2().e]], [[rec2().t]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            //------------Execute Test---------------------------
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, getResults.Count(r => !r.IsError));

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("IntellisenseProvider_GetIntellisenseResults")]
        public void IntellisenseProvider_GetIntellisenseResults_WhenNoOpeningBracketsAndNoRecordsetNoDot_ValidResults()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 4,
                InputText = "City()",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            //------------Execute Test---------------------------
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            //------------Assert Results-------------------------
            Assert.AreEqual(6, getResults.Count);


            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DefaultIntellisenseProvider_GetIntellisenseResults")]
        public void DefaultIntellisenseProvider_GetIntellisenseResults_ContextIsNull_ResultCountIsZero()
        {
            //------------Execute Test---------------------------
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, getResults.Count);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("IntellisenseProvider_GetIntellisenseResults")]
        public void IntellisenseProvider_GetIntellisenseResults_WhenNoOpeningBracketsAndRecordSetDot_ValidResults()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 7,
                InputText = "city().",
                DesiredResultSet = IntellisenseDesiredResultSet.Default,
                FilterType = enIntellisensePartType.RecordsetFields
            };

            //------------Execute Test---------------------------
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            //------------Assert Results-------------------------
            Assert.AreEqual(4, getResults.Count);

            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));

        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("IntellisenseProvider_GetIntellisenseResults")]
        public void IntellisenseProvider_GetIntellisenseResults_WhenNoOpeningBracketsAndOpenRecordset_ValidResults()
        {
            //------------Setup for test--------------------------
            // vs Rs().
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 4,
                InputText = "City(",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            //------------Execute Test---------------------------
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            //------------Assert Results-------------------------
            Assert.AreEqual(6, getResults.Count);

            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("IntellisenseProvider_GetIntellisenseResults")]
        public void IntellisenseProvider_GetIntellisenseResults_WhenOpeningBracketsAndOpenRecordset_ValidResults()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 6,
                InputText = "[[City(",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            //------------Execute Test---------------------------
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            //------------Assert Results-------------------------


            Assert.AreEqual(6, getResults.Count);

            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("IntellisenseProvider_GetIntellisenseResults")]
        public void IntellisenseProvider_GetIntellisenseResults_WhenOpeningBracketsAndNoRecordset_ValidResults()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 9,
                InputText = "[[city().",
                DesiredResultSet = IntellisenseDesiredResultSet.Default,
                FilterType = enIntellisensePartType.RecordsetFields
            };

            //------------Execute Test---------------------------
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            //------------Assert Results-------------------------


            Assert.AreEqual(4, getResults.Count);

            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));
        }

        [TestMethod]
        
        public void GetIntellisenseResultsWithNumberExpectedErrorInResults()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 3,
                InputText = "[[4]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(0, getResults.Count);

        }

        [TestMethod]
        [DoNotParallelize]
        public void GetIntellisenseResults_With_OpenRegion_Expected_AllVarsInResults()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 2,
                InputText = "[[",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(9, getResults.Count);
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Scalar]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Country]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[State]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));
        }

        [TestMethod]
        public void GetIntellisenseResults_With_OpenRegion_AndInRecSetIndex_Expected_AllVarsInResults()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 9,
                InputText = "[[City([[",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(9, getResults.Count);
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Scalar]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Country]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[State]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));
        }

        //BUG 8755
        [TestMethod]
        
        public void GetIntellisenseResultsWithOpenRegionAndStarIndexExpectedNoResults()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 8,
                InputText = "[[City(*",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(3, getResults.Count);

        }

        //BUG 8755
        
        [TestMethod]

        public void GetIntellisenseResultsWithOpenRegionAndOpenRegionStarIndexExpectedNoResults()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 10,
                InputText = "[[City([[*",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(3, getResults.Count);

        }

        [TestMethod]
        
        public void GetIntellisenseResults_With_OpenRegion_AndInRecSetIndex_AndNoParentRegion_Expected_AllVarsInResults()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 7,
                InputText = "City([[",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(9, getResults.Count);
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Scalar]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Country]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[State]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));
        }

        [TestMethod]
        
        public void GetIntellisenseResults_With_OpenRegion_AndInRecSetIndex_AndWithField_Expected_AllVarsInResults()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 9,
                InputText = "[[City([[).Name]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(9, getResults.Count);
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Scalar]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Country]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[State]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));

        }

        //BUG 8755
        [TestMethod]
        public void GetIntellisenseResultsWithInRegionAndStarIndexAndWithFieldExpectedNoResults()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 8,
                InputText = "[[City(*).Name]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(3, getResults.Count);
        }

        //BUG 8755
        [TestMethod]
        public void GetIntellisenseResultsWithInRegionAndNumberIndexAndWithFieldExpectedNoResults()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 9,
                InputText = "[[City(33).Name]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(0, getResults.Count);
        }

        [TestMethod]
        
        public void GetIntellisenseResults_With_InRecSetIndex_AndWithField_Expected_AllVarsInResults()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 7,
                InputText = "City([[).Name",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(9, getResults.Count);
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Scalar]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Country]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[State]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));
        }

        //BUG 8755
        [TestMethod]
        public void GetIntellisenseResultsWithNoRegionAndStarIndexAndWithFieldExpectedNoResults()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 6,
                InputText = "City(*).Name",
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(3, getResults.Count);
        }

        //BUG 8755
        [TestMethod]
        public void GetIntellisenseResultsWithNoRegionAndStarNumberAndWithFieldExpectedNoResults()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 6,
                InputText = "City(4).Name",
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(0, getResults.Count);
        }

        [TestMethod]
        
        public void GetIntellisenseResults_With_OpenRegion_AndInRecSetIndex_AndWithField_Expected_ScalarVarInResults()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 12,
                InputText = "[[City([[Sca).Name]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(1, getResults.Count);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
        }

        [TestMethod]
        [DoNotParallelize]
        public void GetIntellisenseResultsWithOpenRegionAndAfterStarIndexAndWithPartialFieldExpectedScalarVarInResults()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 12,
                InputText = "[[City(*).Na",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(1, getResults.Count);
            Assert.AreEqual("[[City(*).Name]]", getResults[0].ToString());


        }

        //BUG 8755
        [TestMethod]
        public void GetIntellisenseResultsWithOpenRegionAndAfterNumberIndexAndWithPartialFieldExpectedScalarVarInResults()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 12,
                InputText = "[[City(6).Na",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(0, getResults.Count);

        }


        [TestMethod]
        
        public void GetIntellisenseResults_With_OpenRegion_AndInRecSetIndex_AndWithField_Expected_RecSetVarInResults()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 12,
                InputText = "[[City([[Cit).Name]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(6, getResults.Count);
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*)]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City()]]"));
        }

        [TestMethod]
        
        public void GetIntellisenseResults_With_CommaSeperatedRegions_Expected_AllVarsInResults()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 13,
                InputText = "[[Scalar]],[[",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(9, getResults.Count);
            
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*)]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City()]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Country]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Scalar]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[State]]"));


        }

        [TestMethod]
        
        public void GetIntellisenseResults_With_CommaSeperatedRegions_AndWithinIndex_Expected_AllVarsInResults()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 20,
                InputText = "[[Scalar]],[[City([[).Name]],[[Country]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            
            Assert.AreEqual(9, getResults.Count);
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*)]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City()]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Country]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Scalar]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[State]]"));
        }

        [TestMethod]
        public void GetIntellisenseResultsWithCommaSeperatedRegionsAndStarIndexExpectedNoResults()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 19,
                InputText = "[[Scalar]],[[City(*).Name]],[[Country]]",
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(3, getResults.Count);
        }

        //BUG 8755
        [TestMethod]
        public void GetIntellisenseResultsWithCommaSeperatedRegionsAndNumberIndexExpectedNoResults()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 19,
                InputText = "[[Scalar]],[[City(5).Name]],[[Country]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(0, getResults.Count);
        }

        [TestMethod]
        
        public void GetIntellisenseResults_With_CommaSeperatedRegions_AndAfterLastComma_Expected_AllVarsInResults()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 40,
                InputText = "[[City([[Scalar]]).Name]],[[Country]],[[",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual(9, getResults.Count);
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*)]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City()]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Country]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Scalar]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[State]]"));
        }

        [TestMethod]
        
        public void GetIntellisenseResults_With_Sum_Expected_AllVarsInResults()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 6,
                InputText = "Sum([[",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(9, getResults.Count);
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*)]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City()]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Country]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Scalar]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[State]]"));
        }

        [TestMethod]
        
        public void GetIntellisenseResults_With_Sum_AndAfterComma_Expected_AllVarsInResults()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 17,
                InputText = "Sum([[Scalar]],[[",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(9, getResults.Count);
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*)]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City()]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Country]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Scalar]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[State]]"));
        }

        //BUG 8736
        [TestMethod]
        public void GetIntellisenseResultsWithSumAndAfterCommaAndBeforeBraceExpectedAllVarsInResults()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 17,
                InputText = "Sum([[Scalar]],[[)",
                DesiredResultSet = IntellisenseDesiredResultSet.EntireSet
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*)]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City()]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Country]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Scalar]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[State]]"));
        }

        //BUG 8755
        [TestMethod]
        public void GetIntellisenseResultsWhereBracketOfRecordsetIsClosedAndThereIsAFieldAfterClosedBracketExpectedNoResultsAndException()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 8,
                InputText = "[[City().Name]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual(6, getResults.Count);
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*)]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City()]]"));
        }

        [TestMethod]
        [DoNotParallelize]
        public void GetIntellisenseResultsWhereBracketOfRecordsetIsClosedAndThereIsAFieldAfterClosedBracketAndStarIndexExpectedNoResultsAndException()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 9,
                InputText = "[[City(*).Name]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            
            Assert.AreEqual(3, getResults.Count);
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*)]]"));
        }

        [TestMethod]
        public void GetIntellisenseResultsWhereCommaEnteredForInfragisticsFunctonExpectedNoResultsAndException()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 15,
                InputText = "Sum([[Scalar]],",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(0, getResults.Count);
        }

        [TestMethod]
        public void GetIntellisenseResultsWithAdjacentRegionsInParamaterOfInfragisticsFunctionExpectedAllVarsInResults()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 27,
                InputText = "Sum([[Scalar]],[[Scalar]][[",
                DesiredResultSet =
                IntellisenseDesiredResultSet.EntireSet
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(9, getResults.Count);
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*)]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City()]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Country]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Scalar]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[State]]"));
        }

        [TestMethod]
        public void GetIntellisenseResultsWhereCarretPositionPastTheLengthOfTheInputTextExpectedNoResultsAndException()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 16,
                InputText = "Sum([[Scalar]],",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(0, getResults.Count);
        }

        [TestMethod]
        public void GetIntellisenseResultsWhereCarretPositionLessThanZeroExpectedNoResultsAndException()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = -1,
                InputText = "Sum([[Scalar]],",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual(0, getResults.Count);
        }

        [TestMethod]
        public void GetIntellisenseResultsWhereInputTextContainsSpecialCharactersExpectedNoResultsAndException()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 30,
                InputText = "!@#$%^&*()_+[]{}\\|;:'\",./?><",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual(0, getResults.Count);
        }
        
        [TestMethod]
        public void GetIntellisenseResultsWithInRecSetIndexAndWithFieldAndWithClosingSquareBraceExpectedNoResults()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 13,
                InputText = "[[City([[Sca]).Name]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(0, getResults.Count);
        }

        [TestMethod]
        public void GetIntellisenseResultsWithClosingSquareBraceExpectedInvalidExpressionResult()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 9,
                InputText = "[[Scalar]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(0, getResults.Count);
        }
        
        [TestMethod]
        public void GetIntellisenseResultsWithOpenRegionAndInRecSetIndexAndWithFieldExpectedAllResults()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 9,
                InputText = "[[City([[).Name]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(9, getResults.Count);
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*)]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City()]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Country]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Scalar]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[State]]"));
        }

        [TestMethod]
        public void PerformResultInsertionWithRecordsetFilterAndNoRegionExpectedCompleteResult()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 3,
                InputText = "[[Cit",
                DesiredResultSet = IntellisenseDesiredResultSet.Default,
                FilterType = enIntellisensePartType.RecordsetsOnly
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*)]]"), "Intellisense got recordset filtered results incorrectly");
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City()]]"), "Intellisense got recordset filtered results incorrectly");


        }

        [TestMethod]
        public void PerformResultInsertionWithRecordsetFilterExpectedCompleteResult()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 3,
                InputText = "[[C",
                DesiredResultSet = IntellisenseDesiredResultSet.Default,
                FilterType = enIntellisensePartType.RecordsetsOnly
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*)]]"), "Intellisense got recordset filtered results incorrectly");
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City()]]"), "Intellisense got recordset filtered results incorrectly");


        }

        #endregion

        #region PerformResultInsertion

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DefaultIntellisenseProvider_PerformResultsInsertion")]
        public void DefaultIntellisenseProvider_PerformResultsInsertion_WhenClosingRegionWithTextAfter_InsertedNormally()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 29,
                InputText = "some string [[obfsucationStag some string",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            //------------Execute Test---------------------------
            var result = new DefaultIntellisenseProvider().PerformResultInsertion("[[obfsucationStaging]]", context);

            //------------Assert Results-------------------------

            Assert.AreEqual("some string [[obfsucationStaging]] some string", result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DefaultIntellisenseProvider_PerformMultipleSelection")]
        public void DefaultIntellisenseProvider_PerformMultipleSelection_WhenSelectingOpenBracket_InsertedNormally()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 11,
                CaretPositionOnPopup = 0,
                DesiredResultSet = IntellisenseDesiredResultSet.Default,
                FilterType = enIntellisensePartType.None,
                InputText = "s [[a]] s [",
                IsInCalculateMode = false
            };

            //------------Execute Test---------------------------
            var result = new DefaultIntellisenseProvider().PerformResultInsertion("[[b]]", context);

            //------------Assert Results-------------------------

            Assert.AreEqual("s [[a]] s [[b]]", result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DefaultIntellisenseProvider_PerformMultipleSelection")]
        public void DefaultIntellisenseProvider_PerformMultipleSelection_WhenSelectingChar_InsertedNormally()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 11,
                CaretPositionOnPopup = 0,
                DesiredResultSet = IntellisenseDesiredResultSet.Default,
                FilterType = enIntellisensePartType.None,
                InputText = "s [[a]] s b",
                IsInCalculateMode = false
            };

            //------------Execute Test---------------------------
            var result = new DefaultIntellisenseProvider().PerformResultInsertion("[[b]]", context);

            //------------Assert Results-------------------------

            Assert.AreEqual("s [[a]] s [[b]]", result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DefaultIntellisenseProvider_PerformResultInsertion")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DefaultIntellisenseProvider_PerformResultInsertion_ContextIsNull_ThrowsException()
        {
            //------------Setup for test--------------------------
            var provider = new DefaultIntellisenseProvider();
            //------------Execute Test---------------------------
            provider.PerformResultInsertion("", null);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DefaultIntellisenseProvider_PerformResultsInsertion")]
        public void DefaultIntellisenseProvider_PerformResultsInsertion_WhenCaseMisMatched_InsertedNormally()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 8,
                InputText = "[[rs().v",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            //------------Execute Test---------------------------
            var result = new DefaultIntellisenseProvider().PerformResultInsertion("[[rs().Val]]", context);

            //------------Assert Results-------------------------

            Assert.AreEqual("[[rs().Val]]", result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DefaultIntellisenseProvider_PerformResultsInsertion")]
        public void DefaultIntellisenseProvider_PerformResultsInsertion_WhenBlank_InsertedNormally()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 0,
                InputText = "",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            //------------Execute Test---------------------------
            var result = new DefaultIntellisenseProvider().PerformResultInsertion("[[rs().Val]]", context);

            //------------Assert Results-------------------------

            Assert.AreEqual("[[rs().Val]]", result);
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DefaultIntellisenseProvider_GetIntellisenseResults")]
        public void DefaultIntellisenseProvider_GetIntellisenseResults_ActiviDataListIsInErrorButInPutTextDoesNotMatchesVariable_ResultIsNotInError()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 5,
                InputText = "[[num]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            CreateActiveDataListViewModel();

            //------------Execute Test---------------------------
            var result = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            //------------Assert Results-------------------------

            Assert.AreEqual(1, result.Count);
            Assert.IsFalse(result[0].IsError);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DefaultIntellisenseProvider_Dispose")]
        public void DefaultIntellisenseProvider_Dispose_IsDisposedIsSetToTrue()
        {
            var provider = new DefaultIntellisenseProvider();

            provider.Dispose();

            Assert.IsFalse(provider.Optional);
        }

        static void CreateActiveDataListViewModel()
        {
            var mockResourceModel = Dev2MockFactory.SetupResourceModelMock();

            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            dataListViewModel.InitializeDataListViewModel(mockResourceModel.Object);
            dataListViewModel.RecsetCollection.Clear();
            dataListViewModel.ScalarCollection.Clear();

            var scalarDataListItemWithError = DataListItemModelFactory.CreateScalarItemModel("Country", "name of Country", enDev2ColumnArgumentDirection.Both);
            var scalarDataListItemWithNoError = DataListItemModelFactory.CreateScalarItemModel("var", "Random Variable", enDev2ColumnArgumentDirection.Both);
            scalarDataListItemWithError.HasError = true;
            scalarDataListItemWithError.ErrorMessage = "This is an Error";
            dataListViewModel.Add(scalarDataListItemWithError);
            dataListViewModel.Add(scalarDataListItemWithNoError);
            dataListViewModel.WriteToResourceModel();
            DataListSingleton.SetDataList(dataListViewModel);
        }
        

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DefaultIntellisenseProvider_GetIntellisenseResults")]
        public void DefaultIntellisenseProvider_GetIntellisenseResults_InputIsAnOpenRegion_AllVariables()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 2,
                InputText = "[[",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            var getResults = defaultIntellisenseProvider.GetIntellisenseResults(context);

            Assert.AreEqual(9, getResults.Count);
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*).Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City(*)]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().GeoLocation]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City().Name]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[City()]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Country]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[Scalar]]"));
            Assert.IsTrue(getResults.Any(a => a.ToString() == "[[State]]"));

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DefaultIntellisenseProvider_PerformResultsInsertion")]
        public void DefaultIntellisenseProvider_PerformResultsInsertion_InputTextStartsWithAnEqual_NewStringIsInsertedAndEqualIsNotRemoved()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 1,
                InputText = "=",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var result = new DefaultIntellisenseProvider().PerformResultInsertion("[[rs().Val]]", context);

            Assert.AreEqual("=[[rs().Val]]", result);
        }







        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DefaultIntellisenseProvider_PerformResultsInsertion")]
        public void DefaultIntellisenseProvider_PerformResultsInsertion_WhenDoubleBracket_InsertedNormally1()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 2,
                InputText = "[[",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            //------------Execute Test---------------------------
            var result = new DefaultIntellisenseProvider().PerformResultInsertion("[[rs().Val]]", context);

            //------------Assert Results-------------------------
            Assert.AreEqual("[[rs().Val]]", result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DefaultIntellisenseProvider_PerformResultsInsertion")]
        public void DefaultIntellisenseProvider_PerformResultsInsertion_WhenSelectionContainsStar_InsertedNormally1()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 9,
                InputText = "[[rs().va",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            //------------Execute Test---------------------------
            var result = new DefaultIntellisenseProvider().PerformResultInsertion("[[rs(*).Val]]", context);

            //------------Assert Results-------------------------
            Assert.AreEqual("[[rs(*).Val]]", result);
        }

        [TestMethod]
        [TestCategory("IntellisenseTests")]
        [Description("Inserting a scalar into a recordset index")]
        [Owner("Massimo Guerrera")]
        
        public void PerformResultsInsertion_UnitTest_InsertIntoRecordsetIndexWithoutBrackets_InsertedTheRightValue()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 10,
                InputText = "[[recset(t).field]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            Assert.AreEqual("[[recset([[test]]).field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[test]]", context));
        }

        [TestMethod]
        [TestCategory("IntellisenseTests")]
        [Description("Inserting a recordset with a field when just the field is typed")]
        [Owner("Massimo Guerrera")]
        
        public void PerformResultsInsertion_UnitTest_WhenFieldTypeAndRecordsetSelected_FieldReplacedWithSelected()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 4,
                InputText = "[[fi",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            Assert.AreEqual("[[recset().field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset().field]]", context));
        }

        [TestMethod]
        
        public void PerformResultInsertion_With_PartialScalar_AndRegion_Expected_ResultReplacesText()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 5,
                InputText = "[[sca",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            Assert.AreEqual("[[scalar]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[scalar]]", context));
        }

        [TestMethod]
        
        public void PerformResultInsertion_With_PartialRecSet_AndRegion_Expected_ResultReplacesText()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 5,
                InputText = "[[rec",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            Assert.AreEqual("[[recset().field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset().field]]", context));
        }
        
        [TestMethod]
        
        public void PerformResultInsertion_With_PartialField_AndRegion_Expected_ResultReplacesText()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 5,
                InputText = "[[fie",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            Assert.AreEqual("[[recset().field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset().field]]", context));
        }

        [TestMethod]
        
        public void PerformResultInsertion_With_PartialScalar_AndNoRegion_Expected_ResultReplacesText()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 4,
                InputText = "scal",
                DesiredResultSet = IntellisenseDesiredResultSet.Default,
            };

            Assert.AreEqual("[[scalar]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[scalar]]", context));
        }

        //Bug 8437
        [TestMethod]
        
        public void NoFieldResultInsertion_AndMatchOnMiddleOfRecsetName_Expected_ResultReplacesText()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 5,
                InputText = "[[set",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            Assert.AreEqual("[[recset()]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset()]]", context));
        }

        [TestMethod]
        
        public void NoFieldResultInsertion_Where_CaretPositionIsZero_Expected_DoesNotThrowException()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 0,
                InputText = "",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            //The only reason this logic needs to run is to check that a zero caret position doesn't crash it!!!
            var actual = new DefaultIntellisenseProvider().PerformResultInsertion("", context);
            Assert.AreEqual("", actual);
        }

        [TestMethod]
        
        public void NoFieldStarResultInsertion_AndMatchOnRecsetName_AndRegion_Expected_ResultReplacesText()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 5,
                InputText = "[[rec",
                DesiredResultSet = IntellisenseDesiredResultSet.Default,
            };

            Assert.AreEqual("[[recset(*)]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset(*)]]", context));
        }

        [TestMethod]
        
        public void PerformResultInsertion_With_PartialScalar_AndRegion_Expected_ResultAppendsText()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 14,
                InputText = "[[recset([[sca",
                DesiredResultSet = 0,
            };

            Assert.AreEqual("[[recset([[scalar]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[scalar]]", context));
        }

        //Bug 6103
        [TestMethod]
        
        public void PerformResultInsertion_With_PartialScalar_AndRegion_Expected_ResultInsertsText()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 14,
                InputText = "[[recset([[sca).field]]",
                DesiredResultSet = 0,
            };

            Assert.AreEqual("[[recset([[scalar]]).field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[scalar]]", context));
        }

        [TestMethod]
        
        public void PerformResultInsertion_With_PartialRecset_AndRegion_Expected_ResultInsertsText()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 14,
                InputText = "[[recset([[ano).field]]",
                DesiredResultSet = 0,
            };

            var actual = new DefaultIntellisenseProvider().PerformResultInsertion("[[anotherRecset().newfield]]", context);
            Assert.AreEqual("[[recset([[anotherRecset().newfield]]).field]]", actual);
        }

        [TestMethod]
        
        public void PerformResultInsertion_With_PartialScalar_AndRegion_AtDeepWithinExtaIndex_Expected_ResultInsertsText()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 23,
                InputText = "[[recset([[recset([[sca).field]]).field]]",
                DesiredResultSet = 0,
            };

            var performResultInsertion = new DefaultIntellisenseProvider().PerformResultInsertion("[[scalar]]", context);
            Assert.AreEqual("[[recset([[recset([[scalar]]).field]]).field]]", performResultInsertion);
        }

        [TestMethod]
        
        public void PerformResultInsertion_With_PartialScalar_AndRegion_AndWithinPluses_Expected_ResultInsertsText()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 32,
                InputText = "[[recset().field]]+[[Scalar]]+[[+[[fail]]",
                DesiredResultSet = 0
            };

            var actual = new DefaultIntellisenseProvider().PerformResultInsertion("[[Car]]", context);
            Assert.AreEqual("[[recset().field]]+[[Scalar]]+[[Car]]+[[fail]]", actual);
        }

        [TestMethod]
        
        public void PerformResultInsertion_With_PartialScalar_AndRegion_AndAfterPluses_Expected_ResultInsertsText()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 32,
                InputText = "[[recset().field]]+[[Scalar]]+[[",
                DesiredResultSet = 0
            };

            Assert.AreEqual("[[recset().field]]+[[Scalar]]+[[Car]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[Car]]", context));
        }

        [TestMethod]
        
        public void PerformResultInsertion_With_PartialScalar_AndRegion_AndAfterSum_Expected_ResultInsertsText()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 9,
                InputText = "Sum([[Sca",
                DesiredResultSet = 0
            };

            Assert.AreEqual("Sum([[Scalar]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[Scalar]]", context));
        }

        [TestMethod]
        
        public void PerformResultInsertion_With_PartialField_AndRegion_AndAfterIndexed_Expected_ResultInsertsText()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 15,
                InputText = "[[recset(3).fie",
                DesiredResultSet = 0
            };

            Assert.AreEqual("[[recset(3).field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset(3).field]]", context));
        }

        // BUG 8755
        [TestMethod]
        
        public void PerformResultInsertionWithPartialFieldAndRegionAndAfterBlankIndexExpectedResultInsertsText()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 14,
                InputText = "[[recset().fie",
                DesiredResultSet = 0
            };

            Assert.AreEqual("[[recset().field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset().field]]", context));
        }

        // BUG 8755
        [TestMethod]
        
        public void PerformResultInsertionWithPartialFieldAndRegionAndAfterStarIndexExpectedResultInsertsText()

        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 15,
                InputText = "[[recset(*).fie",
                DesiredResultSet = 0
            };

            Assert.AreEqual("[[recset(*).field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset(*).field]]", context));
        }

        //Bug 8736
        [TestMethod]
        public void PerformResultInsertionWithPartialScalarAndFullRegionExpectedResultInsertsText()
        {
            var defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            var intellisenseProviderContext = new IntellisenseProviderContext
            {
                CaretPosition = 3,
                InputText = "[[S]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            const string exprected = "[[Scalar]]";
            var actual = defaultIntellisenseProvider.PerformResultInsertion("[[Scalar]]", intellisenseProviderContext);

            Assert.AreEqual(exprected, actual);
        }


        //Bug 8736
        [TestMethod]
        public void PerformResultInsertionWithPartialRecordsetAndPartialRegionExpectedResultInsertsText()
        {
            var defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            var intellisenseProviderContext = new IntellisenseProviderContext
            {
                CaretPosition = 9,
                InputText = "[[City().",
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };

            const string exprected = "[[City().GeoLocation]]";
            var actual = defaultIntellisenseProvider.PerformResultInsertion("[[City().GeoLocation]]", intellisenseProviderContext);

            Assert.AreEqual(exprected, actual);
        }

        //Bug 8736
        [TestMethod]
        public void PerformResultInsertionWithPartialRecordsetWithIndexAndPartialRegionExpectedResultInsertsText()
        {
            var defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            var intellisenseProviderContext = new IntellisenseProviderContext
            {
                CaretPosition = 10,
                InputText = "[[City(4).",
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };

            const string exprected = "[[City(4).GeoLocation]]";
            var actual = defaultIntellisenseProvider.PerformResultInsertion("[[City(4).GeoLocation]]", intellisenseProviderContext);

            Assert.AreEqual(exprected, actual);
        }

        //Bug 8755
        [TestMethod]
        public void PerformResultInsertionWithPartialRecordsetAndPartialRegionAndStarIndexExpectedResultInsertsText()
        {
            var defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            var intellisenseProviderContext = new IntellisenseProviderContext
            {
                CaretPosition = 10,
                InputText = "[[City(*).",
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };

            const string exprected = "[[City(*).GeoLocation]]";
            var actual = defaultIntellisenseProvider.PerformResultInsertion("[[City(*).GeoLocation]]", intellisenseProviderContext);

            Assert.AreEqual(exprected, actual);
        }

        //Bug 8736
        [TestMethod]
        public void PerformResultInsertionWithPartialRecordsetWithClosedBracketsAndFullRegionExpectedResultInsertsText()
        {
            var defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            var intellisenseProviderContext = new IntellisenseProviderContext
            {
                CaretPosition = 9,
                InputText = "[[City().]]",
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };

            const string exprected = "[[City().GeoLocation]]";
            var actual = defaultIntellisenseProvider.PerformResultInsertion("[[City().GeoLocation]]", intellisenseProviderContext);

            Assert.AreEqual(exprected, actual);
        }

        //Bug 8755
        [TestMethod]
        public void PerformResultInsertionWithPartialRecordsetWithClosedBracketsAndFullRegionAnNumberIndexExpectedResultInsertsText()
        {
            var defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            var intellisenseProviderContext = new IntellisenseProviderContext
            {
                CaretPosition = 11,
                InputText = "[[City(44).]]",
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };

            const string exprected = "[[City(44).GeoLocation]]";
            var actual = defaultIntellisenseProvider.PerformResultInsertion("[[City(44).GeoLocation]]", intellisenseProviderContext);

            Assert.AreEqual(exprected, actual);
        }

        //Bug 8755
        [TestMethod]
        public void PerformResultInsertionWithPartialRecordsetWithClosedBracketsAndFullRegionAnStarIndexExpectedResultInsertsText()
        {
            var defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            var intellisenseProviderContext = new IntellisenseProviderContext
            {
                CaretPosition = 10,
                InputText = "[[City(*).]]",
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };

            const string exprected = "[[City(*).GeoLocation]]";
            var actual = defaultIntellisenseProvider.PerformResultInsertion("[[City(*).GeoLocation]]", intellisenseProviderContext);

            Assert.AreEqual(exprected, actual);
        }
        
        [TestMethod]
        public void PerformResultInsertionWithPartialRecordsetExpectedResultInsertsText()
        {
            var defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            var intellisenseProviderContext = new IntellisenseProviderContext
            {
                CaretPosition = 4,
                InputText = "City",
                DesiredResultSet = IntellisenseDesiredResultSet.Default,
            };

            const string exprected = "[[City()]]";
            var actual = defaultIntellisenseProvider.PerformResultInsertion("[[City()]]", intellisenseProviderContext);

            Assert.AreEqual(exprected, actual);
        }
        
        [TestMethod]
        public void PerformResultInsertionWithPartialRecordsetFieldAfterScalarExpectedCompleteResult()
        {
            const string currentText = "[[index1]][[rec().fi";
            var defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            var intellisenseProviderContext = new IntellisenseProviderContext
            {
                CaretPosition = currentText.Length,
                InputText = currentText,
                DesiredResultSet = IntellisenseDesiredResultSet.Default,
            };

            const string exprected = "[[index1]][[rec().field]]";
            var actual = defaultIntellisenseProvider.PerformResultInsertion("[[rec().field]]", intellisenseProviderContext);
            Assert.AreEqual(exprected, actual, "Inserting a recordset after a scalar from intellisense results performs an incorrect insertion");
        }

        [TestMethod]
        public void PerformResultInsertionWithRecordsetAfterScalarExpectedCompleteResult()
        {
            const string currentText = "[[index1]][[rec";
            var defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            var intellisenseProviderContext = new IntellisenseProviderContext
            {
                CaretPosition = currentText.Length,
                InputText = currentText,
                DesiredResultSet = IntellisenseDesiredResultSet.Default,
            };

            const string exprected = "[[index1]][[rec().field]]";
            var actual = defaultIntellisenseProvider.PerformResultInsertion("[[rec().field]]", intellisenseProviderContext);
            Assert.AreEqual(exprected, actual, "Inserting a recordset after a scalar from intellisense results performs an incorrect insertion");
        }

        #endregion

    }
}
