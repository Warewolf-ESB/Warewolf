
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Interfaces;

namespace Dev2.DataList.Contract
{
    public class SystemTagsTO : IDataListInjectionContents {

        private readonly string _payload;

        public SystemTagsTO(string payload) {
            _payload = payload;
        }

        public bool IsSystemRegion {
            get {
                return true;
            }
        }

        public string ToInjectableState(){
            return _payload;
        }

    }
}
