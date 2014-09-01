using System;
using System.Reflection;
using Vestris.ResourceLib;

// ReSharper disable CheckNamespace
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

        public static Version FetchVersionInfoAsVersion()
        {
            var asm = Assembly.GetExecutingAssembly();
            var versionResource = new VersionResource();
            var fileName = asm.Location;
            versionResource.LoadFrom(fileName);

            Version v = new Version(versionResource.FileVersion);

            return v;
        }
    }
}
