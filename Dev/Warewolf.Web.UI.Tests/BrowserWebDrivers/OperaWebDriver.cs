using OpenQA.Selenium.Opera;

namespace Warewolf.Web.UI.Tests.BrowserWebDrivers
{
    public class OperaWebDriver : BaseWebDriver
    {
        public OperaWebDriver() : base(new OperaDriver(new OperaOptions() { BinaryLocation = GetOperaPath() }))
        {
            
        }
    }
}
