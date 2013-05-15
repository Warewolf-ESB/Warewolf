using System.IO;

namespace Dev2.Core.Tests.Configuration
{
    public class RuntimeConfigurationAssemblyRepositoryHelperMethods
    {
        #region Helper Methods

        public static byte[] GetTestAssemblyData()
        {
            return File.ReadAllBytes(typeof(RuntimeConfigurationAssemblyRepositoryTests).Assembly.Location);
        }

        #endregion

    }
}
