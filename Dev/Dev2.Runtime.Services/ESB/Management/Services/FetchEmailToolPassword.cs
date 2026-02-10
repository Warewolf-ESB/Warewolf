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
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Warewolf.Security.Encryption;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Fetches the decrypted password for an SMTP email tool activity within a workflow.
    /// This service is called over HTTPS to securely transmit the password to the client
    /// when the user opens the email tool's large (edit) view.
    /// </summary>
    public class FetchEmailToolPassword : DefaultEsbManagementEndpoint
    {
        // Matches a DsfSendEmailActivity element with UniqueID and Password attributes (in any order)
        static readonly Regex SendEmailWithUniqueIdAndPassword = new Regex(
            @"<([a-zA-Z0-9]+:)?DsfSendEmailActivity\s[^>]*?UniqueID=""(?<uid>[^""]+)""[^>]*?Password=""(?<pwd>[^""]+)""[^>]*?>",
            RegexOptions.Singleline | RegexOptions.Compiled);

        // Alternative ordering: Password before UniqueID
        static readonly Regex SendEmailWithPasswordAndUniqueId = new Regex(
            @"<([a-zA-Z0-9]+:)?DsfSendEmailActivity\s[^>]*?Password=""(?<pwd>[^""]+)""[^>]*?UniqueID=""(?<uid>[^""]+)""[^>]*?>",
            RegexOptions.Singleline | RegexOptions.Compiled);

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            var result = new ExecuteMessage { HasError = false };

            try
            {
                values.TryGetValue("ResourceID", out StringBuilder resourceIdStr);
                values.TryGetValue("ActivityID", out StringBuilder activityIdStr);

                if (resourceIdStr == null || activityIdStr == null)
                {
                    result.HasError = true;
                    result.SetMessage("ResourceID and ActivityID are required");
                    return serializer.SerializeToBuilder(result);
                }

                if (!Guid.TryParse(resourceIdStr.ToString(), out Guid resourceId))
                {
                    result.HasError = true;
                    result.SetMessage("Invalid ResourceID");
                    return serializer.SerializeToBuilder(result);
                }

                var activityId = activityIdStr.ToString().Trim();

                Dev2Logger.Info($"FetchEmailToolPassword for ResourceId: {resourceId}, ActivityID: {activityId}", GlobalConstants.WarewolfInfo);

                var resourceContents = ResourceCatalog.Instance.GetResourceContents(theWorkspace.ID, resourceId);
                if (resourceContents == null || resourceContents.Length == 0)
                {
                    result.HasError = true;
                    result.SetMessage("Resource not found");
                    return serializer.SerializeToBuilder(result);
                }

                var xaml = resourceContents.ToString();
                var password = ExtractPasswordForActivity(xaml, activityId);

                if (password == null)
                {
                    // Activity not found or has no password - return empty string (not an error)
                    result.SetMessage("");
                }
                else
                {
                    var decryptedPassword = DpapiWrapper.DecryptIfEncrypted(password);
                    result.SetMessage(decryptedPassword);
                }
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                result.HasError = true;
                result.SetMessage("Failed to fetch email tool password: " + err.Message);
            }

            return serializer.SerializeToBuilder(result);
        }

        /// <summary>
        /// Extracts the encrypted password attribute from a DsfSendEmailActivity element
        /// matching the given UniqueID in the workflow XAML.
        /// </summary>
        static string ExtractPasswordForActivity(string xaml, string activityId)
        {
            // Try both attribute orderings (UniqueID before Password, and vice versa)
            foreach (var regex in new[] { SendEmailWithUniqueIdAndPassword, SendEmailWithPasswordAndUniqueId })
            {
                var matches = regex.Matches(xaml);
                foreach (Match match in matches)
                {
                    if (match.Groups["uid"].Value.Equals(activityId, StringComparison.OrdinalIgnoreCase))
                    {
                        return match.Groups["pwd"].Value;
                    }
                }
            }

            return null;
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(
            HandlesType(),
            "<DataList><ResourceID ColumnIODirection=\"Input\"/><ActivityID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FetchEmailToolPasswordService";
    }
}
