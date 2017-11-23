using System.Diagnostics;
using System.Reflection;

namespace Warewolf.Studio.AntiCorruptionLayer
{
    public class Utils
    {
        protected Utils()
        {
        }

        public static string FetchVersionInfo()
        {
            var versionResource = GetFileVersionInfo();
            return versionResource.FileVersion;
        }

        private static FileVersionInfo GetFileVersionInfo()
        {
            var asm = Assembly.GetExecutingAssembly();
            var fileName = asm.Location;
            var versionResource = FileVersionInfo.GetVersionInfo(fileName);
            return versionResource;
        }
    }
}
