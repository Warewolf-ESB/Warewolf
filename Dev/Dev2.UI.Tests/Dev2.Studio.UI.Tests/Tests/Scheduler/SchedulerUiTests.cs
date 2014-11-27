
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Studio.UI.Tests.UIMaps.Scheduler;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Studio.UI.Tests.Tests.Scheduler
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class SchedulerUiTests : UIMapBase
    {
        #region Setup
        [TestInitialize]
        public void TestInit()
        {
            Init();
        }

        #endregion


        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerUiTests")]
        public void SchedulerUiTests_ClickNewButton_NewTaskCreatedWithCorrectInformation()
        {
            using(var schedulerUiMap = new SchedulerUiMap())
            {
                schedulerUiMap.ClickNewTaskButton();
                Assert.AreEqual("New Task1", schedulerUiMap.GetNameText());
                Assert.AreEqual("Enabled", schedulerUiMap.GetStatus());
                Assert.AreEqual("", schedulerUiMap.GetWorkflowName());
                Assert.AreEqual(false, schedulerUiMap.GetRunAsap());
                Assert.AreEqual("", schedulerUiMap.GetUsername());
            }
        }
    }
}
