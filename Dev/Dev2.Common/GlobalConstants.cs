/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.Win32;
using System;
using System.Activities.XamlIntegration;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using Warewolf.Resource.Errors;

namespace Dev2.Common
{
    /*
     * Moved the CustomIcons to Dev2.Studio / AppResources / Converters
     * Since that is where they where being used ;)
     */

    public static class GlobalConstants
    {
        static GlobalConstants()
        {
            SystemEvents.TimeChanged += (sender, args) =>
            {
                CultureInfo.CurrentCulture.ClearCachedData();
            };
            
            SystemEvents.UserPreferenceChanged += (sender, args) =>
            {
                CultureInfo.CurrentCulture.ClearCachedData();
            };
        }

        public static readonly string ExecutionLoggingResultStartTag = "Execution Result [ ";
        public static readonly string ExecutionLoggingResultEndTag = " ]";

        public static readonly string ArmResultText = "Flow Arm";
        
        public static readonly TimeSpan DefaultTimeoutValue = new TimeSpan(0, 0, 20, 0);
        public static readonly string LogFileDateFormat = "yyyy-MM-dd HH:mm:ss,fff";


        public static readonly string LogFileRegex = @"(\d+[-.\/]\d+[-.\/]\d+ \d+[:]\d+[:]\d+,\d+)\s+(\w+)\s+[-]\s+[[](\w+[-]\w+[-]\w+[-]\w+[-]\w+)[]]\s+[-]\s+";

        public static readonly string DefaultServerLogFileConfig = "<log4net>" +
                                             "<appender name=\"LogFileAppender\" type=\"Log4Net.Async.AsyncRollingFileAppender,Log4Net.Async\">" +
                                            "<file type=\"log4net.Util.PatternString\" value=\"%envFolderPath{CommonApplicationData}\\Warewolf\\Server Log\\wareWolf-Server.log\" />" +
    "<!-- Example using environment variables in params -->" +
    "<!-- <file value=\"${TMP}\\log-file.txt\" /> -->" +
    "<appendToFile value=\"true\" />" +
    "<rollingStyle value=\"Size\" />" +
    "<maxSizeRollBackups value=\"1\" />" +
    "<maximumFileSize value=\"200MB\" />" +
    "<!-- An alternate output encoding can be specified -->" +
    "<!-- <encoding value=\"unicodeFFFE\" /> -->" +
    "<layout type=\"log4net.Layout.PatternLayout\">" +
    "<header value=\"[Header]&#xD;&#xA;\" />" +
                                             "<footer value=\"[Footer]&#xD;&#xA;\" />" +
                                             "<conversionPattern value=\"%date %-5level - %message%newline\" />" +
                                             "</layout>" +
                                             "<!-- Alternate layout using XML            " +
                                             "<layout type=\"log4net.Layout.XMLLayout\" /> -->" +
                                             "</appender>" +
                                             "<appender name=\"EventLogLogger\" type=\"log4net.Appender.EventLogAppender\">" +
                                             "<threshold value=\"ERROR\" />" +
                                              "<mapping>" +
                                                "<level value=\"ERROR\" />" +
                                                "<eventLogEntryType value=\"Error\" />" +
                                              "</mapping>" +
                                              "<mapping>" +
                                                 "<level value=\"DEBUG\" />" +
                                                 "<eventLogEntryType value=\"Information\" />" +
                                               "</mapping>" +
                                                "<mapping>" +
                                                "<level value=\"INFO\" />" +
                                                "<eventLogEntryType value=\"Information\" />" +
                                              "</mapping>" +
                                              "<mapping>" +
                                                 "<level value=\"WARN\" />" +
                                                 "<eventLogEntryType value=\"Warning\" />" +
                                               "</mapping>" +
                                             "<logName value=\"Warewolf\"/>" +
                                             "<applicationName value=\"Warewolf Server\"/>" +
                                             "<layout type=\"log4net.Layout.PatternLayout\">" +
                                                "<conversionPattern value=\"%date %-5level - %message%newline\" />" +
                                              "</layout>" +
                                             "</appender>" +
                                             "<!-- Setup the root category, add the appenders and set the default level -->" +
                                             "<root>" +
                                             "<level value=\"DEBUG\" />" +
                                             "<appender-ref ref=\"LogFileAppender\" />" +
                                             "<appender-ref ref=\"EventLogLogger\"/>" +
                                             "</root>" +
                                             "</log4net>";

