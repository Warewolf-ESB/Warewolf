#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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