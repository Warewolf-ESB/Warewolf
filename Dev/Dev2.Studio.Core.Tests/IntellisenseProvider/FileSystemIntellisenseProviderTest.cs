using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using Dev2.Composition;
using Dev2.Intellisense.Helper;
using Dev2.Intellisense.Provider;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.IntellisenseProvider
{
    [TestClass]    
    // ReSharper disable InconsistentNaming
    public class FileSystemIntellisenseProviderTest
    {
        private IResourceModel _resourceModel;

        #region Test Initialization

        [TestInitialize]
        public void Init()
        {
            Monitor.Enter(DataListSingletonTest.DataListSingletonTestGuard);

            ImportService.CurrentContext = CompositionInitializer.InitializeForMeflessBaseViewModel();

            var testEnvironmentModel = ResourceModelTest.CreateMockEnvironment();



            _resourceModel = new ResourceModel(testEnvironmentModel.Object)
            {
                ResourceName = "test",
                ResourceType = ResourceType.Service,
                DataList = @"
            <DataList>
                    <Scalar/>
                    <Country/>
                    <State />
                    <City>
                        <Name/>
                        <GeoLocation />
                    </City>
             </DataList>
            "
            };

            IDataListViewModel setupDatalist = new DataListViewModel();
            DataListSingleton.SetDataList(setupDatalist);
            DataListSingleton.ActiveDataList.InitializeDataListViewModel(_resourceModel);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Monitor.Exit(DataListSingletonTest.DataListSingletonTestGuard);
        }

        #endregion Test Initialization


        [TestMethod]
        public void GetIntellisenseResultsWhereNothingPassedExpectListOfDrives()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 0,
                InputText = "",
                IsInCalculateMode = false,
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };
            var intellisenseProvider = CreateIntellisenseProvider();
            //------------Execute Test---------------------------
            var intellisenseProviderResults = intellisenseProvider.GetIntellisenseResults(context);
            //------------Assert Results-------------------------
            Assert.AreEqual(8, intellisenseProviderResults.Count);
        }

        [TestMethod]
        public void GetIntellisenseResultsWhereDrivePassedExpectFoldersAndFilesOnDrive()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 2,
                InputText = "C:",
                IsInCalculateMode = false,
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };
            var intellisenseProvider = CreateIntellisenseProvider();
            //------------Execute Test---------------------------
            var intellisenseProviderResults = intellisenseProvider.GetIntellisenseResults(context);
            //------------Assert Results-------------------------
            Assert.AreEqual(31, intellisenseProviderResults.Count);

        }

        [TestMethod]
        public void GetIntellisenseResultsWhereDriveAndFolderPassedNoSlashExpectFolder()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 9,
                InputText = @"C:\Users",
                IsInCalculateMode = false,
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };
            //------------Setup for test--------------------------
            var intellisenseProvider = CreateIntellisenseProvider();
            //------------Execute Test---------------------------
            var intellisenseProviderResults = intellisenseProvider.GetIntellisenseResults(context);
            //------------Assert Results-------------------------
            Assert.AreEqual(9, intellisenseProviderResults.Count);
        }

        [TestMethod]
        public void GetIntellisenseResultsWhereDriveAndFolderWithStartOfFileNamePassedExpectFileName()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 13,
                InputText = @"C:\Users\des",
                IsInCalculateMode = false,
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };
            //------------Setup for test--------------------------
            var intellisenseProvider = CreateIntellisenseProvider();
            //------------Execute Test---------------------------
            var intellisenseProviderResults = intellisenseProvider.GetIntellisenseResults(context);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, intellisenseProviderResults.Count);
        }

        [TestMethod]
        public void GetIntellisenseResultsWhereDriveAndFolderWithPartOfFileNamePassedExpectFileName()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 13,
                InputText = @"C:\Users\skt",
                IsInCalculateMode = false,
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };
            //------------Setup for test--------------------------
            var intellisenseProvider = CreateIntellisenseProvider();
            //------------Execute Test---------------------------
            var intellisenseProviderResults = intellisenseProvider.GetIntellisenseResults(context);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, intellisenseProviderResults.Count);
        }

        [TestMethod]
        public void GetIntellisenseResultsWhereNoNetworkExpectFolderNetworkShareInformation()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 2,
                InputText = @"\\",
                IsInCalculateMode = false,
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };
            //------------Setup for test--------------------------
            var intellisenseProvider = CreateIntellisenseProvider();
            //------------Execute Test---------------------------
            var intellisenseProviderResults = intellisenseProvider.GetIntellisenseResults(context);
            //------------Assert Results-------------------------
            Assert.AreEqual(40, intellisenseProviderResults.Count);
        }

        [TestMethod]
        public void GetIntellisenseResultsWhereNetworkPathExpectFolderNetworkShareInformation()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 18,
                InputText = @"\\RSAKLFSVRTFSBLD\",
                IsInCalculateMode = false,
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };
            //------------Setup for test--------------------------
            var intellisenseProvider = CreateIntellisenseProvider();
            //------------Execute Test---------------------------
            var intellisenseProviderResults = intellisenseProvider.GetIntellisenseResults(context);
            //------------Assert Results-------------------------
            Assert.AreEqual(6, intellisenseProviderResults.Count);
        }

        [TestMethod]
        public void GetIntellisenseResultsWhereNetworkPathHasFilesExpectFolderWithFilesNetworkShareInformation()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 36,
                InputText = @"\\RSAKLFSVRTFSBLD\DevelopmentDropOff",
                IsInCalculateMode = false,
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };
            //------------Setup for test--------------------------
            var intellisenseProvider = CreateIntellisenseProvider();
            //------------Execute Test---------------------------
            var intellisenseProviderResults = intellisenseProvider.GetIntellisenseResults(context);
            //------------Assert Results-------------------------
            Assert.AreEqual(16, intellisenseProviderResults.Count);
        }


        [TestMethod]
        public void GetIntellisenseResultsWhereNetworkPathHasFolderExpectFolderInformation()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 41,
                InputText = @"\\RSAKLFSVRTFSBLD\DevelopmentDropOff\_Arch",
                IsInCalculateMode = false,
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };
            //------------Setup for test--------------------------
            var intellisenseProvider = CreateIntellisenseProvider();
            //------------Execute Test---------------------------
            var intellisenseProviderResults = intellisenseProvider.GetIntellisenseResults(context);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, intellisenseProviderResults.Count);
        }

        [TestMethod]
        public void GetIntellisenseResultsWhereNetworkPathHasFileExpectFileInformation()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 44,
                InputText = @"\\RSAKLFSVRTFSBLD\DevelopmentDropOff\LoadTest",
                IsInCalculateMode = false,
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };
            //------------Setup for test--------------------------
            var intellisenseProvider = CreateIntellisenseProvider();
            //------------Execute Test---------------------------
            var intellisenseProviderResults = intellisenseProvider.GetIntellisenseResults(context);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, intellisenseProviderResults.Count);
        }

        [TestMethod]
        public void GetIntellisenseResultsWhereNetworkPathHasMiddleOfFileExpectFileInformation()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 39,
                InputText = @"\\RSAKLFSVRTFSBLD\DevelopmentDropOff\Runt",
                IsInCalculateMode = false,
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };
            //------------Setup for test--------------------------
            var intellisenseProvider = CreateIntellisenseProvider();

            //------------Execute Test---------------------------
            var intellisenseProviderResults = intellisenseProvider.GetIntellisenseResults(context);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, intellisenseProviderResults.Count);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemIntellisenseProvider_PerformMethodInsertion")]
        public void FileSystemIntellisenseProvider_GetIntellisenseResults_EntireSet_ExpectCorrectOutput()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 39,
                InputText = @"\\RSAKLFSVRTFSBLD\DevelopmentDropOff\Runt",
                IsInCalculateMode = false,
                DesiredResultSet = IntellisenseDesiredResultSet.EntireSet
            };
            //------------Setup for test--------------------------
            var intellisenseProvider = CreateIntellisenseProvider();

            //------------Execute Test---------------------------
            var intellisenseProviderResults = intellisenseProvider.GetIntellisenseResults(context);
            //------------Assert Results-------------------------
            Assert.AreEqual(8, intellisenseProviderResults.Count);
        }

        public void FileSystemIntellisenseProvider_ExecuteInsertion(int caretPosition, string inputText, string inserted, string expected)
        {
            //------------Setup for test--------------------------
            var fileSystemIntellisenseProvider = new FileSystemIntellisenseProvider();
            
            //------------Execute Test---------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = caretPosition,
                InputText = inputText,
                IsInCalculateMode = false,
                DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
            };

           var resp =  fileSystemIntellisenseProvider.PerformResultInsertion(inserted, context);
            //------------Assert Results-------------------------
            Assert.AreEqual(resp, expected);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FileSystemIntellisenseProvider_PerformResultInsertion")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FileSystemIntellisenseProvider_PerformResultInsertion_ContextIsNull_ThrowsException()
        {
            //------------Setup for test--------------------------
            var fileSystemIntellisenseProvider = new FileSystemIntellisenseProvider();
            //------------Execute Test---------------------------
            fileSystemIntellisenseProvider.PerformResultInsertion("", null);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DefaultIntellisenseProvider_FileSystemIntellisenseProvider")]
        public void FileSystemIntellisenseProvider_GetIntellisenseResults_ContextIsNull_ResultCountIsZero()
        {
            //------------Execute Test---------------------------
            var getResults = new FileSystemIntellisenseProvider().GetIntellisenseResults(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, getResults.Count);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemIntellisenseProvider_PerformMethodInsertion")]
        public void FileSystemIntellisenseProvider_PerformMethodInsertion_InsertPath_ExpectCorrectOutput()
        {
            FileSystemIntellisenseProvider_ExecuteInsertion(2, "a ", @"c:\", @"a c:\");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemIntellisenseProvider_PerformMethodInsertion")]
        public void FileSystemIntellisenseProvider_PerformMethodInsertion_InsertPathAfterLanguageElement_ExpectCorrectOutput()
        {
            FileSystemIntellisenseProvider_ExecuteInsertion(2, "[[a]] ", @"c:\", @"c:\");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemIntellisenseProvider_PerformMethodInsertion")]
        public void FileSystemIntellisenseProvider_PerformMethodInsertion_EmptyInput_ExpectCorrectOutput()
        {
            FileSystemIntellisenseProvider_ExecuteInsertion(0, "", @"c:\", @"c:\");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemIntellisenseProvider_PerformMethodInsertion")]
        public void FileSystemIntellisenseProvider_PerformMethodInsertion_NegativeCaret_ExpectEmptyOutput()
        {
            FileSystemIntellisenseProvider_ExecuteInsertion(-1, "", @"c:\", @"");
        }
        
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemIntellisenseProvider_PerformMethodInsertion")]
        public void FileSystemIntellisenseProvider_PerformMethodInsertion_InsertPathinsideText_ExpectCorrectOutput()
        {
            FileSystemIntellisenseProvider_ExecuteInsertion(2, "bobthebuilder", @"c:\", @"c:\");
            FileSystemIntellisenseProvider_ExecuteInsertion(2, "bobthebuilder doratheexplorer", @"c:\", @"c:\ doratheexplorer");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FileSystemIntellisenseProvider_Dispose")]
        public void FileSystemIntellisenseProvider_PerformMethodInsertion_Dispose()
        {

            var intellisenseProvider = CreateIntellisenseProvider();
            Assert.IsNotNull(intellisenseProvider.IntellisenseResults);
            intellisenseProvider.Dispose();
            Assert.IsNull(intellisenseProvider.IntellisenseResults);

        }
        static FileSystemIntellisenseProvider CreateIntellisenseProvider()
        {
            var intellisenseProvider = new FileSystemIntellisenseProvider { FileSystemQuery = new FileSystemQueryForTest() };
            return intellisenseProvider;
        }
    }


    class FileSystemQueryForTest : IFileSystemQuery
    {

        #region Implementation of IFileSystemQuery

        public List<string> QueryCollection { get; private set; }
        public void QueryList(string searchPath)
        {
            QueryCollection = new List<string>();
            switch(searchPath)
            {
                case @"\\RSAKLFSVRTFSBLD\DevelopmentDropOff\Runt":
                    AddToList(1);
                    break;
                case @"\\RSAKLFSVRTFSBLD\DevelopmentDropOff\LoadTest":
                    AddToList(1);
                    break;
                case @"\\RSAKLFSVRTFSBLD\DevelopmentDropOff\_Arch":
                    AddToList(1);
                    break;
                case @"\\RSAKLFSVRTFSBLD\DevelopmentDropOff":
                    AddToList(16);
                    break;
                case @"\\RSAKLFSVRTFSBLD\":
                    AddToList(6);
                    break;
                case @"\\":
                    AddToList(40);
                    break;
                case @"C:\Users\skt":
                    AddToList(1);
                    break;
                case @"C:\Users\des":
                    AddToList(1);
                    break;
                case @"C:\Users":
                    AddToList(9);
                    break;
                case @"C:":
                    AddToList(31);
                    break;
                case "":
                    AddToList(8);
                    break;
            }
        }

        void AddToList(int times)
        {
            for(int i = 0; i < times; i++)
            {
                QueryCollection.Add(i.ToString(CultureInfo.InvariantCulture));
            }
        }

        #endregion
    }
}