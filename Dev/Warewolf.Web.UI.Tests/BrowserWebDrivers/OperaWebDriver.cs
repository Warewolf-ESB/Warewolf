using OpenQA.Selenium.Opera;
using System;
using System.IO;

namespace Warewolf.Web.UI.Tests.BrowserWebDrivers
{
    public class OperaWebDriver : BaseWebDriver
    {
        static OperaOptions operaOptions = new OperaOptions() { BinaryLocation = GetOperaPath() };
        public OperaWebDriver() : base(new OperaDriver(@"C:\windows", operaOptions, TimeSpan.FromSeconds(180)))
        {
            operaOptions.AddArguments(new[] { "user-data-dir=" + Path.Combine(Environment.CurrentDirectory, "WebDriverProfiles", "Opera"), "start-maximized" });
        }
    }
}
