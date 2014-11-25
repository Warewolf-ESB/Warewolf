
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.DataList.Contract;

namespace Dev2.Activities.Debug
{

    public class DebugOutputParams : DebugOutputBase
    {
        readonly string _expression;
        readonly string _value;
        readonly Guid _dlId;
        readonly int _indexToUpsertTo;
        readonly string _labelText;

        public DebugOutputParams(string expression, string value, Guid dlId, int indexToUpsertTo)
        {
            _expression = expression;
            _value = value;
            _dlId = dlId;
            _indexToUpsertTo = indexToUpsertTo;
            _labelText = "";
        }

        public string Expression
        {
            get
            {
                return _expression;
            }
        }
        public string Value
        {
            get
            {
                return _value;
            }
        }
        public Guid DlId
        {
            get
            {
                return _dlId;
            }
        }
        public int IndexToUpsertTo
        {
            get
            {
                return _indexToUpsertTo;
            }
        }
        public override string LabelText
        {
            get
            {
                return _labelText;
            }
        }

        public override List<IDebugItemResult> GetDebugItemResult()
        {
            List<IDebugItemResult> debugItemsResults = CreateDebugItemsFromString(Expression, Value, DlId, IndexToUpsertTo, LabelText, enDev2ArgumentType.Output);
            return debugItemsResults;
        }
    }
}
