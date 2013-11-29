namespace Dev2.Runtime.WebServer
{
    public enum WebServerRequestType
    {
        Unknown,

        // See Controllers
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

        EsbSendMemo,

        ResourcesSend,
        ResourcesSendMemo,
        ResourcesSave,
    }
}