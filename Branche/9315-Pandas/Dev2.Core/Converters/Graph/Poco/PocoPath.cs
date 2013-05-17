using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dev2;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Unlimited.Framework.Converters.Graph.Poco
{
    public class PocoPath : BasePath
    {
        #region Class Members

        private static readonly string _seperatorSymbol = ".";
        private static readonly string _enumerableSymbol = "()";

        #endregion Class Members

        #region Constructors

        public PocoPath() 
            : this("","","", "")
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
            List<IPathSegment> segments = new List<IPathSegment>();

            foreach (string segment in ActualPath.Split(SeperatorSymbol.ToCharArray()))
            {
                segments.Add(CreatePathSegment(segment));
            }

            return segments;
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

        public IPathSegment CreatePathSegment(PropertyInfo property)
        {
            return new PocoPathSegment(property.Name, property.PropertyType.IsEnumerable());
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

        public static string SeperatorSymbol
        {
            get
            {
                return _seperatorSymbol;
            }
        }

        #endregion Static Properties
    }
}
