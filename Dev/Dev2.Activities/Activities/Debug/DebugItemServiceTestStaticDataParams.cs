#pragma warning disable
using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics;




namespace Dev2.Activities.Debug
{
    public class DebugItemServiceTestStaticDataParams : DebugOutputBase
    {
        readonly string _operand;
        
        public DebugItemServiceTestStaticDataParams(string value, bool hasError)
            : this(value, hasError, false)
        {
        }

        public DebugItemServiceTestStaticDataParams(string value, bool hasError, bool mockSelected)
        {
            Value = value;
            Type = DebugItemResultType.Value;
            HasError = hasError;
            TestStepHasError = hasError;
            MockSelected = mockSelected;
        }
        
        public string Value { get; }

        public override string LabelText { get; }

        public string Variable { get; }

        public bool HasError { get; }

        public bool TestStepHasError { get; }

        public bool MockSelected { get; }

        public DebugItemResultType Type { get; }

        public override List<IDebugItemResult> GetDebugItemResult()
        {

            var debugItemsResults = new List<IDebugItemResult>{
                 new DebugItemResult
                    {
                        Type = Type,
                        Value = Value,
                        Label = LabelText,
                        Variable = Variable,
                        HasError = HasError,
                        TestStepHasError = TestStepHasError,
                        MockSelected = MockSelected,
                        Operator = string.IsNullOrWhiteSpace(_operand) ? "" : "="
                    }};
            return debugItemsResults;
        }
    }
}