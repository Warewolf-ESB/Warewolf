/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Data.Binary_Objects
{
    [Serializable]
    public class ListOfIndex
    {
        public int MaxValue { get; set; }
        public int MinValue { get; set; }

        public List<int> Indexes { get; private set; }

        private ListOfIndex()
        {

        }

        public ListOfIndex(List<int> indexes)
        {
            Indexes = indexes;
        }

        public int GetMaxIndex()
        {
            int result = -1;

            if(Indexes != null)
            {
                result = Indexes.Max();
            }

            return result;
        }

        public int Count()
        {
            int result = -1;
            if(Indexes != null)
            {
                result = Indexes.Count;
            }
            return result;
        }
    }
}
