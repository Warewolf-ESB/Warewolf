/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.IO;
using System.Threading;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests.Wrappers
{
    /// <summary>
    /// Track-C T1 batch: the wrapper factories in <c>Dev2.Common.Wrappers</c> (each a single
    /// <c>New(...)</c> method) plus cross-platform <see cref="FilePathWrapper.GetDirectoryName"/>
    /// — the existing <see cref="FilePathWrapperTests"/> class is Windows-only via
    /// <c>Assert.Inconclusive</c>, so this fills the gap for the Linux unit jobs.
    /// </summary>
    [TestClass]
    public class WrapperFactoriesTests
    {
        const string Owner = "Ashley Lewis";

        // ---- FilePathWrapper: cross-platform branches ---------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(FilePathWrapper))]
        public void FilePathWrapper_GetDirectoryName_MatchesUnderlyingPathApi()
        {
            // Path.Combine + Path.GetDirectoryName use the active OS's separator, so this
            // test is correct on both Windows and Linux without hardcoded separators.
            var input = Path.Combine("a", "b", "c.txt");
            var expected = Path.GetDirectoryName(input);

            Assert.AreEqual(expected, new FilePathWrapper().GetDirectoryName(input));
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(FilePathWrapper))]
        public void FilePathWrapper_IsPathRooted_RelativePath_ReturnsFalse_CrossPlatform()
        {
            Assert.IsFalse(new FilePathWrapper().IsPathRooted("relative/file.txt"));
        }

        // ---- FileSystemWatcherFactory -------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(FileSystemWatcherFactory))]
        public void FileSystemWatcherFactory_New_ReturnsNonNullWatcherWrapper()
        {
            var factory = new FileSystemWatcherFactory();
            using var watcher = factory.New();
            Assert.IsNotNull(watcher);
            Assert.IsInstanceOfType(watcher, typeof(IFileSystemWatcherWrapper));
            Assert.IsInstanceOfType(watcher, typeof(FileSystemWatcherWrapper));
        }

        // ---- TimerWrapperFactory ------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(TimerWrapperFactory))]
        public void TimerWrapperFactory_New_ReturnsNonNullTimerWrapper_AndIsDisposable()
        {
            var factory = new TimerWrapperFactory();

            // Timeout.Infinite both ways → callback never fires, so the assertions are
            // not racing the runtime.
            var fired = 0;
            using var timer = factory.New(_ => fired++, state: null,
                dueTime: Timeout.Infinite, period: Timeout.Infinite);

            Assert.IsNotNull(timer);
            // Fully qualify — System.Threading also defines an ITimer in newer BCLs.
            Assert.IsInstanceOfType(timer, typeof(Dev2.Common.Interfaces.Wrappers.ITimer));
            Assert.IsInstanceOfType(timer, typeof(TimerWrapper));
            Assert.AreEqual(0, fired, "callback must not fire with Infinite due-time");
        }

        // ---- TimerWrapper Dispose null-return branch ---------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(nameof(TimerWrapper))]
        public void TimerWrapper_Dispose_CalledTwice_DoesNotThrow()
        {
            // First Dispose hits the active-disposal path (_timer.Dispose(); _timer=null;);
            // second Dispose must take the `if (_timer is null) return;` early-return branch.
            var wrapper = new TimerWrapper(_ => { }, state: null,
                dueTime: Timeout.Infinite, period: Timeout.Infinite);
            wrapper.Dispose();
            wrapper.Dispose();
        }
    }
}
