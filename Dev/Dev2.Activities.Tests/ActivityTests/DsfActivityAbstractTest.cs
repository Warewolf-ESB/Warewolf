
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics.CodeAnalysis;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DsfActivityAbstractTest
    {

        #region Public Methods

        #region Get General Settings Tests

        [TestMethod]
// ReSharper disable InconsistentNaming
        public void GetGeneralSettingsFromActivityAbstract_Expected_SettingsReturned()
// ReSharper restore InconsistentNaming
        {
            DsfCountRecordsetActivity testAct = new DsfCountRecordsetActivity { IsSimulationEnabled = false };

            IBinaryDataList testDl = testAct.GetGeneralSettingData();

            Assert.AreEqual(1, testDl.FetchAllEntries().Count);
        }

        #endregion Get General Settings Tests

        #endregion Public Methods

    }
}
