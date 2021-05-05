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
using System.IO;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests.Wrappers
{
    [TestClass]
    public class FileInfoWrapperTests
    {
        [TestMethod]
        public void FileInfoWrapper_CreateDelete()
        {
            var fileName = "c:\\FileInfoWrapper_Construct.txt";
            var fileWrapper = new FileWrapper();
            fileWrapper.WriteAllText(fileName, "FileInfoWrapper_Construct test test");

            IFileInfo fileInfo = new FileInfoWrapper(new FileInfo(fileName));
            var createTime = fileInfo.CreationTime;
            fileInfo.Delete();

            Assert.IsTrue(createTime > (DateTime.Now.Subtract(new TimeSpan(0, 0, 30))));

            Assert.IsFalse(fileWrapper.Exists(fileName));
        }
    }
}
