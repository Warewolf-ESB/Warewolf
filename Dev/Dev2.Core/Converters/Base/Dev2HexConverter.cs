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
using System.Text.RegularExpressions;
using Dev2.Common.Interfaces.Core.Convertors.Base;

namespace Dev2.Converters
{
    internal class Dev2HexConverter : IBaseConverter
    {
        public string ConvertToBase(byte[] payload)
        {
            var result = new StringBuilder("0x");

            foreach (byte t in payload)
            {
                result.Append(Convert.ToString(t, 16));
            }

            return result.ToString();
        }

        public byte[] NeutralizeToCommon(string payload)
        {
            // strip 0x from string if present
            if (payload.StartsWith("0x") || payload.StartsWith("0X"))
            {
                payload = payload.Substring(2);
            }


            var result = new byte[(payload.Length/2)];

            int pos = 0;
            for (int i = 0; i < payload.Length; i += 2)
            {
                result[pos] = Convert.ToByte((payload.Substring(i, 2)), 16);
                pos++;
            }

            return result;
        }

        public bool IsType(string payload)
        {
            bool result = (Regex.IsMatch(payload, @"\A\b[0-9a-fA-F]+\b\Z") ||
                           Regex.IsMatch(payload, @"\A\b(0[xX])?[0-9a-fA-F]+\b\Z"));

            if ((payload.Length%2) != 0)
            {
                result = false;
            }
            return result;
        }

        public Enum HandlesType()
        {
            return enDev2BaseConvertType.Hex;
        }
    }
}