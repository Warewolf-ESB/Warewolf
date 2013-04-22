using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public class RecordSetInstance : IRecordSetInstance, IDataListInjectionContents {

        #region Attributes
        private readonly IList<IList<IDataValue>> _rowData;
        private readonly IRecordSetDefinition _setDef;


        #endregion

        #region Ctor
        internal RecordSetInstance(IRecordSetDefinition def) {
            _rowData = new List<IList<IDataValue>>();
            _setDef = def;
        }
        #endregion

        #region Properties
        public bool IsRecordSet {
            get {
                return true;
            }
        }

        public IList<IList<IDataValue>> RowData {
            get {
                return _rowData;
            }
        }

        public IRecordSetDefinition SetDefinition {

            get {
                return _setDef;
            }
        }

        public IList<IDev2Definition> Columns {

            get {
                return _setDef.Columns;
            }

        }

        public string SetName {

            get {
                return _setDef.SetName;
            }
        }

        public string XmlSetName {

            get {
                return _setDef.SetName.Replace("()", "");
            }
        }

        public bool IsSystemRegion {
            get {
                return false;
            }
        }

        #endregion

        #region Methods
        public void AddRow(IList<IDataValue> rowData) {
            _rowData.Add(rowData);
        }

        public string ToInjectableState() {
            throw new NotImplementedException("RecordsetInstance does not support the ToInjectableState method");
        }

        #endregion

    }
}
