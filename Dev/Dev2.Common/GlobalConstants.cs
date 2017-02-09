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
                // ReSharper disable once ConvertToLambdaExpression
                CultureInfo.CurrentCulture.ClearCachedData();
            };

            /**********************************************************
             * Hear ye Hear ye
             * The event below allows warewolf to react to changes in regional settings by clearing the Culture cache.
             * The method below is not tested using integration tests because it is difficult to change regional settings without restarting explorer.exe
             * If a good way is found to do this, then please add integration tests.
             * don't delete this method
             */
            SystemEvents.UserPreferenceChanged += (sender, args) =>
            {
                // ReSharper disable once ConvertToLambdaExpression
                CultureInfo.CurrentCulture.ClearCachedData();
            };
        }

        public const string ArmResultText = "Flow Arm";
        // ReSharper disable InconsistentNaming
        //Default TimeoutValue
        // ReSharper disable UnusedMember.Global
        public static readonly TimeSpan DefaultTimeoutValue = new TimeSpan(0, 0, 20, 0);

        // ReSharper restore UnusedMember.Global

        public static string DefaultServerLogFileConfig = "<log4net>" +
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
                                             "<conversionPattern value=\"%date [%thread] %-5level - %message%newline\" />" +
                                             "</layout>" +
                                             "<!-- Alternate layout using XML			" +
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
                                                "<conversionPattern value=\"%date [%thread] %-5level - %message%newline\" />" +
                                              "</layout>" +
                                             "</appender>" +
                                             "<!-- Setup the root category, add the appenders and set the default level -->" +
                                             "<root>" +
                                             "<level value=\"DEBUG\" />" +
                                             "<appender-ref ref=\"LogFileAppender\" />" +
                                             "<appender-ref ref=\"EventLogLogger\"/>" +
                                             "</root>" +
                                             "</log4net>";

        public static string DefaultStudioLogFileConfig = "<log4net>" +
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
                                             "<conversionPattern value=\"%date [%thread] %-5level - %message%newline\" />" +
                                             "</layout>" +
                                             "<!-- Alternate layout using XML			" +
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
                                                "<conversionPattern value=\"%date [%thread] %-5level - %message%newline\" />" +
                                              "</layout>" +
                                             "</appender>" +
                                             "<!-- Setup the root category, add the appenders and set the default level -->" +
                                             "<root>" +
                                             "<level value=\"DEBUG\" />" +
                                             "<appender-ref ref=\"LogFileAppender\" />" +
                                             "<appender-ref ref=\"EventLogLogger\"/>" +
                                             "</root>" +
                                             "</log4net>";

        // Max String Size
        // ReSharper disable InconsistentNaming
        public const double MAX_SIZE_FOR_STRING = 1 << 12; // = 4K

        // ReSharper restore InconsistentNaming

        // Max storage buffer size to avoid LOH ;)
        // ReSharper disable InconsistentNaming
        public const int MAX_BUFFER_SIZE = 35000;

        // ReSharper restore InconsistentNaming

        public const double DesignHeightTolerance = 0.00000001;

        // Force Webserver Constants
        // ReSharper disable UnusedMember.Global
        public const int ViewInBrowserForceDownloadSize = 51200; // 500 KB and a file must be downloaded

        //Runtime Configuration
        public const string Dev2RuntimeConfigurationAssemblyName = "Dev2.Runtime.Configuration.dll";

        //Dev2MessageBox DontShowAgainKeys
        public const string Dev2MessageBoxDesignSurfaceTabPasteDialog = "1";

        public const string Dev2MessageBoxNoInputsWhenHyperlinkClickedDialog = "2";

        // WF Constants
        // ReSharper disable InconsistentNaming
        public const int _xamlPoolSize = 5;

        // ReSharper restore InconsistentNaming

        // Constant for unique batch size processing
        // ReSharper disable InconsistentNaming
        public const int _uniqueBatchSize = 1000;

        // ReSharper restore InconsistentNaming

        //Network
        public const int NetworkTimeOut = 30000; //Bug 8796

        public const string NetworkCommunicationErrorTextFormat = "An error occurred while executing the '{0}' command";

        //Resource Constants
        public const string ResourceFileExtension = ".xml";

        public const string XMLPrefix = "~XML~";

        // Cal constants
        public const string CalculateTextConvertPrefix = "!~calculation~!";

        public const string CalculateTextConvertSuffix = "!~~calculation~!";
        public const string CalculateTextConvertFormat = CalculateTextConvertPrefix + "{0}" + CalculateTextConvertSuffix;

        public const string AggregateCalculateTextConvertPrefix = "!~aggcalculation~!";
        public const string AggregateCalculateTextConvertSuffix = "!~~aggcalculation~!";
        public const string AggregateCalculateTextConvertFormat = AggregateCalculateTextConvertPrefix + "{0}" + AggregateCalculateTextConvertSuffix;

        // Website constants
        public const string WebserverReplaceTag = "[[Dev2WebServer]]";

        // JSON constants
        // ReSharper disable InconsistentNaming
        public const string OpenJSON = "<JSON>";

        // ReSharper restore InconsistentNaming
        // ReSharper disable InconsistentNaming
        public const string CloseJSON = "</JSON>";

        // ReSharper restore InconsistentNaming

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

        // ReSharper disable InconsistentNaming
        public const string ActivityRSResult = "Dev2ActivityResults";

        // ReSharper restore InconsistentNaming
        // ReSharper disable InconsistentNaming
        public const string ActivityRSField = "Dev2Result";

        // ReSharper restore InconsistentNaming
        public const string StarExpression = "*";

        public const string EqualsExpression = "=";
        public const int AllIndexes = -1;
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
        public const string RowAnnotation = "index";

        // Plugin Constants For Shape
        public const string PrimitiveReturnValueTag = "PrimitiveReturnValue";

        // Storage Cache Constants
        public const int DefaultColumnSizeLvl1 = 10;
        public const int LogFileNumberOfLines = 15;

        public const int DefaultStorageSegments = 1;
        public const int DefaultStorageSegmentSize = 8 * 1024 * 1024; // 8 MB default buffer size ;)
        public const int DefaultAliasCacheSize = 32 * 1024; // 32KB of alias cache ;)
        public const string DefaultStorageZipEntry = "Dev2Storage";

        public const string DataListIoColDirection = "ColumnIODirection";

        // Decision Wizard Constants
        public const string InjectedDecisionHandler =
            "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack";

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

        // ReSharper disable InconsistentNaming
        public const string VBSerializerToken = "__!__";

        // ReSharper restore InconsistentNaming

        public const string DisplayNamePropertyText = "DisplayName"; // PBI 9220 - 2013.04.29 - TWR

        // Switch Wizard Constants
        public const string SwitchDropWizardLocation = "wwwroot/switch/drop";

        public const string SwitchDragWizardLocation = "wwwroot/switch/drag";
        public const string SwitchExpressionPropertyText = "Expression";
        public const string SwitchExpressionTextPropertyText = "ExpressionText";
        public const string InjectedSwitchDataFetchOld = "Dev2DecisionHandler.Instance.FetchSwitchData";

        public const string InjectedSwitchDataFetch =
            "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData";

        public const string SwitchWizardErrorString = "Couldn't find the resource needed to configure the Switch.";
        public const string SwitchWizardErrorHeading = "Missing System Model Dev2Switch";

        public const string WizardExt = "wiz";

        // Brendon Hack Constants
        public const string PostData = "postData";

        // ReSharper disable InconsistentNaming
        public const string DLID = "dlid";

        // ReSharper restore InconsistentNaming

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
        public const string NoStartNodeError =
            "The workflow must have at least one service or activity connected to the Start Node.";

        // Output TO for Activity Upsert
        public const string OutputTONonRSField = "Field";

        // Old Activities
        public const string NoLongerSupportedMsg = "This activity is no longer supported";

        // Namespace cleanup - Set to false to avoid namespace clean-up ;)
        public const bool runtimeNamespaceClean = true;

        public const string WarewolfGroup = "Warewolf Administrators";
        public const string SchedulerFolderId = "Warewolf";
        public const string SchedulerAgentPath = @"WarewolfAgent.exe";
        public const string SchedulerDebugPath = @"Warewolf\DebugOutPut\";

        public const string SchemaQuery = @"SELECT name AS ROUTINE_NAME
