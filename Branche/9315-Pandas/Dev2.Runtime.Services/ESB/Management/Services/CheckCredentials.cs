using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Runtime.Serialization;
using System.Text;
using Dev2.DynamicServices;
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

            var result = new StringBuilder();

            try
            {
                if(domain.Equals("."))
                {
                    domain = Environment.UserDomainName;
                }
                bool isValid;
                using(var context = new PrincipalContext(ContextType.Domain, domain))
                {
                    isValid = context.ValidateCredentials(username, password);

                    context.Dispose();
                }
                result.Append(isValid ? "<result>Connection successful!</result>" : "<result>Connection failed. Ensure your username and password are valid</result>");
            }
            catch
            {
                result.Append("<result>Connection failed. Ensure your username and password are valid</result>");
            }

            return result.ToString();
        }

        public DynamicService CreateServiceEntry()
        {
            var checkCredentialsService = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = "<DataList><Domain/><Username/><Password/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
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
