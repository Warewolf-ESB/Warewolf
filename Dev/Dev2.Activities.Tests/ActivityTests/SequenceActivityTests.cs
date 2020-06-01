
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
using System.Linq;
using ActivityUnitTests;
using Dev2.Activities;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class SequenceActivityTests : BaseActivityUnitTest
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        public void SequenceActivity_Execute()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activities = new System.Collections.ObjectModel.Collection<System.Activities.Activity>
            {
                new DsfActivity {}
            };
            var sequenceActivity = new DsfSequenceActivity() { UniqueID = uniqueId, Activities = activities };

            var dataObject = new Mock<IDSFDataObject>();
            dataObject.Setup(o => o.IsDebugMode()).Returns(false);
            var executionEnvironment = new Mock<IExecutionEnvironment>().Object;
            dataObject.Setup(o => o.Environment).Returns(executionEnvironment);
            dataObject.Setup(o => o.IsServiceTestExecution).Returns(true);
            
            //---------------Assert Precondition----------------
            var getDebugInputs = sequenceActivity.GetDebugInputs(new Mock<IExecutionEnvironment>().Object, 1);
            Assert.AreEqual(0, getDebugInputs.Count);
            //---------------Execute Test ----------------------
            sequenceActivity.Execute(dataObject.Object, 0);
            //---------------Test Result -----------------------
            Assert.IsNotNull(sequenceActivity.GetChildrenNodes());
            var nodes = sequenceActivity.GetChildrenNodes();
            Assert.AreEqual(1, nodes.Count());
        }
    }
}
