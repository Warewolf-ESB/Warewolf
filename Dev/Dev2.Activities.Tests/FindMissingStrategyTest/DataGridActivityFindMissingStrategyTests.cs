
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common.Interfaces.Core.Convertors.Case;
using Dev2.Enums;
using Dev2.Factories;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.FindMissingStrategyTest
{
    /// <summary>
    /// Summary description for DataGridActivityFindMissingStrategyTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DataGridActivityFindMissingStrategyTests
    {
        public DataGridActivityFindMissingStrategyTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region BaseConvert Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffBaseConvertActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfBaseConvertActivity baseConvertActivity = new DsfBaseConvertActivity();
            baseConvertActivity.ConvertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[FromExpression]]", "Text", "Binary", "[[ToExpression]]", 1), new BaseConvertTO("[[FromExpression2]]", "Text", "Binary", "[[ToExpression2]]", 2) };
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.DataGridActivity);
            List<string> actual = strategy.GetActivityFields(baseConvertActivity);
            List<string> expected = new List<string> { "[[FromExpression]]", "[[ToExpression]]", "[[FromExpression2]]", "[[ToExpression2]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region CaseConvert Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffCaseConvertActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfCaseConvertActivity caseConvertActivity = new DsfCaseConvertActivity();
            caseConvertActivity.ConvertCollection = new List<ICaseConvertTO> { new CaseConvertTO("[[StringToConvert]]", "UPPER", "[[Result]]", 1), new CaseConvertTO("[[StringToConvert2]]", "UPPER", "[[Result2]]", 2) };
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.DataGridActivity);
            List<string> actual = strategy.GetActivityFields(caseConvertActivity);
            List<string> expected = new List<string> { "[[StringToConvert]]", "[[Result]]", "[[StringToConvert2]]", "[[Result2]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region MultiAssign Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffMultiAssignActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfMultiAssignActivity multiAssignActivity = new DsfMultiAssignActivity();
            multiAssignActivity.FieldsCollection = new List<ActivityDTO> { new ActivityDTO("[[AssignRight1]]", "[[AssignLeft1]]", 1), new ActivityDTO("[[AssignRight2]]", "[[AssignLeft2]]", 2) };
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.DataGridActivity);
            List<string> actual = strategy.GetActivityFields(multiAssignActivity);
            List<string> expected = new List<string> { "[[AssignRight1]]", "[[AssignLeft1]]", "[[AssignRight2]]", "[[AssignLeft2]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion
    }
}