        public static readonly string DefaultStudioLogFileConfig = "<log4net>" +
                                             "<appender name=\"LogFileAppender\" type=\"Log4Net.Async.AsyncRollingFileAppender,Log4Net.Async\">" +
                                            "<file type=\"log4net.Util.PatternString\" value=\"${LOCALAPPDATA}\\Warewolf\\Studio Logs\\Warewolf Studio.log\" />" +
    "<!-- Example using environment variables in params -->" +
    "<!-- <file value=\"${TMP}\\log-file.txt\" /> -->" +
    "<appendToFile value=\"true\" />" +
    "<rollingStyle value=\"Size\" />" +
    "<maxSizeRollBackups value=\"1\" />" +
    "<maximumFileSize value=\"200MB\" />" +
    "<!-- An alternate output encoding can be specified -->" +
    "<!-- <encoding value=\"unicodeFFFE\" /> -->" +
    "<layout type=\"log4net.Layout.PatternLayout\">" +
    "<header value=\"[Header]&#xD;&#xA;\" />" +
                                             "<footer value=\"[Footer]&#xD;&#xA;\" />" +
                                             "<conversionPattern value=\"%date %-5level - %message%newline\" />" +
                                             "</layout>" +
                                             "<!-- Alternate layout using XML            " +
                                             "<layout type=\"log4net.Layout.XMLLayout\" /> -->" +
                                             "</appender>" +
                                             "<appender name=\"EventLogLogger\" type=\"log4net.Appender.EventLogAppender\">" +
                                             "<threshold value=\"ERROR\" />" +
                                             "<mapping>" +
                                                "<level value=\"ERROR\" />" +
                                                "<eventLogEntryType value=\"Error\" />" +
                                              "</mapping>" +
                                              "<mapping>" +
                                                 "<level value=\"DEBUG\" />" +
                                                 "<eventLogEntryType value=\"Information\" />" +
                                               "</mapping>" +
                                                "<mapping>" +
                                                "<level value=\"INFO\" />" +
                                                "<eventLogEntryType value=\"Information\" />" +
                                              "</mapping>" +
                                              "<mapping>" +
                                                 "<level value=\"WARN\" />" +
                                                 "<eventLogEntryType value=\"Warning\" />" +
                                               "</mapping>" +
                                             "<logName value=\"Warewolf\"/>" +
                                             "<applicationName value=\"Warewolf Studio\"/>" +
                                             "<layout type=\"log4net.Layout.PatternLayout\">" +
                                                "<conversionPattern value=\"%date %-5level - %message%newline\" />" +
                                              "</layout>" +
                                             "</appender>" +
                                             "<!-- Setup the root category, add the appenders and set the default level -->" +
                                             "<root>" +
                                             "<level value=\"DEBUG\" />" +
                                             "<appender-ref ref=\"LogFileAppender\" />" +
                                             "<appender-ref ref=\"EventLogLogger\"/>" +
                                             "</root>" +
                                             "</log4net>";
        
        public static readonly double MAX_SIZE_FOR_STRING = 1 << 12;
        
        public static readonly int MAX_BUFFER_SIZE = 35000;
        
        public static readonly double DesignHeightTolerance = 0.00000001;
        
        public static readonly int ViewInBrowserForceDownloadSize = 51200;
        
        public static readonly string Dev2RuntimeConfigurationAssemblyName = "Dev2.Runtime.Configuration.dll";
        
        public static readonly string Dev2MessageBoxDesignSurfaceTabPasteDialog = "1";

        public static readonly string Dev2MessageBoxNoInputsWhenHyperlinkClickedDialog = "2";
        
        public static readonly int _xamlPoolSize = 5;
        
