
namespace Dev2.Data.TO
{
    public class ParseTO {

        private string _payload = string.Empty;

        public string Payload {
            get {
                return _payload;
            }
            set {
                _payload = value;
            }
        }

        public int StartIndex { get; set; }

        public int EndIndex { get; set; }

        public ParseTO Child { get; set; }

        public ParseTO Parent { get; set; }

        public bool HangingOpen { get; set; }

        public bool IsRecordSet {

            get {
                bool result = Payload != null && Payload.Contains("(");

                return result;
            }
        }

        public bool IsRoot {

            get {
                return (Parent == null);
            }
        }

        public bool IsLeaf {

            get {
                return (Child == null);
            }
        }
    }
}
