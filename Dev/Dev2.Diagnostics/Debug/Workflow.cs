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
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Dev2.Diagnostics.Debug
{
    public class Workflow : IXmlSerializable
    {
        public IList<DebugState> DebugStates { get; set; }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            DebugStates = new List<DebugState>();
            reader.MoveToContent();
            reader.ReadStartElement();

            while (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "DebugState")
            {
                var item = new DebugState();
                item.ReadXml(reader);
                DebugStates.Add(item);
            }


            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}