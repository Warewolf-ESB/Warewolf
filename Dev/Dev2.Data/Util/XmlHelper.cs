
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
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Dev2.Common;

namespace Dev2.Data.Util
{
    public static class XmlHelper
    {
        public static string MakeErrorsUserReadable(string tag)
        {
            var result = new StringBuilder();
            var readerSettings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment };

            using (TextReader txtreader = new StringReader(tag))
            {
                try
                {
                    using (var reader = XmlReader.Create(txtreader, readerSettings))
                    {
                        var count = 0;

                        while (reader.Read())
                        {
                            using (var fragmentReader = reader.ReadSubtree())
                            {
                                if (fragmentReader.Read())
                                {
                                    try
                                    {
                                    var fragment = XNode.ReadFrom(fragmentReader) as XElement;

                                    if (fragment != null &&
                                        fragment.Name.LocalName ==
                                        GlobalConstants.InnerErrorTag.TrimStart('<').TrimEnd('>'))
                                    {
                                        count++;
                                        result.AppendFormat(" {0} ", count);
                                        result.AppendLine(fragment.Value);
                                    }
                                }
                                    catch (Exception)
                                    {
                                        // There was an issue parsing, must be text node now ;(

                                        count++;
                                        result.AppendFormat(" {0} ", count);
                                        result.AppendLine(fragmentReader.Value);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (InvalidOperationException ioex)
                {
                    Dev2Logger.Log.Error("XmlHelper", ioex);
                    result.Clear();
                    result.Append(tag);
                }
            }

            return result.ToString();
        }

        public static bool IsValidXElement(string toParse)
        {
            try
            {
                // ReSharper disable ReturnValueOfPureMethodIsNotUsed
                XElement.Parse(toParse);
                // ReSharper restore ReturnValueOfPureMethodIsNotUsed
                return true;
            }
            catch(Exception ex)
            {
                Dev2Logger.Log.Error("XmlHelper", ex);
                return false;
            }
        }

    }
}
