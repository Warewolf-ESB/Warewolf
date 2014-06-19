namespace Dev2.Services.Security
{
    public enum WebServerRequestType
    {
        Unknown,

        // See Controllers
        WebGetDecisions,
        WebGetDialogs,
        WebGetServices,
        WebGetSources,
        WebGetSwitch,

        WebGet,
        WebGetContent,
        WebGetImage,
        WebGetScript,
        WebGetView,
        WebInvokeService,
        WebExecuteWorkflow,
        WebBookmarkWorkflow,

        // See Hubs
        HubConnect,

        EsbOnConnected,
        EsbOnReconnected,
        EsbAddDebugWriter,
        EsbFetchExecutePayloadFragment,
        EsbExecuteCommand,
        EsbAddItemMessage,
        EsbSendMemo,
        EsbSendDebugState,
        EsbWrite,

        ResourcesSendMemo,
        WebExecuteInternalService
    }
}