        public static readonly int _uniqueBatchSize = 1000;
        
        public static readonly int NetworkTimeOut = 30000;

        public static readonly string NetworkCommunicationErrorTextFormat = "An error occurred while executing the '{0}' command";
        
        public static readonly string ResourceFileExtension = ".xml";

        public static readonly string XMLPrefix = "~XML~";
        
        public static readonly string CalculateTextConvertPrefix = "!~calculation~!";

        public static readonly string CalculateTextConvertSuffix = "!~~calculation~!";
        public static readonly string CalculateTextConvertFormat = CalculateTextConvertPrefix + "{0}" + CalculateTextConvertSuffix;

        public static readonly string AggregateCalculateTextConvertPrefix = "!~aggcalculation~!";
        public static readonly string AggregateCalculateTextConvertSuffix = "!~~aggcalculation~!";
        public static readonly string AggregateCalculateTextConvertFormat = AggregateCalculateTextConvertPrefix + "{0}" + AggregateCalculateTextConvertSuffix;
        
        public static readonly string WebserverReplaceTag = "[[Dev2WebServer]]";
        
        public static readonly string OpenJSON = "<JSON>";
        
        public static readonly string CloseJSON = "</JSON>";
        
        public static readonly string OutputDefOpenTag = "<Outputs>";

        public static readonly string OutputDefCloseTag = "</Outputs>";
        
        public static readonly string SystemTagNamespace = "Dev2System";

        public static readonly string SystemTagNamespaceSearch = "Dev2System.";
        public static readonly string EvalautionScalar = "Dev2System.Expression";
        public static readonly string EvaluationRsField = "Expression";
        public static readonly string NullEntryNamespace = "NullEntryNamespace";
        public static readonly string ManagementServicePayload = "Dev2System.ManagmentServicePayload";
        public static readonly string ErrorPayload = "Dev2System.Dev2Error";
        
        public static readonly string ActivityRSResult = "Dev2ActivityResults";
        
        public static readonly string ActivityRSField = "Dev2Result";
        
        public static readonly string StarExpression = "*";

        public static readonly string EqualsExpression = "=";
        public static readonly int AllIndexes = -1;
        public static readonly string OkExeResult = "<Dev2Status>Ok</Dev2Status>";
        public static readonly string PostDataStart = "<Dev2PostData>";
        public static readonly string PostDataEnd = "</Dev2PostData>";
        public static readonly string InnerErrorTag = "<InnerError>";
        public static readonly string InnerErrorTagEnd = "</InnerError>";
        public static readonly string EmptyNativeTypeTag = "<NativeType />";
        public static readonly string EmptyValidatorTag = "<Validator />";
        public static readonly string DataListRootTag = "DataList";
        public static readonly string OutputRootTag = "Outputs";
        public static readonly string InputRootTag = "Inputs";
        public static readonly string ActionRootTag = "Action";
        public static readonly string ActionsRootTag = "Actions";
        public static readonly string AllColumns;
        public static readonly string NaughtyTextNode = "#text";
        public static readonly char EvaluationToken = '[';
        public static readonly string RowAnnotation = "index";
        
        public static readonly string PrimitiveReturnValueTag = "PrimitiveReturnValue";
        
        public static readonly int DefaultColumnSizeLvl1 = 10;
        public static readonly int LogFileNumberOfLines = 15;

        public static readonly int DefaultStorageSegments = 1;
        public static readonly int DefaultStorageSegmentSize = 8 * 1024 * 1024; // 8 MB default buffer size ;)
        public static readonly int DefaultAliasCacheSize = 32 * 1024; // 32KB of alias cache ;)
        public static readonly string DefaultStorageZipEntry = "Dev2Storage";

        public static readonly string DataListIoColDirection = "ColumnIODirection";
        
        public static readonly string InjectedDecisionHandler =
            "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack";

