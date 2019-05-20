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
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests
{
    [TestClass]
    public class DirectoryWrapperTests
    {
        private static IDirectory DirectoryWrapperInstance()
        {
            return new DirectoryWrapper();
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void DirectoryWrapper_CleanUp_GivenCFolder_ExpectException()
        {
            DirectoryWrapperInstance().CleanUp(@"C:\");
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void DirectoryWrapper_CleanUp_GivenWindowsFolder_ExpectException()
        {
            DirectoryWrapperInstance().CleanUp(@"C:\Windows");
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void DirectoryWrapper_CleanUp_GivenSystem32Folder_ExpectException()
        {
            DirectoryWrapperInstance().CleanUp(@"C:\Windows\System32");
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void DirectoryWrapper_CleanUp_GivenDesktopFolder_ExpectException()
        {
            DirectoryWrapperInstance().CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void DirectoryWrapper_CleanUp_GivenDesktopDirectoryFolder_ExpectException()
        {
            DirectoryWrapperInstance().CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void DirectoryWrapper_CleanUp_GivenSystem32_ExpectException()
        {
            DirectoryWrapperInstance().CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void DirectoryWrapper_CleanUp_GivenSystemFolder_ExpectException()
        {
            DirectoryWrapperInstance().CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.System));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void DirectoryWrapper_CleanUp_GivenUserProfileFolder_ExpectException()
        {
            DirectoryWrapperInstance().CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void DirectoryWrapper_CleanUp_GivenProgramFilesFolder_ExpectException()
        {
            DirectoryWrapperInstance().CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void DirectoryWrapper_CleanUp_GivenProgramFilesX86Folder_ExpectException()
        {
            DirectoryWrapperInstance().CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void DirectoryWrapper_CleanUp_GivenProgramsFolder_ExpectException()
        {
            DirectoryWrapperInstance().CleanUp(Environment.GetFolderPath(Environment.SpecialFolder.Programs));
        }
    }
}
