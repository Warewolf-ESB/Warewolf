using OpenQA.Selenium.Firefox;
using System;
using System.IO;

namespace Warewolf.Web.UI.Tests.BrowserWebDrivers
{
    public class FirefoxWebDriver : BaseWebDriver
    {
        static FirefoxDriverService driverService = FirefoxDriverService.CreateDefaultService(Environment.CurrentDirectory);
        static FirefoxOptions firefoxOptions = new FirefoxOptions() { Profile = new FirefoxProfile(Path.Combine(Environment.CurrentDirectory, "WebDriverProfiles", "Firefox")) };
        public FirefoxWebDriver() : base(new FirefoxDriver(driverService, FirefoxOptions, TimeSpan.FromMinutes(3)))
        {
            driverService.Port = 18406;
        }

        public static FirefoxOptions FirefoxOptions { get { firefoxOptions.AddArgument("-no-sandbox"); return firefoxOptions; } set => firefoxOptions = value; }
    }
}
