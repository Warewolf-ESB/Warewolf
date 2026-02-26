using Newtonsoft.Json;
using System.Activities;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dev2.Common.X6
{
    public class Constants
    {
        public const string TYPE = "type";
        public const string DISPLAYNAME = "displayname";
        public const string ID = "id";
        public const string ACTIVITY = "activity";

        public const string PROPERTIES = "properties";
        public const string START = "start";
        public const string RECT = "rect";
        public const string CONDITION = "condition";
        public const string FLOWDECISION = "flowdecision";
        public const string DECISION = "decision";
        public const string POLYGON = "polygon";

        public const string EXPRESSION = "expression";
        public const string SWITCH = "switch";
        public const string SEQUENCE = "sequence";
        public const string FLOWSWITCH = "flowswitch";
        public const string SWITCH_EXPRESSION = "switchExpression";
        public const string SWITCH_DEFAULT = "Default";
        public const string SWITCH_VARIABLE = "variable";

        public const string TRUE = "true";
        public const string FALSE = "false";

        public const string DISPLAYTEXT = "displaytext";
        public const string TRUEARMTEXT = "truearmtext";
        public const string FALSEARMTEXT = "falsearmtext";
        public const string AND = "and";

        public const string ISDECISIONARM = "isDecisionArm";
        public const string ISTRUEARM = "isTrue";

        public const string ONERRORDATA = "onerrordata";
        public const string FIELDS = "fields";
        public const string UPDATEDFIELDS = "updatedfields";
        public const string UNIQUEID = "UniqueID";

        public const string PROPERTY_ONERRORVARIABLE = "OnErrorVariable";
        public const string PROPERTY_DISPLAYNAME = "DisplayName";
        public const string PROPERTY_EXPRESSIONTEXT = "ExpressionText";
        public const string PROPERTY_ONERRORWORKFLOW = "OnErrorWorkflow";
        public const string PROPERTY_ISENDEDONERROR = "IsEndedOnError";
        public const string PROPERTY_UNIQUEID = "UniqueID";

        public const string DATABASE_PROCEDURENAME = "procedurename";
        public const string DATABASE_EXECUTEACTIONSTRING = "executeactionstring";
        public const string DATABASE_SERVICESERVER = "serviceserver";
        public const string DATABASE_COMMANDTIMEOUT = "commandtimeout";

        public const string DSFSEQUENCE = "dsfsequenceactivity";

        public const string DSFDOTNETMULTIASSIGNACTIVITY = "DsfDotNetMultiAssignActivity";
        public const string DSFDOTNETMULTIASSIGNAOBJECTCTIVITY = "DsfDotNetMultiAssignObjectActivity";
        public const string DSFDECISION = "DsfDecision";
        public const string DSFFOREACHACTIVITY = "DsfForEachActivity";
        public const string DSFSELECTANDAPPLYACTIVITY = "DsfSelectAndApplyActivity";
        public const string DSFDATAMERGEACTIVITY = "DsfDataMergeActivity";
        public const string DSFDATASPLITACTIVITY = "DsfDataSplitActivity";
        public const string DSFBASECONVERTACTIVITY = "DsfBaseConvertActivity";
        public const string DSFREPLACEACTIVITY = "DsfReplaceActivity";
        public const string DSFCASECONVERTACTIVITY = "DsfCaseConvertActivity";
        public const string DSFINDEXACTIVITY = "DsfIndexActivity";
        public const string DSFFILEREAD = "DsfFileRead";
        public const string FILEREADWITHBASE64 = "FileReadWithBase64";
        public const string DSFFINDRECORDSMULTIPLECRITERIAACTIVITY = "DsfFindRecordsMultipleCriteriaActivity";
        public const string DSFDELETERECORDNULLHANDLERACTIVITY = "DsfDeleteRecordNullHandlerActivity";
        public const string DSFDELETERECORDACTIVITY = "DsfDeleteRecordActivity";
        public const string DSFSORTRECORDSACTIVITY = "DsfSortRecordsActivity";
        public const string DSFCOUNTRECORDSETNULLHANDLERACTIVITY = "DsfCountRecordsetNullHandlerActivity";
        public const string DSFRECORDSETNULLHANDLERLENGTHACTIVITY = "DsfRecordsetNullhandlerLengthActivity";
        public const string DSFUNIQUERECORDSACTIVITY = "DsfUniqueActivity";
        public const string DSFFOLDERREADACTIVITY = "DsfFolderReadActivity";
        public const string DSFFOLDERREAD = "DsfFolderRead";
        public const string DSFPATHCREATE = "DsfPathCreate";
        public const string DSFPATHCOPY = "DsfPathCopy";
        public const string DSFPATHMOVE = "DsfPathMove";
        public const string DSFPATHRENAME = "DsfPathRename";
        public const string DSFPATHDELETE = "DsfPathDelete";
        public const string FILEWRITEWITHBASE64 = "FileWriteWithBase64";
        public const string DSFEXECUTECOMMANDLINEACTIVITY = "DsfExecuteCommandLineActivity";       
        public const string DSFPYTHONACTIVITY = "DsfPythonActivity";
        public const string GATEACTIVITY = "GateActivity";
        public const string DSFDATETIMEDIFFERENCEACTIVITY = "DsfDateTimeDifferenceActivity";
        public const string DSFDOTNETDATETIMEDIFFERENCEACTIVITY = "DsfDotNetDateTimeDifferenceActivity";

        public const string ISNESTED = "isNested";
        public const string PARENTID = "parentId";
        public const string SEQUENCE_NESTED_ACTIVITY_INDEX = "index";

        public const string ISNESTED_INFOREACH = "isNestedInForEach";
        public const string PARENTID_FOREACH = "forEachParentId";

        public const string DISPLAYNAME_SEQUENCE = "Sequence";
        public const string DISPLAYNAME_FOREACH = "For Each";
        public const string DISPLAYNAME_SELECTANDAPPLY = "Select and apply";
        public const string DISPLAYNAME_DATAMERGE = "Data Merge";
        public const string DISPLAYNAME_DATASPLIT = "Data Split";
        public const string DISPLAYNAME_FINDRECORDS = "Find Records";
        public const string DISPLAYNAME_DELETERECORDS = "Delete Records";
        public const string DISPLAYNAME_BASECONVERT = "Base Conversion";
        public const string DISPLAYNAME_CASECONVERT = "Case Conversion";
        public const string DISPLAYNAME_FINDINDEX = "Find Index";
        public const string DISPLAYNAME_REPLACE = "Replace";
        public const string DISPLAYNAME_COUNTRECORDS = "Count Records";
        public const string DISPLAYNAME_LENGTH = "Length";
        public const string DISPLAYNAME_WEBPOST = "Post Web Method";
        public const string DISPLAYNAME_WEBDELETE = "DELETE Web Method";
        public const string DISPLAYNAME_FILEREAD = "Read File";
        public const string DISPLAYNAME_FOLDERREAD = "Folder Read";
        public const string DISPLAYNAME_PATHCREATE = "Create";
        public const string DISPLAYNAME_PATHCOPY = "Copy";
        public const string DISPLAYNAME_PATHMOVE = "Move";
        public const string DISPLAYNAME_PATHRENAME = "Rename";
        public const string DISPLAYNAME_PATHDELETE = "Delete";
        public const string DISPLAYNAME_FILEWRITE = "Write File";
        public const string DISPLAYNAME_COMMANDLINE = "Execute Command Line";        
        public const string DISPLAYNAME_PYTHON = "Python";
        public const string DISPLAYNAME_GATE = "Gate";
        public const string DISPLAYNAME_DATETIMEDIFFERENCE = "Date and Time Difference";

        public const string SELECTANDAPPLY_ALIAS = "alias";
        public const string SELECTANDAPPLY_DATASOURCE = "dataSource";
        public const string SELECTANDAPPLY_APPLYACTIVITYFUNC = "applyActivityFunc";

        public const string NGARGUMENTS = "ngArguments";
        public const string DATAACTION = "Data Action";

        public const string MERGECOLLECTION = "mergecollection";
        public const string UPDATEDMERGECOLLECTION = "updatedmergecollection";
        public const string RESULT = "result";

        public const string CONVERTCOLLECTION = "convertcollection";
        public const string UPDATEDCONVERTCOLLECTION = "updatedconvertcollection";
        public const string RESULTSCOLLECTION = "resultscollection";

        public const string DATASPLIT_SOURCESTRING = "sourcestring";
        public const string DATASPLIT_REVERSEORDER = "reverseorder";
        public const string DATASPLIT_SKIPBLANKROWS = "skipblankrows";

        public const string REPLACE_FIELDS_TO_SEARCH = "FieldsToSearch";
        public const string REPLACE_FIND = "Find";
        public const string REPLACE_REPLACE_WTIH = "ReplaceWith";
        public const string REPLACE_CASE_MATCH = "CaseMatch";
        public const string REPLACE_RESULT = "Result";

        public const string WEBGETACTIVITY = "WebGetActivity";
        public const string DISPLAYNAME_WEBGET = "GET Web Method";
        public const string WEBMETHOD_HEADERS = "headers";
        public const string WEBMETHOD_UPDATEDHEADERS = "updatedheaders";
        public const string WEBMETHOD_QUERYSTRING = "querystring";
        public const string WEBMETHOD_ISRESPONSEBASE64 = "isresponsebase64";
        public const string WEBMETHOD_SOURCEID = "sourceId";

        public const string WEBMETHOD_OUTPUTDESCRIPTION = "outputdescription";
        public const string WEBMETHOD_INPUTS = "inputs";
        public const string WEBMETHOD_OUTPUTS = "outputs";
        public const string WEBMETHOD_ISOBJECT = "isOutputToObject";
        public const string WEBMETHOD_OBJECTNAME = "objectname";
        public const string WEBMETHOD_OBJECTRESULT = "objectresult";

        public const string WEBPOSTACTIVITY = "WebPostActivityNew";
        public const string WEBMETHOD_SETTINGS = "settings";
        public const string WEBMETHOD_CONDITIONS = "conditions";
        public const string WEBMETHOD_TIMEOUT = "timeout";
        public const string WEBMETHOD_POSTDATA = "postdata";
        public const string WEBPUTACTIVITY = "WebPutActivity";
        public const string DISPLAYNAME_WEBPUT = "PUT Web Method";
        public const string WEBMETHOD_ISPUTDATABASE64 = "isputdatabase64";
        public const string WEBDELETEACTIVITY = "DsfWebDeleteActivity";

        public const string SQLSERVERDATABASEACTIVITY = "DsfSqlServerDatabaseActivity";
        public const string DISPLAYNAME_SQLSERVERDATABASE = "SQL Server Database";

        public const string POSTGRESQLDATABASEACTIVITY = "DsfPostgreSqlActivity";
        public const string DISPLAYNAME_POSTGRESQLDATABASE = "Postgre SQL Database";

        public const string MYSQLDATABASEACTIVITY = "DsfMySqlDatabaseActivity";
        public const string DISPLAYNAME_MYSQLDATABASE = "MySQL Database";

        public const string SQLBULKINSERTACTIVITY = "DsfSqlBulkInsertActivity";
        public const string DISPLAYNAME_SQLBULKINSERT = "SQL Bulk Insert";

        public const string SQLBULKINSERT_TABLENAME = "tablename";
        public const string SQLBULKINSERT_BATCHSIZE = "batchsize";
        public const string SQLBULKINSERT_TIMEOUT = "timeout";
        public const string SQLBULKINSERT_CHECKCONSTRAINTS = "checkconstraints";
        public const string SQLBULKINSERT_FIRETRIGGERS = "firetriggers";
        public const string SQLBULKINSERT_USEINTERNALTRANSACTION = "useinternaltransaction";
        public const string SQLBULKINSERT_KEEPIDENTITY = "keepidentity";
        public const string SQLBULKINSERT_KEEPTABLELOCK = "keeptablelock";
        public const string SQLBULKINSERT_IGNOREBLANKROWS = "ignoreblankrows";
        public const string SQLBULKINSERT_INPUTMAPPINGS = "inputmappings";
        public const string SQLBULKINSERT_DATABASE = "database";

        public const string ORACLESQLDATABASEACTIVITY = "DsfOracleDatabaseActivity";
        public const string DISPLAYNAME_ORACLESQLDATABASE = "Oracle Database";


        public const string REDISCACHEACTIVITY = "RedisCacheActivity";
        public const string DISPLAYNAME_REDISCACHE = "Redis Cache";
        public const string REDISCACHE_ACTIVITYFUNC = "ActivityFunc";

        public const string REDISCACHE_KEY = "key";
        public const string REDISCACHE_TTL = "ttl";
        public const string REDISCACHE_RESPONSE = "response";
        public const string REDISCACHE_SOURCEID = "sourceid";

        public const string REDISREMOVEACTIVITY = "RedisRemoveActivity";
        public const string DISPLAYNAME_REDISREMOVE = "Redis Remove";

        public const string REDIS_KEY = "key";
        public const string REDIS_RESPONSE = "response";
        public const string REDIS_SOURCEID = "sourceid";

        // Add these constants for Find Records specific properties
        public const string FINDRECORDS_FIELDSTOSEARCH = "fieldsToSearch";
        public const string FINDRECORDS_STARTINDEX = "startIndex";
        public const string FINDRECORDS_MATCHCASE = "matchCase";
        public const string FINDRECORDS_REQUIREALLTRUE = "requireAllTrue";
        public const string FINDRECORDS_REQUIREALLFIELDSTOMATCH = "requireAllFieldsToMatch";
        public const string FINDRECORDS_RESULTSCOLLECTION = "resultsCollection";

        public const string ADVANCEDRECORDSETACTIVITY = "AdvancedRecordsetActivity";
        public const string DISPLAYNAME_ADVANCEDRECORDSET = "Advanced Recordset";
        public const string ADVANCEDRECORDSET_SQLQUERY = "sqlquery";
        public const string ADVANCEDRECORDSET_RECORDSETNAME = "recordsetname";
        public const string ADVANCEDRECORDSET_DECLAREVARIABLES = "declarevariables";

        public const string DELETERECORDS_TREATNULLASZERO = "treatNullAsZero";
        public const string DELETERECORDS_RECORDSETNAME = "recordsetname";

        public const string DISPLAYNAME_SORTACTIVITY = "Sort Records";
        public const string SORTACTIVITY_FIELD = "sortfield";
        public const string SORTACTIVITY_SELECTEDSORT = "selectedsort";

        public const string FOLDERREAD_INPUTPATH = "inputpath";
        public const string FOLDERREAD_ISFILESSELECTED = "isfilesselected";
        public const string FOLDERREAD_ISFOLDERSSELECTED = "isfoldersselected";
        public const string FOLDERREAD_ISFILESANDFOLDERSSELECTED = "isfilesandfoldersselected";
        public const string FILE_FOLDER_PRIVATEKEYFILE = "privatekeyfile";
        public const string FILE_FOLDER_USERNAME = "username";
        public const string FILE_FOLDER_PASSWORD = "password";

        public const string FILEREAD_INPUTPATH = "inputpath";
        public const string FILEREAD_ISRESULTBASE64 = "isresultbase64";
        
        public const string COUNTRECORDS_RECORDSETNAME = "recordsetname";
        public const string COUNTRECORDS_COUNTNUMBER = "countnumber";
        public const string COUNTRECORDS_TREATNULLASZERO = "treatnullaszero";

        public const string DISPLAYNAME_UNIQUEACTIVITY = "Unique Records";
        public const string UNIQUEACTIVITY_INFIELDS = "infields";
        public const string UNIQUEACTIVITY_RESULTFIELDS = "resultfields";

        public const string LENGTH_RECORDSETNAME = "recordsetname";
        public const string LENGTH_RECORDSLENGTH = "recordslength";
        public const string LENGTH_TREATNULLASZERO = "treatnullaszero";

        public const string RABBITDSFMQPUBLISHACTIVITY = "DsfPublishRabbitMQActivity";
        public const string RABBITMQPUBLISHACTIVITY = "PublishRabbitMQActivity";
        public const string DISPLAYNAME_RABBITMQPUBLISH = "RabbitMQ Publish";
        public const string RABBITMQPUBLISH_SOURCEID = "rabbitmqsourceresourceid";
        public const string RABBITMQPUBLISH_QUEUENAME = "queuename";
        public const string RABBITMQPUBLISH_MESSAGE = "message";
        public const string RABBITMQPUBLISH_BASICPROPERTIES = "basicproperties";
        public const string RABBITMQPUBLISH_SETTINGS_DURABLE = "isdurable";
        public const string RABBITMQPUBLISH_SETTINGS_EXCLUSIVE = "isexclusive";
        public const string RABBITMQPUBLISH_SETTINGS_AUTODELETE = "isautodelete";


        public const string RABBITDSFMQCONSUMEACTIVITY = "DsfConsumeRabbitMQActivity";
        public const string DISPLAYNAME_RABBITMQCONSUME = "RabbitMQ Consume";
        public const string RABBITMQCONSUME_SOURCEID = "rabbitmqsourceresourceid";
        public const string RABBITMQCONSUME_QUEUENAME = "queuename";
        public const string RABBITMQCONSUME_ISOBJECT = "isobject";
        public const string RABBITMQCONSUME_OBJECTNAME = "objectname";
        public const string RABBITMQCONSUME_RESPONSE = "response";
        public const string RABBITMQCONSUME_PREFETCH = "prefetch";
        public const string RABBITMQCONSUME_TIMEOUT = "timeout";
        public const string RABBITMQCONSUME_ACKNOWLEDGE = "acknowledge";
        public const string RABBITMQCONSUME_REQUEUE = "requeue";


        public const string PATHCREATE_OUTPUTPATH = "outputpath";
        public const string PATHCREATE_OVERWRITE = "overwrite";

        public const string PATHCOPY_INPUTPATH = "inputpath";
        public const string PATHCOPY_OUTPUTPATH = "outputpath";
        public const string PATHCOPY_OVERWRITE = "overwrite";
        public const string PATHCOPY_DESTINATIONUSERNAME = "destinationusername";
        public const string PATHCOPY_DESTINATIONPASSWORD = "destinationpassword";
        public const string PATHCOPY_DESTINATIONPRIVATEKEYFILE = "destinationprivatekeyfile";

        public const string PATHMOVE_INPUTPATH = "inputpath";
        public const string PATHMOVE_OUTPUTPATH = "outputpath";
        public const string PATHMOVE_OVERWRITE = "overwrite";
        public const string PATHMOVE_DESTINATIONUSERNAME = "destinationusername";
        public const string PATHMOVE_DESTINATIONPASSWORD = "destinationpassword";
        public const string PATHMOVE_DESTINATIONPRIVATEKEYFILE = "destinationprivatekeyfile";

        public const string DSFZIP = "DsfZip";
        public const string DISPLAYNAME_ZIP = "Zip";
        public const string ZIP_ARCHIVENAME = "archivename";
        public const string ZIP_COMPRESSIONRATIO = "compressionratio";
        public const string ZIP_ARCHIVEPASSWORD = "archivepassword";
        public const string ZIP_INPUTPATH = "inputpath";
        public const string ZIP_OUTPUTPATH = "outputpath";
        public const string ZIP_OVERWRITE = "overwrite";
        public const string ZIP_USERNAME = "username";
        public const string ZIP_PASSWORD = "password";
        public const string ZIP_PRIVATEKEYFILE = "privatekeyfile";
        public const string ZIP_DESTINATIONUSERNAME = "destinationusername";
        public const string ZIP_DESTINATIONPASSWORD = "destinationpassword";
        public const string ZIP_DESTINATIONPRIVATEKEYFILE = "destinationprivatekeyfile";

        public const string PATHDELETE_INPUTPATH = "inputpath";


        public const string UNZIPACTIVITY = "DsfUnZip";
        public const string DISPLAYNAME_UNZIP = "UnZip";
        public const string UNZIP_INPUTPATH = "inputpath";
        public const string UNZIP_USERNAME = "username";
        public const string UNZIP_PASSWORD = "password";
        public const string UNZIP_PRIVATEKEYFILE = "privatekeyfile";
        public const string UNZIP_DESTINATIONOUTPUTPATH = "outputpath";
        public const string UNZIP_DESTINATIONUSERNAME = "destinationusername";
        public const string UNZIP_DESTINATIONPASSWORD = "destinationpassword";
        public const string UNZIP_DESTINATIONPRIVATEKEYFILE = "destinationprivatekeyfile";
        public const string UNZIP_OVERWRITE = "overwrite";
        public const string UNZIP_ARCHIVEPASSWORD = "archivepassword";

        public const string DSFCOMMENTACTIVITY = "DsfCommentActivity";
        public const string DISPLAYNAME_COMMENT = "Comment";
        public const string COMMENT_TEXT = "text";

        public const string FILEWRITE_OUTPUTPATH = "outputpath";
        public const string FILEWRITE_FILECONTENTS = "filecontents";
        public const string FILEWRITE_OVERWRITE = "overwrite";
        public const string FILEWRITE_APPENDTOP = "appendtop";
        public const string FILEWRITE_APPENDBOTTOM = "appendbottom";
        public const string FILEWRITE_ASBASE64 = "filecontentsasbase64";

        public const string DSFJAVASCRIPTACTIVITY = "DsfJavascriptActivity";
        public const string DISPLAYNAME_JAVASCRIPT = "JavaScript";
        public const string JAVASCRIPT_SCRIPT = "script";
        public const string JAVASCRIPT_ESCAPESCRIPT = "escapescript";
        public const string JAVASCRIPT_INCLUDEFILE = "includefile";
        public const string JAVASCRIPT_RESULT = "result";

        public const string DSFRUBYACTIVITY = "DsfRubyActivity";
        public const string DISPLAYNAME_RUBY = "Ruby";
        public const string RUBY_SCRIPT = "script";
        public const string RUBY_ESCAPESCRIPT = "escapescript";
        public const string RUBY_INCLUDEFILE = "includefile";
        public const string RUBY_RESULT = "result";

        public const string COMMANDLINE_COMMANDFILENAME = "commandfilename";
        public const string COMMANDLINE_COMMANDPRIORITY = "commandpriority";
        public const string COMMANDLINE_COMMANDRESULT = "commandresult";

        public const string SUSPENDEXECUTIONACTIVITY = "SuspendExecutionActivity";
        public const string DISPLAYNAME_SUSPENDEXECUTION = "Suspend Execution";
        public const string SUSPENDEXECUTION_SUSPENDOPTION = "suspendoption";
        public const string SUSPENDEXECUTION_PERSISTVALUE = "persistvalue";
        public const string SUSPENDEXECUTION_ALLOWMANUALRESUMPTION = "allowmanualresumption";
        public const string SUSPENDEXECUTION_ENCRYPTDATA = "encryptdata";
        public const string SUSPENDEXECUTION_RESPONSE = "response";
        public const string SUSPENDEXECUTION_SAVEDATAFUNC = "savedatafunc";

        public const string PYTHON_SCRIPT = "python_script";
        public const string PYTHON_ESCAPESCRIPT = "python_escapescript";
        public const string PYTHON_INCLUDEFILE = "python_includefile";
        public const string PYTHON_RESULT = "python_result";

        public const string MANUALRESUMPTIONACTIVITY = "DsfManualResumptionActivity";
        public const string MANUALRESUMPTION_DISPLAYNAME = "Manual Resumption";
        public const string MANUALRESUMPTION_SUSPENSIONID = "suspensionid";
        public const string MANUALRESUMPTION_OVERRIDEINPUTVARIABLE = "overrideinputvariables";
        public const string MANUALRESUMPTION_ACTIVITYFUNC = "OverrideDataFunc";

        public const string GATE_CONDITIONS = "gate_conditions";
        public const string GATE_RETRYENTRYPOINTID = "gate_retryentrypointid";
        public const string GATE_GATEOPTIONS = "gate_gateoptions";
        public const string GATE_DATAFUNC = "dataFunc";
        public const string GATE_APPLYACTIVITYFUNC = "applyActivityFunc";

        // Date Time Difference Activity
        public const string DATETIMEDIFF_INPUT1 = "input1";
        public const string DATETIMEDIFF_INPUT2 = "input2";
        public const string DATETIMEDIFF_INPUTFORMAT = "inputformat";
        public const string DATETIMEDIFF_OUTPUTTYPE = "outputtype";
        public const string DATETIMEDIFF_RESULT = "result";

        // SMTP Email Activity
        public const string DSFDOTNETDATETIMEACTIVITY = "DsfDotNetDateTimeActivity";
        public const string DSFDATETIMEACTIVITY = "DsfDateTimeActivity";
        public const string DISPLAYNAME_DOTNETDATETIME = "Date and Time";
        public const string DOTNETDATETIME_DATETIME = "datetime";
        public const string DOTNETDATETIME_INPUTFORMAT = "inputformat";
        public const string DOTNETDATETIME_OUTPUTFORMAT = "outputformat";
        public const string DOTNETDATETIME_TIMEMODIFIERTYPE = "timemodifiertype";
        public const string DOTNETDATETIME_TIMEMODIFIERAMOUNTDISPLAY = "timemodifieramountdisplay";
        public const string DOTNETDATETIME_TIMEMODIFIERAMOUNT = "timemodifieramount";
        public const string DOTNETDATETIME_RESULT = "result";

        public const string DSFSENDEMAILACTIVITY = "DsfSendEmailActivity";
        public const string DISPLAYNAME_SMTPEMAIL = "Send Email";
        public const string SMTPEMAIL_SOURCEID = "smtpemail_sourceid";
        public const string SMTPEMAIL_FROMACCOUNT = "smtpemail_fromaccount";
        public const string SMTPEMAIL_PASSWORD = "smtpemail_password";
        public const string SMTPEMAIL_TO = "smtpemail_to";
        public const string SMTPEMAIL_CC = "smtpemail_cc";
        public const string SMTPEMAIL_BCC = "smtpemail_bcc";
        public const string SMTPEMAIL_PRIORITY = "smtpemail_priority";
        public const string SMTPEMAIL_SUBJECT = "smtpemail_subject";
        public const string SMTPEMAIL_ATTACHMENTS = "smtpemail_attachments";
        public const string SMTPEMAIL_BODY = "smtpemail_body";
        public const string SMTPEMAIL_ISHTML = "smtpemail_ishtml";

        // Random Activity
        public const string DSFRANDOMACTIVITY = "DsfRandomActivity";
        public const string DISPLAYNAME_RANDOM = "Random";
        public const string RANDOM_TYPE = "randomtype";
        public const string RANDOM_FROM = "from";
        public const string RANDOM_TO = "to";
        public const string RANDOM_LENGTH = "length";
        public const string RANDOM_RESULT = "result";

        public const string DSFEXCHANGEEMAILNEWACTIVITY = "DsfExchangeEmailNewActivity";
        public const string DISPLAYNAME_EXCHANGEEMAIL = "Exchange Email";
        public const string EXCHANGEEMAIL_SOURCEID = "exchangesourceid";
        public const string EXCHANGEEMAIL_TO = "to";
        public const string EXCHANGEEMAIL_CC = "cc";
        public const string EXCHANGEEMAIL_BCC = "bcc";
        public const string EXCHANGEEMAIL_SUBJECT = "subject";
        public const string EXCHANGEEMAIL_ATTACHMENTS = "attachments";
        public const string EXCHANGEEMAIL_BODY = "body";
        public const string EXCHANGEEMAIL_ISHTML = "ishtml";

        // Format Number Activity
        public const string DSFNUMBERFORMATACTIVITY = "DsfNumberFormatActivity";
        public const string DISPLAYNAME_NUMBERFORMAT = "Format Number";
        public const string NUMBERFORMAT_EXPRESSION = "expression";
        public const string NUMBERFORMAT_ROUNDINGTYPE = "roundingtype";
        public const string NUMBERFORMAT_ROUNDINGDECIMALPLACES = "roundingdecimalplaces";
        public const string NUMBERFORMAT_DECIMALPLACESTOSHOW = "decimalplacestoshow";
        public const string NUMBERFORMAT_RESULT = "result";

        // Create JSON Activity
        public const string DSFCREATEJSONACTIVITY = "DsfCreateJsonActivity";
        public const string DISPLAYNAME_CREATEJSON = "Create JSON";
        public const string CREATEJSON_JSONMAPPINGS = "jsonmappings";
        public const string CREATEJSON_UPDATEDJSONMAPPINGS = "updatedjsonmappings";
        public const string CREATEJSON_JSONSTRING = "jsonstring";

        // Calculate Activity
        public const string DSFDOTNETCALCULATEACTIVITY = "DsfDotNetCalculateActivity";
        public const string DISPLAYNAME_CALCULATE = "Calculate";
        public const string CALCULATE_EXPRESSION = "expression";
        public const string CALCULATE_RESULT = "result";

        // Aggregate Calculate Activity
        public const string DSFAGGREGATECALCULATEACTIVITY = "DsfAggregateCalculateActivity";
        public const string DSFDOTNETAGGREGATECALCULATEACTIVITY = "DsfDotNetAggregateCalculateActivity";
        public const string DISPLAYNAME_AGGREGATECALCULATE = "Aggregate Calculate";
        public const string AGGREGATECALCULATE_EXPRESSION = "expression";
        public const string AGGREGATECALCULATE_RESULT = "result";

        // Gather System Information Activity
        public const string DSFGATHERSYSTEMINFORMATIONACTIVITY = "DsfGatherSystemInformationActivity";
        public const string DSFDOTNETGATHERSYSTEMINFORMATIONACTIVITY = "DsfDotNetGatherSystemInformationActivity";
        public const string DISPLAYNAME_GATHERSYSTEMINFORMATION = "Gather System Information";
        public const string GATHERSYSINFO_SYSTEMINFOCOLLECTION = "systeminformationcollection";

        // Web Request (Utility) Activity
        public const string DSFWEBGETREQUESTWITHTIMEOUTACTIVITY = "DsfWebGetRequestWithTimeoutActivity";
        public const string DISPLAYNAME_WEBREQUEST = "Web Request";
        public const string WEBREQUEST_METHOD = "webrequest_method";
        public const string WEBREQUEST_TIMEOUTSECONDS = "webrequest_timeoutseconds";
        public const string WEBREQUEST_TIMEOUTTEXT = "webrequest_timeouttext";
        public const string WEBREQUEST_URL = "webrequest_url";
        public const string WEBREQUEST_HEADERS = "webrequest_headers";
        public const string WEBREQUEST_RESULT = "webrequest_result";
    
    }

    public class X6WorkflowLoadModel
    {
        [JsonProperty("workflowxml")]
        public string WorkflowXml { get; set; }

        [JsonProperty("nodes")]
        public List<Cell> Nodes { get; set; } = new List<Cell>();

        [JsonProperty("edges")]
        public List<Cell> Edges { get; set; } = new List<Cell>();

        [Newtonsoft.Json.JsonIgnore]
        public Dictionary<Activity, Cell> ActivityNodeMap { get; set; }
    }

    public class Cell
    {
        [JsonProperty("position")]
        public Position position { get; set; }

        [JsonProperty("size")]
        public Size size { get; set; }

        [JsonProperty("visible")]
        public bool? visible { get; set; }

        [JsonProperty("shape")]
        public string shape { get; set; }

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonPropertyName("data")]
        public Dictionary<string, object> data { get; set; } = new Dictionary<string, object>();

        //[JsonPropertyName("attrs")]
        //public Dictionary<string, object> Attrs { get; set; } = new Dictionary<string, object>();

        //[JsonProperty("zIndex")]
        //public int ZIndex { get; set; }

        [JsonProperty("source")]
        public Connector Source { get; set; }

        [JsonProperty("target")]
        public Connector Target { get; set; }

        [JsonPropertyName("label")]
        public string label { get; set; }
    }

    public class X6WorkflowSaveModel
    {
        [JsonProperty("resourcename")]
        public string ResourceName { get; set; }

        [JsonProperty("workflowxml")]
        public string WorkflowXml { get; set; }

        [JsonPropertyName("cells")]
        public List<Cell> Cells { get; set; } = new List<Cell>();
    }

    public class Position
    {
        [JsonProperty("x")]
        public float X { get; set; }

        [JsonProperty("y")]
        public float Y { get; set; }

        public Position()
        {

        }

        public Position(float x, float y)
        {
            X = x;
            Y = y;
        }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class Size
    {
        [JsonProperty("width")]
        public float Width { get; set; }

        [JsonProperty("height")]
        public float Height { get; set; }

        public Size() { }

        public Size(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }

    public class Connector
    {
        [JsonProperty("cell")]
        public string Id { get; set; }

        public Connector(string id)
        {
            Id = id;
        }
    }

    public class X6NodeOnErrorData
    {
        [JsonProperty("errorMessage")]
        public string OnErrorVariable { get; set; }
        [JsonProperty("webServiceUrl")]
        public string OnErrorWorkflow { get; set; }
        [JsonProperty("endWorkflow")]
        public bool IsEndedOnError { get; set; }

        public X6NodeOnErrorData() { }
    }
}