        public static readonly string InjectedDecisionHandlerOld = "Dev2DecisionHandler.Instance.ExecuteDecisionStack";
        public static readonly string InjectedDecisionDataListVariable = "AmbientDataList";
        public static readonly string ExpressionPropertyText = "ExpressionText";
        public static readonly string ConditionPropertyText = "Condition";
        public static readonly string TrueArmPropertyText = "TrueLabel";
        public static readonly string FalseArmPropertyText = "FalseLabel";
        public static readonly string DefaultDataListInitalizationString = "DUMMY_DATA";
        public static readonly string DecisionWizardLocation = "wwwroot/decisions/wizard";
        public static readonly string DefaultTrueArmText = "True";
        public static readonly string DefaultFalseArmText = "False";


        public static readonly string VBSerializerToken = "__!__";
        
        public static readonly string DisplayNamePropertyText = "DisplayName"; // PBI 9220 - 2013.04.29 - TWR
        
        public static readonly string SwitchDropWizardLocation = "wwwroot/switch/drop";

        public static readonly string SwitchDragWizardLocation = "wwwroot/switch/drag";
        public static readonly string SwitchExpressionPropertyText = "Expression";
        public static readonly string SwitchExpressionTextPropertyText = "ExpressionText";
        public static readonly string InjectedSwitchDataFetchOld = "Dev2DecisionHandler.Instance.FetchSwitchData";

        public static readonly string InjectedSwitchDataFetch =
            "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData";

        public static readonly string SwitchWizardErrorString = "Couldn't find the resource needed to configure the Switch.";
        public static readonly string SwitchWizardErrorHeading = "Missing System Model Dev2Switch";

        public static readonly string WizardExt = "wiz";
        
        public static readonly string PostData = "postData";
        
        public static readonly string DLID = "dlid";
        
        public static readonly string DecisionWizardErrorString = "Couldn't find the resource needed to configure the Decision.";
        public static readonly string DecisionWizardErrorHeading = "Missing System Model Dev2DecisionStack";
        
        public static readonly string RecordsetJoinChar = "_";
        
        public static readonly string _JSON = "JSON";

        public static readonly string _XML = "XML";
        public static readonly string _XML_DEBUG = "XML_DEBUG";
        public static readonly string _XML_Without_SystemTags = "XMLWithoutSysTags";
        public static readonly string _Studio_XML = "StudioXML";
        public static readonly string _Studio_Debug_XML = "StudioDebugXML";
        public static readonly string _BINARY = "Binary";
        public static readonly string _FIXED_WIZARD = "FixedWizard";
        public static readonly string _DECISION_STACK = "Dev2DecisionStack";
        public static readonly string _DATATABLE = "DataTable";
        public static readonly string _XML_Inputs_Only = "XML only Inputs";
        
        public static readonly string ServicesDirectory = "Services";

        public static readonly string SourcesDirectory = "Sources";
        
        public static readonly string NoStartNodeError =
            "The workflow must have at least one service or activity connected to the Start Node.";
        
        public static readonly string OutputTONonRSField = "Field";
        
        public static readonly string NoLongerSupportedMsg = "This activity is no longer supported";
        
        public static readonly bool runtimeNamespaceClean = true;

        public static readonly string WarewolfGroup = "Warewolf Administrators";
        public static readonly string SchedulerFolderId = "Warewolf";
        public static readonly string SchedulerAgentPath = @"WarewolfAgent.exe";
        public static readonly string SchedulerDebugPath = @"Warewolf\DebugOutPut\";

        public static readonly string SchemaQuery = @"SELECT name AS ROUTINE_NAME
,SCHEMA_NAME(schema_id) AS SPECIFIC_SCHEMA
,type_desc as ROUTINE_TYPE
FROM sys.objects
WHERE type_desc LIKE '%FUNCTION%'
or type_desc LIKE '%Procedure%'";

        public static readonly string SchemaQueryMySql = @"SHOW PROCEDURE STATUS;";

        public static readonly string SchemaQueryPostgreSql = @"
select 
    pp.proname as Name,
    current_database() as Db,    
    pg_get_functiondef(pp.oid),
    pp.proretset,
    t.typname
from pg_proc pp
inner join pg_namespace pn on (pp.pronamespace = pn.oid)
inner join pg_language pl on (pp.prolang = pl.oid)
inner join pg_type t on (pp.prorettype = t.oid)
where pn.nspname = 'public';
";

