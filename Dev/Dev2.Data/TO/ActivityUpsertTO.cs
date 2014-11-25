
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

namespace Dev2.DataList.Contract {
    public class ActivityUpsertTO {

        private readonly IList<IBinaryDataListEntry> _entriesB = new List<IBinaryDataListEntry>();
        private readonly IList<string> _expressions = new List<string>();
        private readonly IList<string> _entriesS = new List<string>();

        public void AddEntry(IBinaryDataListEntry val, string expression) {
            _entriesB.Add(val);
            _expressions.Add(expression);
        }

        public void AddEntry(IBinaryDataListEntry val, string expression, int pos) {
            if (pos >= _expressions.Count)
            {
                _entriesB.Insert(pos, val);
                _expressions.Insert(pos, expression);
            }
            else
            {
                _entriesB[pos] = val;
                _expressions[pos] = expression;
            }
        }

        public void AddEntry(string val, string expression, int pos) {
            if (pos >= _expressions.Count)
            {
                _entriesS.Insert(pos, val);
                _expressions.Insert(pos, expression);
            }
            else
            {
                _entriesS[pos] = val;
                _expressions[pos] = expression;
            }
        }

        public void AddEntry(string val, string expression) {
            _entriesS.Add(val);
            _expressions.Add(expression);
        }

        public IList<IBinaryDataListEntry> FetchBinaryEntries() {
            return _entriesB;
        }

        public IList<string> FetchStringEntries() {
            return _entriesS;
        }

        public IList<string> FetchExpressions() {
            return _expressions;
        }
    }
}
