
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.Common.Interfaces.Core.Graph;

namespace Unlimited.Framework.Converters.Graph.String.Json
{
    [Serializable]
    public class JsonPathSegment : IPathSegment
    {
        #region Constructors

        internal JsonPathSegment()
        {

        }

        internal JsonPathSegment(string name, bool isEnumarable)
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
                return ActualSegment + JsonPath.EnumerableSymbol;
            }

            return ActualSegment;
        }

        public string ToString(bool considerEnumerable)
        {
            if(considerEnumerable && IsEnumarable)
            {
                return ActualSegment + JsonPath.EnumerableSymbol;
            }

            return ActualSegment;
        }

        #endregion Methods
    }
}
