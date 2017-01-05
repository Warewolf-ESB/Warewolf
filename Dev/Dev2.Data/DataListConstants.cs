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
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Dev2.Data.SystemTemplates
{
    public static class DataListConstants
    {

        public readonly static Dev2Decision DefaultDecision = new Dev2Decision { Col1 = string.Empty, Col2 = string.Empty, Col3 = string.Empty, EvaluationFn = enDecisionType.IsNotError };
        public readonly static Dev2DecisionStack DefaultStack = new Dev2DecisionStack { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.AND, TrueArmText = "True", FalseArmText = "False", DisplayText = "Is True?" };

        public static readonly Dev2Switch DefaultSwitch = new Dev2Switch { SwitchVariable = "", DisplayText = null };
        public static readonly Dev2Switch DefaultCase = new Dev2Switch { SwitchVariable = "" };

        // Internal Row Storage Contants
        public static readonly int EmptyRowStartIdx = -1;
        public static readonly double RowGrowthFactor = 1.50;
        public static readonly int MinRowSize = 10;
    }
}
