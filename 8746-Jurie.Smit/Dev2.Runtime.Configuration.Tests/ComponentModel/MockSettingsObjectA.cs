
using Dev2.Runtime.Configuration.ComponentModel;
using Dev2.Runtime.Configuration.ViewModels;
using Dev2.Runtime.Configuration.Views;

namespace Dev2.Runtime.Configuration.Tests.ComponentModel
{
    public class MockSettingsObjectA
    {
        [SettingsObject(typeof(LoggingView), typeof(LoggingViewModel) )]
        public MockSettingsObjectB SettingsB { get; set; }
        [SettingsObject(typeof(LoggingView), typeof(LoggingViewModel))]
        public MockSettingsObjectC SettingsC { get; set; }
    }
}
