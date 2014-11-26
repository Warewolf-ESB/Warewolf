
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Data.Interfaces;

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
