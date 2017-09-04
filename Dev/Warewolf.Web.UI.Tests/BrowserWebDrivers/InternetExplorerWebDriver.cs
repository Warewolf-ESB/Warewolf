using OpenQA.Selenium.IE;
using System;

namespace Warewolf.Web.UI.Tests.BrowserWebDrivers
{
    public class InternetExplorerWebDriver : BaseWebDriver
    {
        static InternetExplorerDriverService driverService = InternetExplorerDriverService.CreateDefaultService(Environment.CurrentDirectory);
        public InternetExplorerWebDriver() : base(new InternetExplorerDriver(driverService, new InternetExplorerOptions(), TimeSpan.FromMinutes(3)))
        {
            driverService.Port = 18406;
        }
    }
}
