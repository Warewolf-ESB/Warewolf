#pragma warning disable
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
using System.Collections.Generic;
using ActivityUnitTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.DataList.Contract;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Activities;
using System.Linq;
using Moq;
using Warewolf.Storage;
using Dev2.Common;
using Dev2.Common.State;

namespace Dev2.Tests.Activities.ActivityTests
{

    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    public class AbstractFileActivityTests : BaseActivityUnitTest
    {
        public TestContext TestContext { get; set; }
        class TestActivity : DsfAbstractFileActivity
        {
            public TestActivity(string displayName) : base(displayName)
            {

            }
            public override IEnumerable<StateVariable> GetState()
            {
                return new StateVariable[0];
            }
            public string InputPath;
            public string Result;
            protected override bool AssignEmptyOutputsToRecordSet => true;
            public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
            {
                if (updates != null && updates.Count == 1)
                {
                    InputPath = updates[0].Item2;
                }
            }
            public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
            {
                var itemUpdate = updates?.FirstOrDefault(tuple => tuple.Item1 == Result);
                if (itemUpdate != null)
                {
                    Result = itemUpdate.Item2;
                }
            }
        public override IList<DsfForEachItem> GetForEachOutputs() => GetForEachItems(Result);
            public override IList<DsfForEachItem> GetForEachInputs() => GetForEachItems(InputPath);
            protected override IList<OutputTO> TryExecuteConcreteAction(IDSFDataObject context, out ErrorResultTO error, int update)
            {
                error = new ErrorResultTO();
                var output = new OutputTO(GlobalConstants.ErrorPayload)
                {
                };
                output.OutputStrings.Add("someString error");
                return new List<OutputTO>
                {
                    output
                };
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Rory McGuire")]
        [TestCategory("DsfAbstractFileActivity_UpdateForEachInputs")]
        public void DsfAbstractFileActivity_FileOutputStringError_ShouldExistInEnvironmentErrors()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            var data = new Mock<IDSFDataObject>();
            data.Setup(o => o.Environment).Returns(() => env);

            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new TestActivity("TestActivity") { InputPath = inputPath, Result = "[[CompanyName]]" };



            //------------Execute Test---------------------------
            act.Execute(data.Object, 0);
            //------------Assert Results-------------------------

            Assert.AreEqual("someString error", env.FetchErrors());
        }
    }
}
