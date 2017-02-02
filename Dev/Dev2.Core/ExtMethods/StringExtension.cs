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
using System.Diagnostics.CodeAnalysis;
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
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static bool IsDate(this string payload)
        {
            bool result = false;

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
            var d = new DateTimeParser();
            int count = 0;
            while (result == false && count < acceptedDateFormats.Count)
            {
                string errorMsg;
                IDateTimeResultTO to;
                result = d.TryParseDateTime(payload, acceptedDateFormats[count], out to, out errorMsg);
                count++;
            }
            return result;
        }
        private static readonly XmlReaderSettings IsXmlReaderSettings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Auto, DtdProcessing = DtdProcessing.Ignore };

        public static bool IsXml(this string payload)
        {
            bool result = false;
            bool isFragment;


            if (IsXml(payload, out isFragment))
            {
                result = true;
            }
            else if (isFragment)
            {
                result = true;
            }

            return result;
        }

        public static bool IsJSON(this string payload)
        {
            if ((payload.StartsWith("{") && payload.EndsWith("}")) || //For object
                (payload.StartsWith("[") && payload.EndsWith("]"))) //For array
            {
                try
                {


                    JsonConvert.DeserializeObject(payload);
                    return true;
                }


                catch
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
            bool isHtml;
            return IsXml(data, out isFragment, out isHtml) && !isFragment && !isHtml;
        }

        /// <summary>
        /// Checks if the info contained in data is well formed XML
        /// </summary>
        static bool IsXml(string data, out bool isFragment, out bool isHtml)
        {
            string trimedData = data.Trim();
            bool result = trimedData.StartsWith("<") && !trimedData.StartsWith("<![CDATA[");

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
                            Dev2Logger.Error("DataListUtil", ex);
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