using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Calculations;
using Infragistics.Calculations.Engine;
using Infragistics.Calculations.CalcManager;

namespace Dev2.MathOperations {
    // PBI: 1214
    //This repository will contain a collection of all the functions available to the Function Manager 
    // to perform evaluations on
    public class FunctionRepository : IFrameworkRepository<IFunction> {
        private List<IFunction> _functions;
        private static IDev2CalculationManager _calcManager = new Dev2CalculationManager();

        internal FunctionRepository() {
            _functions = new List<IFunction>();
        }

        /// <summary>
        /// Returns the entire collection of functions.
        /// </summary>
        /// <returns></returns>
        public ICollection<IFunction> All() {
            if(_functions != null) {
                return _functions;
            }
            else return new List<IFunction>();
        }


        /// <summary>
        /// Finds a collection of functions that satisfy a condition specified by the expression passed in
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public ICollection<IFunction> Find(System.Linq.Expressions.Expression<Func<IFunction, bool>> expression) {
            if(expression != null) {
                return _functions.AsQueryable().Where(expression).ToList();
            }
            else {
                throw new ArgumentNullException("Expression cannot be null");
            }

        }

        /// <summary>
        /// Finds the first function in the collection that satisfies the expression passed in
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public IFunction FindSingle(System.Linq.Expressions.Expression<Func<IFunction, bool>> expression) {
            if(expression != null) {

                try {
                    return _functions.AsQueryable().First(expression);
                }
                catch(InvalidOperationException) {
                    IFunction func = MathOpsFactory.CreateFunction();
                    return func;
                }
            }
            else {
                throw new ArgumentNullException("Expression cannot be null");
            }
        }


        /// <summary>
        /// // Load the Repository of all the functions from the CalculationManager.
        /// </summary>
        public void Load() {
            IEnumerable<CalculationFunction> calcFunctions = _calcManager.GetAllFunctions();

            foreach(CalculationFunction calcFunction in calcFunctions) {
                _functions.Add(MathOpsFactory.CreateFunction(calcFunction.Name, calcFunction.ArgList, calcFunction.ArgDescriptors, calcFunction.Description));
            }

        }

        /// <summary>
        /// Removes a collection of functions from the function repository
        /// </summary>
        /// <param name="instanceObjs"></param>
        public void Remove(ICollection<IFunction> instanceObjs) {
            if(instanceObjs != null) {
                foreach(IFunction func in instanceObjs) {
                    _functions.Remove(func);
                }
            }
            else {
                throw new ArgumentNullException("Cannot remove null List of functions");
            }
        }

        /// <summary>
        /// Removes a function from the function repository
        /// </summary>
        /// <param name="instanceObj"></param>
        public void Remove(IFunction instanceObj) {
            if(instanceObj != null) {
                _functions.Remove(instanceObj);
            }
            else {
                throw new ArgumentNullException("Function cannot be null");
            }
        }

        /// <summary>
        /// Save A collection of new functions to the function library
        /// </summary>
        /// <param name="instanceObjs"></param>
        public void Save(ICollection<IFunction> instanceObjs) {
            if(instanceObjs != null) {
                _functions.AddRange(instanceObjs);
            }
            else {
                throw new ArgumentNullException("Cannot Save a Null list of functions");
            }
        }
        /// <summary>
        /// Save a collection of new user-defined functions to the function library
        /// </summary>
        /// <param name="instanceObj"></param>
        public void Save(IFunction instanceObj) {
            if(instanceObj != null) {
                _functions.Add(instanceObj);
            }
            else {
                throw new ArgumentNullException("Function cannot be null");
            }
        }

        public event EventHandler ItemAdded;

        protected void OnItemAdded()
        {
            if (ItemAdded != null)
            {
                ItemAdded(this, new EventArgs());
            }
        }
    }
}
