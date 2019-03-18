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


namespace Dev2.Common.Interfaces.Core.DynamicServices
{
    /// <summary>
    ///     Indicates the mode that a service is executing in
    /// </summary>
    public enum enDynamicServiceMode
    {
        /// <summary>
        ///     Represents the standard mode where no debugging data is emitted to callers
        /// </summary>
        Normal,

        /// <summary>
        ///     Represents the mode where parameters are validated only and actions are not executed
        /// </summary>
        ValidationOnly,

        /// <summary>
        ///     Represents the mode where debug data is emitted with the results of action execution
        /// </summary>
        Debug
    }

    /// <summary>
    ///     Represents the types of actions that are supported by the Dynamic Service Engine
    /// </summary>
    public enum enActionType
    {
        /// <summary>
        ///     Represents a business rule which must invoke a dynamic service and evaluate its results with an expression
        ///     The only type is currently used here is the BizRule type as all other types are inferred from the
        ///     source e.g. a SqlDatabase source can only be an InvokeStoredProc type
        /// </summary>
        BizRule,

        /// <summary>
        ///     Represents a stored procedure in a Microsoft SQL Server or MySQL database
        /// </summary>
        InvokeStoredProc,

        /// <summary>
        ///     Represents any valid SOAP based Web Service
        /// </summary>
        InvokeWebService,

        /// <summary>
        ///     Represents a Dynamic Service
        /// </summary>
        InvokeDynamicService,

        /// <summary>
        ///     Represents a Management Dynamic Service
        /// </summary>
        InvokeManagementDynamicService,

        /// <summary>
        /// </summary>
        InvokeServiceMethod,

        /// <summary>
        ///     Represents a custom assembly that will be used for extensibility
        /// </summary>
        Plugin, 
        /// <summary>
        ///     Represents a custom assembly that will be used for extensibility
        /// </summary>
        ComPlugin,

        /// <summary>
        ///     Allows definition of a conditional Service Action that executes different steps based on a value
        /// </summary>
        Switch,

        /// <summary>
        ///     Indicates an unknown type of service action. This will not compile and be hosted
        /// </summary>
        Unknown,

        /// <summary>
        ///     Indicates that this action is defined by a workflow
        /// </summary>
        Workflow,

        /// <summary>
        ///     The remote service
        /// </summary>
        RemoteService
    }

    /// <summary>
    ///     Represents the type of data source that will be providing xml data to data consumers
    /// </summary>
    public enum enSourceType
    {
        /// <summary>
        ///     A Microsoft SQL Server database
        /// </summary>
        SqlDatabase,

        /// <summary>
        ///     A MySQL database
        /// </summary>
        MySqlDatabase,
        /// <summary>
        ///     A ODBC database
        /// </summary>
        ODBC,
        /// <summary>
        ///     A Oracle database
        /// </summary>
        Oracle,

        /// <summary>
        ///     A PostgreSQL database
        /// </summary>
        PostgreSQL,
		/// <summary>
		///     A SQLite database
		/// </summary>
		SQLiteDatabase,
		/// <summary>
		///     A SOAP based Web Service: REST web services are not supported
		/// </summary>
		WebService,

        /// <summary>
        ///     A Dynamic Service that exists in the ServiceDefinition File
        /// </summary>
        DynamicService,

        /// <summary>
        ///     A static Management method located in the Unlimited.ServiceLibrary.Endpoint type
        /// </summary>
        ManagementDynamicService,

        /// <summary>
        ///     An assembly that provides custom functionality e.g. Ftp, File Formatting, Workflow Invocation
        /// </summary>
        PluginSource,

        /// <summary>
        ///     Indicates an unknown source type. This will not compile.
        /// </summary>
        Unknown,

        /// <summary>
        ///     A Dev2 server.
        /// </summary>
        Dev2Server,

        EmailSource,
        WebSource,
        OauthSource,
        SharepointServerSource,
        RabbitMQSource,
        ExchangeSource,
        WcfSource,
        ComPluginSource
    }

    /// <summary>
    ///     Represents the type of validation that needs to be done
    /// </summary>
    public enum enValidationType
    {
        /// <summary>
        ///     Validates the input as required
        /// </summary>
        Required,

        /// <summary>
        ///     Validates the input using a regular expression
        /// </summary>
        Regex,

        /// <summary>
        ///     Validates the input using a regular expression and as required
        /// </summary>
        RequiredAndRegex
    }

    /// <summary>
    ///     Represents the types of dynamic service objects that can be hosted
    /// </summary>
    public enum enDynamicServiceObjectType
    {
        /// <summary>
        ///     Encapsulates a business rule
        /// </summary>
        BizRule,

        /// <summary>
        ///     Encapsulates a Dynamic Service
        /// </summary>
        DynamicService,

        /// <summary>
        ///     Represents a service action which is an executable step
        /// </summary>
        ServiceAction,

        /// <summary>
        ///     Represents a case that is used for branching logic
        /// </summary>
        ServiceActionCase,

        /// <summary>
        ///     Represents a container for cases
        /// </summary>
        ServiceActionCases,

        /// <summary>
        ///     Represents a parameter
        /// </summary>
        ServiceActionInput,

        /// <summary>
        ///     Represents a data source to execute against
        /// </summary>
        Source,

        /// <summary>
        ///     Represents a data validator
        /// </summary>
        Validator,
        WorkflowActivity,
        UnitTest
    }

    public enum enApprovalState
    {
        Pending,
        Rejected,
        Approved
    }
}