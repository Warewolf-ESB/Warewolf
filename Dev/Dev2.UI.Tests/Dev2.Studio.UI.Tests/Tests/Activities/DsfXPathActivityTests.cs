
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests.Tests.Activities
{
    /// <summary>
    /// Summary description for DsfXPathActivity
    /// </summary>
    [CodedUITest]
    // ReSharper disable InconsistentNaming
    public class DsfXPathActivityTests : UIMapBase
    {
        #region Fields


        #endregion

        #region Setup
        [TestInitialize]
        public void TestInit()
        {
            Init();
        }

        #endregion

        #region Cleanup
        [TestCleanup]
        public void MyTestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
            
        }

        #endregion

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ToolDesigners_XpathlargeView")]
        public void ToolDesigners_XpathlargeView_TabbingToDone_FocusIsSetToDone()
        {
            var dsfActivityUiMap = new DsfXpathUiMap();
            dsfActivityUiMap.ClickOpenLargeView();
            // Tab to the result box
            for(int j = 0; j < 12; j++)
            {
                KeyboardCommands.SendTab();
            }
            //Check that the focus is in the done button
            Assert.IsTrue(dsfActivityUiMap.GetDoneButton().HasFocus);
        }
    }
}
