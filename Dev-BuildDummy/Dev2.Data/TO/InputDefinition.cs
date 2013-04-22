using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public class InputDefinition : IInputLanguageDefinition {

        #region Properties
        private readonly string _name;
        private readonly string _mapsTo;
        private readonly string _startTagSearch;
        private readonly string _endTagSearch;
        private readonly string _startTagReplace;
        private readonly string _endTagReplace;
        private readonly bool _isEvaluated;

        #endregion

        #region Ctor
        internal InputDefinition(string name, string mapsTo, bool isEvaluated){
            _name = name;
            _mapsTo = mapsTo;
            _startTagSearch = string.Concat("<", mapsTo, ">");
            _endTagSearch = string.Concat("</", mapsTo, ">");
            _startTagReplace = string.Concat("<", name, ">");
            _endTagReplace = string.Concat("</", name, ">");
            _isEvaluated = isEvaluated;
        }
        #endregion

        #region Properties
        public string Name {
            get {
                return _name;
            }
        }

        public string MapsTo {
            get {
                return _mapsTo;
            }
        }

        public string StartTagSearch {
            get {
                return _startTagSearch;
            }
        }

        public string EndTagSearch {

            get {
                return _endTagSearch;
            }
        }

        public string StartTagReplace {

            get {
                return _startTagReplace;
            }
        }

        public string EndTagReplace {

            get {
                return _endTagReplace;
            }
        }

        public bool IsEvaluated {
            get {
                return _isEvaluated;
            }
        }
        #endregion
    }
}
