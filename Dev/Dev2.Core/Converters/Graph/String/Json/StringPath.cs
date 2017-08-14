using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Graph;


namespace Unlimited.Framework.Converters.Graph.String.Json
{
    public class StringPath : IPath {
        string _actualPath;
        string _displayPath;
        string _sampleData;
        string _outputExpression;

        #region Implementation of IPath

        public string ActualPath
        {
            get
            {
                return _actualPath;
            }
            set
            {
                _actualPath = value;
            }
        }
        public string DisplayPath
        {
            get
            {
                return _displayPath;
            }
            set
            {
                _displayPath = value;
            }
        }
        public string SampleData
        {
            get
            {
                return _sampleData;
            }
            set
            {
                _sampleData = value;
            }
        }
        public string OutputExpression
        {
            get
            {
                return _outputExpression;
            }
            set
            {
                _outputExpression = value;
            }
        }

        public IEnumerable<IPathSegment> GetSegements()
        {
            yield break;
        }

        public IPathSegment CreatePathSegment(string pathSegmentString)
        {
            return null;
        }

        #endregion
    }
}