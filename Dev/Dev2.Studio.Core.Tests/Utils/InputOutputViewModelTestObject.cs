
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

namespace Dev2.Core.Tests.Utils {
    public class InputOutputViewModelTestObject {

        #region Properties

        public string Name { get { return _name; } set { _name = value; } }
        public string Value { get { return _value; } set { _value = value; } }
        public string MapsTo { get { return _mapsTo; } set { _mapsTo = value; } }
        public string DefaultValue { get { return _defaultValue; } set { _defaultValue = value; } }
        public bool Required { get { return _required; } set { _required = value; } }
        public string RecordSetName { get { return _recordSetName; } set { _recordSetName = value; } }

        #endregion Properties

        #region Locals

        private string _name;
        private string _value;
        private string _mapsTo;
        private string _defaultValue;
        private bool _required;
        private string _recordSetName;

        #endregion Locals

        #region CTOR

        public InputOutputViewModelTestObject() {
            _name = "vehicleColor";
            _value = "vehicleColor";
            _mapsTo = "testMapsTo";
            _defaultValue = "testDefaultValue";
            _required = true;
            _recordSetName = "testRecSetName";

        }

        public InputOutputViewModelTestObject(string name, string value, string mapsTo, string defaultValue, bool required, string recordSetName) {
            _name = name;
            _value = value;
            _mapsTo = mapsTo;
            _defaultValue = defaultValue;
            _required = required;
            _recordSetName = recordSetName;
        }

        #endregion CTOR

        #region Methods


        #endregion Methods
    }
}
