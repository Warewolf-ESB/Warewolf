using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public class DataValue : IDataValue, IDataListInjectionContents {

        #region Properties
        private readonly string _value;
        private readonly string _tagName;
        private readonly bool _isSystemRegion;
        #endregion

        #region Ctor
        internal DataValue(string value, string tagName, bool isSystemRegion) {
            _value = value;
            _tagName = tagName;
            _isSystemRegion = isSystemRegion;
        }
        #endregion

        #region Properties
        public string Value {
            get {
                return _value;
            }
        }

        public string Tag {
            get {
                return _tagName;
            }
        }

        public bool IsSystemRegion {
            get {
                return _isSystemRegion;
            }
            
        }
        #endregion

        public string ToInjectableState() {
            throw new NotImplementedException("DataValue does not implement the ToInjectableState method.");
        }
    }
}
