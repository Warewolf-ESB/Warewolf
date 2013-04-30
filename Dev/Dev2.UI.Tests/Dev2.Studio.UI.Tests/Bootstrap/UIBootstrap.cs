using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests.Bootstrap
{
    [TestClass]
    public class UIBootstrap
    {

        [AssemblyInitialize]
        public static void Init(TestContext ctx)
        {
            // TODO : Start server and studio
        }

        [AssemblyCleanup]
        public static void Teardown()
        {
            // TODO : Stop server and studio  
        }
    }
}
