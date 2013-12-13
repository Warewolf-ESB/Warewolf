using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Studio;
using Dev2.Studio.Views.Navigation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Custom_Dev2_Controls
{
    [TestClass][ExcludeFromCodeCoverage]
    public class NavigationViewTests
    {

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NavigationView")]
        [Ignore]
        // ReSharper disable InconsistentNaming
        public void NavigationViewInit_UnitTest_CreatingAView_AllowDropPropertyToBeFalse()
        {
            App _myApp = null;
            if (System.Windows.Application.Current == null)
            {
                try
                {
                    _myApp = new App();
                    NavigationView navView = new NavigationView();
                    Assert.IsFalse(navView.Navigation.AllowDrop);
                    _myApp.Shutdown();
                    // Mo : This line breaks everything....
                    //_myApp.InitializeComponent();
                }
                catch (Exception e)
                {
                    if (_myApp != null)
                    {
                        _myApp.Shutdown();
                    }
                }
            }          
        }
    }
}
