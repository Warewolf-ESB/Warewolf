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
using Dev2.Common;
using Dev2.Common.Interfaces.Core.Graph;
using Newtonsoft.Json.Linq;


namespace Unlimited.Framework.Converters.Graph.String.Json
{
    [Serializable]
    public class JsonPath : BasePath
    {
        #region Class Members

        const string _seperatorSymbol = ".";
        const string _enumerableSymbol = "()";

        #endregion Class Members

        #region Constructors

        public JsonPath()
            : this("", "", "", "")
        {
        }

        public JsonPath(string actualPath, string displayPath)
            : this(actualPath, displayPath, "", "")
        {
        }

        public JsonPath(string actualPath, string displayPath, string outputExpression)
            : this(actualPath, displayPath, outputExpression, "")
        {
        }

        public JsonPath(string actualPath, string displayPath, string outputExpression, string sampleData)
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

            foreach (string segment in ActualPath.Split(SeperatorSymbol.ToCharArray()))
            {
                segments.Add(CreatePathSegment(segment));
            }

            return segments;
        }

        public override IPathSegment CreatePathSegment(string pathSegmentString)
        {
            JsonPathSegment pathSegment;
            pathSegment = pathSegmentString.EndsWith(EnumerableSymbol) ? new JsonPathSegment(pathSegmentString.TrimEnd(EnumerableSymbol.ToArray()), true) : new JsonPathSegment(pathSegmentString, false);
            return pathSegment;
        }

        public IPathSegment CreatePathSegment(JProperty jProperty) => new JsonPathSegment(jProperty.Name, jProperty.IsEnumerable());

        #endregion Methods

        #region Static Properties

        public static string EnumerableSymbol => _enumerableSymbol;

        public static string SeperatorSymbol => _seperatorSymbol;

        #endregion Static Properties
    }
}