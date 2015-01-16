
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
using Infragistics.Calculations.CalcManager;

namespace Dev2.MathOperations {
    public interface IFunction {
        string FunctionName { get; }
        IList<string> arguments { get; }
        IList<string> ArgumentDescriptions { get; }
        string Description { get; }
        void CreateCustomFunction(string functionName, List<string> arguments, string description,
            Func<double[], double> function, IDev2CalculationManager calcManager);
        void CreateCustomFunction(string functionName, List<string> arguments, List<string> argumentDescriptions, string description,
            Func<double[], double> function, IDev2CalculationManager calcManager);
    }
}
