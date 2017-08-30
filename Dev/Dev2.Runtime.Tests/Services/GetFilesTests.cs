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
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Enums;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class GetFilesTest
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void HandlesType_Returns_GetFiles()
        {
            //------------Setup for test-------------------------
            var getFiles = new GetFiles();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual("GetFiles", getFiles.HandlesType());
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetResourceID_Returns_EmptyGuid()
        {
            //------------Setup for test-------------------------
            var getFiles = new GetFiles();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            var resourceID = getFiles.GetResourceID(new Dictionary<string, StringBuilder>());
            Assert.AreEqual(Guid.Empty, resourceID);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetAuthorizationContextForService_Returns_Any()
        {
            //------------Setup for test-------------------------
            var getFiles = new GetFiles();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            var authorizationContextForService = getFiles.GetAuthorizationContextForService();
            Assert.AreEqual(AuthorizationContext.Any, authorizationContextForService);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void BuildFileListing_Given_FileInfo_Returns_NewFileListing()
        {
            //------------Setup for test-------------------------
            FileSystemInfo fileSystemInfo = new FileSytemInfoMock();
            GetFiles files = new GetFiles();
            //------------Execute Test---------------------------
            var results = files.BuildFileListing(fileSystemInfo);
            //------------Assert Results-------------------------
            Assert.IsNotNull(results);
            Assert.IsNotNull(results.GetType() == typeof(FileListing));
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Name, fileSystemInfo.Name);
            Assert.AreEqual(results.FullName, fileSystemInfo.FullName);
        }        
    }
    public class FileSytemInfoMock : FileSystemInfo
    {
        public override void Delete()
        {
        }

        public override string Name => "DummyFileName.extension";
        public override bool Exists => true;

        public override string FullName => @"SomeDirectory\DummyFileName.extension";
    }
}


