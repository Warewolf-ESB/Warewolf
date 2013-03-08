using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class MainViewTests
    {
        #region Layout Tests

        [TestMethod]
        public void DefaultDockingLayout()
        {
//            CompositionInitializer.DefaultInitialize();

//            if (Application.Current == null)
//            {
//                // This code is needed to load the resource dictionaries for the app, with out this the instantiation
//                // of any view that uses a resource from app resources will fail.
//                App application = new App();
//            }

//            MainView mvm = new MainView();
//            XamDockManager dockManager  = mvm.FindName("dockManager") as XamDockManager;

//            Assert.IsNotNull(dockManager, "The dock manager is missing from the main view.");

//            MemoryStream ms = new MemoryStream();
//            dockManager.SaveLayout(ms);
//            ms.Seek(0, SeekOrigin.Begin);
//            StreamReader sr = new StreamReader(ms);

//            string actual = sr.ReadToEnd();
//            string expected = @"<?xml version=""1.0"" encoding=""utf-8""?>
//<xamDockManager version=""12.1.20121.2107"">
//  <contentPanes>
//    <contentPane name=""Variables"" location=""DockedLeft"" />
//    <contentPane name=""Toolbox"" location=""DockedLeft"" />
//    <contentPane name=""Explorer"" location=""DockedRight"" />
//    <contentPane name=""OutputPane"" location=""DockedRight"" />
//  </contentPanes>
//  <panes>
//    <splitPane splitterOrientation=""Vertical"" location=""DockedLeft"" extent=""300"">
//      <splitPane splitterOrientation=""Horizontal"">
//        <contentPane name=""Variables"" />
//        <contentPane name=""Toolbox"" />
//      </splitPane>
//    </splitPane>
//    <splitPane splitterOrientation=""Vertical"" location=""DockedRight"" extent=""300"">
//      <tabGroup selectedIndex=""-1"">
//        <contentPane name=""Explorer"" />
//        <contentPane name=""OutputPane"" />
//      </tabGroup>
//    </splitPane>
//  </panes>
//  <documents splitterOrientation=""Vertical"">
//    <splitPane splitterOrientation=""Vertical"">
//      <tabGroup name=""TabManager"" selectedIndex=""-1"" />
//    </splitPane>
//  </documents>
//</xamDockManager>";

//            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Cant relaibly use UI components in a unit test.");
        }

        #endregion Layout Tests
    }
}
