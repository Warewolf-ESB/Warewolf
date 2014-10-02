
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
using System.IO;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Security;
using Dev2.Services.Security;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Checks a users permissions on the local file system
    /// </summary>
    public class SecurityWrite : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            if(values == null)
            {
                throw new InvalidDataException("Empty values passed.");
            }

            StringBuilder securitySettings;
            values.TryGetValue("SecuritySettings", out securitySettings);
            StringBuilder timeoutPeriodString;
            values.TryGetValue("TimeoutPeriod", out timeoutPeriodString);

            if(securitySettings == null || securitySettings.Length == 0)
            {
                throw new InvalidDataException("Empty Security Settings passed.");
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();

            try
            {
                var securitySettingsTO = serializer.Deserialize<SecuritySettingsTO>(securitySettings);
                if(securitySettingsTO == null)
                {
                    throw new InvalidDataException("The security settings are not valid.");
                }

                Write(securitySettings);
                ServerAuthorizationService.Instance.SecurityService.Read();
            }
            catch(Exception e)
            {
                throw new InvalidDataException(string.Format("The security settings are not valid. Error: {0}", e.Message));
            }

            ExecuteMessage msg = new ExecuteMessage { HasError = false };
            msg.SetMessage("Success");

            return serializer.SerializeToBuilder(msg);
        }

        public static void Write(SecuritySettingsTO securitySettingsTO)
        {
            VerifyArgument.IsNotNull("securitySettingsTO", securitySettingsTO);
            var securitySettings = new Dev2JsonSerializer().SerializeToBuilder(securitySettingsTO);
            Write(securitySettings);
        }

        static void Write(StringBuilder securitySettings)
        {
            try
            {
                DoFileEncryption(securitySettings.ToString());

                // Deny ACL was causing "Access to the path is denied." errors 
                // so Barney decided it was OK not to do it.
            }
            catch(Exception e)
            {
                throw new InvalidDataException(string.Format("The permissions passed is not a valid list of permissions. Error: {0}", e.Message));
            }
        }

        static void DoFileEncryption(string permissions)
        {
            var byteConverter = new ASCIIEncoding();
            var encryptedData = SecurityEncryption.Encrypt(permissions);
            byte[] dataToEncrypt = byteConverter.GetBytes(encryptedData);

            using(var outStream = new FileStream(ServerSecurityService.FileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
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
                DataListSpecification = "<DataList><SecuritySettings ColumnIODirection=\"Input\"></SecuritySettings><Result/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
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
