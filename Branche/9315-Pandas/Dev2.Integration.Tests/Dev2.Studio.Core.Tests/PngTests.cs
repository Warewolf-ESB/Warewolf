using System;
using Dev2.Composition;
using Dev2.Integration.Tests.MEF;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Windows.Controls.Ribbon;
using System.Activities.Presentation.Toolbox;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Views;
using Dev2.Studio;

namespace Dev2.Integration.Tests.Dev2.Studio.Core.Tests
{
    /// <summary>
    /// Summary description for PngTests
    /// </summary>
    [TestClass]
    public class PngTests
    {
        private static ToolboxUserControl toolbox;
        private static ToolboxCategory ControlFlowCat;
        private static ToolboxCategory LoopConstructsCat;
        private static ToolboxCategory RecordsetCat;
        private static ToolboxCategory UtilityCat;
        private static ToolboxCategory FileOrFolderCat;
        private static object _testLock = new object();

        public PngTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;
        static App _myApp;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
         [ClassInitialize()]
         public static void MyClassInitialize(TestContext testContext)
         {
            
         }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {

            
        }
        //
        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            Monitor.Enter(_testLock);
//            if(System.Windows.Application.Current == null)
//            {
//                try
//                {
//                    _myApp = new App();
//                }
//                catch(Exception e)
//                {
//                    
//                }
//                _myApp = null;
//            }
            // Sashen.Naidoo - 12:02:2012 - In order to facilitate the resource dictionary on the Studio
            //                              We have to instantiate the Studio, but not start it up
            //                              once this has occured, we have the Studio's context and the designer icons
            //                              work.


            if (toolbox == null)
            {
                toolbox = new ToolboxUserControl();
                ResourceDictionary dict = new ResourceDictionary();
                Uri uri = new Uri("/Dev2.Studio;component/Resources/ResourceDictionary.xaml", UriKind.Relative);
                dict.Source = uri;
                toolbox.Resources.MergedDictionaries.Add(dict);
                uri = new Uri("/Dev2.Studio;component/Resources/Brushes.xaml", UriKind.Relative);
                dict.Source = uri;
                toolbox.Resources.MergedDictionaries.Add(dict);
                ControlFlowCat = toolbox.GetToolboxCategoryByName("Control Flow");
                LoopConstructsCat = toolbox.GetToolboxCategoryByName("Loop Constructs");
                RecordsetCat = toolbox.GetToolboxCategoryByName("Recordset");
                UtilityCat = toolbox.GetToolboxCategoryByName("Utility");
                FileOrFolderCat = toolbox.GetToolboxCategoryByName("File and Folder");
            }
            ImportService.CurrentContext = CompositionInitializer.InitializeMockedWindowNavigationBehavior();
        }
        //
        // Use TestCleanup to run code after each test has run
        //
        [TestCleanup()]
        public void MyTestCleanup()
        {

            
        }
        
        #endregion

        #region ActivityDesigner Tests

        [TestMethod]
        public void Comment_Designer_Icon_Expected_Pass_NoException()
        {

            DsfCommentActivityDesigner designer = new DsfCommentActivityDesigner();
            ImageDrawing icon = designer.Icon.Drawing as ImageDrawing;
            BitmapSource image = icon.ImageSource as BitmapSource;
            Assert.IsTrue(image != null);
        }

        [TestMethod]
        public void FileRead_Designer_Icon_Expected_Pass_NoException()
        {
            DsfFileReadDesigner designer = new DsfFileReadDesigner();
            ImageDrawing icon = designer.Icon.Drawing as ImageDrawing;
            BitmapSource image = icon.ImageSource as BitmapSource;
            Assert.IsTrue(image != null);
        }

        [TestMethod]
        public void FileWrite_Designer_Icon_Expected_Pass_NoException()
        {
            DsfFileWriteDesigner designer = new DsfFileWriteDesigner();
            ImageDrawing icon = designer.Icon.Drawing as ImageDrawing;
            BitmapSource image = icon.ImageSource as BitmapSource;
            Assert.IsTrue(image != null);
        }

