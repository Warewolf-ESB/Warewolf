using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core
{
    public static class Extentions
    {
        public static bool IsEnvironmentConnected(this IEnvironmentModel model)
        {
            return model != null && model.IsConnected;
        }
    }
}
