using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using System.Collections.Generic;

namespace Dev2.Data.SystemTemplates
{
    public static class DataListConstants
    {

        public readonly static Dev2Decision DefaultDecision = new Dev2Decision() { Col1 = string.Empty, Col2 = string.Empty, Col3 = string.Empty, EvaluationFn = enDecisionType.IsNotError };
        public readonly static Dev2DecisionStack DefaultStack = new Dev2DecisionStack() { TheStack = new List<Dev2Decision>() { DefaultDecision }, Mode = Dev2DecisionMode.AND, TrueArmText = "True", FalseArmText = "False" };

        public static readonly Dev2Switch DefaultSwitch = new Dev2Switch() { SwitchVariable = "[[Dummy]]" };
        public static readonly Dev2Switch DefaultCase = new Dev2Switch() { SwitchVariable = "" };
    }
}
