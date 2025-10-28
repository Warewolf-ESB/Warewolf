using Dev2.Common.X6;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Dev2.WorkflowConverters
{
    public class CommonHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GenerateNodeId() => Guid.NewGuid().ToString();

        public static Cell CreateEdge(string sourceId, string targetId, string label = "")
        {
            return new Cell
            {
                id = GenerateNodeId(),
                Source = new Connector(sourceId),
                Target = new Connector(targetId),
                label = label,
                data = new Dictionary<string, object>
                {
                    [Constants.TYPE] = Constants.SEQUENCE
                }
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

    }

    public static class CommonHelperExtensions
    {
        public static bool TryGetBool(this Dictionary<string, object> data, string key, out bool value)
        {
            return CommonHelper.TryGetBool(data, key, out value);
        }

        public static bool TryGetString(this Dictionary<string, object> data, string key, out string value)
        {
            return CommonHelper.TryGetString(data, key, out value); 
        }

        public static bool TryGetInt(this Dictionary<string, object> data, string key, out int value)
        {
            return CommonHelper.TryGetInt(data, key, out value);
        }

        public static bool TryGetGuid(this Dictionary<string, object> data, string key, out Guid value)
        {
            return CommonHelper.TryGetGuid(data, key, out value);
        }
    }
}
