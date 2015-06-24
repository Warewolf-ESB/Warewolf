
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Xml;

namespace Dev2.Integration.Tests.MEF
{
    public class DataListValueInjector
    {
        public string InjectDataListValue(string xmlString, string nameOfNode, string innerTextToInject)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(xmlString);
            var selectSingleNode = xDoc.SelectSingleNode("//" + nameOfNode);
            if(selectSingleNode != null)
            {
                selectSingleNode.InnerText = innerTextToInject;
            }
            return xDoc.OuterXml.Replace("&gt;", ">").Replace("&lt;", "<");
        }
    }
}
