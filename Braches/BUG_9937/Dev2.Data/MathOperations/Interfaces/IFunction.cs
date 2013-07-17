using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Calculations;
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
