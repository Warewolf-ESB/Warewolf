#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace Warewolf.Data
{
    /// <summary>
    /// General DataList utility methods
    /// </summary>

    public static class DataListUtilBase
    {

        public static bool IsXml(string data)
        {
            var isXml = XmlHelper.IsXml(data, out bool isFragment, out bool isHtml);
            return isXml && !isFragment && !isHtml;
        }

        public static bool IsXml(string data, out bool isFragment) => XmlHelper.IsXml(data, out isFragment, out bool isHtml) && !isFragment && !isHtml;

        public static string AdjustForEncodingIssues(string payload)
        {
            var trimedData = payload.Trim();
            var isXml = trimedData.StartsWith("<", StringComparison.Ordinal) && !trimedData.StartsWith("<![CDATA[", StringComparison.Ordinal);

            if (!isXml)
            {
                trimedData = TrimNonXmlData(trimedData);
            }
            var bomMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (trimedData.StartsWith(bomMarkUtf8, StringComparison.OrdinalIgnoreCase))
            {
                trimedData = trimedData.Remove(0, bomMarkUtf8.Length);
            }

            trimedData = trimedData.Replace("\0", "");
            return trimedData;
        }
        private static string TrimNonXmlData(string trimedData)
        {
            var nonXmlData = trimedData;
            if (nonXmlData.Length > 1 && nonXmlData[1] == '<' && nonXmlData[2] == '?')
            {
                nonXmlData = nonXmlData.Substring(1);
            }
            else if (nonXmlData.Length > 2 && nonXmlData[2] == '<' && nonXmlData[3] == '?')
            {
                nonXmlData = nonXmlData.Substring(2);
            }
            else
            {
                if (nonXmlData.Length > 3 && nonXmlData[3] == '<' && nonXmlData[4] == '?')
                {
                    nonXmlData = nonXmlData.Substring(3);
                }
            }

            return nonXmlData;
        }

    }
}
