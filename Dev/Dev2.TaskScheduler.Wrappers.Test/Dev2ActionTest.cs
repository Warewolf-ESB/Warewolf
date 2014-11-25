
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Action = Microsoft.Win32.TaskScheduler.Action;

namespace Dev2.TaskScheduler.Wrappers.Test
{
    [TestClass]
    public class Dev2ActionTest
    {
        [TestInitialize]
        public void Init()
        {

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2Action_Construct")]
        public void TaskShedulerWrapper_Dev2Action_Construct()
        {
            using ( Action act = new Microsoft.Win32.TaskScheduler.ExecAction("bob","dave","jane"))
            {
                Dev2Action wrapped = new Dev2Action(act);
                wrapped.Id = Guid.NewGuid().ToString();
                Assert.AreEqual(act.ActionType, wrapped.ActionType);
                Assert.AreEqual(act.Id, wrapped.Id);
                Assert.AreEqual(act, wrapped.Instance);
            }
        }


    }
}
