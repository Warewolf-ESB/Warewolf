using System;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Unlimited.Framework.Converters.Graph.String.Xml
{
    [Serializable]
    public class XmlPathSegment : IPathSegment
    {
        #region Constructors

        internal XmlPathSegment()
        {

        }

        internal XmlPathSegment(string name, bool isEnumarable)
        {
            ActualSegment = name;
            IsEnumarable = isEnumarable;
        }

        internal XmlPathSegment(string name, bool isEnumarable, bool isAttribute)
        {
            ActualSegment = name;
            IsEnumarable = isEnumarable;
            IsAttribute = isAttribute;
        }

        #endregion Constructors

        #region Properties

        public string ActualSegment { get; set; }
        public string DisplaySegment { get; set; }
        public bool IsEnumarable { get; set; }
        public bool IsAttribute { get; set; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            if(IsEnumarable)
            {
                return ActualSegment + XmlPath.EnumerableSymbol;
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
                return ActualSegment + XmlPath.EnumerableSymbol;
            }
            else
            {
                return ActualSegment;
            }
        }

        #endregion Methods
    }
}
