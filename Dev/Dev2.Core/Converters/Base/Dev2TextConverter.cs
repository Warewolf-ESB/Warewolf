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
using Dev2.Common.Interfaces.Core.Convertors.Base;

namespace Dev2.Converters
{
    internal class Dev2TextConverter : IBaseConverter
    {
        public string ConvertToBase(byte[] payload)
        {
            return (Encoding.UTF8.GetString(payload));
        }

        public byte[] NeutralizeToCommon(string payload)
        {
            var encoder = new UTF8Encoding();
            return (encoder.GetBytes(payload));
        }

        public bool IsType(string payload)
        {
            //2013.02.13: Ashley Lewis - Bug 8725, Task 8836
            return true;
        }

        public Enum HandlesType()
        {
            return enDev2BaseConvertType.Text;
        }
    }
}