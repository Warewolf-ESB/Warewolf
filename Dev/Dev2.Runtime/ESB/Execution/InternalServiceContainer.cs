#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Execution
{
    /// <summary>
    /// Execute an internal or management service
    /// </summary>
    public class InternalServiceContainer : EsbExecutionContainer
    {
        readonly IEsbManagementServiceLocator _managementServiceLocator;

        public InternalServiceContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, EsbExecuteRequest request)
            : this(sa, dataObj, theWorkspace, esbChannel, request, null)
        {
        }

        public InternalServiceContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, EsbExecuteRequest request, IEsbManagementServiceLocator managementServiceLocator)
            : base(sa, dataObj, theWorkspace, esbChannel, request)
        {
            
            if(request.Args == null)
            {
                if (sa.DataListSpecification == null)
                {
                    sa.DataListSpecification = new StringBuilder("<DataList></DataList>");
                }
                var dataListTo = new DataListTO(sa.DataListSpecification.ToString());
                request.Args = new Dictionary<string, StringBuilder>();
                foreach(var input in dataListTo.Inputs)
                {
                    var warewolfEvalResult = dataObj.Environment.Eval(DataListUtil.AddBracketsToValueIfNotExist(input),0);
                    if (warewolfEvalResult.IsWarewolfAtomResult && warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult scalarResult && !scalarResult.Item.IsNothing)
                    {
                        request.Args.Add(input, new StringBuilder(scalarResult.Item.ToString()));
                    }

                }
            }

            _managementServiceLocator = managementServiceLocator ?? new EsbManagementServiceLocator();
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
                    if(Request.Args == null)
                    {
                        GenerateRequestDictionaryFromDataObject(out invokeErrors);
                        errors.MergeErrors(invokeErrors);
                    }
                    if (CanExecute(eme))
                    {
                        Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.ServerUser,()=>
                        {
                            ExecuteService(eme);
                            result = DataObject.DataListID;
                        });
                        errors.MergeErrors(invokeErrors);
                    }
                    else
                    {
                        SetMessage(errors, eme);
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

        private void ExecuteService(IEsbManagementEndpoint eme)
        {
            var res = eme.Execute(Request.Args, TheWorkspace);
            if (res == null)
            {
                Dev2Logger.Error($"Null result return from internal service:{ServiceAction.Name}", GlobalConstants.WarewolfError);
            }
            Request.ExecuteResult = res;
        }

        private void SetMessage(ErrorResultTO errors, IEsbManagementEndpoint eme)
        {
            var serializer = new Dev2JsonSerializer();
            var msg = new ExecuteMessage { HasError = true };
            switch (eme.GetAuthorizationContextForService())
            {
                case AuthorizationContext.View:
                    msg.SetMessage(ErrorResource.NotAuthorizedToViewException);
                    break;
                case AuthorizationContext.Execute:
                    msg.SetMessage(ErrorResource.NotAuthorizedToExecuteException);
                    break;
                case AuthorizationContext.Contribute:
                    msg.SetMessage(ErrorResource.NotAuthorizedToContributeException);
                    break;
                case AuthorizationContext.DeployTo:
                    msg.SetMessage(ErrorResource.NotAuthorizedToDeployToException);
                    break;
                case AuthorizationContext.DeployFrom:
                    msg.SetMessage(ErrorResource.NotAuthorizedToDeployFromException);
                    break;
                case AuthorizationContext.Administrator:
                    msg.SetMessage(ErrorResource.NotAuthorizedToAdministratorException);
                    break;
                default:
                    Request.ExecuteResult = serializer.SerializeToBuilder(msg);
                    errors.AddError(ErrorResource.NotAuthorizedToExecuteException);
                    break;
            }
        }

        bool CanExecute(IEsbManagementEndpoint eme) => CanExecute(eme.GetResourceID(Request.Args), DataObject, eme.GetAuthorizationContextForService());

        public override bool CanExecute(Guid resourceId, IDSFDataObject dataObject, AuthorizationContext authorizationContext)
        {
            var isAuthorized = ServerAuthorizationService.Instance.IsAuthorized(authorizationContext, resourceId);
            return isAuthorized;
        }

        public override IDSFDataObject Execute(IDSFDataObject inputs, IDev2Activity activity) => null;

        void GenerateRequestDictionaryFromDataObject(out ErrorResultTO errors)
        {
            errors = null;
            Request.Args = new Dictionary<string, StringBuilder>();
        }
    }
}
