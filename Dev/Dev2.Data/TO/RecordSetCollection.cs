/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Data.Interfaces;

namespace Dev2.DataList.Contract
{
    public class RecordSetCollection : IRecordSetCollection {

        #region Attributes
        readonly IList<IRecordSetDefinition> _recordSets;
        readonly IList<string> _recordSetNames;

        #endregion

        #region Ctor
        internal RecordSetCollection(IList<IRecordSetDefinition> recordSets, IList<string> recordSetNames) {
            _recordSets = recordSets;
            _recordSetNames = recordSetNames;
        }
        #endregion

        #region Properties
        public IList<IRecordSetDefinition> RecordSets => _recordSets;

        public IList<string> RecordSetNames => _recordSetNames;

        #endregion
    }
}
