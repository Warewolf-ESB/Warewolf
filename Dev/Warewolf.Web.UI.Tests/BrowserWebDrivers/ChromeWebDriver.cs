using OpenQA.Selenium.Chrome;
using System;
using System.IO;

namespace Warewolf.Web.UI.Tests.BrowserWebDrivers
{
    public class ChromeWebDriver : BaseWebDriver
    {
        static ChromeDriverService driverService = ChromeDriverService.CreateDefaultService(Environment.CurrentDirectory);
        static ChromeOptions chromeOptions = new ChromeOptions();
        public ChromeWebDriver() : base(new ChromeDriver(driverService, chromeOptions, TimeSpan.FromMinutes(3)))
        {
            driverService.Port = 18406;
            chromeOptions.AddArguments(new[] { "user-data-dir=" + Path.Combine(Environment.CurrentDirectory, "WebDriverProfiles", "Chrome"), "start-maximized" });
        }
    }
}
