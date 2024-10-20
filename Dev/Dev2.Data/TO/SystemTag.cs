/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Interfaces;

namespace Dev2.DataList.Contract
{
    public class SystemTag : ISystemTag {

        #region Attributes
        readonly string _startTag;
        readonly string _endTag;
        #endregion

        #region Ctor

        public SystemTag(string tagName) {
            _startTag = string.Concat("<", tagName, ">");
            _endTag = string.Concat("</", tagName, ">");
        }
        #endregion

        #region Properties
        public string StartTag => _startTag;

        public string EndTag => _endTag;

        #endregion
    }
}
