
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
using System.Runtime.Serialization.Formatters.Binary;

namespace Dev2.DataList.Contract.Extensions
{
    public static class ErrorResultTOExtensionMethods
    {
        private static readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();

        public static byte[] ToByteArray(this ErrorResultTO errors)
        {
            byte[] result;
            using(MemoryStream ms = new MemoryStream())
            {
                _binaryFormatter.Serialize(ms, errors);
                result = ms.ToArray();
                ms.Close();
                ms.Dispose();
            }

            return result;
        }

        public static ErrorResultTO FromByteArray(byte[] errorsData)
        {
            ErrorResultTO result;

            using(MemoryStream ms = new MemoryStream(errorsData))
            {
                ms.Seek(0, 0);
                result = (ErrorResultTO)_binaryFormatter.Deserialize(ms);
                ms.Close();
                ms.Dispose();
            }

            return result;
        }
    }
}
