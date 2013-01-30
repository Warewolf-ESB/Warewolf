using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
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
                bool result = false;

                if (Payload != null && Payload.Contains("(")) {
                    result = true;
                }

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
