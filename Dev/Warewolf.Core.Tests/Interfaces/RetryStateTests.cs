/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Core.Tests.Interfaces
{
    [TestClass]
    public class RetryStateTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(RetryState))]
        public void RetryState_Properties_GetAndSet()
        {
            var activity = new Mock<IDev2Activity>().Object;

            var retryState = new RetryState
            {
                NumberOfRetries = 3,
                GateToRetry = activity
            };

            Assert.AreEqual(3, retryState.NumberOfRetries);
            Assert.AreSame(activity, retryState.GateToRetry);
        }
    }
}
