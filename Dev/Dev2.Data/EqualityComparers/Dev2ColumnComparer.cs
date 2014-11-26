
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList.Contract.EqualityComparers
{
    public class Dev2ColumnComparer : IEqualityComparer<Dev2Column>
    {
        private static Dev2ColumnComparer _instance = new Dev2ColumnComparer();

        public static Dev2ColumnComparer Instance
        {
            get
            {
                return _instance;
            }
        }

        public bool Equals(Dev2Column x, Dev2Column y)
        {           
            return x != null && y != null && x.ColumnName == y.ColumnName && x.ColumnDescription == y.ColumnDescription;            
        }

        public int GetHashCode(Dev2Column obj)
        {
            return obj.GetHashCode();
        }
    }
}
