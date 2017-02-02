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
using System.Linq;
using Dev2.Common.Interfaces.Core.Graph;

namespace Unlimited.Framework.Converters.Graph.Poco
{
    [Serializable]
    public class PocoPath : BasePath
    {
        #region Class Members

        private const string _seperatorSymbol = ".";
        private const string _enumerableSymbol = "()";

        #endregion Class Members

        #region Constructors

        public PocoPath()
            : this("", "", "", "")
        {
        }

        public PocoPath(string actualPath, string displayPath)
            : this(actualPath, displayPath, "", "")
        {
        }

        public PocoPath(string actualPath, string displayPath, string outputExpression)
            : this(actualPath, displayPath, outputExpression, "")
        {
        }

        public PocoPath(string actualPath, string displayPath, string outputExpression, string sampleData)
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
            return ActualPath.Split(SeperatorSymbol.ToCharArray()).Select(CreatePathSegment).ToList();
        }

        public override IPathSegment CreatePathSegment(string pathSegmentString)
        {
            PocoPathSegment pathSegment;
            if (pathSegmentString.EndsWith(EnumerableSymbol))
            {
                pathSegment = new PocoPathSegment(pathSegmentString.TrimEnd(EnumerableSymbol.ToArray()), true);
            }
            else
            {
                pathSegment = new PocoPathSegment(pathSegmentString, false);
            }
            return pathSegment;
        }

        public IPathSegment CreatePathSegment(string name, bool isEnumerable)
        {
            return new PocoPathSegment(name, isEnumerable);
        }

        #endregion Methods

        #region Static Properties

        public static string EnumerableSymbol => _enumerableSymbol;

        public static string SeperatorSymbol => _seperatorSymbol;

        #endregion Static Properties
    }
}