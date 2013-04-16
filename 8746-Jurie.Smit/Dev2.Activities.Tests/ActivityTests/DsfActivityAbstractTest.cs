using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace ActivityUnitTests.ActivityTests
{
    [TestClass]
    public class DsfActivityAbstractTest //: NativeActivityTest
    {

        #region Public Methods

        #region Get General Settings Tests

        [TestMethod]
        public void GetGeneralSettingsFromActivityAbstract_Expected_SettingsReturned()
        {
            DsfCountRecordsetActivity testAct = new DsfCountRecordsetActivity() { IsSimulationEnabled = false };

            IBinaryDataList testDl = testAct.GetGeneralSettingData();
            Assert.IsTrue(testDl.FetchAllEntries().Count == 1);
        }

        #endregion Get General Settings Tests

        #endregion Public Methods

    }
}
