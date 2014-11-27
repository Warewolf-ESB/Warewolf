
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Caliburn.Micro;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Parsers;
using Dev2.DataList.Contract;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.InterfaceImplementors;
using Dev2.Studio.ViewModels.DataList;
using Dev2.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.IntellisenseProvider
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DefaultIntellisenseProviderTest
    {
        // ReSharper disable InconsistentNaming
        private IResourceModel _resourceModel;

        #region Test Initialization

        [TestInitialize]
        public void Init()
        {
            PrivateType p = new PrivateType(typeof(Dev2DataLanguageParser));
            var cache = p.GetStaticField("_expressionCache") as ConcurrentDictionary<string, IList<IIntellisenseResult>>;
            Assert.IsNotNull(cache);
            cache.Clear();
            var cache2 = p.GetStaticField("_payloadCache") as ConcurrentDictionary<Tuple<string, string>, IList<IIntellisenseResult>>;
            Assert.IsNotNull(cache2);
            cache2.Clear();
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
            Assert.IsFalse(provider.IsDisposed);
            Assert.IsFalse(provider.IsUpdated);
            Assert.AreEqual(string.Empty, provider.FilterCondition);
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
                InputText = "city()",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            //------------Execute Test---------------------------
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            //------------Assert Results-------------------------
            Assert.AreEqual(5, getResults.Count);

            Assert.AreEqual("[[City(", getResults[0].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[1].ToString());
            Assert.AreEqual("[[City(*).Name]]", getResults[2].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[3].ToString());
            Assert.AreEqual("[[City(*).GeoLocation]]", getResults[4].ToString());
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
            Assert.AreEqual(5, getResults.Count);

            Assert.AreEqual("[[City(", getResults[0].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[1].ToString());
            Assert.AreEqual("[[City(*).Name]]", getResults[2].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[3].ToString());
            Assert.AreEqual("[[City(*).GeoLocation]]", getResults[4].ToString());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("IntellisenseProvider_GetIntellisenseResults")]
        public void IntellisenseProvider_GetIntellisenseResults_RequestIsNotFromProviderTextBox_ProviderTextBoxIsSetToTheRequestTextBox()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 7,
                InputText = "city().",
                DesiredResultSet = IntellisenseDesiredResultSet.Default,
                FilterType = enIntellisensePartType.RecordsetFields,
                TextBox = new IntellisenseTextBox(true)
            };

            var provider = new DefaultIntellisenseProvider();
            var initialTextbox = provider.TextBox;
            provider.GetIntellisenseResults(context);
            Assert.AreEqual(null, initialTextbox);
            Assert.AreEqual(context.TextBox, provider.TextBox);
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
                InputText = "city(",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            //------------Execute Test---------------------------
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            //------------Assert Results-------------------------
            Assert.AreEqual(5, getResults.Count);

            Assert.AreEqual("[[City(", getResults[0].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[1].ToString());
            Assert.AreEqual("[[City(*).Name]]", getResults[2].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[3].ToString());
            Assert.AreEqual("[[City(*).GeoLocation]]", getResults[4].ToString());
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
                InputText = "[[city(",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            //------------Execute Test---------------------------
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            //------------Assert Results-------------------------
            Assert.AreEqual(6, getResults.Count);

            Assert.AreEqual("[[City(", getResults[0].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[1].ToString());
            Assert.AreEqual("[[City(*).Name]]", getResults[2].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[3].ToString());
            Assert.AreEqual("[[City(*).GeoLocation]]", getResults[4].ToString());
            Assert.AreEqual("Invalid Expression", getResults[5].ToString());
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
            Assert.AreEqual(6, getResults.Count);

            Assert.AreEqual("[[City(", getResults[0].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[1].ToString());
            Assert.AreEqual("[[City(*).Name]]", getResults[2].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[3].ToString());
            Assert.AreEqual("[[City(*).GeoLocation]]", getResults[4].ToString());
            Assert.AreEqual("Invalid Expression", getResults[5].ToString());
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResultsWithNumberExpectedErrorInResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 3,
                InputText = "[[4]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(1, getResults.Count);
            Assert.AreEqual("Variable name [[4]] begins with a number", getResults[0].Description);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_OpenRegion_Expected_AllVarsInResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 2,
                InputText = "[[",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(9, getResults.Count);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City(", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City(*).Name]]", getResults[5].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[6].ToString());
            Assert.AreEqual("[[City(*).GeoLocation]]", getResults[7].ToString());
            Assert.AreEqual("Invalid Expression", getResults[8].ToString());
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_OpenRegion_AndInRecSetIndex_Expected_AllVarsInResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 9,
                InputText = "[[City([[",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(9, getResults.Count);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City(", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City(*).Name]]", getResults[5].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[6].ToString());
            Assert.AreEqual("[[City(*).GeoLocation]]", getResults[7].ToString());
            Assert.AreEqual("Invalid Expression", getResults[8].ToString());
        }

        //BUG 8755
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResultsWithOpenRegionAndStarIndexExpectedNoResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 8,
                InputText = "[[City(*",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(1, getResults.Count);
            Assert.AreEqual("Invalid Expression", getResults[0].ToString());
        }

        //BUG 8755
        // ReSharper disable InconsistentNaming
        [TestMethod]
        // ReSharper restore InconsistentNaming
        public void GetIntellisenseResultsWithOpenRegionAndOpenRegionStarIndexExpectedNoResults()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 10,
                InputText = "[[City([[*",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(1, getResults.Count);
            Assert.AreEqual("Invalid Expression", getResults[0].ToString());
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_OpenRegion_AndInRecSetIndex_AndNoParentRegion_Expected_AllVarsInResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 7,
                InputText = "City([[",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(9, getResults.Count);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City(", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City(*).Name]]", getResults[5].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[6].ToString());
            Assert.AreEqual("[[City(*).GeoLocation]]", getResults[7].ToString());
            Assert.AreEqual("Invalid Expression", getResults[8].ToString());
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_OpenRegion_AndInRecSetIndex_AndWithField_Expected_AllVarsInResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 9,
                InputText = "[[City([[).Name]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(9, getResults.Count);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City(", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City(*).Name]]", getResults[5].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[6].ToString());
            Assert.AreEqual("[[City(*).GeoLocation]]", getResults[7].ToString());
            Assert.AreEqual("Invalid Expression", getResults[8].ToString());

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

            Assert.AreEqual(0, getResults.Count);
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
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_InRecSetIndex_AndWithField_Expected_AllVarsInResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 7,
                InputText = "City([[).Name",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(9, getResults.Count);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City(", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City(*).Name]]", getResults[5].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[6].ToString());
            Assert.AreEqual("[[City(*).GeoLocation]]", getResults[7].ToString());
            Assert.AreEqual("Invalid Expression", getResults[8].ToString());
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

            Assert.AreEqual(0, getResults.Count);
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
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_OpenRegion_AndInRecSetIndex_AndWithField_Expected_ScalarVarInResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 12,
                InputText = "[[City([[sca).Name]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(2, getResults.Count);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("Invalid Expression", getResults[1].ToString());
        }

        //BUG 8755
        [TestMethod]
        public void GetIntellisenseResultsWithOpenRegionAndAfterStarIndexAndWithPartialFieldExpectedScalarVarInResults()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 12,
                InputText = "[[City(*).Na",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(2, getResults.Count);
            Assert.AreEqual("[[City().Name]]", getResults[0].ToString());
            Assert.AreEqual("Invalid Expression", getResults[1].ToString());

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

            Assert.AreEqual(2, getResults.Count);
            Assert.AreEqual("[[City().Name]]", getResults[0].ToString());
            Assert.AreEqual("Invalid Expression", getResults[1].ToString());
        }


        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_OpenRegion_AndInRecSetIndex_AndWithField_Expected_RecSetVarInResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 12,
                InputText = "[[City([[Cit).Name]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(6, getResults.Count);
            Assert.AreEqual("[[City(", getResults[0].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[1].ToString());
            Assert.AreEqual("[[City(*).Name]]", getResults[2].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[3].ToString());
            Assert.AreEqual("[[City(*).GeoLocation]]", getResults[4].ToString());
            Assert.AreEqual("Invalid Expression", getResults[5].ToString());

        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_CommaSeperatedRegions_Expected_AllVarsInResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 13,
                InputText = "[[Scalar]],[[",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(9, getResults.Count);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City(", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City(*).Name]]", getResults[5].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[6].ToString());
            Assert.AreEqual("[[City(*).GeoLocation]]", getResults[7].ToString());
            Assert.AreEqual("Invalid Expression", getResults[8].ToString());

        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_CommaSeperatedRegions_AndWithinIndex_Expected_AllVarsInResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 20,
                InputText = "[[Scalar]],[[City([[).Name]],[[Country]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(9, getResults.Count);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City(", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City(*).Name]]", getResults[5].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[6].ToString());
            Assert.AreEqual("[[City(*).GeoLocation]]", getResults[7].ToString());
            Assert.AreEqual("Invalid Expression", getResults[8].ToString());
        }

        //BUG 8755
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

            Assert.AreEqual(0, getResults.Count);
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
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_CommaSeperatedRegions_AndAfterLastComma_Expected_AllVarsInResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 40,
                InputText = "[[City([[Scalar]]).Name]],[[Country]],[[",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(9, getResults.Count);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City(", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City(*).Name]]", getResults[5].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[6].ToString());
            Assert.AreEqual("[[City(*).GeoLocation]]", getResults[7].ToString());
            Assert.AreEqual("Invalid Expression", getResults[8].ToString());
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_Sum_Expected_AllVarsInResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 6,
                InputText = "Sum([[",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(9, getResults.Count);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City(", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City(*).Name]]", getResults[5].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[6].ToString());
            Assert.AreEqual("[[City(*).GeoLocation]]", getResults[7].ToString());
            Assert.AreEqual("Invalid Expression", getResults[8].ToString());
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void GetIntellisenseResults_With_Sum_AndAfterComma_Expected_AllVarsInResults()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 17,
                InputText = "Sum([[Scalar]],[[",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };
            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(9, getResults.Count);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City(", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City(*).Name]]", getResults[5].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[6].ToString());
            Assert.AreEqual("[[City(*).GeoLocation]]", getResults[7].ToString());
            Assert.AreEqual("Invalid Expression", getResults[8].ToString());
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

            Assert.AreEqual(9, getResults.Count);
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City(", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City(*).Name]]", getResults[5].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[6].ToString());
            Assert.AreEqual("[[City(*).GeoLocation]]", getResults[7].ToString());
            Assert.AreEqual("Invalid Expression", getResults[8].ToString());
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

            Assert.AreEqual(0, getResults.Count);
        }

        //BUG 8755
        [TestMethod]
        public void GetIntellisenseResultsWhereBracketOfRecordsetIsClosedAndThereIsAFieldAfterClosedBracketAndStarIndexExpectedNoResultsAndException()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 9,
                InputText = "[[City(*).Name]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(0, getResults.Count);
        }

        //BUG 8736
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

        //BUG 8736
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
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City(", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City(*).Name]]", getResults[5].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[6].ToString());
            Assert.AreEqual("[[City(*).GeoLocation]]", getResults[7].ToString());
            Assert.AreEqual("Invalid Expression", getResults[8].ToString());
        }

        //BUG 8736
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

        //BUG 8736
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

        //BUG 8736
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

        //2013.04.16: Ashley Lewis - for Bug 6103
        [TestMethod]
        public void GetIntellisenseResultsWithInRecSetIndexAndWithFieldAndWithClosingSquareBraceExpectedNoResults()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 13,
                InputText = "[[City([[sca]).Name]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual("Invalid expression: Opening and closing brackets dont match.", getResults[0].Description);
        }

        [TestMethod]
        public void GetIntellisenseResultsWithClosingSquareBraceExpectedInvalidExpressionResult()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 9,
                InputText = "[[scalar]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual("Invalid expression: Opening and closing brackets dont match.", getResults[0].Description);
        }

        //2013.04.22: Ashley Lewis - for Bug 6103 QA Feedback
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
            Assert.AreEqual("[[Scalar]]", getResults[0].ToString());
            Assert.AreEqual("[[Country]]", getResults[1].ToString());
            Assert.AreEqual("[[State]]", getResults[2].ToString());
            Assert.AreEqual("[[City(", getResults[3].ToString());
            Assert.AreEqual("[[City().Name]]", getResults[4].ToString());
            Assert.AreEqual("[[City(*).Name]]", getResults[5].ToString());
            Assert.AreEqual("[[City().GeoLocation]]", getResults[6].ToString());
            Assert.AreEqual("[[City(*).GeoLocation]]", getResults[7].ToString());
            Assert.AreEqual("Invalid Expression", getResults[8].ToString());
        }

        //2013.06.11: Ashley Lewis for bug 8759 - intellisense for partial field
        public void GetIntellisenseResultsWithPartialFieldAndScalarsInIndexExpectedVarInResultIndices()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 22,
                InputText = "[[City([[Scalar]]).Nam",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(1, getResults.Count);
            Assert.AreEqual("[[City([[Scalar]]).Name]]", getResults[0].ToString());
        }
        public void GetIntellisenseResultsWithPartialFieldAndScalarsInIndexExpectedNoVarInResultIndices()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 22,
                InputText = "[[City([[Scalar]]).Name]], [[City().Nam",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(1, getResults.Count);
            Assert.AreEqual("[[City().Name]]", getResults[0].ToString());
        }
        public void GetIntellisenseResultsWithTrailingCommaExpectedNoResults()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 27,
                InputText = "[[City([[Scalar]]).Name]], ",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual(0, getResults.Count);
        }

        //2013.05.29: Ashley Lewis for bug 9472 - RecorsetsOnly filter tests
        [TestMethod]
        public void PerformResultInsertionWithRecordsetFilterAndNoRegionExpectedCompleteResult()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 3,
                InputText = "[[Cit",
                DesiredResultSet = IntellisenseDesiredResultSet.Default,
                State = true,
                FilterType = enIntellisensePartType.RecordsetsOnly
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[City()]]", getResults[0].ToString(), "Intellisense got recordset filtered results incorrectly");
            Assert.AreEqual("Invalid Expression", getResults[1].ToString());
        }

        [TestMethod]
        public void PerformResultInsertionWithRecordsetFilterExpectedCompleteResult()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 3,
                InputText = "[[C",
                DesiredResultSet = IntellisenseDesiredResultSet.Default,
                State = true,
                FilterType = enIntellisensePartType.RecordsetsOnly
            };

            var getResults = new DefaultIntellisenseProvider().GetIntellisenseResults(context);
            Assert.AreEqual("[[City()]]", getResults[0].ToString(), "Intellisense got recordset filtered results incorrectly");
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
        public void DefaultIntellisenseProvider_GetIntellisenseResults_ActiviDataListIsInErrorAndInPutTextMatchesVariable_ResultIsInError()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 0,
                InputText = "[[Country]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            CreateActiveDataListViewModel();

            //------------Execute Test---------------------------
            var result = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            //------------Assert Results-------------------------

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0].IsError);
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
            var isDiposedAferConstruction = provider.IsDisposed;
            provider.Dispose();
            Assert.IsFalse(isDiposedAferConstruction);
            Assert.IsTrue(provider.IsDisposed);
            Assert.IsFalse(provider.Optional);
            Assert.AreEqual(null, provider.CachedDataList);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DefaultIntellisenseProvider_GetIntellisenseResult")]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void DefaultIntellisenseProvider_GetIntellisenseResult_AfterDispose_ThrowsException()
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 5,
                InputText = "[[num]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var provider = new DefaultIntellisenseProvider();
            provider.Dispose();
            provider.GetIntellisenseResults(context);
        }

        static void CreateActiveDataListViewModel()
        {
            var mockResourceModel = Dev2MockFactory.SetupResourceModelMock();

            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            dataListViewModel.InitializeDataListViewModel(mockResourceModel.Object);
            dataListViewModel.RecsetCollection.Clear();
            dataListViewModel.ScalarCollection.Clear();

            var scalarDataListItemWithError = DataListItemModelFactory.CreateDataListModel("Country", "name of Country", enDev2ColumnArgumentDirection.Both);
            var scalarDataListItemWithNoError = DataListItemModelFactory.CreateDataListModel("var", "Random Variable", enDev2ColumnArgumentDirection.Both);
            scalarDataListItemWithError.HasError = true;
            scalarDataListItemWithError.ErrorMessage = "This is an Error";
            dataListViewModel.ScalarCollection.Add(scalarDataListItemWithError);
            dataListViewModel.ScalarCollection.Add(scalarDataListItemWithNoError);
            dataListViewModel.WriteToResourceModel();
            DataListSingleton.SetDataList(dataListViewModel);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DefaultIntellisenseProvider_PerformResultsInsertion")]
        public void DefaultIntellisenseProvider_PerformResultsInsertion_CaretPositionIsBetweenVariables_NewStringIsInsertedBetweenVariables()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 8,
                InputText = "[[varA]][[varB]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var result = new DefaultIntellisenseProvider().PerformResultInsertion("[[rs().Val]]", context);

            Assert.AreEqual("[[varA]][[rs().Val]][[varB]]", result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DefaultIntellisenseProvider_PerformResultsInsertion")]
        public void DefaultIntellisenseProvider_PerformResultsInsertion_SelectedInputIsScalarAndCaretPositionIsOnRecordsetField_RecorsetFieldIsAScalar()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 14,
                InputText = "[[sum([[b]]).b]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            var result = new DefaultIntellisenseProvider().PerformResultInsertion("[[ba]]", context);

            Assert.AreEqual("[[sum([[b]]).[[ba]]]]", result);
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

            var result = new DefaultIntellisenseProvider().GetIntellisenseResults(context);

            Assert.AreEqual("[[Scalar]]", result[0].Name);
            Assert.AreEqual("[[Country]]", result[1].Name);
            Assert.AreEqual("[[State]]", result[2].Name);
            Assert.AreEqual("[[City(", result[3].Name);
            Assert.AreEqual("[[City().Name]]", result[4].Name);
            Assert.AreEqual("[[City(*).Name]]", result[5].Name);
            Assert.AreEqual("[[City().GeoLocation]]", result[6].Name);
            Assert.AreEqual("[[City(*).GeoLocation]]", result[7].Name);
            Assert.AreEqual("Invalid Expression", result[8].Name);
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
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DefaultIntellisenseProvider_UpdateIntellisenseMessage")]
        public void DefaultIntellisenseProvider_UpdateIntellisenseMessage_IsUpdatedIsFalse_IsUpdatedIsTrue()
        {
            var provider = new DefaultIntellisenseProvider();
            var beforeUpdate = provider.IsUpdated;

            provider.Handle(new UpdateIntellisenseMessage());

            Assert.IsFalse(beforeUpdate);
            Assert.IsTrue(provider.IsUpdated);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DefaultIntellisenseProvider_FilterCondition")]
        public void DefaultIntellisenseProvider_FilterCondition_IsEmpty_UpdatedWithValue()
        {
            var provider = new DefaultIntellisenseProvider();
            var beforeUpdate = provider.FilterCondition;

            provider.FilterCondition = "updated";

            Assert.AreEqual("", beforeUpdate);
            Assert.AreEqual("updated", provider.FilterCondition);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DefaultIntellisenseProvider_UpdateIntellisenseMessage")]
        public void DefaultIntellisenseProvider_UpdateIntellisenseMessage_IntellisenseTextBoxToolTipIsNull_ToolTipIsUpdated()
        {
            //------------Setup for test--------------------------
            var intellisenseTextBox = new IntellisenseTextBox(true);

            var context = new IntellisenseProviderContext
            {
                CaretPosition = 2,
                InputText = "[[",
                DesiredResultSet = IntellisenseDesiredResultSet.Default,
                FilterType = enIntellisensePartType.All,
                TextBox = intellisenseTextBox
            };

            var provider = new DefaultIntellisenseProvider();
            provider.GetIntellisenseResults(context);
            var toolTipBefore = intellisenseTextBox.ToolTip;

            //------------Execute Test---------------------------
            provider.Handle(new UpdateIntellisenseMessage());

            Assert.IsNull(toolTipBefore);
            Assert.IsNotNull(intellisenseTextBox.ToolTip);
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
        // ReSharper disable InconsistentNaming
        public void PerformResultsInsertion_UnitTest_InsertIntoRecordsetIndexWithoutBrackets_InsertedTheRightValue()
        // ReSharper restore InconsistentNaming
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
        // ReSharper disable InconsistentNaming
        public void PerformResultsInsertion_UnitTest_WhenFieldTypeAndRecordsetSelected_FieldReplacedWithSelected()
        // ReSharper restore InconsistentNaming
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
        // ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialScalar_AndRegion_Expected_ResultReplacesText()
        // ReSharper restore InconsistentNaming
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
        // ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialRecSet_AndRegion_Expected_ResultReplacesText()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 5,
                InputText = "[[rec",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            Assert.AreEqual("[[recset().field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset().field]]", context));
        }

        //2013.01.24: Ashley Lewis - Bug 8105
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialField_AndRegion_Expected_ResultReplacesText()
        // ReSharper restore InconsistentNaming
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
        // ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialScalar_AndNoRegion_Expected_ResultReplacesText()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 4,
                InputText = "scal",
                DesiredResultSet = IntellisenseDesiredResultSet.Default,
                State = true
            };

            Assert.AreEqual("[[scalar]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[scalar]]", context));
        }

        //Bug 8437
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void NoFieldResultInsertion_AndMatchOnMiddleOfRecsetName_Expected_ResultReplacesText()
        // ReSharper restore InconsistentNaming
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
        // ReSharper disable InconsistentNaming
        public void NoFieldResultInsertion_Where_CaretPositionIsZero_Expected_DoesNotThrowException()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 0,
                InputText = "",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            //The only reason this logic needs to run is to check that a zero caret position doesn't crash it!!!
            string actual = new DefaultIntellisenseProvider().PerformResultInsertion("", context);
            Assert.AreEqual("", actual);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void NoFieldStarResultInsertion_AndMatchOnRecsetName_AndRegion_Expected_ResultReplacesText()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 5,
                InputText = "[[rec",
                DesiredResultSet = IntellisenseDesiredResultSet.Default,
                State = true
            };

            Assert.AreEqual("[[recset(*)]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[recset(*)]]", context));
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialScalar_AndRegion_Expected_ResultAppendsText()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 14,
                InputText = "[[recset([[sca",
                DesiredResultSet = 0,
                State = true
            };

            Assert.AreEqual("[[recset([[scalar]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[scalar]]", context));
        }

        //Bug 6103
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialScalar_AndRegion_Expected_ResultInsertsText()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 14,
                InputText = "[[recset([[sca).field]]",
                DesiredResultSet = 0,
                State = true
            };

            Assert.AreEqual("[[recset([[scalar]]).field]]", new DefaultIntellisenseProvider().PerformResultInsertion("[[scalar]]", context));
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialRecset_AndRegion_Expected_ResultInsertsText()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 14,
                InputText = "[[recset([[ano).field]]",
                DesiredResultSet = 0,
                State = true
            };

            var actual = new DefaultIntellisenseProvider().PerformResultInsertion("[[anotherRecset().newfield]]", context);
            Assert.AreEqual("[[recset([[anotherRecset().newfield]]).field]]", actual);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialScalar_AndRegion_AtDeepWithinExtaIndex_Expected_ResultInsertsText()
        // ReSharper restore InconsistentNaming
        {
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 23,
                InputText = "[[recset([[recset([[sca).field]]).field]]",
                DesiredResultSet = 0,
                State = true
            };

            var performResultInsertion = new DefaultIntellisenseProvider().PerformResultInsertion("[[scalar]]", context);
            Assert.AreEqual("[[recset([[recset([[scalar]]).field]]).field]]", performResultInsertion);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialScalar_AndRegion_AndWithinPluses_Expected_ResultInsertsText()
        // ReSharper restore InconsistentNaming
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
        // ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialScalar_AndRegion_AndAfterPluses_Expected_ResultInsertsText()
        // ReSharper restore InconsistentNaming
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
        // ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialScalar_AndRegion_AndAfterSum_Expected_ResultInsertsText()
        // ReSharper restore InconsistentNaming
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
        // ReSharper disable InconsistentNaming
        public void PerformResultInsertion_With_PartialField_AndRegion_AndAfterIndexed_Expected_ResultInsertsText()
        // ReSharper restore InconsistentNaming
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
        // ReSharper disable InconsistentNaming
        public void PerformResultInsertionWithPartialFieldAndRegionAndAfterBlankIndexExpectedResultInsertsText()
        // ReSharper restore InconsistentNaming
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
        // ReSharper disable InconsistentNaming
        public void PerformResultInsertionWithPartialFieldAndRegionAndAfterStarIndexExpectedResultInsertsText()
        // ReSharper restore InconsistentNaming
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
            DefaultIntellisenseProvider defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            IntellisenseProviderContext intellisenseProviderContext = new IntellisenseProviderContext
            {
                CaretPosition = 3,
                InputText = "[[S]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default
            };

            const string exprected = "[[Scalar]]";
            string actual = defaultIntellisenseProvider.PerformResultInsertion("[[Scalar]]", intellisenseProviderContext);

            Assert.AreEqual(exprected, actual);
        }

        //Bug 8736
        [TestMethod]
        public void PerformResultInsertionWithPartialRecordsetAndFullRegionExpectedResultInsertsText()
        {
            DefaultIntellisenseProvider defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            IntellisenseProviderContext intellisenseProviderContext = new IntellisenseProviderContext
            {
                CaretPosition = 6,
                InputText = "[[City(]]",
                DesiredResultSet = IntellisenseDesiredResultSet.Default,
                State = true
            };

            const string expected = "[[City().GeoLocation]]";
            string actual = defaultIntellisenseProvider.PerformResultInsertion("[[City().GeoLocation]]", intellisenseProviderContext);

            Assert.AreEqual(expected, actual);
        }

        //Bug 8736
        [TestMethod]
        public void PerformResultInsertionWithPartialRecordsetAndPartialRegionExpectedResultInsertsText()
        {
            DefaultIntellisenseProvider defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            IntellisenseProviderContext intellisenseProviderContext = new IntellisenseProviderContext
            {
                CaretPosition = 9,
                InputText = "[[City().",
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };

            const string exprected = "[[City().GeoLocation]]";
            string actual = defaultIntellisenseProvider.PerformResultInsertion("[[City().GeoLocation]]", intellisenseProviderContext);

            Assert.AreEqual(exprected, actual);
        }

        //Bug 8736
        [TestMethod]
        public void PerformResultInsertionWithPartialRecordsetWithIndexAndPartialRegionExpectedResultInsertsText()
        {
            DefaultIntellisenseProvider defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            IntellisenseProviderContext intellisenseProviderContext = new IntellisenseProviderContext
            {
                CaretPosition = 10,
                InputText = "[[City(4).",
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };

            const string exprected = "[[City(4).GeoLocation]]";
            string actual = defaultIntellisenseProvider.PerformResultInsertion("[[City(4).GeoLocation]]", intellisenseProviderContext);

            Assert.AreEqual(exprected, actual);
        }

        //Bug 8755
        [TestMethod]
        public void PerformResultInsertionWithPartialRecordsetAndPartialRegionAndStarIndexExpectedResultInsertsText()
        {
            DefaultIntellisenseProvider defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            IntellisenseProviderContext intellisenseProviderContext = new IntellisenseProviderContext
            {
                CaretPosition = 10,
                InputText = "[[City(*).",
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };

            const string exprected = "[[City(*).GeoLocation]]";
            string actual = defaultIntellisenseProvider.PerformResultInsertion("[[City(*).GeoLocation]]", intellisenseProviderContext);

            Assert.AreEqual(exprected, actual);
        }

        //Bug 8736
        [TestMethod]
        public void PerformResultInsertionWithPartialRecordsetWithClosedBracketsAndFullRegionExpectedResultInsertsText()
        {
            DefaultIntellisenseProvider defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            IntellisenseProviderContext intellisenseProviderContext = new IntellisenseProviderContext
            {
                CaretPosition = 9,
                InputText = "[[City().]]",
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };

            const string exprected = "[[City().GeoLocation]]";
            string actual = defaultIntellisenseProvider.PerformResultInsertion("[[City().GeoLocation]]", intellisenseProviderContext);

            Assert.AreEqual(exprected, actual);
        }

        //Bug 8755
        [TestMethod]
        public void PerformResultInsertionWithPartialRecordsetWithClosedBracketsAndFullRegionAnNumberIndexExpectedResultInsertsText()
        {
            DefaultIntellisenseProvider defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            IntellisenseProviderContext intellisenseProviderContext = new IntellisenseProviderContext
            {
                CaretPosition = 11,
                InputText = "[[City(44).]]",
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };

            const string exprected = "[[City(44).GeoLocation]]";
            string actual = defaultIntellisenseProvider.PerformResultInsertion("[[City(44).GeoLocation]]", intellisenseProviderContext);

            Assert.AreEqual(exprected, actual);
        }

        //Bug 8755
        [TestMethod]
        public void PerformResultInsertionWithPartialRecordsetWithClosedBracketsAndFullRegionAnStarIndexExpectedResultInsertsText()
        {
            DefaultIntellisenseProvider defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            IntellisenseProviderContext intellisenseProviderContext = new IntellisenseProviderContext
            {
                CaretPosition = 10,
                InputText = "[[City(*).]]",
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };

            const string exprected = "[[City(*).GeoLocation]]";
            string actual = defaultIntellisenseProvider.PerformResultInsertion("[[City(*).GeoLocation]]", intellisenseProviderContext);

            Assert.AreEqual(exprected, actual);
        }

        //Bug 8736
        [TestMethod]
        public void PerformResultInsertionWithPartialRecordsetExpectedResultInsertsText()
        {
            DefaultIntellisenseProvider defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            IntellisenseProviderContext intellisenseProviderContext = new IntellisenseProviderContext
            {
                CaretPosition = 4,
                InputText = "City",
                DesiredResultSet = IntellisenseDesiredResultSet.Default,
                State = true
            };

            const string exprected = "[[City()]]";
            string actual = defaultIntellisenseProvider.PerformResultInsertion("[[City()]]", intellisenseProviderContext);

            Assert.AreEqual(exprected, actual);
        }

        //2013.06.24: Ashley Lewis for bug 8760 - inserting a recset from results after a scalar
        [TestMethod]
        public void PerformResultInsertionWithPartialRecordsetFieldAfterScalarExpectedCompleteResult()
        {
            const string currentText = "[[index1]][[rec().fi";
            DefaultIntellisenseProvider defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            IntellisenseProviderContext intellisenseProviderContext = new IntellisenseProviderContext
            {
                CaretPosition = currentText.Length,
                InputText = currentText,
                DesiredResultSet = IntellisenseDesiredResultSet.Default,
                State = true
            };

            const string exprected = "[[index1]][[rec().field]]";
            string actual = defaultIntellisenseProvider.PerformResultInsertion("[[rec().field]]", intellisenseProviderContext);
            Assert.AreEqual(exprected, actual, "Inserting a recordset after a scalar from intellisense results performs an incorrect insertion");
        }
        [TestMethod]
        public void PerformResultInsertionWithRecordsetAfterScalarExpectedCompleteResult()
        {
            const string currentText = "[[index1]][[rec";
            DefaultIntellisenseProvider defaultIntellisenseProvider = new DefaultIntellisenseProvider();
            IntellisenseProviderContext intellisenseProviderContext = new IntellisenseProviderContext
            {
                CaretPosition = currentText.Length,
                InputText = currentText,
                DesiredResultSet = IntellisenseDesiredResultSet.Default,
                State = true
            };

            const string exprected = "[[index1]][[rec().field]]";
            string actual = defaultIntellisenseProvider.PerformResultInsertion("[[rec().field]]", intellisenseProviderContext);
            Assert.AreEqual(exprected, actual, "Inserting a recordset after a scalar from intellisense results performs an incorrect insertion");
        }

        #endregion

    }
}
