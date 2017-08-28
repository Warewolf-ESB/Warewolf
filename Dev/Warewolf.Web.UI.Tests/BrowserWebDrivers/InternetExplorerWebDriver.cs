using OpenQA.Selenium.IE;
using System;

namespace Warewolf.Web.UI.Tests.BrowserWebDrivers
{
    public class InternetExplorerWebDriver : BaseWebDriver
    {
        public InternetExplorerWebDriver() : base(new InternetExplorerDriver(@"C:\Windows", new InternetExplorerOptions(), TimeSpan.FromSeconds(180)))
        {
            
        }
    }
}
