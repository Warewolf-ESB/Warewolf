using System.Reflection;

namespace Dev2.Studio.Utils
{
    public static class VersionInfo
    {

        public static string FetchVersionInfo()
        {
            var asm = Assembly.GetExecutingAssembly();
            var asmName = asm.GetName();
            var ver = asmName.Version;

            return ver.ToString();
        }
    }
}
