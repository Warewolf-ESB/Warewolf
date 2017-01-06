/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
        WebExecuteService,
        WebExecuteSecureWorkflow,
        WebExecutePublicWorkflow,
        WebExecuteGetLogFile,
        WebExecuteGetRootLevelApisJson,
        WebExecuteGetApisJsonForFolder,
        WebBookmarkWorkflow,

        // See Hubs
        HubConnect,

        EsbOnConnected,
        EsbOnDisconnected,
        EsbOnReconnected,
        EsbAddDebugWriter,
        EsbFetchExecutePayloadFragment,
        EsbExecuteCommand,
        EsbAddItemMessage,
        EsbSendMemo,
        EsbFetchResourcesAffectedMemo,
        EsbSendDebugState,
        EsbWrite,

        ResourcesSendMemo,
        WebExecuteInternalService
    }
}
