
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Collections
{
    public class ConcurrentDictionarySafe<TKey, TValue> : ConcurrentDictionary<TKey, TValue>
    {
        // This property causes null reference exception during JSON deserialization 
        public new TValue this[TKey key]
        {
            get
            {
                TValue result;
                TryGetValue(key, out result);
                return result;
            }
            set { base[key] = value; }
        }
    }

    // Use the following attribute in consuming classes to prevent null reference exceptions during JSON deserialization 
    //
    // [JsonConverter(typeof(ConcurrentDictionarySafeConverter<Type, bool>))]
    //
    public class ConcurrentDictionarySafeConverter<TKey, TValue> : JsonConverter
    {
        readonly Type _targetType = typeof(ConcurrentDictionarySafe<TKey, TValue>);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var converter = new KeyValuePairConverter();
            var dict = (ConcurrentDictionarySafe<TKey, TValue>)value;
            var enumerator = dict.GetEnumerator();
            writer.WriteStartArray();
            while(enumerator.MoveNext())
            {
                var entry = enumerator.Current;
                converter.WriteJson(writer, entry, serializer);

            }
            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var result = existingValue as ConcurrentDictionarySafe<TKey, TValue> ?? (objectType == _targetType ? new ConcurrentDictionarySafe<TKey, TValue>() : (ConcurrentDictionarySafe<TKey, TValue>)Activator.CreateInstance(objectType));

            var entryType = typeof(KeyValuePair<TKey, TValue>);
            var converter = new KeyValuePairConverter();

            reader.Read();
            while(reader.TokenType == JsonToken.StartObject)
            {
                var entry = (KeyValuePair<TKey, TValue>)converter.ReadJson(reader, entryType, null, serializer);
                result.AddOrUpdate(entry.Key, entry.Value, (key, value) => default(TValue));
                reader.Read();
            }
            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == _targetType;
        }
    }
}
