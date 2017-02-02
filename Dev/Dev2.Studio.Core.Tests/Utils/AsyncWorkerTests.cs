/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Threading;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Utils
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class AsyncWorkerTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AsyncWorker_Start")]
        public async Task AsyncWorker_Start_BackgroundWorkerDoesNotThrowAnException_ForegroundWorkIsCalled()
        {
            //------------Setup for test--------------------------
            var asyncWorker = new AsyncWorker();
            //------------Execute Test---------------------------
            var foregroundWorkWasCalled = false;
            var onerrorWorkIsCalled = false;
            await asyncWorker.Start(() =>
                {
                    //Do something interesting
                }, () =>
                {
                    foregroundWorkWasCalled = true;
                }, e =>
                {
                    onerrorWorkIsCalled = true;
                });
            //------------Assert Results-------------------------
            Assert.IsTrue(foregroundWorkWasCalled);
            Assert.IsFalse(onerrorWorkIsCalled);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AsyncWorker_Start")]
        public async Task AsyncWorker_Start_BackgroundWorkerThrowAnException_ErrorWorkIsCalled()
        {
            //------------Setup for test--------------------------
            var asyncWorker = new AsyncWorker();
            //------------Execute Test---------------------------
            var foregroundWorkWasCalled = false;
            var onerrorWorkIsCalled = false;
            await asyncWorker.Start(() =>
            {
                throw new Exception("Something went extremely wrong");
            }, () =>
            {
                foregroundWorkWasCalled = true;
            }, e =>
            {
                onerrorWorkIsCalled = true;
            });
            //------------Assert Results-------------------------
            Assert.IsFalse(foregroundWorkWasCalled);
            Assert.IsTrue(onerrorWorkIsCalled);
        }

        public static Mock<IAsyncWorker> CreateSynchronousAsyncWorker()
        {
            var mockWorker = new Mock<IAsyncWorker>();
            mockWorker.Setup(r => r.Start(It.IsAny<Action>(), It.IsAny<Action>()))
                .Returns((Action backgroundAction, Action foregroundAction) =>
                {
                    var task = new Task(() =>
                    {
                        backgroundAction.Invoke();
                        foregroundAction.Invoke();
                    });
                    task.RunSynchronously();
                    return task;
                });
            return mockWorker;
        }
    }
}
