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
using System.Text.RegularExpressions;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FindSourcesByType : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                string type = null;
                bool removePassword = false;
                values.TryGetValue("Type", out StringBuilder tmp);
                if (tmp != null)
                {
                    type = tmp.ToString();
                }

                if (string.IsNullOrEmpty(type))
                {
                    throw new ArgumentNullException("type");
                }

                values.TryGetValue("RemovePassword", out StringBuilder tmp2);
                if (tmp2 != null)
                {
                    bool.TryParse(tmp2.ToString(), out removePassword);
                }

                Dev2Logger.Info("Find Sources By Type. " + type, GlobalConstants.WarewolfInfo);
                if (Enum.TryParse(type, true, out enSourceType sourceType))
                {
                    var result = ResourceCatalog.Instance.GetModels(theWorkspace.ID, sourceType);
                    if (result != null)
                    {
                        var serializer = new Dev2JsonSerializer();
                        var serializedResult = serializer.SerializeToBuilder(result);

                        if (removePassword)
                        {
                            // Remove password values from the serialized JSON
                            var sanitized = RemovePasswordsFromJson(serializedResult.ToString());
                            return new StringBuilder(sanitized);
                        }

                        return serializedResult;
                    }
                }
                return new StringBuilder();
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                throw;
            }
        }

        private string RemovePasswordsFromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return json;
            }

            // Replace password field values with empty string
            // Pattern matches: "Password":"any value" and replaces with "Password":""
            // This handles escaped quotes and various characters in password values
            var pattern = @"""Password""\s*:\s*""[^""]*""";
            var replacement = @"""Password"":""""";
            
            return Regex.Replace(json, pattern, replacement, RegexOptions.IgnoreCase);
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Type ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FindSourcesByType";
    }
}
