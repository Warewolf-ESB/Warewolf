using System;
using System.IO;
using System.Reflection;

namespace Dev2.Common
{
    public static class GlobalConstants
    {
        // Force Webserver Contants
        public const int ViewInBrowserForceDownloadSize = 51200; // 500 KB and a file must be downloaded

        //Runtime Configuration
        public const string Dev2RuntimeConfigurationAssemblyName = "Dev2.Runtime.Configuration.dll";

        //Dev2MessageBox DontShowAgainKeys
        public const string Dev2MessageBoxDesignSurfaceTabPasteDialog = "1";

        // WF Constants
        public const int _xamlPoolSize = 5;

        //Network
        public const int NetworkTimeOut = 30000; //Bug 8796
        public const string NetworkCommunicationErrorTextFormat = "An error occured while executing the '{0}' command";

        //Resource Constants
        public const string ResourceFileExtension = ".xml";

        //Windows Service constants
        public const string ServiceName = "Warewolf Server";

        // Cal constants
        public const string CalculateTextConvertPrefix = "!~calculation~!";
        public const string CalculateTextConvertSuffix = "!~~calculation~!";
        public const string CalculateTextConvertFormat = CalculateTextConvertPrefix + "{0}" + CalculateTextConvertSuffix;

        // Website constants
        public const string MetaTagsHolder = @"<Dev2HTML Type=""Meta""/>";
        public const string WebpageCellContainer = "Webpart";
        public const string WebpartRenderError = "<Fragement>Error executing webpart's service</Fragment>";
        public const string WebserverReplaceTag = "[[Dev2WebServer]]";

        // JSON constants
        public const string OpenJSON = "<JSON>";
        public const string CloseJSON = "</JSON>";

        // Service Action Constants
        public const string OutputDefOpenTag = "<Outputs>";
        public const string OutputDefCloseTag = "</Outputs>";

        // DataList constants
        public const string SystemTagNamespace = "Dev2System";
        public const string SystemTagNamespaceSearch = "Dev2System.";
        public const string EvalautionScalar = "Dev2System.Expression";
        public const string EvaluationRsField = "Expression";
        public const string NullEntryNamespace = "NullEntryNamespace";
        public const string ManagementServicePayload = "Dev2System.ManagmentServicePayload";
        public const string ErrorPayload = "Dev2System.Dev2Error";
        public const string ActivityRSResult = "Dev2ActivityResults";
        public const string ActivityRSField = "Dev2Result";
        public const string StarExpression = "*";
        public const string EqualsExpression = "=";
        public const int AllIndexes = -1;
        public static readonly Guid NullDataListID = Guid.Empty;
        public const string OkExeResult = "<Dev2Status>Ok</Dev2Status>";
        public const string PostDataStart = "<Dev2PostData>";
        public const string PostDataEnd = "</Dev2PostData>";
        public const string InnerErrorTag = "<InnerError>";
        public const string InnerErrorTagEnd = "</InnerError>";
        public const string EmptyNativeTypeTag = "<NativeType />";
        public const string EmptyValidatorTag = "<Validator />";
        public const string DataListRootTag = "DataList";
        public const string OutputRootTag = "Outputs";
        public const string InputRootTag = "Inputs";
        public const string ActionRootTag = "Action";
        public const string ActionsRootTag = "Actions";
        public const string AllColumns = null;
        public const string NaughtyTextNode = "#text";
        public const char EvaluationToken = '[';

        public const string DefaultDataListCacheSizeLvl2MemoryPercentage = "85";
        //public const string DefaultDataListCacheSizeLvl2MemoryPollingInterval = "00:00:05";
        //public const string DefaultDataListCacheSizeLvl2MegaByteSize = "2048";

