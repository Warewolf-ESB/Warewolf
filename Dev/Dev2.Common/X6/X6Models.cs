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
        public const string DSFFINDRECORDSMULTIPLECRITERIAACTIVITY = "DsfFindRecordsMultipleCriteriaActivity";

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

        public const string DISPLAYNAME_BASECONVERT = "Base Conversion";
        public const string DISPLAYNAME_CASECONVERT = "Case Conversion";
        public const string DISPLAYNAME_FINDINDEX = "Find Index";
        public const string DISPLAYNAME_REPLACE = "Replace";
        public const string DISPLAYNAME_WEBPOST = "Post Web Method";
        public const string DISPLAYNAME_WEBDELETE = "DELETE Web Method";

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
