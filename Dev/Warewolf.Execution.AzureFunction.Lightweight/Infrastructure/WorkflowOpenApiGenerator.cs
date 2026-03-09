using Dev2.Data;
using Dev2.Data.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Warewolf.Execution.AzureFunction.Lightweight
{
    /// <summary>
    /// Generates a minimal OpenAPI 3.0 specification for a Warewolf workflow,
    /// derived from its DataList input/output variable definitions.
    ///
    /// Mirrors the behaviour of <c>GetOpenAPIServiceHandler</c> without requiring
    /// a full Warewolf server or ResourceCatalog. The spec is built by reading the
    /// workflow XML file directly and parsing the DataList element.
    ///
    /// Supports all three Warewolf variable types:
    ///   - Scalar      → query param / response property, type: string
    ///   - Recordset   → query param / response property, type: object with field properties
    ///   - JSON object → query param / response property, type: object (IsJson="true" in DataList)
    /// </summary>
    internal static class WorkflowOpenApiGenerator
    {
        /// <summary>
        /// Generates an OpenAPI 3.0 JSON spec string for the given workflow file.
        /// </summary>
        internal static string Generate(string workflowFilePath, string workflowName, Uri requestUri)
        {
            var dataList = ReadDataList(workflowFilePath);
            return BuildSpec(workflowName, requestUri, dataList);
        }

        static string ReadDataList(string filePath)
        {
            try
            {
                var xe = XElement.Load(filePath);
                return xe.Element("DataList")?.ToString() ?? "<DataList />";
            }
            catch
            {
                return "<DataList />";
            }
        }

        static string BuildSpec(string workflowName, Uri requestUri, string dataList)
        {
            var baseUrl = $"{requestUri.Scheme}://{requestUri.Host}";

            // Full URL (including query string) used for title/description — mirrors
            // GetOpenAPIOutputForServiceList which passes webServerUrl as both values.
            var fullUrl = requestUri.ToString();

            // Path key: PathAndQuery with .api stripped — mirrors
            // pathAndQuery.Replace(".api", "") in BuildJsonOpenAPIPathObject.
            // Using PathAndQuery (not AbsolutePath) preserves the query string in the key.
            var path = requestUri.PathAndQuery.Replace(".api", "", StringComparison.OrdinalIgnoreCase);

            var spec = new JObject
            {
                { "openapi", "3.0.1" },
                {
                    "info", new JObject
                    {
                        { "title", fullUrl },
                        { "description", fullUrl },
                        { "version", "1" }
                    }
                },
                { "servers", new JArray(new JObject { { "url", baseUrl } }) },
                {
                    "paths", new JObject
                    {
                        {
                            path, new JObject
                            {
                                {
                                    "get", new JObject
                                    {
                                        { "tags", new JArray("") },
                                        { "description", "" },
                                        { "parameters", BuildParameters(dataList) },
                                        { "responses", BuildResponses(dataList) }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            return spec.ToString(Formatting.Indented);
        }

        static JArray BuildParameters(string dataList)
        {
            var parameters = new JArray();
            try
            {
                var dataListTo = new DataListTO(dataList);
                var jsonObjectNames = GetJsonObjectNames(dataList, isInput: true);

                // Scalar and JSON object inputs (both appear as plain names in DataListTO.Inputs).
                // IsJson=true variables are typed as object; plain scalars as string.
                foreach (var input in dataListTo.Inputs.Where(s => !DataListUtil.IsValueRecordset(s)))
                {
                    var schema = jsonObjectNames.Contains(input)
                        ? new JObject { { "type", "object" } }
                        : new JObject { { "type", "string" } };
                    parameters.Add(new JObject
                    {
                        { "name", input },
                        { "in", "query" },
                        { "required", true },
                        { "schema", schema }
                    });
                }

                // Recordset inputs → one query param per recordset, object schema with field properties.
                // Mirrors BuildParametersObject groupBy logic in ExecutionEnvironmentUtils.
                foreach (var group in dataListTo.Inputs.Where(DataListUtil.IsValueRecordset)
                                                       .GroupBy(DataListUtil.ExtractRecordsetNameFromValue))
                {
                    parameters.Add(new JObject
                    {
                        { "name", group.Key },
                        { "in", "query" },
                        { "required", true },
                        { "schema", BuildRecordsetSchema(group) }
                    });
                }
            }
            catch { }
            return parameters;
        }

        static JObject BuildResponses(string dataList)
        {
            var properties = new JObject();
            try
            {
                var dataListTo = new DataListTO(dataList);
                var jsonObjectNames = GetJsonObjectNames(dataList, isInput: false);

                // Scalar and JSON object outputs.
                // IsJson=true variables are typed as object; plain scalars as string.
                foreach (var output in dataListTo.Outputs.Where(s => !DataListUtil.IsValueRecordset(s)))
                {
                    properties[output] = jsonObjectNames.Contains(output)
                        ? new JObject { { "type", "object" } }
                        : new JObject { { "type", "string" } };
                }

                // Recordset outputs → object property with field sub-properties.
                // Mirrors BuildResponseSchema groupBy logic in ExecutionEnvironmentUtils.
                foreach (var group in dataListTo.Outputs.Where(DataListUtil.IsValueRecordset)
                                                        .GroupBy(DataListUtil.ExtractRecordsetNameFromValue))
                {
                    properties[group.Key] = BuildRecordsetSchema(group);
                }
            }
            catch { }

            return new JObject
            {
                {
                    "200", new JObject
                    {
                        { "description", "Success" },
                        {
                            "content", new JObject
                            {
                                {
                                    "application/json", new JObject
                                    {
                                        {
                                            "schema", new JObject
                                            {
                                                { "type", "object" },
                                                { "properties", properties }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Builds an OpenAPI object schema for a recordset group.
        /// Mirrors BuildPropertyDefinition in ExecutionEnvironmentUtils:
        /// each field in the group becomes a string property keyed by its field name only.
        /// </summary>
        static JObject BuildRecordsetSchema(IGrouping<string, string> group)
        {
            var fieldProperties = new JObject();
            foreach (var field in group)
            {
                var fieldName = DataListUtil.ExtractFieldNameOnlyFromValue(field);
                if (!string.IsNullOrWhiteSpace(fieldName))
                {
                    fieldProperties[fieldName] = new JObject { { "type", "string" } };
                }
            }
            return new JObject
            {
                { "type", "object" },
                { "properties", fieldProperties }
            };
        }

        /// <summary>
        /// Parses the raw DataList XML to find elements marked with IsJson="true".
        /// DataListTO exposes these as plain scalar names, indistinguishable from regular
        /// scalars without inspecting the source XML. This method identifies them so the
        /// spec can emit type: object instead of type: string.
        /// </summary>
        static HashSet<string> GetJsonObjectNames(string dataList, bool isInput)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var root = XElement.Parse(dataList);
                foreach (var el in root.Elements())
                {
                    if (el.Attribute("IsJson")?.Value.Equals("true", StringComparison.OrdinalIgnoreCase) != true)
                    {
                        continue;
                    }
                    var ioDir = el.Attribute("ColumnIODirection")?.Value ?? string.Empty;
                    var matches = isInput
                        ? ioDir == "Input" || ioDir == "Both"
                        : ioDir == "Output" || ioDir == "Both";
                    if (matches)
                    {
                        names.Add(el.Name.LocalName);
                    }
                }
            }
            catch { }
            return names;
        }
    }
}