        public static readonly string SchemaQueryOracle = @"SELECT OBJECT_NAME AS Name,OWNER AS Db,OBJECT_TYPE as ROUTINE_TYPE FROM ALL_OBJECTS WHERE OWNER = '{0}' AND OBJECT_TYPE IN('FUNCTION','PROCEDURE')";
        public static readonly string ExplorerItemModelFormat = "Dev2.Models.ExplorerItemModel";
        public static readonly string UpgradedExplorerItemModelFormat = "Warewolf.Studio.ViewModels.ExplorerItemViewModel";
        public static readonly string VersionDownloadPath = "Installers\\";
        public static readonly string VersionFolder = "VersionControl";
        public static readonly Guid NullDataListID = Guid.Empty;
        
        public static readonly Guid ServerWorkspaceID = Guid.Empty;

        public static readonly string NullPluginValue = "NULL";
        
        public static readonly int ResourceCatalogCapacity = 150;

        public static readonly int ResourceCatalogPruneAmt = 15;

        public static readonly String PublicUsername = @"\";
        
        public static readonly string GACPrefix = "GAC:";
        
        public static readonly string EmptyDependcyListElement = "<XamlDefinition />";
        
        public static readonly string RemoteServerInvoke = "RemoteWarewolfServer";
        
        public static readonly string RemoteDebugServerInvoke = "RemoteWarewolfServerDebug";
        
        public static readonly string LongTimePattern = CultureInfo.InvariantCulture.DateTimeFormat.LongTimePattern;
        public static readonly string ShortTimePattern = CultureInfo.InvariantCulture.DateTimeFormat.ShortDatePattern;

        public static readonly string ShortDateTimePattern = CultureInfo.InvariantCulture.DateTimeFormat.ShortDatePattern;
        public static readonly string Dev2DotNetDefaultDateTimeFormat = ShortDateTimePattern + " " + LongTimePattern;
        public static readonly string GlobalDefaultNowFormat = CultureInfo.InvariantCulture.DateTimeFormat.SortableDateTimePattern;
        
        public static readonly string Dev2DotNetDefaultDateTimeFormat = ShortTimePattern + " " + LongTimePattern;
        
        public static readonly int NetworkComputerNameQueryFreq = 900000;

        private static TimeSpan transactionTimeout = new TimeSpan(1, 0, 0, 0);

        public static readonly string AnythingToXmlPathSeperator = ",";
        public static readonly string AnytingToXmlCommaToken = "__COMMA__";
        
        public static readonly string ExecuteWebRequestString = "About to execute web request [ '{0}' ] for User [ '{1}' : '{2}' : '{3}' ] with DataObject Payload [ '{4}' ]";
        public static readonly string ExecutionForServiceString = "Execution for Service Name: '{0}' Resource Id: '{1}' Mode: '{2}'";

        public static readonly string WarewolfInfo = "Warewolf Info";
        public static readonly string WarewolfError = "Warewolf Error";
        public static readonly string WarewolfDebug = "Warewolf Debug";
        
        public static readonly string ResourcePickerWorkflowString = "DsfWorkflowActivity";

        public static readonly string SerializableResourceQuote = "__QUOTE__";
        public static readonly string SerializableResourceSingleQuote = "__SQUOTE__";

        public static readonly int MemoryItemCountCompactLevel = 500;
        
        public static readonly string CalcExpressionNow = "!~calculation~!now()!~~calculation~!";

        public static readonly string NotEqualsUnicodeChar = "?";
        public static readonly string GreaterThenOrEqualToUnicodeChar = "=";
        public static readonly string LessThenOrEqualToUnicodeChar = "=";

        public static readonly List<string> FindRecordsOperations = new List<string>
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
            "Is NULL",
            "Is Not NULL",
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
            "Not XML",
            "There is No Error",
            "There is An Error"
        };


