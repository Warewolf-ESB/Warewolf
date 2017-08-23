using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System;
using System.IO;

namespace Warewolf.Web.UI.Tests.BrowserWebDrivers
{
    public class ChromeWebDriver : BaseWebDriver
    {
        static ChromeOptions chromeOptions = new ChromeOptions();
        public ChromeWebDriver() : base(new ChromeDriver(chromeOptions))
        {
            chromeOptions.AddArguments(new[] { "user-data-dir=" + Path.Combine(Environment.CurrentDirectory, "WebDriverProfiles", "Chrome"), "start-maximized" });
        }
    }
}
