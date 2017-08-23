using OpenQA.Selenium.Opera;

namespace Warewolf.Web.UI.Tests.BrowserWebDrivers
{
    public class OperaWebDriver : BaseWebDriver
    {
        public OperaWebDriver() : base(new OperaDriver(operaOptions))
        {
            OperaOptions operaOptions = new OperaOptions() { BinaryLocation = GetOperaPath() };
            operaOptions.AddArguments(new[] { "user-data-dir=" + Path.Combine(Environment.CurrentDirectory, "WebDriverProfiles", "Opera"), "start-maximized" });
        }
    }
}
