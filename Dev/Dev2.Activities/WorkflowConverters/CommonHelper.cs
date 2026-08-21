using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.X6;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.TO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Framework.Converters.Graph.Ouput;
using Unlimited.Framework.Converters.Graph.String.Json;
using Warewolf.Core;
using Warewolf.Data.Options;
using Warewolf.Options;

namespace Dev2.WorkflowConverters
{
    public class CommonHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GenerateNodeId() => Guid.NewGuid().ToString();

        public static Cell CreateEdge(string sourceId, string targetId, string label = "", bool isDecisionArm = false, bool isTrueArm = false)
        {
            var data = new Dictionary<string, object>
            {
                [Constants.TYPE] = Constants.SEQUENCE
            };

            // FlowDecision.True/False branches are round-tripped by X6ToWorkflowConverter's
            // HandleDecisionConnection, which reads these two data flags back off the edge
            // (see Constants.ISDECISIONARM/ISTRUEARM). Without them the reconstructed
            // FlowDecision has null True/False branches, silently dropping all conditional
            // branching on round-trip.
            if (isDecisionArm)
            {
                data[Constants.ISDECISIONARM] = true;
                data[Constants.ISTRUEARM] = isTrueArm;
            }

            return new Cell
            {
                id = GenerateNodeId(),
                Source = new Connector(sourceId),
                Target = new Connector(targetId),
                label = label,
                data = data
            };
        }

        public static bool TryGetBool(Dictionary<string, object> data, string key, out bool value)
        {
            value = false;
            if (data == null || !data.TryGetValue(key, out var raw) || raw == null)
                return false;

            return raw switch
            {
                bool b => (value = b) == b,
                string s when bool.TryParse(s, out var parsed) => (value = parsed) == parsed,
                _ => false
            };
        }

        public static bool TryGetString(Dictionary<string, object> data, string key, out string value)
        {
            value = string.Empty;
            if (data == null || !data.TryGetValue(key, out var raw) || raw == null)
                return false;

            switch (raw)
            {
                case string s:
                    value = s;
                    return true;
                default:
                    value = raw.ToString();
                    return !string.IsNullOrEmpty(value);
            }
        }

        /// <summary>
        /// Converts an already-read <c>cell.data</c> value into a <see cref="JArray"/>, accepting
        /// either a real <see cref="JArray"/> or a string containing a JSON array.
        ///
        /// <para>
        /// The string form is supported because <c>get_tool_schema</c> documented every collection
        /// field as "a JSON-encoded array of ...". Callers following that documentation sent a
        /// string, the previous <c>value as JArray</c> cast produced <c>null</c> for it, and the
        /// whole collection was dropped with no error at all — the activity then executed
        /// "successfully" and returned nothing (observed for Assign on warewolfserver-mcp,
        /// 2026-08-21).
        /// </para>
        ///
        /// <para>
        /// Returns <c>false</c> — and never throws — when the value is null, is a string that is
        /// not valid JSON or not a JSON array, or is neither an array nor a string. Callers that
        /// need such input to fail loudly validate it separately (see
        /// <c>Warewolf.Execution.Lightweight</c>'s <c>validate_workflow</c>); keeping this lenient
        /// means the Studio converters still accept exactly what they accepted before.
        /// </para>
        /// </summary>
        public static bool TryAsJArray(object raw, out JArray array)
        {
            array = null;

            switch (raw)
            {
                case null:
                    return false;
                case JArray jArray:
                    array = jArray;
                    return true;
                case string s when !string.IsNullOrWhiteSpace(s):
                    try
                    {
                        array = JArray.Parse(s);
                        return true;
                    }
                    catch (JsonException)
                    {
                        return false;
                    }
                default:
                    return false;
            }
        }