        public static readonly int VersionCount = 20;


        public static readonly string WebServiceTimeoutMessage =
            "Output mapping took too long. More then 10 seconds. Please use the JSONPath feature ( green icon above ) to reduce your dataset complexity. You can find out more on JSONPath at http://goessner.net/articles/JsonPath/";
        
        public static readonly int MaxWorkflowsToExecute = 1010;

        public static readonly int MaxNumberOfWorkflowWaits = 10000;
        public static readonly int WorkflowWaitTime = 60;
        public static readonly string DropboxPathMalformdedException = "Dropbox path contains an invalid character";
        public static string WebServerPort { get; set; }
        public static string WebServerSslPort { get; set; }
        public static readonly ConcurrentDictionary<Guid, TextExpressionCompilerResults> Resultscache = new ConcurrentDictionary<Guid, TextExpressionCompilerResults>();

        public static void InvalidateCache(Guid resourceId)
        {
            if (Resultscache.ContainsKey(resourceId))
            {
                bool removed = Resultscache.TryRemove(resourceId, out TextExpressionCompilerResults val);
                if (!removed)
                {
                    Resultscache.TryRemove(resourceId, out val);
                }
            }
        }

        public static readonly int AddPopupTimeDelay = 2000;
        private static GenericPrincipal _user;
        public static readonly double RowHeight = 30;
        public static readonly double RowHeaderHeight = 30;

        public static IPrincipal GenericPrincipal
        {
            get
            {
                if (_user == null)
                {
                    var genericIdentity = new GenericIdentity("GenericPublicUser");
                    var user = new GenericPrincipal(genericIdentity, new string[0]);
                    _user = user;
                }
                return _user;
            }
        }

        public static string UserAgentString => "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";

        public static string TempLocation
        {
            get
            {
                var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                var warewolfFolder = Path.Combine(appDataFolder, "Warewolf");
                if (!Directory.Exists(warewolfFolder))
                {
                    Directory.CreateDirectory(warewolfFolder);
                }
                var tempPath = Path.Combine(appDataFolder, "Warewolf", "Temp");
                if (!Directory.Exists(tempPath))
                {
                    Directory.CreateDirectory(tempPath);
                }
                return tempPath;
            }
        }

        public static int MinCompressVersionMinor
        {
            get
            {
#pragma warning disable 162
                return 7;
#pragma warning restore 162
            }
        }

        public static int MinCompressVersionMajor
        {
            get
            {
#pragma warning disable 162
                return 0;
#pragma warning restore 162
            }
        }

        public static string ApplicationJsonHeader { get; } = "application/json";
        public static string ApplicationXmlHeader { get; } = "application/xml";
        public static string ApplicationTextHeader { get; } = "text/plain";
        public static string ContentType { get;}= "Content-Type";
        public static string SaveReasonForDeploy { get; } = "Deploy";
        public static TimeSpan TransactionTimeout { get => transactionTimeout; set => transactionTimeout = value; }

        public static readonly string DropboxPathNotFoundException = "Dropbox location cannot be found";
        public static readonly string DropboxPathNotFileException = "Please specify the path of a file in Dropbox";
        public static readonly string DropBoxSuccess = "Success";
        public static readonly string DropBoxFailure = "Failed";
        public static readonly string GlobalCounterName = "All";
        public static readonly string Warewolf = "Warewolf";
        public static readonly string WarewolfServices = "Warewolf Services";
        public static readonly string UserEchoURL = "http://community.warewolf.io/topics/249-https-connection-from-localhost-to-a-remote-server/";

        public static void HandleEmptyParameters(object paramaTer, string name)
        {
            try
            {
                var stringParam = (string)paramaTer;
                if (string.IsNullOrEmpty(stringParam))
                {
                    throw new ArgumentNullException(name, string.Format(ErrorResource.NoValueProvided, name));
                }
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (Exception)
            {
                if (paramaTer == null)
                {
                    throw new ArgumentNullException(name, string.Format(ErrorResource.NoValueProvided, name));
                }
            }
        }
    }
}
