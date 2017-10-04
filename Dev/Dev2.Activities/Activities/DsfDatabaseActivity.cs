/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Dev2.Services.Execution;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities
{
    public class DsfDatabaseActivity : DsfActivity,IEquatable<DsfDatabaseActivity>
    {
        public IServiceExecution ServiceExecution { get; protected set; }

        #region Overrides of DsfActivity

        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO errors, int update)
        {
            var execErrors = new ErrorResultTO();

            errors = new ErrorResultTO();
            errors.MergeErrors(execErrors);

            if (ServiceExecution is DatabaseServiceExecution databaseServiceExecution)
            {
                databaseServiceExecution.InstanceInputDefinitions = inputs; // set the output mapping for the instance ;)
                databaseServiceExecution.InstanceOutputDefintions = outputs; // set the output mapping for the instance ;)
            }
            ServiceExecution.Execute(out execErrors, update);
            var fetchErrors = execErrors.FetchErrors();
            foreach(var error in fetchErrors)
            {
                dataObject.Environment.Errors.Add(error);
            }
            
            errors.MergeErrors(execErrors);

        }

        #region Overrides of DsfActivity

        protected override void BeforeExecutionStart(IDSFDataObject dataObject, ErrorResultTO tmpErrors)
        {
            base.BeforeExecutionStart(dataObject, tmpErrors);
            ServiceExecution = new DatabaseServiceExecution(dataObject);
            ServiceExecution.BeforeExecution(tmpErrors);
        }

        protected override void AfterExecutionCompleted(ErrorResultTO tmpErrors)
        {
            base.AfterExecutionCompleted(tmpErrors);
            ServiceExecution.AfterExecution(tmpErrors);
        }

        #endregion

        #endregion

        public bool Equals(DsfDatabaseActivity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) 
                && Equals(ServiceExecution, other.ServiceExecution);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DsfDatabaseActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (ServiceExecution != null ? ServiceExecution.GetHashCode() : 0);
            }
        }
    }
}