        [TestMethod]
        public void FolderRead_Designer_Icon_Expected_Pass_NoException()
        {
            DsfFolderReadDesigner designer = new DsfFolderReadDesigner();
            ImageDrawing icon = designer.Icon.Drawing as ImageDrawing;
            BitmapSource image = icon.ImageSource as BitmapSource;
            Assert.IsTrue(image != null);
        }

        [TestMethod]
        public void PathCopy_Designer_Icon_Expected_Pass_NoException()
        {
            DsfPathCopyDesigner designer = new DsfPathCopyDesigner();
            ImageDrawing icon = designer.Icon.Drawing as ImageDrawing;
            BitmapSource image = icon.ImageSource as BitmapSource;
            Assert.IsTrue(image != null);
        }

        [TestMethod]
        public void PathCreate_Designer_Icon_Expected_Pass_NoException()
        {
            DsfPathCreateDesigner designer = new DsfPathCreateDesigner();
            ImageDrawing icon = designer.Icon.Drawing as ImageDrawing;
            BitmapSource image = icon.ImageSource as BitmapSource;
            Assert.IsTrue(image != null);
        }

        [TestMethod]
        public void PathDelete_Designer_Icon_Expected_Pass_NoException()
        {
            DsfPathDeleteDesigner designer = new DsfPathDeleteDesigner();
            ImageDrawing icon = designer.Icon.Drawing as ImageDrawing;
            BitmapSource image = icon.ImageSource as BitmapSource;
            Assert.IsTrue(image != null);
        }

        [TestMethod]
        public void PathMove_Designer_Icon_Expected_Pass_NoException()
        {
            DsfPathMoveDesigner designer = new DsfPathMoveDesigner();
            ImageDrawing icon = designer.Icon.Drawing as ImageDrawing;
            BitmapSource image = icon.ImageSource as BitmapSource;
            Assert.IsTrue(image != null);
        }

        [TestMethod]
        public void PathRename_Designer_Icon_Expected_Pass_NoException()
        {
            DsfPathRenameDesigner designer = new DsfPathRenameDesigner();
            ImageDrawing icon = designer.Icon.Drawing as ImageDrawing;
            BitmapSource image = icon.ImageSource as BitmapSource;
            Assert.IsTrue(image != null);
        }

        [TestMethod]
        public void Unzip_Designer_Icon_Expected_Pass_NoException()
        {
            DsfUnzipDesigner designer = new DsfUnzipDesigner();
            ImageDrawing icon = designer.Icon.Drawing as ImageDrawing;
            BitmapSource image = icon.ImageSource as BitmapSource;
            Assert.IsTrue(image != null);
        }

        [TestMethod]
        public void Zip_Designer_Icon_Expected_Pass_NoException()
        {
            DsfZipDesigner designer = new DsfZipDesigner();
            ImageDrawing icon = designer.Icon.Drawing as ImageDrawing;
            BitmapSource image = icon.ImageSource as BitmapSource;
            Assert.IsTrue(image != null);
        }

        [TestMethod]
        public void Calculate_Designer_Icon_Expected_Pass_NoException()
        {
            DsfCalculateActivityDesigner designer = new DsfCalculateActivityDesigner();
            ImageDrawing icon = designer.Icon.Drawing as ImageDrawing;
            BitmapSource image = icon.ImageSource as BitmapSource;
            Assert.IsTrue(image != null);
        }

        [TestMethod]
        public void CountRecordset_Designer_Icon_Expected_Pass_NoException()
        {
            DsfCountRecordsetActivityDesigner designer = new DsfCountRecordsetActivityDesigner();
            ImageDrawing icon = designer.Icon.Drawing as ImageDrawing;
            BitmapSource image = icon.ImageSource as BitmapSource;
            Assert.IsTrue(image != null);
        }

        [TestMethod]
        public void Http_Designer_Icon_Expected_Pass_NoException()
        {
            DsfHttpActivityDesigner designer = new DsfHttpActivityDesigner();
            ImageDrawing icon = designer.Icon.Drawing as ImageDrawing;
            BitmapSource image = icon.ImageSource as BitmapSource;
            Assert.IsTrue(image != null);
        }

