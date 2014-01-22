using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Unlimited.Framework.Converters.Graph.String.Xml
{
    public class XmlPath : BasePath
    {
        #region Class Members

        const string _nodeSeperatorSymbol = ".";
        const string _attributeSeperatorSymbol = ":";
        const string _enumerableSymbol = "()";

        #endregion Class Members

        #region Constructors

        public XmlPath()
            : this("", "", "", "")
        {
        }

        public XmlPath(string actualPath, string displayPath)
            : this(actualPath, displayPath, "", "")
        {
        }


        public XmlPath(string actualPath, string displayPath, string outputExpression)
            : this(actualPath, displayPath, outputExpression, "")
        {
        }

        public XmlPath(string actualPath, string displayPath, string outputExpression, string sampleData)
        {
            ActualPath = actualPath;
            DisplayPath = displayPath;
            SampleData = sampleData;
            OutputExpression = outputExpression;
        }

        #endregion Constructors

        #region Methods

        public override IEnumerable<IPathSegment> GetSegements()
        {
            List<IPathSegment> segments = new List<IPathSegment>();

            foreach(string segment in ActualPath.Split(NodeSeperatorSymbol.ToCharArray()))
            {
                string[] nestedSegments = segment.Split(AttributeSeperatorSymbol.ToCharArray());

                if(nestedSegments.Length >= 1)
                {
                    segments.Add(CreatePathSegment(nestedSegments[0]));
                }

                if(nestedSegments.Length >= 2)
                {
                    segments.Add(CreateAttributePathSegment(nestedSegments[1]));
                }
            }

            return segments;
        }

        public override IPathSegment CreatePathSegment(string pathSegmentString)
        {
            XmlPathSegment xmlPathSegment;
            if(pathSegmentString.EndsWith(EnumerableSymbol))
            {
                xmlPathSegment = new XmlPathSegment(pathSegmentString.TrimEnd(EnumerableSymbol.ToArray()), true);
            }
            else
            {
                xmlPathSegment = new XmlPathSegment(pathSegmentString, false);
            }
            return xmlPathSegment;
        }

        public IPathSegment CreatePathSegment(XElement element)
        {
            return new XmlPathSegment(element.Name.ToString(), element.Elements().Count() > 1);
        }

        public IPathSegment CreatePathSegment(XAttribute attribute)
        {
            return new XmlPathSegment(attribute.Name.ToString(), false);
        }

        public IPathSegment CreateAttributePathSegment(string attribute)
        {
            return new XmlPathSegment(attribute, false, true);
        }

        #endregion Methods

        #region Static Properties

        public static string EnumerableSymbol
        {
            get
            {
                return _enumerableSymbol;
            }
        }

        public static string NodeSeperatorSymbol
        {
            get
            {
                return _nodeSeperatorSymbol;
            }
        }

        public static string AttributeSeperatorSymbol
        {
            get
            {
                return _attributeSeperatorSymbol;
            }
        }

        #endregion Static Properties
    }
}