,SCHEMA_NAME(schema_id) AS SPECIFIC_SCHEMA
,type_desc as ROUTINE_TYPE
FROM sys.objects
WHERE type_desc LIKE '%FUNCTION%'
or type_desc LIKE '%Procedure%'";

        public const string SchemaQueryMySql = @"SHOW PROCEDURE STATUS;";

        public const string SchemaQueryPostgreSql = @"
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

        public const string SchemaQueryOracle = @"SELECT OBJECT_NAME AS Name,OWNER AS Db,OBJECT_TYPE as ROUTINE_TYPE FROM ALL_OBJECTS WHERE OWNER = '{0}' AND OBJECT_TYPE IN('FUNCTION','PROCEDURE')";
        public const string ExplorerItemModelFormat = "Dev2.Models.ExplorerItemModel";
        public const string UpgradedExplorerItemModelFormat = "Warewolf.Studio.ViewModels.ExplorerItemViewModel";
        public const string VersionDownloadPath = "Installers\\";
        public const string VersionFolder = "VersionControl";
        public static readonly Guid NullDataListID = Guid.Empty;

        // Server WorkspaceID
        public static readonly Guid ServerWorkspaceID = Guid.Empty;

        public static readonly string NullPluginValue = "NULL";

        // Resource Catalog Constants
        public static int ResourceCatalogCapacity = 150;

        public static int ResourceCatalogPruneAmt = 15;

        // Security
        //public const string BuiltInAdministrator = "BuiltIn\\Administrators";
        // ReSharper disable ConvertToConstant.Global
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        public static String PublicUsername = @"\";

        // GAC
        public static readonly string GACPrefix = "GAC:";

        // Used both Resource's LoadDependencies method
        public static readonly string EmptyDependcyListElement = "<XamlDefinition />";

        // Remote workflow custom header attribute ;)
        public static readonly string RemoteServerInvoke = "RemoteWarewolfServer";

        // Remote workflow custom header attribute ;)
        public static readonly string RemoteDebugServerInvoke = "RemoteWarewolfServerDebug";

        // Date Time
        // ReSharper disable MemberCanBePrivate.Global
        public static readonly string LongTimePattern = CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern;

        public static readonly string ShortTimePattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
        public static readonly string Dev2DotNetDefaultDateTimeFormat = ShortTimePattern + " " + LongTimePattern;
        public static readonly string Dev2CustomDefaultDateTimeFormat = "d MM yyyy 24h:min.ss sp";
        public const string GlobalDefaultNowFormat = "yyyy/MM/dd hh:mm:ss.fff tt";

        // Query Network Computer Names
        public static readonly int NetworkComputerNameQueryFreq = 900000;

        public static TimeSpan TransactionTimeout = new TimeSpan(1, 0, 0, 0);

        public static string AnythingToXmlPathSeperator = ",";
        public static string AnytingToXmlCommaToken = "__COMMA__";

        // Resource Picker
        public static string ResourcePickerWorkflowString = "DsfWorkflowActivity";

        public static string SerializableResourceQuote = "__QUOTE__";
        public static string SerializableResourceSingleQuote = "__SQUOTE__";

        public static int MemoryItemCountCompactLevel = 500;

        //Calculate expressions
        public static string CalcExpressionNow = "!~calculation~!now()!~~calculation~!";

        public static string NotEqualsUnicodeChar = "?";
        public static string GreaterThenOrEqualToUnicodeChar = "=";
        public static string LessThenOrEqualToUnicodeChar = "=";

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

        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public static int VersionCount = 20;

        // ReSharper restore UnusedAutoPropertyAccessor.Global
        public static string WebServiceTimeoutMessage =
            "Output mapping took too long. More then 10 seconds. Please use the JSONPath feature ( green icon above ) to reduce your dataset complexity. You can find out more on JSONPath at http://goessner.net/articles/JsonPath/";

        // Limit WF execution
        public static int MaxWorkflowsToExecute = 1010;

        public static int MaxNumberOfWorkflowWaits = 10000;
        public static int WorkflowWaitTime = 60;
        public static string DropboxPathMalformdedException = "Dropbox path contains an invalid character";
        public static string WebServerPort { get; set; }
        public static string WebServerSslPort { get; set; }
        public static ConcurrentDictionary<Guid, TextExpressionCompilerResults> Resultscache = new ConcurrentDictionary<Guid, TextExpressionCompilerResults>();

        public static void InvalidateCache(Guid resourceId)
        {
            if (Resultscache.ContainsKey(resourceId))
            {
                TextExpressionCompilerResults val;
                bool removed = Resultscache.TryRemove(resourceId, out val);
                if (!removed)
                {
                    Resultscache.TryRemove(resourceId, out val);
                }
            }
        }

        public static int AddPopupTimeDelay = 2000;
        private static GenericPrincipal _user;
        public static double RowHeight = 30;
        public static double RowHeaderHeight = 30;

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
                //#if DEBUG
                //                return Assembly.GetExecutingAssembly().GetName().Version.Minor-1;
                //#endif
                // ReSharper disable once HeuristicUnreachableCode
#pragma warning disable 162
                return 7;
#pragma warning restore 162
            }
        }

        public static int MinCompressVersionMajor
        {
            get
            {
                //#if DEBUG
                //                return Assembly.GetExecutingAssembly().GetName().Version.Major-1;
                //#endif
                // ReSharper disable once HeuristicUnreachableCode
#pragma warning disable 162
                return 0;
#pragma warning restore 162
            }
        }

        public static string DropboxPathNotFoundException = "Dropbox location cannot be found";
        public static string DropboxPathNotFileException = "Please specify the path of a file in Dropbox";
        public static string DropBoxSuccess = "Success";
        public static string DropBoxFailure = "Failed";
        public static string GlobalCounterName = "All";
        public static string Warewolf = "Warewolf";
        public static string WarewolfServices = "Warewolf Services";
        public static string UserEchoURL = "http://community.warewolf.io/topics/249-https-connection-from-localhost-to-a-remote-server/";
        // ReSharper restore InconsistentNaming
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

        


        // ReSharper restore UnusedMember.Global
        // ReSharper restore ConvertToConstant.Global
        // ReSharper restore FieldCanBeMadeReadOnly.Global
        // ReSharper restore MemberCanBePrivate.Global
    }
}
