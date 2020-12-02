/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Driver.ManuallyResumedState.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class ManuallyResumedStateTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManuallyResumedState))]
        public void ManuallyResumedState_Constructor_Success()
        {
            var values = "environment";
            var manuallyResumedState = new Persistence.ManuallyResumedState(values);
            Assert.AreEqual("Manually Resumed", manuallyResumedState.Reason);
            Assert.IsTrue(manuallyResumedState.IsFinal);
            Assert.AreEqual("ManuallyResumed", manuallyResumedState.Name);
            Assert.IsFalse(manuallyResumedState.IgnoreJobLoadException);
            Assert.IsNotNull(manuallyResumedState.ResumedAt);

            var data = manuallyResumedState.SerializeData();
            data.TryGetValue("ManuallyResumedAt", out string manuallyResumedAt);
            data.TryGetValue("OverrideValues", out string overrideValues);
            var val = "environment";
            Assert.AreEqual(2, data.Count);
            Assert.IsNotNull(manuallyResumedAt);
            Assert.AreEqual(val, overrideValues);
        }
    }
}