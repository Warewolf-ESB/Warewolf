using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Calculations.Engine;

namespace Infragistics.Calculations.CalcManager {
    public interface IDev2CalculationManager {
        CalculationValue CalculateFormula(string formula);
        IEnumerable<CalculationFunction> GetAllFunctions();
        bool RegisterUserDefinedFunction(CustomCalculationFunction customCalculationFunction);
    }
}
