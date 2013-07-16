using System;
using System.Threading;
using Dev2.Studio;
using Dev2.Studio.Views.Navigation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Custom_Dev2_Controls
{
    [TestClass]
    public class NavigationViewTests
    {
        private static object _testLock = new object();

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NavigationView")]
// ReSharper disable InconsistentNaming
        public void NavigationViewInit_UnitTest_CreatingAView_AllowDropPropertyToBeFalse()
// ReSharper restore InconsistentNaming
        {
            Monitor.Enter(_testLock);
            App _myApp;
            if (System.Windows.Application.Current == null)
            {
                try
                {
                    _myApp = new App();
                    // Mo : This line breaks everything....
                    //_myApp.InitializeComponent();
                }
                catch (Exception e)
                {

                }
            }          

            NavigationView navView = new NavigationView();
            Assert.IsFalse(navView.Navigation.AllowDrop);
            Monitor.Exit(_testLock);
        }
    }
}
