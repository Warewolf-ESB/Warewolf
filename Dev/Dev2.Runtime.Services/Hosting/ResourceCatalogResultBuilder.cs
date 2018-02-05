using Dev2.Common.Interfaces.Hosting;

namespace Dev2.Runtime.Hosting
{
    static class ResourceCatalogResultBuilder
    {
        public static ResourceCatalogResult CreateSuccessResult(string msg) => new ResourceCatalogResult()
        {
            Status = ExecStatus.Success,
            Message = msg
        };

        public static ResourceCatalogResult CreateFailResult(string msg) => new ResourceCatalogResult()
        {
            Status = ExecStatus.Fail,
            Message = msg
        };

        public static ResourceCatalogResult CreateAccessViolationResult(string msg)
        {
            return new ResourceCatalogResult()
            {
                Status = ExecStatus.AccessViolation,
                Message = msg
            };
        }
        public static ResourceCatalogResult CreateDuplicateMatchResult(string msg) => new ResourceCatalogResult
        {
            Status = ExecStatus.DuplicateMatch,
            Message = msg
        };

        public static ResourceCatalogResult CreateNoMatchResult(string msg) => new ResourceCatalogResult
        {
            Status = ExecStatus.NoMatch,
            Message = msg
        };

        public static ResourceCatalogResult CreateNoWildcardsAllowedhResult(string msg) => new ResourceCatalogResult
        {
            Status = ExecStatus.NoWildcardsAllowed,
            Message = msg
        };
    }
}