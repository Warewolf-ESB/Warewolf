/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Net;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Integration.Tests.Activities
{
    /// <summary>
    /// Summary description for FtpTests
    /// </summary>
    [TestClass]
    public class FtpTests
    {
        [ClassInitialize]
        public static void InitPaths(TestContext ctx)
        {
            CreateDirOnFtp(ParserStrings.PathOperations_FTP_NoAuth + "DontDelete");
            CreateDirOnFtp(ParserStrings.PathOperations_FTP_NoAuth + "DontDelete\\SubFolder1");
            CreateDirOnFtp(ParserStrings.PathOperations_FTP_NoAuth + "DontDelete\\SubFolder2");
            CreateDirOnFtp(ParserStrings.PathOperations_FTP_NoAuth + "DontDelete\\SubFolder3");
            CreateDirOnFtp(ParserStrings.PathOperations_FTP_NoAuth + "DontDelete\\SubFolder4");
            CreateDirOnFtp(ParserStrings.PathOperations_FTP_NoAuth + "DontDelete\\SubFolder5");
            CreateDirOnFtp(ParserStrings.PathOperations_FTP_NoAuth + "DontDelete\\SubFolder6");
            CreateDirOnFtp(ParserStrings.PathOperations_FTP_Auth + "DontDelete");
            CreateDirOnFtp(ParserStrings.PathOperations_FTP_Auth + "DontDelete\\SubFolder1");
            CreateDirOnFtp(ParserStrings.PathOperations_FTP_Auth + "DontDelete\\SubFolder2");
            CreateDirOnFtp(ParserStrings.PathOperations_FTP_Auth + "DontDelete\\SubFolder3");
            CreateDirOnFtp(ParserStrings.PathOperations_FTP_Auth + "DontDelete\\SubFolder4");
            CreateDirOnFtp(ParserStrings.PathOperations_FTP_Auth + "DontDelete\\SubFolder5");
            CreateDirOnFtp(ParserStrings.PathOperations_FTP_Auth + "DontDelete\\SubFolder6");
        }

        private static void CreateDirOnFtp(string dirPath)
        {
            WebRequest request = WebRequest.Create(dirPath);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            request.Credentials = new NetworkCredential(ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            try
            {
                request.GetResponse();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #region ListDirectory Tests
        [TestMethod]
        public void ListDirectoryWithNoUsername_InValidPath_Expected_Error()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_NoAuth + "DontDelete2/", "", "");
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);

            try
            {
                FTPPro.ListDirectory(path);
                Assert.Fail();
            }
            catch(Exception)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void ListDirectoryWithNoUsername_ValidPath_Expected_List()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_NoAuth + "DontDelete/", "", "");
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> tmp = FTPPro.ListDirectory(path);

            Assert.AreEqual(6, tmp.Count);
        }

        [TestMethod]
        public void ListDirectoryWithValidUsername_ValidPath_Expected_List()
        {
            IActivityIOPath path = ActivityIOFactory.CreatePathFromString(ParserStrings.PathOperations_FTP_Auth + "DontDelete/", ParserStrings.PathOperations_Correct_Username, ParserStrings.PathOperations_Correct_Password);
            IActivityIOOperationsEndPoint FTPPro = ActivityIOFactory.CreateOperationEndPointFromIOPath(path);
            IList<IActivityIOPath> tmp = FTPPro.ListDirectory(path);

            Assert.AreEqual(6, tmp.Count);
        }

        #endregion ListDirectory Tests
    }
}
