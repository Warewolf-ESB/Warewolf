/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Text;
using System.Xml;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.DB
{
    internal class MS_SQL_Sanitizer : AbstractSanitizer, IDataProviderSanitizer
    {
        /// <summary>
        ///     Cleans up From XML data as per stored proc invoke, ie column headers like XML_F52E2B61-18A1-11d1-B105-00805F49916B
        /// </summary>
        /// <param name="xmlFormatedPayload"></param>
        /// <returns></returns>
        public string SanitizePayload(string xmlFormatedPayload)
        {
            var result = new StringBuilder();

            var xDoc = new XmlDocument();
            xDoc.LoadXml(xmlFormatedPayload);
            XmlNodeList nl = xDoc.SelectNodes("//NewDataSet/Table/*[starts-with(local-name(),'XML_')]");
            int foundXMLFrags = 0;

            if (nl != null)
            {
                foreach (XmlNode n in nl)
                {
                    string tmp = n.InnerXml;
                    result = result.Append(tmp);
                    foundXMLFrags++;
                }
            }

            string res = result.ToString();

            if (foundXMLFrags >= 1)
            {
                res = "<FromXMLPayloads>" + res + "</FromXMLPayloads>";
            }
            else if (foundXMLFrags == 0)
            {
                res = xmlFormatedPayload;
            }

            return RemoveDelimiting(res);
        }

        public enSupportedDBTypes HandlesType()
        {
            return enSupportedDBTypes.MSSQL;
        }
    }
}