using System.Collections.Generic;

namespace Dev2.DataList.Contract
{
    public class OutputTO {
        private string _outPutDescription;
        private IList<string> _outputStrings;

        internal OutputTO(string outputDescription) {
            _outPutDescription = outputDescription;
            _outputStrings = new List<string>();

        }

        internal OutputTO(string outputDescription, IList<string> outputStrings) {
            _outPutDescription = outputDescription;
            _outputStrings = outputStrings;
            

        }

        internal OutputTO(string outputDescription, string outputStrings) {
            _outPutDescription = outputDescription;
            _outputStrings = new List<string> { outputStrings };


        }
        
        public string OutPutDescription {
            get {
                return _outPutDescription;
            }
        }

        public IList<string> OutputStrings{
            get {                
                return _outputStrings;
            }
        }
    }
}
