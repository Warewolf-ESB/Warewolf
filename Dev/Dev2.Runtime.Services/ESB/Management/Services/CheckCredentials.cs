/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    ///     Check a user's credentials
    /// </summary>
    internal class CheckCredentials : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            string domain = null;
            string username = null;
            string password = null;

            StringBuilder tmp;
            values.TryGetValue("Domain", out tmp);
            if (tmp != null)
            {
                domain = tmp.ToString();
            }

            values.TryGetValue("Username", out tmp);

            if (tmp != null)
            {
                username = tmp.ToString();
            }

            values.TryGetValue("Password", out tmp);

            if (tmp != null)
            {
                password = tmp.ToString();
            }


            if (string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new InvalidDataContractException("Domain or Username or Password is missing");
            }

            var result = new ExecuteMessage {HasError = false};

            try
            {
                if (domain.Equals("."))
                {
                    domain = Environment.UserDomainName;
                }
                bool isValid;
                using (var context = new PrincipalContext(ContextType.Domain, domain))
                {
                    isValid = context.ValidateCredentials(username, password);

                    context.Dispose();
                }
                result.SetMessage(isValid
                    ? "<result>Connection successful!</result>"
                    : "<result>Connection failed. Ensure your username and password are valid</result>");
            }
            catch
            {
                result.SetMessage("<result>Connection failed. Ensure your username and password are valid</result>");
            }

            var serializer = new Dev2JsonSerializer();

            return serializer.SerializeToBuilder(result);
        }

        public DynamicService CreateServiceEntry()
        {
            var checkCredentialsService = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification =
                    new StringBuilder(
                        "<DataList><Domain ColumnIODirection=\"Input\"/><Username ColumnIODirection=\"Input\"/><Password ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
            };

            var checkCredentialsServiceAction = new ServiceAction
            {
                Name = HandlesType(),
                ActionType = enActionType.InvokeManagementDynamicService,
                SourceMethod = HandlesType()
            };

            checkCredentialsService.Actions.Add(checkCredentialsServiceAction);

            return checkCredentialsService;
        }

        public string HandlesType()
        {
            return "CheckCredentialsService";
        }
    }
}