using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Instrumentation.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Instrumentation;

namespace Warewolf.ApplicationTracker.Tests
{
    [TestClass()]
    public class ApplicationTrackerFactoryTests
    {
        [TestMethod()]
        public void GetApplicationTrackerProviderTest()
        {
          IApplicationTracker  applicationTracker = RevulyticsTracker.GetTrackerInstance();
          Assert.IsNotNull(applicationTracker, "Unable to get RevulyticsTracker");
        }
    }
}