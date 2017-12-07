/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
        readonly string _name;
        readonly string _mapsTo;
        readonly string _startTagSearch;
        readonly string _endTagSearch;
        readonly string _startTagReplace;
        readonly string _endTagReplace;
        readonly bool _isEvaluated;

        #endregion

        #region Ctor
        public InputDefinition(string name, string mapsTo, bool isEvaluated){
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
        public string Name => _name;

        public string MapsTo => _mapsTo;

        public string StartTagSearch => _startTagSearch;

        public string EndTagSearch => _endTagSearch;

        public string StartTagReplace => _startTagReplace;

        public string EndTagReplace => _endTagReplace;

        public bool IsEvaluated => _isEvaluated;

        #endregion
    }
}
