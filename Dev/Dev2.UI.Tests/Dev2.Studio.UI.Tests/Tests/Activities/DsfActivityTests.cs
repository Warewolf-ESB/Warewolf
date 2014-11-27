
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
    /// Summary description for DsfActivityTests
    /// </summary>
    [CodedUITest]
    // ReSharper disable InconsistentNaming
    public class DsfActivityTests : UIMapBase
    {

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
            RestartStudioOnFailure();
            TabManagerUIMap.CloseAllTabs();
        }

        #endregion

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DsfActivityTests_CodedUI")]
        public void DsfActivityTests_CodedUI_SetInitialFocusElementIfNoInputs_HelpTextIsNotEmpty()
        {
            const string expectedHelpText = @"Only variables go in here.
Insert the variable that you want the output of the workflow to be mapped into. By default similar matches from the variable list are used where possible.
You can use [[Scalar]] as well as [[Recordset().Fields]].
Using recordset () will add a new record and (*) will assign every record.";

            using(var dsfActivityUiMap = new DsfActivityUiMap())
            {

                dsfActivityUiMap.DragWorkflowOntoDesigner("Sql Bulk Insert Test", "TEST");
                Playback.Wait(500);
                dsfActivityUiMap.ClickHelp();
                string actualHelpText = dsfActivityUiMap.GetHelpText();
                Assert.AreEqual(expectedHelpText, actualHelpText);
            }
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfActivityTests_CodedUI")]
        public void DsfActivityTests_CodedUI_GetAServiceWilALongDisplayName_TheDisplayNameBoxWidthSizeWillBe174()
        {
            using(var dsfActivityUiMap = new DsfActivityUiMap())
            {
                dsfActivityUiMap.DragWorkflowOntoDesigner("this name is so long the display name", "TEST");
                dsfActivityUiMap.ClickCloseMapping();
                Assert.AreEqual(178, dsfActivityUiMap.GetDisplayNameMaxWidth());
            }
        }
    }
}
