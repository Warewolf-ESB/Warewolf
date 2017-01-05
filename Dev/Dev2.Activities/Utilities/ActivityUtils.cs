/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Activities;
using Dev2.Common.Interfaces.DB;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Core;
// ReSharper disable InconsistentNaming

namespace Dev2.Utilities
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class ActivityUtils
    {
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static DsfSqlServerDatabaseActivity GetDsfSqlServerDatabaseActivity(DsfDatabaseActivity dbActivity, DbService service, DbSource source)
        {
            DsfSqlServerDatabaseActivity dsfSqlServerDatabaseActivity = null;
            if(dbActivity != null)
            {
                dsfSqlServerDatabaseActivity = new DsfSqlServerDatabaseActivity
                {
                    ResourceID = dbActivity.ResourceID,
                    DisplayName = dbActivity.DisplayName,
                    ProcedureName = service.Method.ExecuteAction,
                    SourceId = source.ResourceID,
                    Inputs = TranslateInputMappingToInputs(dbActivity.InputMapping),
                    Outputs = TranslateOutputMappingToOutputs(dbActivity.OutputMapping),
                    ToolboxFriendlyName = dbActivity.ToolboxFriendlyName,
                    IconPath = dbActivity.IconPath,
                    ServiceName = dbActivity.ServiceName,
                    DataTags = dbActivity.DataTags,
                    ResultValidationRequiredTags = dbActivity.ResultValidationRequiredTags,
                    ResultValidationExpression = dbActivity.ResultValidationExpression,
                    FriendlySourceName = dbActivity.FriendlySourceName,
                    EnvironmentID = dbActivity.EnvironmentID,
                    Type = dbActivity.Type,
                    ActionName = dbActivity.ActionName,
                    RunWorkflowAsync = dbActivity.RunWorkflowAsync,
                    Category = dbActivity.Category,
                    ServiceUri = dbActivity.ServiceUri,
                    ServiceServer = dbActivity.ServiceServer,
                    UniqueID = dbActivity.UniqueID,
                    ParentServiceName = dbActivity.ParentServiceName,
                    ParentServiceID = dbActivity.ParentServiceID,
                    ParentWorkflowInstanceId = dbActivity.ParentWorkflowInstanceId,
                    ParentInstanceID = dbActivity.ParentInstanceID,
                };
            }
            return dsfSqlServerDatabaseActivity;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static DsfMySqlDatabaseActivity GetDsfMySqlDatabaseActivity(DsfDatabaseActivity dbActivity, DbSource source, DbService service)
        {
            var dsfMySqlDatabaseActivity = new DsfMySqlDatabaseActivity
            {
                ResourceID = dbActivity.ResourceID,
                DisplayName = dbActivity.DisplayName,
                SourceId = source.ResourceID,
                ProcedureName = service.Method.ExecuteAction,
                Inputs = TranslateInputMappingToInputs(dbActivity.InputMapping),
                Outputs = TranslateOutputMappingToOutputs(dbActivity.OutputMapping),
                ToolboxFriendlyName = dbActivity.ToolboxFriendlyName,
                IconPath = dbActivity.IconPath,
                ServiceName = dbActivity.ServiceName,
                DataTags = dbActivity.DataTags,
                ResultValidationRequiredTags = dbActivity.ResultValidationRequiredTags,
                ResultValidationExpression = dbActivity.ResultValidationExpression,
                FriendlySourceName = dbActivity.FriendlySourceName,
                EnvironmentID = dbActivity.EnvironmentID,
                Type = dbActivity.Type,
                ActionName = dbActivity.ActionName,
                RunWorkflowAsync = dbActivity.RunWorkflowAsync,
                Category = dbActivity.Category,
                ServiceUri = dbActivity.ServiceUri,
                ServiceServer = dbActivity.ServiceServer,
                UniqueID = dbActivity.UniqueID,
                ParentServiceName = dbActivity.ParentServiceName,
                ParentServiceID = dbActivity.ParentServiceID,
                ParentWorkflowInstanceId = dbActivity.ParentWorkflowInstanceId,
                ParentInstanceID = dbActivity.ParentInstanceID,
            };
            return dsfMySqlDatabaseActivity;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static DsfOracleDatabaseActivity GetDsfOracleDatabaseActivity(DsfDatabaseActivity dbActivity, DbService service, DbSource source)
        {
            var DsfOracleDatabaseActivity = new DsfOracleDatabaseActivity
            {
                ResourceID = dbActivity.ResourceID,
                SourceId = source.ResourceID,
                ProcedureName = service.Method.ExecuteAction,
                Inputs = TranslateInputMappingToInputs(dbActivity.InputMapping),
                Outputs = TranslateOutputMappingToOutputs(dbActivity.OutputMapping),
                ToolboxFriendlyName = dbActivity.ToolboxFriendlyName,
                IconPath = dbActivity.IconPath,
                ServiceName = dbActivity.ServiceName,
                DataTags = dbActivity.DataTags,
                ResultValidationRequiredTags = dbActivity.ResultValidationRequiredTags,
                ResultValidationExpression = dbActivity.ResultValidationExpression,
                FriendlySourceName = dbActivity.FriendlySourceName,
                EnvironmentID = dbActivity.EnvironmentID,
                Type = dbActivity.Type,
                ActionName = dbActivity.ActionName,
                RunWorkflowAsync = dbActivity.RunWorkflowAsync,
                Category = dbActivity.Category,
                ServiceUri = dbActivity.ServiceUri,
                ServiceServer = dbActivity.ServiceServer,
                UniqueID = dbActivity.UniqueID,
                ParentServiceName = dbActivity.ParentServiceName,
                ParentServiceID = dbActivity.ParentServiceID,
                ParentWorkflowInstanceId = dbActivity.ParentWorkflowInstanceId,
                ParentInstanceID = dbActivity.ParentInstanceID,
            };
            return DsfOracleDatabaseActivity;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static DsfODBCDatabaseActivity GetDsfODBCDatabaseActivity(DsfDatabaseActivity dbActivity, DbService service, DbSource source)
        {
            var dsfOdbcDatabaseActivity = new DsfODBCDatabaseActivity
            {
                ResourceID = dbActivity.ResourceID,
                SourceId = source.ResourceID,
                CommandText = service.Method.ExecuteAction,
                Inputs = TranslateInputMappingToInputs(dbActivity.InputMapping),
                Outputs = TranslateOutputMappingToOutputs(dbActivity.OutputMapping),
                ToolboxFriendlyName = dbActivity.ToolboxFriendlyName,
                IconPath = dbActivity.IconPath,
                ServiceName = dbActivity.ServiceName,
                DataTags = dbActivity.DataTags,
                ResultValidationRequiredTags = dbActivity.ResultValidationRequiredTags,
                ResultValidationExpression = dbActivity.ResultValidationExpression,
                FriendlySourceName = dbActivity.FriendlySourceName,
                EnvironmentID = dbActivity.EnvironmentID,
                Type = dbActivity.Type,
                ActionName = dbActivity.ActionName,
                RunWorkflowAsync = dbActivity.RunWorkflowAsync,
                Category = dbActivity.Category,
                ServiceUri = dbActivity.ServiceUri,
                ServiceServer = dbActivity.ServiceServer,
                UniqueID = dbActivity.UniqueID,
                ParentServiceName = dbActivity.ParentServiceName,
                ParentServiceID = dbActivity.ParentServiceID,
                ParentWorkflowInstanceId = dbActivity.ParentWorkflowInstanceId,
                ParentInstanceID = dbActivity.ParentInstanceID,
            };
            return dsfOdbcDatabaseActivity;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static DsfPostgreSqlActivity GetDsfPostgreSqlActivity(DsfDatabaseActivity dbActivity, DbService service, DbSource source)
        {
            DsfPostgreSqlActivity dsfSqlServerDatabaseActivity = null;
            if (dbActivity != null)
            {
                dsfSqlServerDatabaseActivity = new DsfPostgreSqlActivity
                {
                    ResourceID = dbActivity.ResourceID,
                    DisplayName = dbActivity.DisplayName,
                    ProcedureName = service.Method.ExecuteAction,
                    SourceId = source.ResourceID,
                    Inputs = TranslateInputMappingToInputs(dbActivity.InputMapping),
                    Outputs = TranslateOutputMappingToOutputs(dbActivity.OutputMapping),
                    ToolboxFriendlyName = dbActivity.ToolboxFriendlyName,
                    IconPath = dbActivity.IconPath,
                    ServiceName = dbActivity.ServiceName,
                    DataTags = dbActivity.DataTags,
                    ResultValidationRequiredTags = dbActivity.ResultValidationRequiredTags,
                    ResultValidationExpression = dbActivity.ResultValidationExpression,
                    FriendlySourceName = dbActivity.FriendlySourceName,
                    EnvironmentID = dbActivity.EnvironmentID,
                    Type = dbActivity.Type,
                    ActionName = dbActivity.ActionName,
                    RunWorkflowAsync = dbActivity.RunWorkflowAsync,
                    Category = dbActivity.Category,
                    ServiceUri = dbActivity.ServiceUri,
                    ServiceServer = dbActivity.ServiceServer,
                    UniqueID = dbActivity.UniqueID,
                    ParentServiceName = dbActivity.ParentServiceName,
                    ParentServiceID = dbActivity.ParentServiceID,
                    ParentWorkflowInstanceId = dbActivity.ParentWorkflowInstanceId,
                    ParentInstanceID = dbActivity.ParentInstanceID,
                };
            }
            return dsfSqlServerDatabaseActivity;
        }
        public static ICollection<IServiceOutputMapping> TranslateOutputMappingToOutputs(string outputMapping)
        {
            var outputDefs = DataListFactory.CreateOutputParser().Parse(outputMapping);
            return outputDefs.Select(outputDef =>
            {
                if (DataListUtil.IsValueRecordset(outputDef.RawValue))
                {
                    return new ServiceOutputMapping(outputDef.Name, outputDef.RawValue, outputDef.RecordSetName);
                }
                return new ServiceOutputMapping(outputDef.Name, outputDef.RawValue, "");
            }).Cast<IServiceOutputMapping>().ToList();
        }

        public static ICollection<IServiceInput> TranslateInputMappingToInputs(string inputMapping)
        {
            var inputDefs = DataListFactory.CreateInputParser().Parse(inputMapping);
            return inputDefs.Select(inputDef => new ServiceInput(inputDef.Name, inputDef.RawValue)
            {
                EmptyIsNull = inputDef.EmptyToNull,
                RequiredField = inputDef.IsRequired
            }).Cast<IServiceInput>().ToList();
        }
    }
}
