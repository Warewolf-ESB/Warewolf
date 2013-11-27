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
