
using Dev2.Runtime.Configuration.ComponentModel;
using Dev2.Runtime.Configuration.ViewModels;
using Dev2.Runtime.Configuration.Views;

namespace Dev2.Runtime.Configuration.Tests.ComponentModel
{
    public class MockSettingsObjectC
    {
        [SettingsObject(typeof(LoggingView), typeof(LoggingViewModel))]
        public MockSettingsObjectA SettingsA { get; set; }
    }
}
