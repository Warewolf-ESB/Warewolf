
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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Dev2.Data.Binary_Objects
{
    public class BinaryDataListUtil
    {

        public string SerializeDeferredItem(object item)
        {
            string result;
            using(MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formater = new BinaryFormatter();
                formater.Serialize(ms, item);

                result = Convert.ToBase64String(ms.ToArray());

                ms.Dispose();
            }

            return result;

        }

        public T DeserializeDeferredItem<T>(string item)
        {

            using(MemoryStream ms = new MemoryStream(Convert.FromBase64String(item)))
            {
                BinaryFormatter formater = new BinaryFormatter();

                return (T)formater.Deserialize(ms);
            }

        }
    }
}
