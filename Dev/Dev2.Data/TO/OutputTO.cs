/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Dev2.DataList.Contract
{
    public class OutputTO {
        private readonly string _outPutDescription;
        private readonly IList<string> _outputStrings;

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
        
        public string OutPutDescription => _outPutDescription;

        public IList<string> OutputStrings => _outputStrings;
    }
}