        public const int DefaultDataListMaxCacheSizeLvl0 = 20000;
        public const int DefaultDataListMaxCacheSizeLvl1 = 20000; // 20k rows in the 0 tier cache ;)
        public const int DefaultDataListCreateCacheSizeLvl1 = 30000;
        public const int DefaultBlobCreateCacheSizeLvl1 = 100;
        public const int DefaultCachePageSizeLvl1 = 1100;
        public const int DefaultColumnSizeLvl1 = 10;
        public const int DefaultIntellisenseCacheLvl1 = 100;
        public const int DefaultNamespaceSizeLvl1 = 10;
        public const int DefaultConcurrentStorageAccsors = 4;

        public const string DataListIoColDirection = "ColumnIODirection";

        // Decision Wizard Constants
        public const string InjectedDecisionHandler = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack";
        public const string InjectedDecisionHandlerOld = "Dev2DecisionHandler.Instance.ExecuteDecisionStack";
        public const string InjectedDecisionDataListVariable = "AmbientDataList";
        public const string ExpressionPropertyText = "ExpressionText";
        public const string ConditionPropertyText = "Condition";
        public const string TrueArmPropertyText = "TrueLabel";
        public const string FalseArmPropertyText = "FalseLabel";
        public const string DefaultDataListInitalizationString = "DUMMY_DATA";
        public const string DecisionWizardLocation = "wwwroot/decisions/wizard";
        public const string DefaultTrueArmText = "True";
        public const string DefaultFalseArmText = "False";
        public const string VBSerializerToken = "__!__";

        public const string DisplayNamePropertyText = "DisplayName"; // PBI 9220 - 2013.04.29 - TWR

        // Switch Wizard Constants
        public const string SwitchDropWizardLocation = "wwwroot/switch/drop";
        public const string SwitchDragWizardLocation = "wwwroot/switch/drag";
        public const string SwitchExpressionPropertyText = "Expression";
        public const string SwitchExpressionTextPropertyText = "ExpressionText";
        public const string InjectedSwitchDataFetchOld = "Dev2DecisionHandler.Instance.FetchSwitchData";
        public const string InjectedSwitchDataFetch = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData";

        public const string SwitchWizardErrorString = "Couldn't find the resource needed to configure the Switch.";
        public const string SwitchWizardErrorHeading = "Missing System Model Dev2Switch";

        public const string WizardExt = "wiz";


        // Brendon Hack Constants
        public const string PostData = "postData";
        public const string DLID = "dlid";

        public const string DecisionWizardErrorString = "Couldn't find the resource needed to configure the Decision.";
        public const string DecisionWizardErrorHeading = "Missing System Model Dev2DecisionStack";

        //Input/Ouput variable suffix      
        public const string RecordsetJoinChar = "_";

        // Internal Fixed DataList types
        public const string _JSON = "JSON";
        public const string _XML = "XML";
        public const string _XML_DEBUG = "XML_DEBUG";
        public const string _XML_Without_SystemTags = "XMLWithoutSysTags";
        public const string _Studio_XML = "StudioXML";
        public const string _Studio_Debug_XML = "StudioDebugXML";
        public const string _BINARY = "Binary";
        public const string _FIXED_WIZARD = "FixedWizard";
        public const string _DECISION_STACK = "Dev2DecisionStack";
        public const string _DATATABLE = "DataTable";

        //Resource directories
        public const string ServicesDirectory = "Services";
        public const string SourcesDirectory = "Sources";

        // No start node error message
        public const string NoStartNodeError = "The workflow must have at least one service or activity connected to the Start Node.";

        // Output TO for Activity Upsert
        public const string OutputTONonRSField = "Field";
        // Old Activities
        public const string NoLongerSupportedMsg = "This activity is no longer supported";
        // Namespace cleanup - Set to false to avoid namespace clean-up ;)
        public const bool runtimeNamespaceClean = true;

        public static readonly Guid ServerWorkspaceID = Guid.Empty;

        // GAC
        public static readonly string GACPrefix = "GAC:";

        // Used both Resource's LoadDependencies method
        public static readonly string EmptyDependcyListElement = "<XamlDefinition />";

        // Remote workflow custom header attribute ;)
        public static readonly string RemoteServerInvoke = "RemoteWarewolfServer";
    }
}