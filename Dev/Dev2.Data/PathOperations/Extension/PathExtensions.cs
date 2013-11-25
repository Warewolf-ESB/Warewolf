using Dev2.PathOperations;

namespace Dev2.Data.PathOperations.Extension
{
    public static class PathExtensions
    {
        public static string Combine(this IActivityIOOperationsEndPoint endpoint, string with)
        {
            if (endpoint.IOPath.Path.EndsWith(endpoint.PathSeperator()))
            {
                return endpoint.IOPath.Path + with;
            }
            return endpoint.IOPath.Path + endpoint.PathSeperator() + with;
        }
    }
}