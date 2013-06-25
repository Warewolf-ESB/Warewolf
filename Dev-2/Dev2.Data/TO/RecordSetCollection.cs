using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public class RecordSetCollection : IRecordSetCollection {

        #region Attributes
        private readonly IList<IRecordSetDefinition> _recordSets;
        private readonly IList<string> _recordSetNames;

        #endregion

        #region Ctor
        internal RecordSetCollection(IList<IRecordSetDefinition> recordSets, IList<string> recordSetNames) {
            _recordSets = recordSets;
            _recordSetNames = recordSetNames;
        }
        #endregion

        #region Properties
        public IList<IRecordSetDefinition> RecordSets {

            get {
                return _recordSets;
            }
        }

        public IList<string> RecordSetNames {
            get {
                return _recordSetNames;
            }
        }

        #endregion
    }
}
