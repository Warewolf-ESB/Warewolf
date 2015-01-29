using System.Reflection;
using Vestris.ResourceLib;

namespace Warewolf.Studio.Core
{
    public class Utils
    {
        public static string FetchVersionInfo()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            var versionResource = new VersionResource();
            string fileName = asm.Location;
            versionResource.LoadFrom(fileName);
            return versionResource.FileVersion;
        }
    }
}
