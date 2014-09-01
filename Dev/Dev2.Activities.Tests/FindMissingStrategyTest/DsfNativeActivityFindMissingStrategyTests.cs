using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics.Debug;
using Dev2.Enums;
using Dev2.Factories;
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

        #region Overrides of DsfNativeActivity<string>

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
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

        #endregion
    }
}
