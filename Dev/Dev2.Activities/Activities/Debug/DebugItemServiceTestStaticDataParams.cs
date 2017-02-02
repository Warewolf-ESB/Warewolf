using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics;
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Activities.Debug
{
    public class DebugItemServiceTestStaticDataParams : DebugOutputBase
    {
        readonly string _operand;

        public DebugItemServiceTestStaticDataParams(string value, bool hasError = false, bool mockSelected = false)
        {
            Value = value;
            Type = DebugItemResultType.Value;
            HasError = hasError;
            TestStepHasError = hasError;
            MockSelected = mockSelected;
        }

        public DebugItemServiceTestStaticDataParams(string value, string variable, bool mockSelected = false)
        {
            Value = value;
            Variable = variable;
            Type = DebugItemResultType.Variable;
            MockSelected = mockSelected;
        }

        public DebugItemServiceTestStaticDataParams(string value, string variable, string operand, bool mockSelected = false)
        {
            Value = value;
            _operand = operand;
            Variable = variable;
            Type = DebugItemResultType.Variable;
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