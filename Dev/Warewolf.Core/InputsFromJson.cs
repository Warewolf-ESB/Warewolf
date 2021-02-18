/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.DB;
using Dev2.Communication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Warewolf.Core
{
    public static class InputsFromJson
    {
        private static JObject GetEnvironmentJObject(string serializedEnv)
        {
            var serializer = new Dev2JsonSerializer();
            JObject jsonEnv = null;
            try
            {
                jsonEnv = serializer.Deserialize<JObject>(serializedEnv);
            }
            catch (JsonReaderException jre)
            {
                Dev2Logger.Error($"Error Deserializing: {serializedEnv}", jre, GlobalConstants.WarewolfError);
            }

            return jsonEnv;
        }
        public static void FromJson(string serialized, List<IServiceInput> inputs)
        {
            if (string.IsNullOrEmpty(serialized))
            {
                return;
            }

            var jsonEnv = GetEnvironmentJObject(serialized);
            if (jsonEnv is null)
            {
                return;
            }

            var env = (JObject)jsonEnv.Property("Environment")?.Value;
            var jsonScalars = env?.Property("scalars")?.Value as JObject;
            var jsonRecSets = env?.Property("record_sets")?.Value as JObject;
            var jsonJObjects = env?.Property("json_objects")?.Value as JObject;

            AssignScalarData(inputs, jsonScalars);
            AssignRecSetData(inputs, jsonRecSets);
            AssignJsonData(inputs, jsonJObjects);

        }

        private static void AssignScalarData(ICollection<IServiceInput> inputs, JObject jsonScalars)
        {
            if (jsonScalars is null)
            {
                return;
            }

            foreach (var scalarObj in jsonScalars.Properties())
            {
                inputs.Add(new ServiceInput($"{scalarObj.Name}", (string)scalarObj.Value));
            }
        }

        private static void AssignRecSetData(ICollection<IServiceInput> inputs, JObject jsonRecSets)
        {
            if (jsonRecSets is null)
            {
                return;
            }

            foreach (var recSetObj in jsonRecSets.Properties())
            {
                AssignRecSetData(inputs, recSetObj);
            }
        }

        private static void AssignRecSetData(ICollection<IServiceInput> inputs, JProperty recSetObj)
        {
            void AssignRecSetDataItem(IReadOnlyList<JToken> positionItems, JProperty recSetData)
            {
                if (recSetData.Name != "WarewolfPositionColumn")
                {
                    var dataItems = (recSetData.Value as JArray).ToList();
                    int i = 0;
                    foreach (var dataValue in dataItems)
                    {
                        var index = positionItems[i].ToString();
                        inputs.Add(new ServiceInput($"{recSetObj.Name}({index}).{recSetData.Name}", dataValue.ToString()));
                        i++;
                    }
                }
            }

            if (recSetObj != null && recSetObj.Value is JObject recSetDataObj)
            {
                var positionItems = (recSetDataObj.Property("WarewolfPositionColumn").Value as JArray).ToList();
                foreach (var recSetData in recSetDataObj.Properties())
                {
                    AssignRecSetDataItem(positionItems, recSetData);
                }
            }
        }

        private static void AssignJsonData(ICollection<IServiceInput> inputs, JObject jsonJObjects)
        {
            if (jsonJObjects is null)
            {
                return;
            }

            foreach (var jsonObj in jsonJObjects.Properties())
            {
                inputs.Add(new ServiceInput($"@{jsonObj.Name}", jsonObj.Value.ToString()));
            }
        }
    }
}