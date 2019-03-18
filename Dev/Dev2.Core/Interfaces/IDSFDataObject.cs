#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Interfaces.Enums;
using Dev2.Web;
using Warewolf.Storage.Interfaces;
using Dev2;

namespace Dev2.Interfaces
{
    public interface IDSFDataObject
    {
        Dictionary<int, List<Guid>> ThreadsToDispose { get; set; }

        string CurrentBookmarkName { get; set; }
        Guid WorkflowInstanceId { get; set; }
        string ServiceName { get; set; }
        string TestName { get; set; }
        bool IsServiceTestExecution { get; set; }
        bool IsDebugFromWeb { get; set; }
        string ParentWorkflowInstanceId { get; set; }
        string ParentServiceName { get; set; }
        StringBuilder DataList { get; set; }
        bool WorkflowResumeable { get; set; }
        bool IsDebug { get; set; }
        Guid WorkspaceID { get; set; }
        Guid ResourceID { get; set; }
        Guid SourceResourceID { get; set; }
        Guid OriginalInstanceID { get; set; }
        Guid ClientID { get; set; }
        bool IsOnDemandSimulation { get; set; }
        Guid ServerID { get; set; }
        string VersionNumber { get; set; }
        int NumberOfSteps { get; set; }
        IPrincipal ExecutingUser { get; set; }
        Guid DatalistOutMergeID { get; set; }
        enDataListMergeTypes DatalistOutMergeType { get; set; }
        enTranslationDepth DatalistOutMergeDepth { get; set; }
        DataListMergeFrequency DatalistOutMergeFrequency { get; set; }

        Guid DatalistInMergeID { get; set; }
        enDataListMergeTypes DatalistInMergeType { get; set; }
        enTranslationDepth DatalistInMergeDepth { get; set; }

        Guid ExecutionCallbackID { get; set; }
        Guid BookmarkExecutionCallbackID { get; set; }
        string ParentInstanceID { get; set; }

        EmitionTypes ReturnType { get; set; }

        // 2012.11.05 : Travis.Frisinger - Added for Binary DataList
        Guid DataListID { get; set; }
        bool IsDataListScoped { get; set; }
        bool ForceDeleteAtNextNativeActivityCleanup { get; set; }
        StringBuilder RawPayload { get; set; }
        StringBuilder RemoteInvokeResultShape { get; set; }
        bool RemoteInvoke { get; set; }
        //TODO: Change this to Guid
        string RemoteInvokerID { get; set; }
        IList<IDebugState> RemoteDebugItems { get; set; }
        string RemoteServiceType { get; set; }

        ExecutionOrigin ExecutionOrigin { get; set; }
        string ExecutionOriginDescription { get; set; }
        bool IsFromWebServer { get; set; }

        Guid EnvironmentID { get; set; }

        int ParentThreadID { get; set; }

        Guid DebugSessionID { get; set; }
        Guid ParentID { get; set; }

        bool RunWorkflowAsync { get; set; }
        bool IsDebugNested { get; set; }

        bool IsRemoteInvoke { get; }
        bool IsRemoteInvokeOverridden { get; set; }

        int ForEachNestingLevel { get; set; }
        IDSFDataObject Clone();
        bool IsDebugMode();
        bool IsRemoteWorkflow();
        IExecutionEnvironment Environment { get; set; }
        IExecutionToken ExecutionToken { get; set; }

        void PopEnvironment();
        void PushEnvironment(IExecutionEnvironment env);

        IEsbChannel EsbChannel { get; set; }
        int ForEachUpdateValue { get; set; }
        DateTime StartTime { get; set; }
        Guid DebugEnvironmentId { get; set; }
        bool RemoteNonDebugInvoke { get; set; }
        bool StopExecution { get; set; }
        IServiceTestModelTO ServiceTest { get; set; }
        List<Guid> TestsResourceIds { get; set; }
        Guid? ExecutionID { get; set; }
        string WebUrl { get; set; }
        bool IsSubExecution { get; set; }
        string QueryString { get; set; }

        IDev2WorkflowSettings Settings { get; set; }
        IStateNotifier StateNotifier { get; set; }
    }
}