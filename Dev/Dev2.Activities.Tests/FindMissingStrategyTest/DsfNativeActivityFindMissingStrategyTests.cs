#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
using System.Activities;
using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.State;
using Dev2.Factories;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;


namespace Dev2.Tests.Activities.FindMissingStrategyTest
{
    [TestClass]
    public class DsfNativeActivityFindMissingStrategyTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DsfNativeActivity_FindMissing")]
        public void DsfNativeActivity_FindMissing_FindsCorrectProperties()
        {
            //------------Setup for test--------------------------
            var expected = new List<string> { "[[ErrorVar]]", "http://ServerName:77/Services/LogErrors?Error=[[ErrorMsg]]" };
            var dsfNativeActivity = new TestDsfNativeActivity
            {
                OnErrorVariable = expected[0],
                OnErrorWorkflow = expected[1]
            };

            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);

            //------------Execute Test---------------------------
            var actual = strategy.GetActivityFields(dsfNativeActivity);

            //------------Assert Results-------------------------
            CollectionAssert.AreEqual(expected, actual);
        }
    }

    public class TestDsfNativeActivity : DsfNativeActivity<string>
    {
        public TestDsfNativeActivity()
            : base(false, "Test", new Mock<IDebugDispatcher>().Object)
        {
        }

        public override IEnumerable<StateVariable> GetState()
        {
            return new StateVariable[0];
        }

        public override List<string> GetOutputs()
        {
            return new List<string>();
        }

        #region Overrides of DsfNativeActivity<string>

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return null;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return null;
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
        }

        #endregion


        public bool Equals(TestDsfNativeActivity other)
        {
            return ReferenceEquals(this, other);
        }
        public override bool Equals(object obj)
        {
            if (obj is TestDsfNativeActivity instance)
            {
                return Equals(instance);
            }
            return false;
        }
    }
}
