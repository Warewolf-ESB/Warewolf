using System;

namespace AzureFunctions.ProfilerServices.Utility
{
    public static class GlobalVariables
    {
        public static string genericBaseURL = Environment.GetEnvironmentVariable("GenericBaseURL");
        public static string genericEngineResourceAppId = Environment.GetEnvironmentVariable("GenericEngineResourceAppId");
        // Used only when GenericEngineResourceAppId is unset: the VM-era ProfilerServices-CD-*-VM releases
        // call the VM Warewolf server with this token.
        public static string genericSecureWarewolfAuthorizationToken = Environment.GetEnvironmentVariable("GenericSecureWarewolfAuthorizationToken");
        // True once tuprofiler*-function has been re-pointed at the Lightweight Execution Engine.
        public static bool usesEngineIdentity => !string.IsNullOrWhiteSpace(genericEngineResourceAppId);

        public static string? ElasticSearchHost { get; } = Environment.GetEnvironmentVariable("GenericESLoggingSourceHost");
        public static string? ElasticSearchUsername { get; } = Environment.GetEnvironmentVariable("GenericESLoggingUsername");
        public static string? ElasticSearchPassword { get; } = Environment.GetEnvironmentVariable("GenericESLoggingPassword");
        public static string? ElasticConfigurationIndexName { get; } = Environment.GetEnvironmentVariable("AzureFunctionProfilerServicesElasticIndexName");

        public static string elasticSearchHost = Environment.GetEnvironmentVariable("WarewolfGenericESLoggingSourceHost") + ":" + Environment.GetEnvironmentVariable("WarewolfGenericESLoggingSourcePortNo");
        public static string blobStorageConnection = Environment.GetEnvironmentVariable("AzureFunctionLoggingConnectionStringProfilerServices");
    }
}
