#if DEBUG
using com.sun.tools.javadoc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2
{
    [TestClass]
    public class CoverageEntryPoint
    {
        [TestMethod]
        public void CoverageEntrypoint()=>Main.main();
    }
}
#endif