
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
