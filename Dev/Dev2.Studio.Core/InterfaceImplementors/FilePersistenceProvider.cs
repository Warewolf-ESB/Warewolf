
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.IO;
using System.Text;
using System.Xml.Serialization;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core
{
    public class FilePersistenceProvider : IFilePersistenceProvider
    {
        public void Write(string containerName, string data)
        {
            File.WriteAllText(containerName, data);
        }

        public void Delete(string containerName)
        {
            File.Delete(containerName);
        }

        public string Read(string containerName)
        {
            return File.ReadAllText(containerName);
        }

        public static string Serialize<T>(T obj)
        {
            var type = obj.GetType();

            var sb = new StringBuilder();
            using(var writer = new StringWriter(sb))
            {
                var s = new XmlSerializer(type);
                s.Serialize(writer, obj);
            }
            return sb.ToString();
        }

        public static T Deserialize<T>(string xml)
        {
            var type = typeof(T);

            using(var reader = new StringReader(xml))
            {
                var s = new XmlSerializer(type);
                return (T)s.Deserialize(reader);
            }
        }
    }
}
