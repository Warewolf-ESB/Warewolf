
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
using Dev2.Common.Interfaces.Data;
using Dev2.Data.Interfaces;

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
