using OpenQA.Selenium.Chrome;
using System;

namespace Warewolf.Web.UI.Tests.BrowserWebDrivers
{
    public class ChromeIncognitoWebDriver : BaseWebDriver
    {
        static ChromeOptions chromeOptions = new ChromeOptions();
        public ChromeIncognitoWebDriver() : base(new ChromeDriver(@"C:\Windows", chromeOptions, TimeSpan.FromSeconds(180)))
        {
            chromeOptions.AddArguments(new[] { "--test-type", "start-maximized", "--incognito" });
        }
    }
}
