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
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Security;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Checks a users permissions on the local file system
    /// </summary>
    public class SecurityWrite : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            if(values == null)
            {
                throw new InvalidDataException(ErrorResource.EmptyValuesPassed);
            }

            StringBuilder securitySettings;
            values.TryGetValue("SecuritySettings", out securitySettings);
            StringBuilder timeoutPeriodString;
            values.TryGetValue("TimeoutPeriod", out timeoutPeriodString);

            if(securitySettings == null || securitySettings.Length == 0)
            {
                throw new InvalidDataException(ErrorResource.EmptySecuritySettingsPassed);
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();

            try
            {
                var securitySettingsTo = serializer.Deserialize<SecuritySettingsTO>(securitySettings);
                if(securitySettingsTo == null)
                {
                    throw new InvalidDataException(ErrorResource.InvalidSecuritySettings);
                }

                Write(securitySettings);
                ServerAuthorizationService.Instance.SecurityService.Read();
            }
            catch(Exception e)
            {
                throw new InvalidDataException(ErrorResource.InvalidSecuritySettings + $" Error: {e.Message}");
            }

            ExecuteMessage msg = new ExecuteMessage { HasError = false };
            msg.SetMessage("Success");

            return serializer.SerializeToBuilder(msg);
        }

        public static void Write(SecuritySettingsTO securitySettingsTo)
        {
            VerifyArgument.IsNotNull("securitySettingsTO", securitySettingsTo);
            var securitySettings = new Dev2JsonSerializer().SerializeToBuilder(securitySettingsTo);
            Write(securitySettings);
        }

        static void Write(StringBuilder securitySettings)
        {
            try
            {
                DoFileEncryption(securitySettings.ToString());
            }
            catch(Exception e)
            {
                throw new InvalidDataException(string.Format(ErrorResource.PermissionsPassedNotValid, e.Message));
            }
        }

        static void DoFileEncryption(string permissions)
        {
            var byteConverter = new ASCIIEncoding();
            var encryptedData = SecurityEncryption.Encrypt(permissions);
            byte[] dataToEncrypt = byteConverter.GetBytes(encryptedData);
            using(var outStream = new FileStream(EnvironmentVariables.ServerSecuritySettingsFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            {
                outStream.SetLength(0);
                outStream.Write(dataToEncrypt, 0, dataToEncrypt.Length);
                outStream.Flush();
            }
        }

        public DynamicService CreateServiceEntry()
        {
            var dynamicService = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = new StringBuilder("<DataList><SecuritySettings ColumnIODirection=\"Input\"></SecuritySettings><Result/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
            };

            var serviceAction = new ServiceAction
            {
                Name = HandlesType(),
                ActionType = enActionType.InvokeManagementDynamicService,
                SourceMethod = HandlesType()
            };

            dynamicService.Actions.Add(serviceAction);

            return dynamicService;
        }

        public string HandlesType()
        {
            return "SecurityWriteService";
        }
    }
}
