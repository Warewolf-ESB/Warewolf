using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Launcher;

namespace Dev2.Activities.Specs
{
    [TestClass]
    public static class BuildConfig
    {
        [AssemblyInitialize]
        public static void Apply(TestContext context) => TestLauncher.DisableDocker = Job_Definitions.GetDisableDockerValue();
    }
}
