/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests
{
    [TestClass]
    public class DirectoryHelperTests
    {
        public static IDirectoryHelper DirectoryHelperInstance()
        {
            return new DirectoryHelper();
        }
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsCFolderExpectException()
        {
            DirectoryHelperInstance().CleanUp(@"C:\");
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsWindowsFolderExpectException()
        {
            DirectoryHelperInstance().CleanUp(@"C:\Windows");
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsSystem32FolderExpectException()
        {
            DirectoryHelperInstance().CleanUp(@"C:\Windows\System32");
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsDesktopFolderExpectException()
        {
            DirectoryHelperInstance().CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsDesktopDirectoryFolderExpectException()
        {
            DirectoryHelperInstance().CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsSystem32ExpectException()
        {
            DirectoryHelperInstance().CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsSystemFolderExpectException()
        {
            DirectoryHelperInstance().CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.System));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsUserProfileFolderExpectException()
        {
            DirectoryHelperInstance().CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsProgramFilesFolderExpectException()
        {
            DirectoryHelperInstance().CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsProgramFilesX86FolderExpectException()
        {
            DirectoryHelperInstance().CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CleanUpWhereIsProgramsFolderExpectException()
        {
            DirectoryHelperInstance().CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.Programs));
        }
    }
}
