using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public class Dev2Definition : IDev2Definition {

        #region Attributes
        private readonly string _name;
        private readonly string _mapsTo;
        private readonly string _recordSet;
        private readonly string _value;
        private readonly bool _isEvaluated;
        private readonly string _defaultValue;
        private readonly bool _isRequired;
        private readonly string _rawValue;
        #endregion

        #region Ctor
        internal Dev2Definition(string name, string mapsTo, string value, bool isEvaluated, string defaultValue, bool isRequired, string rawValue) : this(name, mapsTo, value, string.Empty, isEvaluated, defaultValue, isRequired, rawValue) {}

        internal Dev2Definition(string name, string mapsTo, string value, string recordSet, bool isEvaluated, string defaultValue, bool isRequired, string rawValue) : this(name, mapsTo, value, recordSet, isEvaluated, defaultValue, isRequired, rawValue, false) {
        }

        internal Dev2Definition(string name, string mapsTo, string value, string recordSet, bool isEvaluated, string defaultValue, bool isRequired, string rawValue, bool emptyToNull)
        {
            _name = name;
            _mapsTo = mapsTo;
            _value = value;
            _recordSet = recordSet;
            _isEvaluated = isEvaluated;
            _defaultValue = defaultValue;
            _isRequired = isRequired;
            _rawValue = rawValue;
            EmptyToNull = emptyToNull;
        }
        #endregion

        #region Properties
        public string Name{
            get {
                return _name;
            }
        }

        public string MapsTo {
            get {
                return _mapsTo;
            }
        }

        public string Value {
            get {
                return _value;
            }
        }

        public bool IsRecordSet {
            get {
                return !( (_recordSet == null) || _recordSet.Equals(string.Empty));
            }
        }

        public string RecordSetName {
            get {
                return _recordSet;
            }
        }

        public bool IsEvaluated {
            get {
                return _isEvaluated;
            }
        }

        public string DefaultValue {
            get {
                return _defaultValue;
            }
        }

        public bool IsRequired {
            get {
                return _isRequired;
            }
        
        }

        public string RawValue {

            get {
                return _rawValue;
            }
        }

        public bool EmptyToNull { get; private set; }
        #endregion
    }
}
