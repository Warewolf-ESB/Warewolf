using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Composition;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.Explorer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class ExplorerViewModelTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsNullExceptionForEnvironmentRepo()
        {
            ImportServiceContext ctx = CompositionInitializer.InitializeMockedMainViewModel();
            var vm = new ExplorerViewModel(null);
        }
    }
}