        [TestMethod]
        public void SortRecords_Designer_Icon_Expected_Pass_NoException()
        {
            DsfSortRecordsActivityDesigner designer = new DsfSortRecordsActivityDesigner();
            ImageDrawing icon = designer.Icon.Drawing as ImageDrawing;
            BitmapSource image = icon.ImageSource as BitmapSource;
            Assert.IsTrue(image != null);
        }

        [TestMethod]
        public void WebPage_Designer_Icon_Expected_Pass_NoException()
        {
            DsfWebPageActivityDesigner designer = new DsfWebPageActivityDesigner();
            ImageDrawing icon = designer.Icon.Drawing as ImageDrawing;
            BitmapSource image = icon.ImageSource as BitmapSource;
            Assert.IsTrue(image != null);
        }

        [TestMethod]
        public void WebSite_Designer_Icon_Expected_Pass_NoException()
        {
            DsfWebSiteActivityDesigner designer = new DsfWebSiteActivityDesigner();
            ImageDrawing icon = designer.Icon.Drawing as ImageDrawing;
            BitmapSource image = icon.ImageSource as BitmapSource;
            Assert.IsTrue(image != null);
        }

        #endregion ActivityDesigner Tests

        #region Toolbox Tests

        [TestMethod]
        public void Comment_Icon_Expected_Pass_NoException()
        {
            ToolboxItemWrapper item = UtilityCat.Tools.Single(c => c.DisplayName == "Comment");
            Assert.AreEqual("/images/comment_add.png", item.BitmapName);
        }

        [TestMethod]
        public void FileRead_Icon_Expected_Pass_NoException()
        {
            ToolboxItemWrapper item = FileOrFolderCat.Tools.Single(c => c.DisplayName == "Read File");
            Assert.AreEqual("/Images/fileread.png", item.BitmapName);
        }

        [TestMethod]
        public void FileWrite_Icon_Expected_Pass_NoException()
        {
            ToolboxItemWrapper item = FileOrFolderCat.Tools.Single(c => c.DisplayName == "Write");
            Assert.AreEqual("/Images/writefile.png", item.BitmapName);
        }

        [TestMethod]
        public void FolderRead_Icon_Expected_Pass_NoException()
        {
            ToolboxItemWrapper item = FileOrFolderCat.Tools.Single(c => c.DisplayName == "Read Folder");
            Assert.AreEqual("/Images/folderread.png", item.BitmapName);
        }

        [TestMethod]
        public void PathCopy_Icon_Expected_Pass_NoException()
        {
            ToolboxItemWrapper item = FileOrFolderCat.Tools.Single(c => c.DisplayName == "Copy");
            Assert.AreEqual("/Images/copy.png", item.BitmapName);
        }

        [TestMethod]
        public void PathCreate_Icon_Expected_Pass_NoException()
        {
            ToolboxItemWrapper item = FileOrFolderCat.Tools.Single(c => c.DisplayName == "Create");
            Assert.AreEqual("/Images/createfileorfolder.png", item.BitmapName);
        }

        [TestMethod]
        public void PathDelete_Icon_Expected_Pass_NoException()
        {
            ToolboxItemWrapper item = FileOrFolderCat.Tools.Single(c => c.DisplayName == "Delete");
            Assert.AreEqual("/Images/delete.png", item.BitmapName);
        }

        [TestMethod]
        public void PathMove_Icon_Expected_Pass_NoException()
        {
            ToolboxItemWrapper item = FileOrFolderCat.Tools.Single(c => c.DisplayName == "Move");
            Assert.AreEqual("/Images/move.png", item.BitmapName);
        }

        [TestMethod]
        public void PathRename_Icon_Expected_Pass_NoException()
        {
            ToolboxItemWrapper item = FileOrFolderCat.Tools.Single(c => c.DisplayName == "Rename");
            Assert.AreEqual("/Images/rename.png", item.BitmapName);
        }

