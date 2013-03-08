using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Dev2.DynamicServices;
using System.DirectoryServices.AccountManagement;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Check a user's credentials
    /// </summary>
    class CheckCredentials : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {

            IDynamicServicesHost theHost = theWorkspace.Host;

            string domain;
            string username;
            string password;

            values.TryGetValue("Domain", out domain);
            values.TryGetValue("Username", out username);
            values.TryGetValue("Password", out password);

            if(string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new InvalidDataContractException("Domain or Username or Password is missing");
            }

            StringBuilder result = new StringBuilder();

            bool isValid = false;
            try
            {
                if (domain.Equals("."))
                    domain = Environment.UserDomainName;
                using (var context = new PrincipalContext(ContextType.Domain, domain))
                {
                    isValid = context.ValidateCredentials(username, password);

                    context.Dispose();
                }
                if (isValid)
                {
                    result.Append("<result>Connection successful!</result>");
                }
                else
                {
                    result.Append("<result>Connection failed. Ensure your username and password are valid</result>");
                }
            }
            catch
            {
                result.Append("<result>Connection failed. Ensure your username and password are valid</result>");
            }

            return result.ToString();
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService checkCredentialsService = new DynamicService();
            checkCredentialsService.Name = HandlesType();
            checkCredentialsService.DataListSpecification = "<root><Domain/><Username/><Password/></root>";

            ServiceAction checkCredentialsServiceAction = new ServiceAction();
            checkCredentialsServiceAction.Name = HandlesType();
            checkCredentialsServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            checkCredentialsServiceAction.SourceMethod = HandlesType();

            checkCredentialsService.Actions.Add(checkCredentialsServiceAction);

            return checkCredentialsService;
        }

        public string HandlesType()
        {
            return "CheckCredentialsService";
        }
    }
}
