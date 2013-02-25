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
            TextReader txtreader = new StringReader(tag);
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
                                var fragment = XNode.ReadFrom(fragmentReader) as XElement;

                                if (fragment != null && fragment.Name.LocalName == GlobalConstants.InnerErrorTag.TrimStart('<').TrimEnd('>'))
                                {
                                    count++;
                                    result.AppendFormat(" {0} ", count);
                                    result.AppendLine(fragment.Value);
                                }
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException e)
            {
                result.Clear();
                result.Append(tag);
            }

            return result.ToString();
        }

    }
}
