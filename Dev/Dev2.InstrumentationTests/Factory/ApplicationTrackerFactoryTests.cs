using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Instrumentation.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Instrumentation.Factory.Tests
{
    [TestClass()]
    public class ApplicationTrackerFactoryTests
    {
        [TestMethod()]
        public void GetApplicationTrackerProviderTest()
        {
            IApplicationTracker applicationTracker = ApplicationTrackerFactory.GetApplicationTrackerProvider();
            Assert.IsNotNull(applicationTracker, "Unable to get RevulyticsTracker");
        }

    


    }
}