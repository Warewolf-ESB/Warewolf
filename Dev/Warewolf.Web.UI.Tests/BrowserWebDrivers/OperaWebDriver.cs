using OpenQA.Selenium.Opera;
using System;
using System.IO;

namespace Warewolf.Web.UI.Tests.BrowserWebDrivers
{
    public class OperaWebDriver : BaseWebDriver
    {
        static OperaDriverService driverService = OperaDriverService.CreateDefaultService(@"C:\Windows");
        static OperaOptions operaOptions = new OperaOptions() { BinaryLocation = GetOperaPath() };
        public OperaWebDriver() : base(new OperaDriver(driverService, operaOptions, TimeSpan.FromMinutes(3)))
        {
            driverService.Port = 18406;
            operaOptions.AddAdditionalCapability("browserName", "chrome");
            operaOptions.AddAdditionalCapability("browserVersion", "62.0");
            operaOptions.AddArguments(new[] { "--user-data-dir=" + Path.Combine(Environment.CurrentDirectory, "WebDriverProfiles", "Opera"), "start-maximized" });
        }
    }
}
