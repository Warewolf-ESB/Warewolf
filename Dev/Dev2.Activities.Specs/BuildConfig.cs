﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Launcher;

namespace Dev2.Activities.Specs
{
    [TestClass]
    public static class BuildConfig
    {
        [AssemblyInitialize]
        public static void Apply(TestContext context) => TestLauncher.EnableDocker = Job_Definitions.GetEnableDockerValue();
    }
}
