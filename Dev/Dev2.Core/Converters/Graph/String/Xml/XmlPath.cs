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
using System.Linq;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Core.Graph;

namespace Unlimited.Framework.Converters.Graph.String.Xml
{
    [Serializable]
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
            var segments = new List<IPathSegment>();

            foreach (string segment in ActualPath.Split(NodeSeperatorSymbol.ToCharArray()))
            {
                var nestedSegments = segment.Split(AttributeSeperatorSymbol.ToCharArray());

                if (nestedSegments.Length >= 1)
                {
                    segments.Add(CreatePathSegment(nestedSegments[0]));
                }

                if (nestedSegments.Length >= 2)
                {
                    segments.Add(CreateAttributePathSegment(nestedSegments[1]));
                }
            }

            return segments;
        }

        public override IPathSegment CreatePathSegment(string pathSegmentString)
        {
            XmlPathSegment xmlPathSegment;
            xmlPathSegment = pathSegmentString.EndsWith(EnumerableSymbol) ? new XmlPathSegment(pathSegmentString.TrimEnd(EnumerableSymbol.ToArray()), true) : new XmlPathSegment(pathSegmentString, false);
            return xmlPathSegment;
        }

        public IPathSegment CreatePathSegment(XElement element) => new XmlPathSegment(element.Name.ToString(), element.Elements().Count() > 1);

        public IPathSegment CreatePathSegment(XAttribute attribute) => new XmlPathSegment(attribute.Name.ToString(), false);

        public IPathSegment CreateAttributePathSegment(string attribute) => new XmlPathSegment(attribute, false, true);

        #endregion Methods

        #region Static Properties

        public static string EnumerableSymbol => _enumerableSymbol;

        public static string NodeSeperatorSymbol => _nodeSeperatorSymbol;

        public static string AttributeSeperatorSymbol => _attributeSeperatorSymbol;

        #endregion Static Properties
    }
}