using System;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Unlimited.Framework.Converters.Graph.Poco
{
    [Serializable]
    internal class PocoPathSegment : IPathSegment
    {
        #region Constructors

        internal PocoPathSegment()
        {

        }

        internal PocoPathSegment(string name, bool isEnumarable)
        {
            ActualSegment = name;
            IsEnumarable = isEnumarable;
        }

        #endregion Constructors

        #region Properties

        public string ActualSegment { get; set; }
        public string DisplaySegment { get; set; }
        public bool IsEnumarable { get; set; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            if(IsEnumarable)
            {
                return ActualSegment + PocoPath.EnumerableSymbol;
            }
            else
            {
                return ActualSegment;
            }
        }

        public string ToString(bool considerEnumerable)
        {
            if(considerEnumerable && IsEnumarable)
            {
                return ActualSegment + PocoPath.EnumerableSymbol;
            }
            else
            {
                return ActualSegment;
            }
        }

        #endregion Methods
    }
}
