using System;
using System.Configuration;
using System.IO;

namespace Dev2.Common
{
    public static class GlobalConstants
    {

        // WF Constants
        public const int _xamlPoolSize = 5;

        //Network
        public const int NetworkTimeOut = 10000; //Bug 8796
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
        public const string NullEntryNamespace = "NullEntryNamespace";
        public const string ManagementServicePayload = "Dev2System.ManagmentServicePayload";
        public const string ErrorPayload = "Dev2System.Error";
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
        public const string DataListRootTag = "DataList";
        public const string AllColumns = null;
        public const string NaughtyTextNode = "#text";
        public const char EvaluationToken = '[';
        
        public const string DefaultDataListCacheSizeLvl2MemoryPercentage = "85";
        //public const string DefaultDataListCacheSizeLvl2MemoryPollingInterval = "00:00:05";
        //public const string DefaultDataListCacheSizeLvl2MegaByteSize = "2048";

        // Read Memory limit from config file ;)
        //public const string DefaultDataListCacheSizeLvl2MegaByteSize = ConfigurationManager.AppSettings["DataListLvl2CacheCapacity"];
        //public const string DefaultDataListCacheSizeLvl2MemoryPollingInterval = ConfigurationManager.AppSettings["DataListLvl2CachePollInterval"];

        public const int DefaultDataListMaxCacheSizeLvl0 = 20000;
        public const int DefaultDataListMaxCacheSizeLvl1 = 20000; // 40k rows in the 0 tier cache ;)
        public const int DefaultDataListCreateCacheSizeLvl1 = 30000;
        public const int DefaultCachePageSizeLvl1 = 1100;
        public const int DefaultColumnSizeLvl1 = 10;
        public const int DefaultIntellisenseCacheLvl1 = 100;
        public const int DefaultNamespaceSizeLvl1 = 10;

        public const int DefaultObjectCacheSize = 100;

        public const string DataListIoColDirection = "ColumnIODirection";

        // Decision Wizard Constants
        public const string InjectedDecisionHandler = "Dev2DecisionHandler.Instance.ExecuteDecisionStack"; // Amdend in GetBaseUnlimitedFlowchartActivity()
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

        // Switch Wizard Constants
        public const string SwitchDropWizardLocation = "wwwroot/switch/drop";
        public const string SwitchDragWizardLocation = "wwwroot/switch/drag";
        public const string SwitchExpressionPropertyText = "Expression";
        public const string SwitchExpressionTextPropertyText = "ExpressionText";
        public const string InjectedSwitchDataFetch = "Dev2DecisionHandler.Instance.FetchSwitchData";

        public const string SwitchWizardErrorString = "Couldn't find the resource needed to configure the Switch.";
        public const string SwitchWizardErrorHeading = "Missing System Model Dev2Switch";


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
        public const string _BINARY = "Binary";
        public const string _FIXED_WIZARD = "FixedWizard";
        public const string _DECISION_STACK = "Dev2DecisionStack";

        //Resource directories
        public const string ServicesDirectory = "Services";
        public const string SourcesDirectory = "Sources";

        // Output TO for Activity Upsert
        public const string OutputTONonRSField = "Field";
        // Old Activities
        public const string NoLongerSupportedMsg = "This activity is no longer supported";
        // Namespace cleanup - Set to false to avoid namespace clean-up ;)
        public const bool runtimeNamespaceClean = true;

        public static string ApplicationPath
        {
            get
            {
                return Directory.GetCurrentDirectory();
            }
        }

        public static readonly Guid ServerWorkspaceID = Guid.Empty;

        public static string WorkspacePath
        {
            get
            {
                return Path.Combine(ApplicationPath, "Workspaces");
            }
        }

        public static string GetWorkspacePath(Guid workspaceID)
        {
            return workspaceID == ServerWorkspaceID
                   ? ApplicationPath
                   : Path.Combine(WorkspacePath, workspaceID.ToString());
        }

    }
}
