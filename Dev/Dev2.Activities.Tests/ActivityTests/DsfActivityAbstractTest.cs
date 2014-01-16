using System.Diagnostics.CodeAnalysis;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DsfActivityAbstractTest //: NativeActivityTest
    {

        #region Public Methods

        #region Get General Settings Tests

        [TestMethod]
        public void GetGeneralSettingsFromActivityAbstract_Expected_SettingsReturned()
        {
            DsfCountRecordsetActivity testAct = new DsfCountRecordsetActivity() { IsSimulationEnabled = false };

            IBinaryDataList testDl = testAct.GetGeneralSettingData();

            Assert.AreEqual(1, testDl.FetchAllEntries().Count);
        }

        #endregion Get General Settings Tests

        #endregion Public Methods

    }
}
