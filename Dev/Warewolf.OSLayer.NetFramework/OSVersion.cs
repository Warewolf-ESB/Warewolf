using System;

namespace Warewolf.OSLayer
{
    public class OSLayerVersion
    {
        public static System.Version GetOSFramework()
        {
            return Environment.Version;
        }
    }
}
