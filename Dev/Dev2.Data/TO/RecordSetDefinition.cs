using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;

namespace Dev2.DataList.Contract
{
    public class RecordSetDefinition : IRecordSetDefinition {

        #region Attributes
        private readonly string _setName;
        private readonly IList<IDev2Definition> _columns;

        #endregion

        #region Ctor
        internal RecordSetDefinition(string setName, IList<IDev2Definition> columns) {
            _setName = setName;
            _columns = columns;
        }

        #endregion

        #region Properties
        public string SetName {
            get {
                return _setName;
            }
        }

        public string XmlSetName {

            get {
                return _setName.Replace("()", "");
            }

        }

        public IList<IDev2Definition> Columns {
            get {
                return _columns;
            }
        }
        #endregion

    }
}
