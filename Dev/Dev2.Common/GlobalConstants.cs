using System;
using System.Collections.Generic;
using System.Globalization;

namespace Dev2.Common
{

    /*
     * Moved the CustomIcons to Dev2.Studio / AppResources / Converters
     * Since that is where they where being used ;)
     */

    public static class GlobalConstants
    {
        //Default TimeoutValue
        public static readonly TimeSpan DefaultTimeoutValue = new TimeSpan(0, 0, 20, 0);

        // Max String Size
        public const double MAX_SIZE_FOR_STRING = 1 << 12; // = 4K

        // Max storage buffer size to avoid LOH ;)
        public const int MAX_BUFFER_SIZE = 35000;

        // Force Webserver Constants
        public const int ViewInBrowserForceDownloadSize = 51200; // 500 KB and a file must be downloaded

        //Runtime Configuration
        public const string Dev2RuntimeConfigurationAssemblyName = "Dev2.Runtime.Configuration.dll";

        //Dev2MessageBox DontShowAgainKeys
        public const string Dev2MessageBoxDesignSurfaceTabPasteDialog = "1";

        // WF Constants
        public const int _xamlPoolSize = 5;

        // Constant for unique batch size processing 
        public const int _uniqueBatchSize = 1000;

        //Network
        public const int NetworkTimeOut = 30000; //Bug 8796
        public const string NetworkCommunicationErrorTextFormat = "An error occurred while executing the '{0}' command";

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

        // Storage Cache Constants
        public const int DefaultColumnSizeLvl1 = 10;
        public const int DefaultStorageSegments = 8;
        public const int DefaultStorageSegmentSize = 64 * 1024 * 1024; // 64 MB default buffer size ;)
        public const int DefaultAliasCacheSize = 32 * 1024; // 32KB of alias cache ;)
        public const string DefaultStorageZipEntry = "Dev2Storage";

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
        public const string _XML_Inputs_Only = "XML only Inputs";

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

        // Server WorkspaceID
        public static readonly Guid ServerWorkspaceID = Guid.Empty;

        public static readonly string NullPluginValue = "NULL";

        // Resource Catalog Constants
        public static int ResourceCatalogCapacity = 150;
        public static int ResourceCatalogPruneAmt = 15;


        // GAC
        public static readonly string GACPrefix = "GAC:";

        // Used both Resource's LoadDependencies method
        public static readonly string EmptyDependcyListElement = "<XamlDefinition />";

        // Remote workflow custom header attribute ;)
        public static readonly string RemoteServerInvoke = "RemoteWarewolfServer";

        // Date Time
        public static readonly string LongTimePattern = CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern;
        public static readonly string ShortTimePattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
        public static readonly string Dev2DotNetDefaultDateTimeFormat = ShortTimePattern + " " + LongTimePattern;
        public static readonly string Dev2CustomDefaultDateTimeFormat = "d MM yyyy 24h:min.ss sp";

        // Query Network Computer Names
        public static readonly int NetworkComputerNameQueryFreq = 900000;

        public static TimeSpan TransactionTimeout = new TimeSpan(1, 0, 0, 0);

        public static string AnythingToXmlPathSeperator = ",";
        public static string AnytingToXmlCommaToken = "__COMMA__";

        // Resource Picker
        public static string ResourcePickerWorkflowString = "DsfWorkflowActivity";
        public static string ResourcePickerServiceString = "DsfServiceActivity";

        public static string SerializableResourceQuote = "__QUOTE__";
        public static string SerializableResourceSingleQuote = "__SQUOTE__";

        public static int MemoryItemCountCompactLevel = 500;

        //Calculate expressions
        public static string CalcExpressionNow = "!~calculation~!now()!~~calculation~!";

        public static string NotEqualsUnicodeChar = "≠";
        public static string GreaterThenOrEqualToUnicodeChar = "≥";
        public static string LessThenOrEqualToUnicodeChar = "≤";

        public static List<string> FindRecordsOperations = new List<string>
            {
                "=",
                ">",
                "<",
                "<> (Not Equal)",
                ">=",
                "<=",
                "Starts With",
                "Ends With",
                "Contains",
                "Doesn't Start With",                  
                "Doesn't End With",                
                "Doesn't Contain",
                "Is Alphanumeric",
                "Is Base64",
                "Is Between",
                "Is Binary",                 
                "Is Date",
                "Is Email",                
                "Is Hex",
                "Is Numeric",
                "Is Regex",                
                "Is Text",
                "Is XML",
                "Not Alphanumeric",
                "Not Base64",
                "Not Between",
                "Not Binary",
                "Not Date",
                "Not Email",
                "Not Hex",
                "Not Numeric",
                "Not Regex",
                "Not Text",
                "Not XML"                
        };

        public static string WebServerPort { get; set; }
        public static string WebServerSslPort { get; set; }

        public static string WebServiceTimeoutMessage = "Output mapping took too long. More then 10 seconds. Please use the JSONPath feature ( green icon above ) to reduce your dataset complexity. You can find out more on JSONPath at http://goessner.net/articles/JsonPath/";
    }
}