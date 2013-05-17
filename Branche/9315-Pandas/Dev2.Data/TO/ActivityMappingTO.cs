using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract {
    public class ActivityMappingTO {

        private IRecordSetCollection _rsCol;
        private IList<IDev2Definition> _scalars;
        private IDataListInjectionContents _systemRegion;

        public IRecordSetCollection Recordsets {

            get {
                return _rsCol;
            }

            set {
                _rsCol = value;
            }
        }

        public IList<IDev2Definition> Scalars {
            
            get {
                return _scalars;
            }

            set {
                _scalars = value;
            }
        }

        public IDataListInjectionContents SystemRegion {
            get {
                return _systemRegion;
            }
            set {
                _systemRegion = value;
            }
        }
    }
}
