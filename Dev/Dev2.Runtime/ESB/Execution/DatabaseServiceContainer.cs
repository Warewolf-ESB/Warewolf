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
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Management;
using Dev2.Services.Execution;
using Warewolf.Execution;

namespace Dev2.Runtime.ESB.Execution
{
    public class DatabaseServiceContainer : EsbExecutionContainer
    {
        readonly IServiceExecution _databaseServiceExecution;

        public DatabaseServiceContainer(IServiceExecution databaseServiceExecution)
        {
            _databaseServiceExecution = databaseServiceExecution;
        }

        public override Guid Execute(out ErrorResultTO errors, int update)
        {
            errors = new ErrorResultTO();
            _databaseServiceExecution.BeforeExecution(errors);

            if (_databaseServiceExecution is DatabaseServiceExecution databaseServiceExecution)
            {
                databaseServiceExecution.InstanceInputDefinitions = InstanceInputDefinition;
                databaseServiceExecution.InstanceOutputDefintions = InstanceOutputDefinition;
            }

            var result = _databaseServiceExecution.Execute(out errors, update);
            _databaseServiceExecution.AfterExecution(errors);
            return result;
        }

        public override bool CanExecute(Guid resourceId, IDSFDataObject dataObject, AuthorizationContext authorizationContext) => true;
        public override bool CanExecute(IEsbManagementEndpoint eme, IDSFDataObject dataObject)
        {  var resourceId = eme.GetResourceID(Request.Args);
            var authorizationContext = eme.GetAuthorizationContextForService();
            var isFollower = string.IsNullOrWhiteSpace(Config.Cluster.LeaderServerKey);
            if (isFollower && eme.CanExecute(new CanExecuteArg{ IsFollower = isFollower }))
            {
                return false;
            }
            return CanExecute(resourceId, dataObject, authorizationContext);
        }

        public override IDSFDataObject Execute(IDSFDataObject inputs, IDev2Activity activity) => null;
    }
}
