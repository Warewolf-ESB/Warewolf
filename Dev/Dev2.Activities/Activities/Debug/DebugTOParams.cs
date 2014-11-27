
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.TO;

namespace Dev2.Activities.Debug
{
    // ReSharper disable InconsistentNaming
    public class DebugTOParams : DebugOutputBase
    // ReSharper restore InconsistentNaming
    {

        readonly string _labelText;
        readonly DebugTO _debugTo;
        readonly bool _isInput;
        readonly string _leftLabel;
        readonly string _rightLabel;
        readonly List<string> _regions;

        public DebugTOParams(DebugTO debugTo, bool isInput, string leftLabel = "", string rightLabel = "", List<string> regions = null)
        {
            _debugTo = debugTo;
            _isInput = isInput;
            _leftLabel = leftLabel;
            _rightLabel = rightLabel;
            _labelText = "";
            _regions = regions;

        }

        public override string LabelText
        {
            get
            {
                return _labelText;
            }
        }

        public DebugTO DebugTo
        {
            get
            {
                return _debugTo;
            }
        }

        public override List<IDebugItemResult> GetDebugItemResult()
        {
            List<IDebugItemResult> debugItemsResults =
                _isInput
                ? CreateDebugItemForInput(DebugTo, LabelText, _leftLabel, _rightLabel, _regions)
                : CreateDebugItemForOutput(DebugTo, LabelText, _regions);
            return debugItemsResults;
        }
    }
}
