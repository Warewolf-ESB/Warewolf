using System.Reflection;
using Vestris.ResourceLib;

namespace Dev2.Studio.Utils
{
    public static class VersionInfo
    {

        public static string FetchVersionInfo()
        {
            var asm = Assembly.GetExecutingAssembly();
            var versionResource = new VersionResource();
            var fileName = asm.Location;
            versionResource.LoadFrom(fileName);

            return versionResource.FileVersion;
        }
    }
}
