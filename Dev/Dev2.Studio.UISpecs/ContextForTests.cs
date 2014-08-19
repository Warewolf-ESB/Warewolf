using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Specs
{
    [TestClass]
    public class ContextForTests
    {
        public static string DeploymentDirectory;
        public static bool IsLocal = false;

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext testCtx)
        {
            DeploymentDirectory = testCtx.DeploymentDirectory;
            IsLocal = testCtx.Properties["ControllerName"] == null || testCtx.Properties["ControllerName"].ToString() == "localhost:6901";
        }
    }
}