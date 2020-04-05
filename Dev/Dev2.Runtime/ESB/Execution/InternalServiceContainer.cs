#pragma warning disable
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
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Workspaces;
using Warewolf.Esb;
using Warewolf.Execution;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Execution
{
    /// <summary>
    /// Execute an internal or management service
    /// </summary>
    public class InternalServiceContainer : EsbExecutionContainer
    {
        readonly IEsbManagementServiceLocator _managementServiceLocator;
        private IInternalExecutionContext _internalExecutionContext;

        public InternalServiceContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, EsbExecuteRequest request, IInternalExecutionContext internalExecutionContext)
            : this(sa, dataObj, theWorkspace, esbChannel, request, null, internalExecutionContext)
        {
        }

        public InternalServiceContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, EsbExecuteRequest request, IEsbManagementServiceLocator managementServiceLocator, IInternalExecutionContext internalExecutionContext)
            : base(sa, dataObj, theWorkspace, esbChannel, request)
        {
            if (request.Args == null)
            {
                if (sa.DataListSpecification == null)
                {
                    sa.DataListSpecification = new StringBuilder("<DataList></DataList>");
                }

                var dataListTo = new DataListTO(sa.DataListSpecification.ToString());
                request.Args = new Dictionary<string, StringBuilder>();
                foreach (var input in dataListTo.Inputs)
                {
                    var warewolfEvalResult = dataObj.Environment.Eval(DataListUtil.AddBracketsToValueIfNotExist(input), 0);
                    if (warewolfEvalResult.IsWarewolfAtomResult && warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult scalarResult && !scalarResult.Item.IsNothing)
                    {
                        request.Args.Add(input, new StringBuilder(scalarResult.Item.ToString()));
                    }
                }
            }

            _managementServiceLocator = managementServiceLocator ?? new EsbManagementServiceLocator();
            _internalExecutionContext = internalExecutionContext;
        }

        public override Guid Execute(out ErrorResultTO errors, int update)
        {
            errors = new ErrorResultTO();
            var invokeErrors = new ErrorResultTO();
            var result = GlobalConstants.NullDataListID;

            try
            {
                var eme = _managementServiceLocator.LocateManagementService(ServiceAction.Name);

                if (eme != null)
                {
                    // Web request for internal service ;)
                    if (Request.Args == null)
                    {
                        GenerateRequestDictionaryFromDataObject(out invokeErrors);
                        errors.MergeErrors(invokeErrors);
                    }

                    if (CanExecute(eme, DataObject, out var errorMessage))
                    {
                        Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.ServerUser, () =>
                        {
                            ExecuteService(eme, _internalExecutionContext);
                            result = DataObject.DataListID;
                        });
                        errors.MergeErrors(invokeErrors);
                    }
                    else
                    {
                        var serializer = new Dev2JsonSerializer();
                        var msg = new ExecuteMessage {HasError = true};
                        errors.AddError(errorMessage);
                        msg.SetMessage(errorMessage);
                        Request.ExecuteResult = serializer.SerializeToBuilder(msg);
                    }

                    Request.WasInternalService = true;
                }
                else
                {
                    errors.AddError(string.Format(ErrorResource.CouldNotLocateManagementService, ServiceAction.ServiceName));
                }
            }
            catch (Exception ex)
            {
                errors.AddError(ex.Message);
            }

            return result;
        }

        private void ExecuteService(IEsbManagementEndpoint eme, IInternalExecutionContext internalExecutionContext)
        {
            if (eme is IContextualInternalService contextualInternalService)
            {
                var res = contextualInternalService.Execute(internalExecutionContext);
                if (res == null)
                {
                    Dev2Logger.Error($"Null result return from internal service:{ServiceAction.Name}",
                        GlobalConstants.WarewolfError);
                }

                Request.ExecuteResult = res;
            }
            else
            {
                var res = eme.Execute(Request.Args, TheWorkspace);
                if (res == null)
                {
                    Dev2Logger.Error($"Null result return from internal service:{ServiceAction.Name}",
                        GlobalConstants.WarewolfError);
                }

                Request.ExecuteResult = res;
            }
        }

        private string GetAuthorizationErrorMessage(IEsbManagementEndpoint eme)
        {
            switch (eme.GetAuthorizationContextForService())
            {
                case AuthorizationContext.View:
                    return ErrorResource.NotAuthorizedToViewException;
                case AuthorizationContext.Execute:
                    return ErrorResource.NotAuthorizedToExecuteException;
                case AuthorizationContext.Contribute:
                    return ErrorResource.NotAuthorizedToContributeException;
                case AuthorizationContext.DeployTo:
                    return ErrorResource.NotAuthorizedToDeployToException;
                case AuthorizationContext.DeployFrom:
                    return ErrorResource.NotAuthorizedToDeployFromException;
                case AuthorizationContext.Administrator:
                    return ErrorResource.NotAuthorizedToAdministratorException;
                case AuthorizationContext.None:
                case AuthorizationContext.Any:
                default:
                    return ErrorResource.NotAuthorizedToExecuteException;
            }
        }

        public override bool CanExecute(Guid resourceId, IDSFDataObject dataObject, AuthorizationContext authorizationContext)
        {
            var isAuthorized = ServerAuthorizationService.Instance.IsAuthorized(authorizationContext, resourceId.ToString());
            return isAuthorized;
        }

        private bool CanExecute(IEsbManagementEndpoint eme, IDSFDataObject dataObject, out string errorMessage)
        {
            var resourceId = eme.GetResourceID(Request.Args);
            var authorizationContext = eme.GetAuthorizationContextForService();
            var isFollower = !string.IsNullOrWhiteSpace(Config.Cluster.LeaderServerKey);
            var serviceAllowsWhenFollowing = eme.CanExecute(new CanExecuteArg{ IsFollower = isFollower });
            if (isFollower && !serviceAllowsWhenFollowing)
            {
                errorMessage = ErrorResource.NotAuthorizedToExecuteOnFollower;
                return false;
            }

            var result = CanExecute(resourceId, dataObject, authorizationContext);
            errorMessage = !result ? GetAuthorizationErrorMessage(eme) : string.Empty;
            return result;
        }

        public override IDSFDataObject Execute(IDSFDataObject inputs, IDev2Activity activity) => null;

        void GenerateRequestDictionaryFromDataObject(out ErrorResultTO errors)
        {
            errors = null;
            Request.Args = new Dictionary<string, StringBuilder>();
        }
    }
}