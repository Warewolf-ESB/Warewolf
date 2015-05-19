
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Intellisense.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class FileSystemQueryTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetFilesListing")]
        public void FileSystemQuery_GetFilesListing_CorrectPath_ExpectResults()
        {
            var dir = new Mock<IDirectory>();
            dir.Setup(a => a.GetFileSystemEntries(It.IsAny<string>(), It.IsAny<string>())).Returns(new[] { "a", "b", "c" });
            dir.Setup(a => a.Exists(It.IsAny<string>())).Returns(true);
            var files = FileSystemQuery.GetFilesListing("bob", 'o', dir.Object);

            Assert.AreEqual(files.Count,3);
            Assert.AreEqual("a",files[0]);
            Assert.AreEqual("b", files[1]);
            Assert.AreEqual("c", files[2]);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetFilesListing")]
        public void FileSystemQuery_GetFilesListing_NoSeperator_ExpectNoResults()
        {
            var dir = new Mock<IDirectory>();
            dir.Setup(a => a.GetFileSystemEntries(It.IsAny<string>(), It.IsAny<string>())).Returns(new[] { "a", "b", "c" });
            var files = FileSystemQuery.GetFilesListing("bob", 'c', dir.Object);
            Assert.AreEqual(files.Count, 0);

        }
        [TestMethod,ExpectedException(typeof(ArgumentNullException))]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetFilesListing")]
        public void FileSystemQuery_GetFilesListing_NullDir_ExpectException()
        {
             FileSystemQuery.GetFilesListing("bob", 'c', null);
   

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetFoldersAndFiles")]
        public void FileSystemQuery_GetFoldersAndFiles_NullDirectory_ExpectException()
        {
            //------------Setup for test--------------------------
           FileSystemQuery.GetFoldersAndFiles(null, 'c', new DirectoryWrapper());
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }


        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetFoldersAndFiles")]
        public void FileSystemQuery_GetFoldersAndFiles_NullPath_ExpectException()
        {
            //------------Setup for test--------------------------
            FileSystemQuery.GetFoldersAndFiles("b", 'c', null);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetFoldersAndFiles")]
        public void FileSystemQuery_GetFoldersAndFiles_ValidPathAndDir_ExpectResults()
        {
            //------------Setup for test--------------------------
            
            var dir = new Mock<IDirectory>();
            dir.Setup(a => a.Exists(It.IsAny<string>())).Returns(true);
            dir.Setup(a => a.GetFileSystemEntries(It.IsAny<string>())).Returns(new[] {"a", "d", "f"});
            //------------Execute Test---------------------------
            var files = FileSystemQuery.GetFoldersAndFiles("bob", 'o', dir.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(files.Count,3);
            Assert.AreEqual(files[0],"a");
            Assert.AreEqual(files[1], "d");
            Assert.AreEqual(files[2], "f");

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetFoldersAndFiles")]
        public void FileSystemQuery_GetFoldersAndFiles_NonExistentPath_ExpectResults()
        {
            //------------Setup for test--------------------------

            var dir = new Mock<IDirectory>();
            dir.Setup(a => a.Exists("bob")).Returns(false);
            dir.Setup(a => a.Exists("bo")).Returns(true);
            dir.Setup(a => a.GetFileSystemEntries(It.IsAny<string>(),It.IsAny<string>())).Returns(new[] { "b", "d", "f" });
            //------------Execute Test---------------------------
            var files = FileSystemQuery.GetFoldersAndFiles("bob", 'o', dir.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(files.Count, 3);
            Assert.AreEqual(files[0], "b");
            Assert.AreEqual(files[1], "d");
            Assert.AreEqual(files[2], "f");

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetServerFolderShare")]
        public void FileSystemQuery_GetServerFolderShare_NullPath_ExpectFalse()
        {
            //------------Setup for test--------------------------

           //------------Execute Test---------------------------
            var query = new FileSystemQuery();
            string sServerFolderShare;
            var res = query.GetServerFolderShare(null, out sServerFolderShare);
            //------------Assert Results-------------------------
            Assert.IsFalse(res);
            
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetServerFolderShare")]
        public void FileSystemQuery_GetServerFolderShare_PathLength_ExpectFalse()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var query = new FileSystemQuery();
            string sServerFolderShare;
            var res = query.GetServerFolderShare("bob", out sServerFolderShare);
            //------------Assert Results-------------------------
            Assert.IsFalse(res);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetServerFolderShare")]
        public void FileSystemQuery_GetServerFolderShare_InvalidStartsWith_ExpectFalse()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var query = new FileSystemQuery();
            string sServerFolderShare;
            var res = query.GetServerFolderShare("bobthebuilder", out sServerFolderShare);
            //------------Assert Results-------------------------
            Assert.IsFalse(res);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetServerFolderShare")]
        public void FileSystemQuery_GetServerFolderShare_MultipleSlashesNoShareName_ExpectFalse()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var query = new FileSystemQuery();
            string sServerFolderShare;
            var res = query.GetServerFolderShare("\\\\bobthebuilder", out sServerFolderShare);
            //------------Assert Results-------------------------
            Assert.IsFalse(res);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FileSystemQuery_Constructor_NullDirectory_ThrowsException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
// ReSharper disable ObjectCreationAsStatement
            new FileSystemQuery(null, new DirectoryEntryFactory(), new ShareCollectionFactory());
// ReSharper restore ObjectCreationAsStatement
            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FileSystemQuery_Constructor_NullDirectoryEntryFactory_ThrowsException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            // ReSharper disable ObjectCreationAsStatement
            new FileSystemQuery(new DirectoryWrapper(), null, new ShareCollectionFactory());
            // ReSharper restore ObjectCreationAsStatement
            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FileSystemQuery_Constructor_NullShareCollectionFactory_ThrowsException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            // ReSharper disable ObjectCreationAsStatement
            new FileSystemQuery(new DirectoryWrapper(), new DirectoryEntryFactory(), null);
            // ReSharper restore ObjectCreationAsStatement
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetServerFolderShare")]
        public void FileSystemQuery_GetServerFolderShare_MultipleSlashesShareName_ExpectTrue()
        {
            //------------Setup for test--------------------------
            var dir = new Mock<IDirectory>();
            dir.Setup(a => a.Exists("\\\\bobthebuilder\\dave")).Returns(true);
      
            //------------Execute Test---------------------------
            var query = new FileSystemQuery(dir.Object, new DirectoryEntryFactory(),new ShareCollectionFactory());
            string sServerFolderShare;
            var res = query.GetServerFolderShare("\\\\bobthebuilder\\dave", out sServerFolderShare);
            //------------Assert Results-------------------------
            Assert.IsTrue(res);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetServerFolderShare")]
        public void FileSystemQuery_GetServerFolderShare_MultipleSlashesShareNameDoesNotExist_ExpectFalse()
        {
            //------------Setup for test--------------------------
            var dir = new Mock<IDirectory>();
            dir.Setup(a => a.Exists("\\\\bobthebuilder\\dave")).Returns(false).Verifiable();

            //------------Execute Test---------------------------
            var query = new FileSystemQuery(dir.Object, new DirectoryEntryFactory(), new ShareCollectionFactory());
            string sServerFolderShare;
            var res = query.GetServerFolderShare("\\\\bobthebuilder\\dave", out sServerFolderShare);
            //------------Assert Results-------------------------
            Assert.IsFalse(res);
            dir.Verify(a => a.Exists("\\\\bobthebuilder\\dave"));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetServerFolderShare")]
        public void FileSystemQuery_GetServerFolderShare_DefaultValue_ExpectTrue()
        {
            //------------Setup for test--------------------------
            var dir = new Mock<IDirectory>();
            dir.Setup(a => a.Exists("\\\\bobthebuilder\\dave")).Returns(false).Verifiable();

            //------------Execute Test---------------------------
            var query = new FileSystemQuery(dir.Object, new DirectoryEntryFactory(), new ShareCollectionFactory());
            string sServerFolderShare;
            var res = query.GetServerFolderShare("\\\\bobthebuilder\\dave\\", out sServerFolderShare);
            //------------Assert Results-------------------------
            Assert.IsTrue(res);
            Assert.AreEqual(@"\\BOBTHEBUILDER\DAVE\",sServerFolderShare);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetFilesAndFoldersIncludingNetwork")]
        public void FileSystemQuery_GetFilesAndFoldersIncludingNetwork_MultipleSlashesShareNameDoesNotExist_ExpectFalse()
        {
            //------------Setup for test--------------------------
            var dir = new Mock<IDirectory>();
            dir.Setup(a => a.Exists("\\\\bobthebuilder\\dave")).Returns(true).Verifiable();
            dir.Setup(a => a.GetFileSystemEntries("\\\\bobthebuilder\\dave")).Returns(new string[0]);
            //------------Execute Test---------------------------
            var query = new FileSystemQuery(dir.Object, new DirectoryEntryFactory(), new ShareCollectionFactory());

            var res = query.GetFilesAndFoldersIncludingNetwork("\\\\bobthebuilder\\dave",new List<string>(), '\\');
            //------------Assert Results-------------------------
            Assert.AreEqual(res.Count,1);
            Assert.AreEqual("\\\\bobthebuilder\\dave\\".ToUpper(), res[0]);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_FindNetworkComputers")]
        public void FileSystemQuery_FindNetworkComputers_ValidEntries_ExpectReturned()
        {
            //------------Setup for test--------------------------
            var dirFact = new Mock<IDirectoryEntryFactory>();
            var dir = new Mock<IDirectoryEntry>();
            var children = new Mock<IDirectoryEntries>();
            var gChildren = new Mock<IDirectoryEntries>();
            var actualChildren = new List<Mock<IDirectoryEntry>>
                {
                    new Mock<IDirectoryEntry>()

                };
           var actualGChildren = new List<Mock<IDirectoryEntry>>
                {
                    new Mock<IDirectoryEntry>()

                };
            actualGChildren.ForEach(
                b=>b.Setup(a=>a.Name).Returns("a"));
            actualGChildren.ForEach(
                b => b.Setup(a => a.SchemaClassName).Returns("Computer"));
            actualChildren.ForEach(
                b=>b.Setup(a=>a.SchemaClassName).Returns("Computer"));
            dirFact.Setup(a => a.Create(It.IsAny<string>())).Returns(dir.Object);

            dir.Setup(a => a.Children).Returns(children.Object);
            children.Setup(a => a.GetEnumerator())
                .Returns(actualChildren.Select(a=>a.Object)
                .GetEnumerator());
            actualChildren.First().Setup(a => a.Children).Returns(gChildren.Object);
            gChildren.Setup(a => a.GetEnumerator()).Returns(actualGChildren.Select(a => a.Object).GetEnumerator());
            //------------Execute Test---------------------------
            var query = new FileSystemQuery(new DirectoryWrapper(), dirFact.Object, new ShareCollectionFactory());

            var res = query.FindNetworkComputers();
            //------------Assert Results-------------------------
            Assert.AreEqual(res.Count, 1);
            Assert.AreEqual("\\\\a", res[0]);

        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetComputerNamesOnNetwork")]
        public void FileSystemQuery_GetComputerNamesOnNetwork_ValidPath_ExpectDirectoryEntryValuesReturned()
        {
            //------------Setup for test--------------------------
            var query = GetFileSystemQuery();

            var res = query.GetComputerNamesOnNetwork("\\\\bob", new List<string>());
            //------------Assert Results-------------------------
            Assert.AreEqual(res.Count, 1);
            Assert.AreEqual("\\\\a", res[0]);

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetComputerNamesOnNetwork")]
        public void FileSystemQuery_GetComputerNamesOnNetwork_ValidPath_ExpectDirectoryEntryValuesReturnedWhenCacheIsPopulated()
        {
            //------------Setup for test--------------------------
            var query = GetFileSystemQuery();

            var res = query.GetComputerNamesOnNetwork("\\\\bob", new List<string>());
            //------------Assert Results-------------------------
            Assert.AreEqual(res.Count, 1);
            Assert.AreEqual("\\\\a", res[0]);
            res = query.GetComputerNamesOnNetwork("\\\\dave", new List<string>());
            Assert.AreEqual(res.Count, 1);
            Assert.AreEqual("\\\\a", res[0]);
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetComputerNamesOnNetwork")]
        public void FileSystemQuery_GetComputerNamesOnNetwork_InvalidPath_ExpectEmpty()
        {
            //------------Setup for test--------------------------
            var query = GetFileSystemQuery();

            var res = query.GetComputerNamesOnNetwork("bob", new List<string>());
            //------------Assert Results-------------------------
            Assert.AreEqual(res.Count, 0);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetComputerNamesOnNetwork")]
        public void FileSystemQuery_GetComputerNamesOnNetwork_NullPath_ExpectEmpty()
        {
            //------------Setup for test--------------------------
            var query = GetFileSystemQuery();

            var res = query.GetComputerNamesOnNetwork(null, new List<string>());
            //------------Assert Results-------------------------
            Assert.AreEqual(res.Count, 0);

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetServerNameFromInput")]
        public void FileSystemQuery_GetServerNameFromInput_ValidPath_ExpectValue()
        {
            //------------Setup for test--------------------------
            var query = GetFileSystemQuery();
            var output = new List<string>();
            string qs = "mp";
            var res = query.GetServerNameFromInput(@"\\bob\", ref output,ref qs );
            //------------Assert Results-------------------------
            Assert.IsTrue(res);
            Assert.AreEqual("bob",qs);

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetServerNameFromInput")]
        public void FileSystemQuery_GetServerNameFromInput_InValidPath_ExpectNoValue()
        {
            //------------Setup for test--------------------------
            var query = GetFileSystemQuery();
            var output = new List<string>();
            string qs = "mp";
            var res = query.GetServerNameFromInput(@"\bob\", ref output, ref qs);
            //------------Assert Results-------------------------
            Assert.IsFalse(res);
            Assert.AreEqual("mp", qs);

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetServerNameFromInput")]
        public void FileSystemQuery_GetServerNameFromInput_InValidPathTermination_ExpectNoValue()
        {
            //------------Setup for test--------------------------
            var query = GetFileSystemQuery();
            var output = new List<string>();
            string qs = "mp";
            var res = query.GetServerNameFromInput(@"\\bob", ref output, ref qs);
            //------------Assert Results-------------------------
            Assert.IsFalse(res);
            Assert.AreEqual("mp", qs);

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetServerNameFromInput")]
        public void FileSystemQuery_GetServerNameFromInput_Noshares_ExpectNoValue()
        {
            //------------Setup for test--------------------------
            var query = GetFileSystemQuery();
            var output = new List<string>();
            string qs = "mp";
            var res = query.GetServerNameFromInput(@"\\bob", ref output, ref qs);
            //------------Assert Results-------------------------
            Assert.IsFalse(res);
            Assert.AreEqual("mp", qs);

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetSharesInformationFromSpecifiedServer")]
        public void FileSystemQuery_GetSharesInformationFromSpecifiedServer_HasShares_Expectalue()
        {
            //------------Setup for test--------------------------
            var query = GetFileSystemQuery();
            var output = new List<string>();

            query.GetSharesInformationFromSpecifiedServer(@"\\bob",  output);
            //------------Assert Results-------------------------

            Assert.AreEqual(output.Count, 1);
            Assert.AreEqual(output.First(), @"\\a\b");
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetSharesInformationFromSpecifiedServer")]
        public void FileSystemQuery_GetSharesInformationFromSpecifiedServer_NoShares_ExpectEmpty()
        {
            //------------Setup for test--------------------------
            var query = GetFileSystemQuery(false);
            var output = new List<string>();

            query.GetSharesInformationFromSpecifiedServer(@"\\bob", output);
            //------------Assert Results-------------------------

            Assert.AreEqual(output.Count, 0);
  
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetAllFilesAndFolders")]
        public void FileSystemQuery_GetAllFilesAndFolders_ValidServer_ExpectValues()
        {
            //------------Setup for test--------------------------
            var query = GetFileSystemQuery();
            var output = new List<string>();

            query.GetAllFilesAndFolders(@"\\bob\", output,'\\');
            //------------Assert Results-------------------------

            Assert.AreEqual(output.Count, 1);
            Assert.AreEqual(output.First(), @"\\a\b");

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetAllFilesAndFolders")]
        public void FileSystemQuery_GetAllFilesAndFolders_InValidServerValidDrive_ExpectValues()
        {
            //------------Setup for test--------------------------
            var query = GetFileSystemQuery();
            var files = new List<string>();

            query.GetAllFilesAndFolders(@"bob", files, 'o');
            //------------Assert Results-------------------------

            Assert.AreEqual(files.Count, 3);
            Assert.AreEqual(files[0], "b");
            Assert.AreEqual(files[1], "d");
            Assert.AreEqual(files[2], "f");

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetAllFilesAndFolders")]
        public void FileSystemQuery_GetAllFilesAndFolders_NullServer_ExpectValues()
        {
            //------------Setup for test--------------------------
            var query = GetFileSystemQuery();
            var files = new List<string>();

            query.GetAllFilesAndFolders(null, files, 'o');
            //------------Assert Results-------------------------

            Assert.AreEqual(files.Count, 0);


        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetFilesAndFoldersFromDrive")]
        public void FileSystemQuery_GetFilesAndFoldersFromDrive_ValidSearchPath_ExpectValues()
        {
            //------------Setup for test--------------------------
            var dir = new Mock<IDirectory>();
            dir.Setup(a => a.GetFileSystemEntries(It.IsAny<string>())).Returns(new[] { "a", "b", "c" });
            dir.Setup(a => a.Exists(It.IsAny<string>())).Returns(true);
            var query = new FileSystemQuery(dir.Object, new DirectoryEntryFactory(), new ShareCollectionFactory());
            var files = new List<string>();

            files= query.GetFilesAndFoldersFromDrive(@"c:\bob", files, 'o');
            //------------Assert Results-------------------------

            Assert.AreEqual(files.Count, 3);
            Assert.AreEqual("a", files[0]);
            Assert.AreEqual("b", files[1]);
            Assert.AreEqual("c", files[2]);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetFilesAndFoldersFromDrive")]
        public void FileSystemQuery_GetFilesAndFoldersFromDrive_NullSearchPath_ExpectCurrentValues()
        {
            //------------Setup for test--------------------------
            var dir = new Mock<IDirectory>();
            dir.Setup(a => a.GetFileSystemEntries(It.IsAny<string>())).Returns(new[] { "a", "b", "c" });
            dir.Setup(a => a.Exists(It.IsAny<string>())).Returns(true);
            var query = new FileSystemQuery(dir.Object, new DirectoryEntryFactory(), new ShareCollectionFactory());
            var files = new List<string>{"non"};

            files = query.GetFilesAndFoldersFromDrive(null, files, 'o');
            //------------Assert Results-------------------------

            Assert.AreEqual(files.Count, 1);
            Assert.AreEqual("non", files[0]);

        }
         [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemQuery_GetFilesAndFoldersFromDrive")]
        public void FileSystemQuery_GetFilesAndFoldersFromDrive_NonDriveSearchPath_ExpectCurrentValues()
        {
            //------------Setup for test--------------------------
            var dir = new Mock<IDirectory>();
            dir.Setup(a => a.GetFileSystemEntries(It.IsAny<string>())).Returns(new[] { "a", "b", "c" });
            dir.Setup(a => a.Exists(It.IsAny<string>())).Returns(true);
            var query = new FileSystemQuery(dir.Object, new DirectoryEntryFactory(), new ShareCollectionFactory());
            var files = new List<string> { "non" };

            files = query.GetFilesAndFoldersFromDrive("bobo", files, 'o');
            //------------Assert Results-------------------------

            Assert.AreEqual(files.Count, 1);
            Assert.AreEqual("non", files[0]);

        }

         [TestMethod]
         [Owner("Leon Rajindrapersadh")]
         [TestCategory("FileSystemQuery_SearchForFileAndFolders")]
         public void FileSystemQuery_SearchForFileAndFolders_DriveSearchPath_ExpectCurrentValues()
         {
             //------------Setup for test--------------------------
             var dir = new Mock<IDirectory>();
             dir.Setup(a => a.GetFileSystemEntries(It.IsAny<string>())).Returns(new[] { "a", "b", "c" });
             dir.Setup(a => a.Exists(It.IsAny<string>())).Returns(true);
             var query = new FileSystemQuery(dir.Object, new DirectoryEntryFactory(), new ShareCollectionFactory());
             var files = new List<string> { "non" };

             files = query.SearchForFileAndFolders(@"c:", files, '\\');
             //------------Assert Results-------------------------

             Assert.AreEqual(files.Count, 3);
             Assert.AreEqual("a", files[0]);
             Assert.AreEqual("b", files[1]);
             Assert.AreEqual("c", files[2]);
         }

         [TestMethod]
         [Owner("Leon Rajindrapersadh")]
         [TestCategory("FileSystemQuery_SearchForFileAndFolders")]
         public void FileSystemQuery_SearchForFileAndFolders_ShareSearchPath_ExpectSharesFromShareCollection()
         {
             //------------Setup for test--------------------------
             var dir = new Mock<IDirectory>();
             dir.Setup(a => a.GetFileSystemEntries(It.IsAny<string>())).Returns(new[] { "a", "b", "c" });
             dir.Setup(a => a.Exists(It.IsAny<string>())).Returns(true);
             var query = GetFileSystemQuery();
             var files = new List<string>();

             files = query.SearchForFileAndFolders(@"\\bob\", files, '\\');
             //------------Assert Results-------------------------

             Assert.AreEqual(files.Count, 1);
             Assert.AreEqual(@"\\a\b", files[0]);

         }

         [TestMethod]
         [Owner("Leon Rajindrapersadh")]
         [TestCategory("FileSystemQuery_QueryList")]
         public void FileSystemQuery_QueryList_EmptyPath_ExpectLocalDrives()
         {
             //------------Setup for test--------------------------
             var dir = new Mock<IDirectory>();
             dir.Setup(a => a.GetLogicalDrives()).Returns(new[] { "a", "b", "c" });
             var query = new FileSystemQuery(dir.Object, new DirectoryEntryFactory(), new ShareCollectionFactory());


             query.QueryList("");
             //------------Assert Results-------------------------
             var files = query.QueryCollection;
             Assert.AreEqual( 3,files.Count);
             Assert.AreEqual("a", files[0]);
             Assert.AreEqual("b", files[1]);
             Assert.AreEqual("c", files[2]);
         }

         [TestMethod]
         [Owner("Leon Rajindrapersadh")]
         [TestCategory("FileSystemQuery_QueryList")]
         public void FileSystemQuery_QueryList_NullPath_ExpectLocalDrives()
         {
             //------------Setup for test--------------------------
             var dir = new Mock<IDirectory>();
             dir.Setup(a => a.GetLogicalDrives()).Returns(new[] { "a", "b", "c" });
             var query = new FileSystemQuery(dir.Object, new DirectoryEntryFactory(), new ShareCollectionFactory());


             query.QueryList(null);
             //------------Assert Results-------------------------
             var files = query.QueryCollection;
             Assert.AreEqual(3, files.Count);
             Assert.AreEqual("a", files[0]);
             Assert.AreEqual("b", files[1]);
             Assert.AreEqual("c", files[2]);
         }
         [TestMethod]
         [Owner("Leon Rajindrapersadh")]
         [TestCategory("FileSystemQuery_QueryList")]
         public void FileSystemQuery_QueryList_DirSearchPath_ExpectValues()
         {
             //------------Setup for test--------------------------
             var dir = new Mock<IDirectory>();
             dir.Setup(a => a.GetFileSystemEntries(It.IsAny<string>())).Returns(new[] { "a", "b", "c" });
             dir.Setup(a => a.Exists(It.IsAny<string>())).Returns(true);
             var query = GetFileSystemQuery();
          

             query.QueryList(@"\\bob\");
             //------------Assert Results-------------------------
             var files = query.QueryCollection;
             Assert.AreEqual(files.Count, 1);
             Assert.AreEqual(@"\\a\b", files[0]);

         }

        private static FileSystemQuery GetFileSystemQuery(bool hasShares = true)
        {

            var dirFact = new Mock<IDirectoryEntryFactory>();
            var dir = new Mock<IDirectoryEntry>();
            var dirLocal = new Mock<IDirectory>();
            dirLocal.Setup(a => a.Exists("bob")).Returns(false);
            dirLocal.Setup(a => a.Exists("bo")).Returns(true);
            dirLocal.Setup(a => a.GetFileSystemEntries(It.IsAny<string>(), It.IsAny<string>())).Returns(new[] { "b", "d", "f" });

            var children = new Mock<IDirectoryEntries>();
            var gChildren = new Mock<IDirectoryEntries>();
            var actualChildren = new List<Mock<IDirectoryEntry>>
                {
                    new Mock<IDirectoryEntry>()
                };
            var actualGChildren = new List<Mock<IDirectoryEntry>>
                {
                    new Mock<IDirectoryEntry>()
                };
            actualGChildren.ForEach(
                b => b.Setup(a => a.Name).Returns("a"));
            actualGChildren.ForEach(
                b => b.Setup(a => a.SchemaClassName).Returns("Computer"));
            actualChildren.ForEach(
                b => b.Setup(a => a.SchemaClassName).Returns("Computer"));
            dirFact.Setup(a => a.Create(It.IsAny<string>())).Returns(dir.Object);

            dir.Setup(a => a.Children).Returns(children.Object);
            children.Setup(a => a.GetEnumerator())
                    .Returns(actualChildren.Select(a => a.Object)
                                           .GetEnumerator());
            actualChildren.First().Setup(a => a.Children).Returns(gChildren.Object);
            gChildren.Setup(a => a.GetEnumerator()).Returns(actualGChildren.Select(a => a.Object).GetEnumerator());
            IList<Share> shares = hasShares? new List<Share>{new Share("a","b",ShareType.Disk)}  : new List<Share>();
            var sFact = new Mock<IShareCollectionFactory>();
            sFact.Setup(a => a.CreateShareCollection(It.IsAny<string>())).Returns(new ShareCollection(shares));

            //------------Execute Test---------------------------
            var query = new FileSystemQuery(dirLocal.Object, dirFact.Object, sFact.Object);
         
            return query;
        }
    }
}
