using OpenQA.Selenium.Chrome;
using System;

namespace Warewolf.Web.UI.Tests.BrowserWebDrivers
{
    public class ChromeIncognitoWebDriver : BaseWebDriver
    {
        static ChromeDriverService driverService = ChromeDriverService.CreateDefaultService(Environment.CurrentDirectory);
        static ChromeOptions chromeOptions = new ChromeOptions();
        public ChromeIncognitoWebDriver() : base(new ChromeDriver(driverService, chromeOptions, TimeSpan.FromMinutes(3)))
        {
            driverService.Port = 18406;
            chromeOptions.AddArguments(new[] { "--test-type", "start-maximized", "--incognito" });
        }
    }
}