        public static bool TryGetInt(Dictionary<string, object> data, string key, out int value)
        {
            value = default;

            if (data == null || !data.TryGetValue(key, out var raw) || raw == null)
                return false;

            switch (raw)
            {
                case int i:
                    value = i;
                    return true;
                case long l:
                    value = (int)l;
                    return true;
                case string s when int.TryParse(s, out var parsed):
                    value = parsed;
                    return true;
                default:
                    return false;
            }
        }

        public static bool TryGetGuid(Dictionary<string, object> data, string key, out Guid value)
        {
            value = default;

            if (data == null || !data.TryGetValue(key, out var raw) || raw == null)
                return false;

            switch (raw)
            {
                case Guid g:
                    value = g;
                    return true;
                case string s when Guid.TryParse(s, out var parsed):
                    value = parsed;
                    return true;
                default:
                    return false;
            }
        }

        private static readonly JsonSerializerSettings OutputDescSerializerSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
        };

        public static bool TryGetOutputs(IDictionary<string, object> data, out IList<IServiceOutputMapping> outputs)
        {
            return TryGetOutputs(data, Constants.WEBMETHOD_OUTPUTS, out outputs);
        }

        public static bool TryGetOutputs(IDictionary<string, object> data, string key, out IList<IServiceOutputMapping> outputs)
        {
            outputs = null;
            if (!data.TryGetValue(key, out var raw) || raw is not JArray arr) return false;

            outputs = arr
                .Children<JObject>()
                .Select(child =>
                {
                    var mapping = new ServiceOutputMapping(
                        child.Value<string>(nameof(ServiceOutputMapping.MappedFrom)) ?? string.Empty,
                        child.Value<string>(nameof(ServiceOutputMapping.MappedTo)) ?? string.Empty,
                        child.Value<string>(nameof(ServiceOutputMapping.RecordSetName)) ?? string.Empty);

                    if (child[nameof(ObservableObject.Path)] is JObject pathObj)
                    {
                        mapping.Path = new JsonPath(
                            pathObj.Value<string>(nameof(JsonPath.ActualPath)) ?? string.Empty,
                            pathObj.Value<string>(nameof(JsonPath.DisplayPath)) ?? string.Empty,
                            pathObj.Value<string>(nameof(JsonPath.OutputExpression)) ?? string.Empty,
                            pathObj.Value<string>(nameof(JsonPath.SampleData)) ?? string.Empty);
                    }

                    return (IServiceOutputMapping)mapping;
                })
                .ToList();

            return true;
        }

        public static bool TryGetOutputDescription(IDictionary<string, object> data, out IOutputDescription outputDescription)
        {
            outputDescription = null;
            if (!data.TryGetValue(Constants.WEBMETHOD_OUTPUTDESCRIPTION, out var raw) || raw is not JObject obj) return false;

            var serializer = JsonSerializer.Create(OutputDescSerializerSettings);
            var od = new OutputDescription();

            var formatToken = obj[nameof(OutputDescription.Format)];
            if (formatToken != null && formatToken.Type != JTokenType.Null)
            {
                od.Format = formatToken.ToObject<OutputFormats>(serializer);
            }

            var dssToken = obj[nameof(OutputDescription.DataSourceShapes)];
            if (dssToken is JArray dssArray)
            {
                od.DataSourceShapes = dssArray
                    .Children<JObject>()
                    .Select(shapeToken =>
                    {
                        var shape = new DataSourceShape();
                        var pathsToken = shapeToken[nameof(DataSourceShape.Paths)];
                        if (pathsToken is JArray pathsArray)
                        {
                            shape.Paths = pathsArray
                                .Children<JObject>()
                                .Select(p =>
                                    (IPath)new JsonPath(
                                        p.Value<string>(nameof(JsonPath.ActualPath)) ?? string.Empty,
                                        p.Value<string>(nameof(JsonPath.DisplayPath)) ?? string.Empty,
                                        p.Value<string>(nameof(JsonPath.OutputExpression)) ?? string.Empty,
                                        p.Value<string>(nameof(JsonPath.SampleData)) ?? string.Empty))
                                .ToList();
                        }
                        return (IDataSourceShape)shape;
                    })
                    .ToList();
            }

            outputDescription = od;
            return true;
        }

        /// <summary>
        /// Returns the first of <paramref name="keys"/> whose value can be read as a JSON array.
        /// Delegates to <see cref="TryAsJArray"/>, so a string containing a JSON array is accepted
        /// as well as a real <see cref="JArray"/> — this is what everything built on top of it
        /// (notably <see cref="TryGetList{TConcrete,TInterface}"/>) inherits. A key whose value is
        /// unusable is skipped, exactly as a non-array value was skipped before.
        /// </summary>
        public static bool TryGetJArray(IDictionary<string, object> data, out JArray array, params string[] keys)
        {
            array = null;
            if (data == null || keys == null || keys.Length == 0) return false;

            foreach (var key in keys)
            {
                if (!data.TryGetValue(key, out var raw) || !TryAsJArray(raw, out var ja)) continue;
                array = ja;
                return true;
            }
            return false;
        }

        public static bool TryGetList<TConcrete, TInterface>(IDictionary<string, object> data, out IList<TInterface> list, params string[] keys)
            where TConcrete : class, TInterface
            where TInterface : class
        {
            list = null;
            if (!TryGetJArray(data, out var arr, keys)) return false;

            try
            {
                var concrete = arr.ToObject<List<TConcrete>>();
                list = concrete?.Cast<TInterface>().ToList();
                return list != null;
            }
            catch (Exception)
            {
                list = null;
                return false;
            }
        }

        public static bool TryGetHeaders(IDictionary<string, object> data, out IList<INameValue> headers) =>
            TryGetList<NameValue, INameValue>(data, out headers, Constants.WEBMETHOD_UPDATEDHEADERS, Constants.WEBMETHOD_HEADERS);

        public static bool TryGetInputs(IDictionary<string, object> data, out IList<Common.Interfaces.DB.IServiceInput> inputs) =>
            TryGetList<ServiceInput, Common.Interfaces.DB.IServiceInput>(data, out inputs, Constants.WEBMETHOD_INPUTS);

        public static bool TryGetSettings(IDictionary<string, object> data, out IList<INameValue> settings) =>
            TryGetList<NameValue, INameValue>(data, out settings, Constants.WEBMETHOD_SETTINGS);

        public static bool TryGetConditions(IDictionary<string, object> data, out IList<FormDataConditionExpression> conditions)
        {
            conditions = null;
            if (!data.TryGetValue(Constants.WEBMETHOD_CONDITIONS, out var raw) || raw is not JArray arr) return false;

            conditions = arr
                .Children<JObject>()
                .Select(child =>
                {
                    var key = child.Value<string>(nameof(FormDataConditionExpression.Key)) ?? string.Empty;

                    IFormDataCondition formDataCond = null;
                    if (child[nameof(FormDataConditionExpression.Cond)] is JObject condObj)
                    {
                        // TableType (enum may come as int or string)
                        var tableType = enFormDataTableType.Text;
                        var tableTypeToken = condObj[nameof(FormDataCondition.TableType)];
                        if (tableTypeToken != null)
                        {
                            if (tableTypeToken.Type == JTokenType.Integer)
                            {
                                tableType = (enFormDataTableType)tableTypeToken.Value<int>();
                            }
                            else if (tableTypeToken.Type == JTokenType.String)
                            {
                                Enum.TryParse(tableTypeToken.Value<string>(), true, out tableType);
                            }
                        }

                        var valueToken = condObj[nameof(FormDataConditionText.Value)];
                        var fileBase64Token = condObj[nameof(FormDataConditionFile.FileBase64)];
                        var fileNameToken = condObj[nameof(FormDataConditionFile.FileName)];

                        // Decide concrete condition
                        if (tableType == enFormDataTableType.File)
                        {
                            formDataCond = new FormDataConditionFile
                            {
                                TableType = enFormDataTableType.File,
                                FileBase64 = fileBase64Token?.Value<string>() ?? string.Empty,
                                FileName = fileNameToken?.Value<string>() ?? string.Empty
                            };
                        }
                        else
                        {
                            formDataCond = new FormDataConditionText
                            {
                                TableType = enFormDataTableType.Text,
                                Value = valueToken?.Value<string>() ?? string.Empty
                            };
                        }
                    }

                    return new FormDataConditionExpression
                    {
                        Key = key,
                        Cond = formDataCond
                    };
                })
                .ToList();

            return true;
        }

        public static bool TryGetInputMappings(IDictionary<string, object> data, out IList<DataColumnMapping> inputMappings)
        {
            inputMappings = null;
            if (!data.TryGetValue(Constants.SQLBULKINSERT_INPUTMAPPINGS, out var raw)) return false;

            try
            {
                if (raw is JArray arr)
                {
                    var mappings = arr
                        .Children<JObject>()
                        .Select(child =>
                        {
                            var mapping = new DataColumnMapping
                            {
                                InputColumn = child.Value<string>(nameof(DataColumnMapping.InputColumn)) ?? string.Empty,
                                IndexNumber = child.Value<int?>(nameof(DataColumnMapping.IndexNumber)) ?? 0,
                                Inserted = child.Value<bool?>(nameof(DataColumnMapping.Inserted)) ?? false
                            };

                            if (child[nameof(DataColumnMapping.OutputColumn)] is JObject outputColObj)
                            {
                                mapping.OutputColumn = new DbColumn
                                {
                                    ColumnName = outputColObj.Value<string>(nameof(DbColumn.ColumnName)) ?? string.Empty,
                                    MaxLength = outputColObj.Value<int?>(nameof(DbColumn.MaxLength)) ?? 0,
                                    IsNullable = outputColObj.Value<bool?>(nameof(DbColumn.IsNullable)) ?? false,
                                    IsAutoIncrement = outputColObj.Value<bool?>(nameof(DbColumn.IsAutoIncrement)) ?? false
                                };

                                // Handle SqlDataType enum
                                var sqlDataTypeToken = outputColObj[nameof(DbColumn.SqlDataType)];
                                if (sqlDataTypeToken != null)
                                {
                                    if (sqlDataTypeToken.Type == JTokenType.Integer)
                                    {
                                        mapping.OutputColumn.SqlDataType = (System.Data.SqlDbType)sqlDataTypeToken.Value<int>();
                                    }
                                    else if (sqlDataTypeToken.Type == JTokenType.String && Enum.TryParse<System.Data.SqlDbType>(sqlDataTypeToken.Value<string>(), true, out var sqlDbType))
                                    {
                                        mapping.OutputColumn.SqlDataType = sqlDbType;
                                    }
                                }

                                // Handle DataType
                                var dataTypeToken = outputColObj[nameof(DbColumn.DataType)];
                                if (dataTypeToken != null)
                                {
                                    var dataTypeName = dataTypeToken.Value<string>();
                                    if (!string.IsNullOrEmpty(dataTypeName))
                                    {
                                        // Map common type names to .NET types
                                        mapping.OutputColumn.DataType = dataTypeName switch
                                        {
                                            "System.String" or "String" => typeof(string),
                                            "System.Int32" or "Int32" => typeof(int),
                                            "System.Int64" or "Int64" => typeof(long),
                                            "System.Decimal" or "Decimal" => typeof(decimal),
                                            "System.DateTime" or "DateTime" => typeof(DateTime),
                                            "System.Boolean" or "Boolean" => typeof(bool),
                                            "System.Byte[]" or "Byte[]" => typeof(byte[]),
                                            "System.Guid" or "Guid" => typeof(Guid),
                                            _ => Type.GetType(dataTypeName) ?? typeof(string)
                                        };
                                    }
                                }
                            }

                            return mapping;
                        })
                        .ToList();

                    inputMappings = mappings;
                    return true;
                }
                else if (raw is IList<DataColumnMapping> existingMappings)
                {
                    inputMappings = existingMappings;
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                inputMappings = null;
                return false;
            }
        }

        public static bool TryGetFindRecordsCollection(IDictionary<string, object> data, out IList<FindRecordsTO> findRecordsCollection)
        {
            findRecordsCollection = null;
            if (!data.TryGetValue(Constants.FINDRECORDS_RESULTSCOLLECTION, out var raw)) return false;

            try
            {
                if (raw is JArray arr)
                {
                    var collection = arr
                        .Children<JObject>()
                        .Select(child =>
                        {
                            var findRecord = new FindRecordsTO
                            {
                                SearchType = child.Value<string>(nameof(FindRecordsTO.SearchType)) ?? string.Empty,
                                SearchCriteria = child.Value<string>(nameof(FindRecordsTO.SearchCriteria)) ?? string.Empty,
                                From = child.Value<string>(nameof(FindRecordsTO.From)) ?? string.Empty,
                                To = child.Value<string>(nameof(FindRecordsTO.To)) ?? string.Empty,
                                IndexNumber = child.Value<int?>(nameof(FindRecordsTO.IndexNumber)) ?? 0,
                                Inserted = child.Value<bool?>(nameof(FindRecordsTO.Inserted)) ?? false,
                                IsFromFocused = child.Value<bool?>(nameof(FindRecordsTO.IsFromFocused)) ?? false,
                                IsToFocused = child.Value<bool?>(nameof(FindRecordsTO.IsToFocused)) ?? false,
                                IsSearchCriteriaEnabled = child.Value<bool?>(nameof(FindRecordsTO.IsSearchCriteriaEnabled)) ?? false,
                                IsSearchCriteriaFocused = child.Value<bool?>(nameof(FindRecordsTO.IsSearchCriteriaFocused)) ?? false,
                                IsSearchCriteriaVisible = child.Value<bool?>(nameof(FindRecordsTO.IsSearchCriteriaVisible)) ?? true,
                                IsSearchTypeFocused = child.Value<bool?>(nameof(FindRecordsTO.IsSearchTypeFocused)) ?? false
                            };

                            // Deserialize WhereOptionList if present
                            var whereOptionListToken = child[nameof(FindRecordsTO.WhereOptionList)];
                            if (whereOptionListToken is JArray whereOptionArray)
                            {
                                findRecord.WhereOptionList = whereOptionArray.ToObject<List<string>>();
                            }

                            return findRecord;
                        })
                        .ToList();

                    findRecordsCollection = collection;
                    return true;
                }
                else if (raw is IList<FindRecordsTO> existingCollection)
                {
                    findRecordsCollection = existingCollection;
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                findRecordsCollection = null;
                return false;
            }
        }

        public static bool TryGetRabbitMqPublishOptions(IDictionary<string, object> data, out RabbitMqPublishOptions basicProperties)
        {
            basicProperties = null;
            if (!data.TryGetValue(Constants.RABBITMQPUBLISH_BASICPROPERTIES, out var raw)) return false;

            try
            {
                if (raw is JObject obj)
                {
                    basicProperties = new RabbitMqPublishOptions();

                    // Deserialize AutoCorrelation
                    var autoCorrelationToken = obj[nameof(RabbitMqPublishOptions.AutoCorrelation)];
                    if (autoCorrelationToken is JObject autoCorrelationObj)
                    {
                        var correlationTypeToken = autoCorrelationObj[nameof(AutoCorrelation.Correlation)];
                        if (correlationTypeToken != null)
                        {
                            var correlationAction = CorrelationAction.ExecutionID;

                            if (correlationTypeToken.Type == JTokenType.Integer)
                            {
                                correlationAction = (CorrelationAction)correlationTypeToken.Value<int>();
                            }
                            else if (correlationTypeToken.Type == JTokenType.String)
                            {
                                Enum.TryParse(correlationTypeToken.Value<string>(), true, out correlationAction);
                            }

                            switch (correlationAction)
                            {
                                case CorrelationAction.Manual:
                                    var manual = new Manual();
                                    var correlationIdToken = autoCorrelationObj[nameof(Manual.CorrelationID)];
                                    if (correlationIdToken != null)
                                    {
                                        manual.CorrelationID = correlationIdToken.Value<string>() ?? string.Empty;
                                    }
                                    basicProperties.AutoCorrelation = manual;
                                    break;
                                case CorrelationAction.CustomTransactionID:
                                    basicProperties.AutoCorrelation = new CustomTransactionID();
                                    break;
                                case CorrelationAction.ExecutionID:
                                default:
                                    basicProperties.AutoCorrelation = new ExecutionID();
                                    break;
                            }
                        }
                    }

                    return true;
                }
                else if (raw is RabbitMqPublishOptions existing)
                {
                    basicProperties = existing;
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                basicProperties = null;
                return false;
            }
        }
    }

    public static class CommonHelperExtensions
    {
        public static bool TryGetBool(this Dictionary<string, object> data, string key, out bool value) =>
            CommonHelper.TryGetBool(data, key, out value);

        public static bool TryGetString(this Dictionary<string, object> data, string key, out string value) =>
            CommonHelper.TryGetString(data, key, out value);

        public static bool TryGetInt(this Dictionary<string, object> data, string key, out int value) =>
            CommonHelper.TryGetInt(data, key, out value);

        public static bool TryGetGuid(this Dictionary<string, object> data, string key, out Guid value) =>
            CommonHelper.TryGetGuid(data, key, out value);

        public static bool TryGetList<TConcrete, TInterface>(this Dictionary<string, object> data, string key, out IList<TInterface> list)
            where TConcrete : class, TInterface
            where TInterface : class =>
            CommonHelper.TryGetList<TConcrete, TInterface>(data, out list, key);

        public static bool TryGetOutputs(this IDictionary<string, object> data, out IList<IServiceOutputMapping> outputs) =>
            CommonHelper.TryGetOutputs(data, out outputs);

        public static bool TryGetOutputDescription(this IDictionary<string, object> data, out IOutputDescription outputDescription) =>
            CommonHelper.TryGetOutputDescription(data, out outputDescription);

        public static bool TryGetHeaders(this IDictionary<string, object> data, out IList<INameValue> headers) =>
            CommonHelper.TryGetHeaders(data, out headers);

        public static bool TryGetInputs(this IDictionary<string, object> data, out IList<Dev2.Common.Interfaces.DB.IServiceInput> inputs) =>
            CommonHelper.TryGetInputs(data, out inputs);

        public static bool TryGetSettings(this IDictionary<string, object> data, out IList<INameValue> settings) =>
            CommonHelper.TryGetSettings(data, out settings);

        public static bool TryGetConditions(this IDictionary<string, object> data, out IList<FormDataConditionExpression> conditions) =>
            CommonHelper.TryGetConditions(data, out conditions);

        public static bool TryGetInputMappings(this IDictionary<string, object> data, out IList<DataColumnMapping> inputMappings) =>
            CommonHelper.TryGetInputMappings(data, out inputMappings);

        public static bool TryGetFindRecordsCollection(this IDictionary<string, object> data, out IList<FindRecordsTO> findRecordsCollection) =>
            CommonHelper.TryGetFindRecordsCollection(data, out findRecordsCollection);

        public static bool TryGetRabbitMqPublishOptions(this IDictionary<string, object> data, out RabbitMqPublishOptions basicProperties) =>
            CommonHelper.TryGetRabbitMqPublishOptions(data, out basicProperties);
    }
}
