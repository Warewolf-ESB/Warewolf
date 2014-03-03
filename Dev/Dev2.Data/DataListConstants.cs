using System;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data.SystemTemplates
{
    public static class DataListConstants
    {

        public readonly static Dev2Decision DefaultDecision = new Dev2Decision() { Col1 = string.Empty, Col2 = string.Empty, Col3 = string.Empty, EvaluationFn = enDecisionType.IsNotError };
        public readonly static Dev2DecisionStack DefaultStack = new Dev2DecisionStack() { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.AND, TrueArmText = "True", FalseArmText = "False", DisplayText = "Is True?" };

        public static readonly Dev2Switch DefaultSwitch = new Dev2Switch() { SwitchVariable = "", DisplayText = null };
        public static readonly Dev2Switch DefaultCase = new Dev2Switch() { SwitchVariable = "" };

        public static readonly IBinaryDataListEntry baseEntry = Dev2BinaryDataListFactory.CreateEntry(GlobalConstants.NullEntryNamespace, string.Empty, Guid.NewGuid());
        public static readonly IBinaryDataListItem baseItem = Dev2BinaryDataListFactory.CreateBinaryItem(string.Empty, string.Empty);
        public static readonly IBinaryDataListItem emptyItem = Dev2BinaryDataListFactory.CreateBinaryItem(string.Empty, string.Empty);

        // Internal Row Storage Contants
        public static readonly int EmptyRowStartIdx = -1;
        public static readonly double RowGrowthFactor = 1.50;
        public static readonly int MinRowSize = 10;
    }
}