        [TestMethod]
        public void Unzip_Icon_Expected_Pass_NoException()
        {
            ToolboxItemWrapper item = FileOrFolderCat.Tools.Single(c => c.DisplayName == "Unzip");
            Assert.AreEqual("/Images/unzip.png", item.BitmapName);
        }

        [TestMethod]
        public void Zip_Icon_Expected_Pass_NoException()
        {
            ToolboxItemWrapper item = FileOrFolderCat.Tools.Single(c => c.DisplayName == "Zip");
            Assert.AreEqual("/Images/zip.png", item.BitmapName);
        }

        [TestMethod]
        public void Calculate_Icon_Expected_Pass_NoException()
        {
            ToolboxItemWrapper item = UtilityCat.Tools.Single(c => c.DisplayName == "Calculate");
            Assert.AreEqual("/images/calculator.png", item.BitmapName);
        }

        [TestMethod]
        public void CountRecordset_Icon_Expected_Pass_NoException()
        {
            ToolboxItemWrapper item = RecordsetCat.Tools.Single(c => c.DisplayName == "Count Records");
            Assert.AreEqual("/images/counter.png", item.BitmapName);
        }

        [TestMethod]
        public void DataSplit_Icon_Expected_Pass_NoException()
        {
            ToolboxItemWrapper item = UtilityCat.Tools.Single(c => c.DisplayName == "Data Split");
            Assert.AreEqual("/images/split.png", item.BitmapName);
        }

        [TestMethod]
        public void DateTime_Icon_Expected_Pass_NoException()
        {
            ToolboxItemWrapper item = UtilityCat.Tools.Single(c => c.DisplayName == "Date and Time");
            Assert.AreEqual("/images/calendar-day.png", item.BitmapName);
        }

        [TestMethod]
        public void ForEach_Icon_Expected_Pass_NoException()
        {
            ToolboxItemWrapper item = LoopConstructsCat.Tools.Single(c => c.DisplayName == "For Each");
            Assert.AreEqual("/images/Loop.png", item.BitmapName);
        }

        [TestMethod]
        public void FormatNumber_Icon_Expected_Pass_NoException()
        {
            ToolboxItemWrapper item = UtilityCat.Tools.Single(c => c.DisplayName == "Format Number");
            Assert.AreEqual("/images/FormatNumber.png", item.BitmapName);
        }

        [TestMethod]
        public void MutiAssign_Icon_Expected_Pass_NoException()
        {
            ToolboxItemWrapper item = UtilityCat.Tools.Single(c => c.DisplayName == "Assign");
            Assert.AreEqual("/images/assign.png", item.BitmapName);
        }

        [TestMethod]
        public void SortRecords_Icon_Expected_Pass_NoException()
        {
            ToolboxItemWrapper item = RecordsetCat.Tools.Single(c => c.DisplayName == "Sort Records");
            Assert.AreEqual("/images/sorting.png", item.BitmapName);
        }

        // 28-Jan-2013 - Test commented out by Michael since Human Interfaces are not being included in Release 2
        /*
        [TestMethod]
        public void WebPage_Icon_Expected_Pass_NoException()
        {
            ToolboxItemWrapper item = UtilityCat.Tools.Single(c => c.DisplayName == "Human Interface");
            Assert.AreEqual("/images/User.png", item.BitmapName);
        }
         */

        #endregion Toolbox Tests

        #region Private Methods

        private List<RibbonButton> GetRibbonButtonsRecusively(DependencyObject dp)
        {
            List<RibbonButton> imageList = new List<RibbonButton>();
            int numOfChildren = VisualTreeHelper.GetChildrenCount(dp);
            int count = 0;
            while (count < numOfChildren)
            {
                DependencyObject dpObj = VisualTreeHelper.GetChild(dp, count);
                RibbonButton tmpButton = dpObj as RibbonButton;
                if (tmpButton != null && tmpButton.DataContext is ToolboxItemWrapper)
                {
                    imageList.Add(tmpButton);
                }
                imageList.AddRange(GetRibbonButtonsRecusively(dpObj));
            }

            return imageList;
        }

        #endregion Private Methods
    }
}
