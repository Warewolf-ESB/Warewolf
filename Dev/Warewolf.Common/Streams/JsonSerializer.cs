/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Newtonsoft.Json;
using System.Text;

namespace Warewolf.Streams
{
    public class JsonSerializer : ISerializer, IDeserializer
    {
        public T Deserialize<T>(byte[] value) => JsonConvert.DeserializeObject<T>(UTF8Encoding.UTF8.GetString(value));
        public byte[] Serialize<T>(T value) => UTF8Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
    }
}
