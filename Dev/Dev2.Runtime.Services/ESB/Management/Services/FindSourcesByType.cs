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

using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

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

            try
            {
                using var document = JsonDocument.Parse(json);
                return RemovePasswordsFromElement(document.RootElement);
            }
            catch
            {
                // Fallback to original if parsing fails
                return json;
            }
        }

        private string RemovePasswordsFromElement(JsonElement element)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false });

            WriteElementWithPasswordsRemoved(writer, element);
            writer.Flush();

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        private void WriteElementWithPasswordsRemoved(Utf8JsonWriter writer, JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    writer.WriteStartObject();
                    foreach (var property in element.EnumerateObject())
                    {
                        writer.WritePropertyName(property.Name);

                        if (property.Name.Equals("Password", StringComparison.OrdinalIgnoreCase))
                        {
                            writer.WriteStringValue("");
                        }
                        else
                        {
                            WriteElementWithPasswordsRemoved(writer, property.Value);
                        }
                    }
                    writer.WriteEndObject();
                    break;

                case JsonValueKind.Array:
                    writer.WriteStartArray();
                    foreach (var item in element.EnumerateArray())
                    {
                        WriteElementWithPasswordsRemoved(writer, item);
                    }
                    writer.WriteEndArray();
                    break;

                default:
                    element.WriteTo(writer);
                    break;
            }
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Type ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FindSourcesByType";
    }
}
