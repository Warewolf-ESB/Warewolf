
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Text;

namespace Dev2.DataList.Contract
{
    public class DataRegion {
        readonly StringBuilder _regionData;

        public DataRegion() {
            _regionData = new StringBuilder();
        }

        public StringBuilder RegionData {
            get {
                return _regionData;
            }

        }

        public string RootLevelToken {
            get;
            set;
        }

        public DataRegion Parent {
            get;
            set;
        }

        public DataRegion Child {
            get;
            set;
        }


        public bool IsOpen { get; set; }

        public bool IsRootLevel { get; set; }

        public bool IsTokenGenerated { get; set; }

        public override string ToString() {
            return _regionData.ToString();
        }

        public string Value { get; set; }

        public bool HasParent {

            get {
                return Parent != null;
            }
        }

        public bool HasChild {

            get {
                return Child != null;
            }
        }   
    }
}
