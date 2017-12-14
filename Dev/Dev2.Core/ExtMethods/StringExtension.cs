/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Dev2.Common;
using Dev2.Common.DateAndTime;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using Newtonsoft.Json;

namespace Dev2
{
    public static class StringExtension
    {
    
        public static bool IsDate(this string payload)
        {
            var result = false;

            if (string.IsNullOrEmpty(payload))
            {
                return false;
            }

            var acceptedDateFormats = new List<string>
            {
                "yyyymmdd",
                "mmddyyyy",
                "yyyyddmm",
                "ddmmyyyy",
                "yyyy/mm/dd",
                "dd/mm/yyyy",
                "yyyy/dd/mm",
                "mm/dd/yyyy",
                "yyyy-mm-dd",
                "dd-mm-yyyy",
                "mm-dd-yyyy",
                "yyyy-dd-mm",
                @"dd\mm\yyyy",
                @"yyyy\mm\dd",
                @"yyyy\dd\mm",
                @"mm\dd\yyyy",
                "dd mm yyyy",
                "mm dd yyyy",
                "yyyy mm dd",
                "yyyy dd mm",
                "yyyy mm dd",
                "dd.mm.yyyy",
                "mm.dd.yyyy",
                "yyyy.mm.dd",
                "yyyy.dd.mm"
            };
            var d = new Dev2DateTimeParser();
            var count = 0;
            while (!result && count < acceptedDateFormats.Count)
            {
                result = d.TryParseDateTime(payload, acceptedDateFormats[count], out IDateTimeResultTO to, out string errorMsg);
                count++;
            }
            return result;
        }
        static readonly XmlReaderSettings IsXmlReaderSettings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Auto, DtdProcessing = DtdProcessing.Ignore };

        public static bool IsXml(this string payload)
        {
            var result = false;


            if (IsXml(payload, out bool isFragment))
            {
                result = true;
            }
            else
            {
                if (isFragment)
                {
                    result = true;
                }
            }

            return result;
        }

        public static bool IsJSON(this string payload)
        {
            var value = payload.TrimStart();
            value = value.TrimEnd();
            if ((value.StartsWith("{") && value.EndsWith("}")) || //For object
                (value.StartsWith("[") && value.EndsWith("]"))) //For array
            {
                try
                {


                    JsonConvert.DeserializeObject(value);
                    return true;
                }


                catch (Exception ex)
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the info contained in data is well formed XML
        /// </summary>
        public static bool IsXml(string data, out bool isFragment)
        {
            return IsXml(data, out isFragment, out bool isHtml) && !isFragment && !isHtml;
        }

        /// <summary>
        /// Checks if the info contained in data is well formed XML
        /// </summary>
        static bool IsXml(string data, out bool isFragment, out bool isHtml)
        {
            var trimedData = data.Trim();
            var result = trimedData.StartsWith("<") && !trimedData.StartsWith("<![CDATA[");

            isFragment = false;
            isHtml = false;

            if (result)
            {
                using (TextReader tr = new StringReader(trimedData))
                {
                    using (XmlReader reader = XmlReader.Create(tr, IsXmlReaderSettings))
                    {

                        try
                        {
                            long nodeCount = 0;
                            while (reader.Read() && !isHtml && !isFragment && reader.NodeType != XmlNodeType.Document)
                            {
                                nodeCount++;

                                if (reader.NodeType != XmlNodeType.CDATA)
                                {
                                    if (reader.NodeType == XmlNodeType.Element && reader.Name.ToLower() == "html" && reader.Depth == 0)
                                    {
                                        isHtml = true;
                                        result = false;
                                    }

                                    if (reader.NodeType == XmlNodeType.Element && nodeCount > 1 && reader.Depth == 0)
                                    {
                                        isFragment = true;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Dev2Logger.Error("DataListUtil", ex, GlobalConstants.WarewolfError);
                            tr.Close();
                            reader.Close();
                            isFragment = false;
                            result = false;
                        }
                    }
                }
            }

            return result;
        }
    }
}