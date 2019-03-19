#pragma warning disable
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

namespace Dev2.Data.TO
{
    public class ParseTO : IParseTO
    {

        string _payload = string.Empty;

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

        public IParseTO Child { get; set; }

        public IParseTO Parent { get; set; }

        public bool HangingOpen { get; set; }

        public bool IsRecordSet {

            get {
                var result = Payload != null && Payload.Contains("(");

                return result;
            }
        }

        public bool IsRoot => Parent == null;

        public bool IsLeaf => Child == null;
    }
}
