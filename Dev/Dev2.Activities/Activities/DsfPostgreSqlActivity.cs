﻿#pragma warning disable
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
using System.ComponentModel;
using System.Linq;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Dev2.Services.Execution;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Resource.Errors;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("Postgre", "PostgreSQL", ToolType.Native, "8999E59B-38A3-43BB-A92F-6090C5C3EA80", "Dev2.Activities", "1.0.0.0", "Legacy", "Database", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Database_PostgreSQL")]
    public class DsfPostgreSqlActivity : DsfActivity,IEquatable<DsfPostgreSqlActivity>
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IServiceExecution ServiceExecution { get; protected set; }
        public string ProcedureName { get; set; }
        public int? CommandTimeout { get; set; }
        public DsfPostgreSqlActivity()
        {
            Type = "PostgreSQL Database Connector";
            DisplayName = "PostgreSQL Database";
        }

        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            var execErrors = new ErrorResultTO();

            tmpErrors = new ErrorResultTO();
            tmpErrors.MergeErrors(execErrors);

            if (string.IsNullOrEmpty(ProcedureName))
            {
                tmpErrors.AddError(ErrorResource.NoActionsInSelectedDB);
                return;
            }


            if (ServiceExecution is DatabaseServiceExecution databaseServiceExecution)
            {
                databaseServiceExecution.Inputs = Inputs.Select(a => new ServiceInput { EmptyIsNull = a.EmptyIsNull, Name = a.Name, RequiredField = a.RequiredField, Value = a.Value, TypeName = a.TypeName } as IServiceInput).ToList();
                databaseServiceExecution.Outputs = Outputs;
            }

            ServiceExecution.Execute(out execErrors, update);

            var fetchErrors = execErrors.FetchErrors();

            foreach (var error in fetchErrors)
            {
                dataObject.Environment.Errors.Add(error);
            }
            tmpErrors.MergeErrors(execErrors);
        }

        protected override void BeforeExecutionStart(IDSFDataObject dataObject, ErrorResultTO tmpErrors)
        {
            base.BeforeExecutionStart(dataObject, tmpErrors);
            var databaseServiceExecution = new DatabaseServiceExecution(dataObject)
            {
                ProcedureName = ProcedureName,
            };
            if (CommandTimeout != null)
            {
                databaseServiceExecution.CommandTimeout = CommandTimeout.Value;
            }
            ServiceExecution = databaseServiceExecution;
            ServiceExecution.GetSource(SourceId);
            ServiceExecution.BeforeExecution(tmpErrors);
        }

        protected override void AfterExecutionCompleted(ErrorResultTO tmpErrors)
        {
            base.AfterExecutionCompleted(tmpErrors);
            ServiceExecution.AfterExecution(tmpErrors);
        }

        public override enFindMissingType GetFindMissingType() => enFindMissingType.DataGridActivity;

        public bool Equals(DsfPostgreSqlActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other)
                && string.Equals(SourceId.ToString(), other.SourceId.ToString())
                && string.Equals(ProcedureName, other.ProcedureName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((DsfPostgreSqlActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (SourceId.GetHashCode());
                if (ProcedureName != null)
                {
                    hashCode = (hashCode * 397) ^ (ProcedureName.GetHashCode());
                }
                return hashCode;
            }
        }
    }
}
