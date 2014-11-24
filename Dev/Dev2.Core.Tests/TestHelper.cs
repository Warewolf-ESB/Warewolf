
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
using System.Text;

namespace Dev2.DynamicServices.Test
{
    public static class TestHelper
    {

        public static byte[] ConvertStringToByteArray(string payload)
        {
            char[] charArray = payload.ToCharArray();

            byte[] byteArray = new byte[charArray.Length];



            for(int i = 0; i < charArray.Length; i++)
            {

                byteArray[i] = Convert.ToByte(charArray[i]);

            }

            return byteArray;
        }

        public static string ConvertByteArrayToString(byte[] payload)
        {
            string result = Encoding.ASCII.GetString(payload);

            return result;
        }
    }
}
