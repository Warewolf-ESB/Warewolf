using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Calculations.Engine;
using Infragistics.Collections;

namespace Infragistics.Calculations.CalcManager {
    public class Dev2CalculationManager : IDev2CalculationManager {

        #region Private Members

        private UltraCalcEngine _calcEngine; 

        #endregion Private Members
         
        public Dev2CalculationManager() {
            _calcEngine = new UltraCalcEngine();

        }

        #region Interface Implementors

        public CalculationValue CalculateFormula(string formula) {
            FormulaCalculationReference reference = new FormulaCalculationReference(this, formula);

            ICalculationFormula calcFormula = reference.Formula;

            if(calcFormula.HasSyntaxError)
                throw new CalculationException(SRUtil.GetString("LER_Exception_34", calcFormula.SyntaxError));

            return calcFormula.Evaluate(reference);
        }

        public IEnumerable<CalculationFunction> GetAllFunctions() {
            UltraCalcFunctionFactory factory = _calcEngine.FunctionFactory;

            return new TypedEnumerable<CalculationFunction>(factory);
        }

        public bool RegisterUserDefinedFunction(CustomCalculationFunction userDefinedFunction) {
            return _calcEngine.AddFunction(userDefinedFunction);
        }

        #endregion Interface Implementors


        internal ICalculationFormula CompileFormula(FormulaCalculationReference baseReference, string formula, bool suppressSyntaxErrorEvent) {
			CoreUtilities.ValidateNotNull( baseReference );
			CoreUtilities.ValidateNotEmpty( formula );

			UltraCalcFormula calcFormula = new UltraCalcFormula( baseReference, formula, _calcEngine.FunctionFactory );

			return calcFormula;
        }
    }
}
