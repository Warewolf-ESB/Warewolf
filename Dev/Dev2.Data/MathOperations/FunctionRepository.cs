
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
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Infragistics.Calculations.CalcManager;
using Infragistics.Calculations.Engine;

// ReSharper disable CheckNamespace
namespace Dev2.MathOperations
{
    // PBI: 1214
    //This repository will contain a collection of all the functions available to the Function Manager 
    // to perform evaluations on
    public class FunctionRepository : IFrameworkRepository<IFunction>
    {
        private readonly List<IFunction> _functions;
        private static readonly IDev2CalculationManager CalcManager = new Dev2CalculationManager();
        private bool _isDisposed;

        internal FunctionRepository()
        {
            _functions = new List<IFunction>();
        }

        /// <summary>
        /// Returns the entire collection of functions.
        /// </summary>
        /// <returns></returns>
        public ICollection<IFunction> All()
        {
            if(_functions != null)
            {
                return _functions;
            }
            return new List<IFunction>();
        }

        /// <summary>
        /// Finds a collection of functions that satisfy a condition specified by the expression passed in
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public ICollection<IFunction> Find(System.Linq.Expressions.Expression<Func<IFunction, bool>> expression)
        {
            if(expression != null)
            {
                return _functions.AsQueryable().Where(expression).ToList();
            }
// ReSharper disable NotResolvedInText
            throw new ArgumentNullException(@"Expression cannot be null");
// ReSharper restore NotResolvedInText
        }

        /// <summary>
        /// Finds the first function in the collection that satisfies the expression passed in
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public IFunction FindSingle(System.Linq.Expressions.Expression<Func<IFunction, bool>> expression)
        {
            if(expression != null)
            {

                try
                {
                    return _functions.AsQueryable().First(expression);
                }
                catch(InvalidOperationException ioex)
                {
                    Dev2Logger.Log.Error(ioex);
                    IFunction func = MathOpsFactory.CreateFunction();
                    return func;
                }
            }
            return null;
        }


        /// <summary>
        /// // Load the Repository of all the functions from the CalculationManager.
        /// </summary>
        public void Load()
        {
            IEnumerable<CalculationFunction> calcFunctions = CalcManager.GetAllFunctions();

            foreach(CalculationFunction calcFunction in calcFunctions)
            {
                _functions.Add(MathOpsFactory.CreateFunction(calcFunction.Name, calcFunction.ArgList, calcFunction.ArgDescriptors, calcFunction.Description));
            }

        }

        /// <summary>
        /// Removes a collection of functions from the function repository
        /// </summary>
        /// <param name="instanceObjs"></param>
        public void Remove(ICollection<IFunction> instanceObjs)
        {
            if(instanceObjs != null)
            {
                foreach(IFunction func in instanceObjs)
                {
                    _functions.Remove(func);
                }
            }
            else
            {
// ReSharper disable NotResolvedInText
                throw new ArgumentNullException("Cannot remove null List of functions");
// ReSharper restore NotResolvedInText
            }
        }

        /// <summary>
        /// Removes a function from the function repository
        /// </summary>
        /// <param name="instanceObj"></param>
        public void Remove(IFunction instanceObj)
        {
            if(instanceObj != null)
            {
                _functions.Remove(instanceObj);
            }
            else
            {
// ReSharper disable NotResolvedInText
                throw new ArgumentNullException("Function cannot be null");
// ReSharper restore NotResolvedInText
            }
        }

        /// <summary>
        /// Save A collection of new functions to the function library
        /// </summary>
        /// <param name="instanceObjs"></param>
        public void Save(ICollection<IFunction> instanceObjs)
        {
            if(instanceObjs != null)
            {
                _functions.AddRange(instanceObjs);
            }
            else
            {
// ReSharper disable NotResolvedInText
                throw new ArgumentNullException("Cannot Save a Null list of functions");
// ReSharper restore NotResolvedInText
            }
        }
        /// <summary>
        /// Save a collection of new user-defined functions to the function library
        /// </summary>
        /// <param name="instanceObj"></param>
        public string Save(IFunction instanceObj)
        {
            if(instanceObj != null)
            {
                _functions.Add(instanceObj);
            }
            else
            {
// ReSharper disable NotResolvedInText
                throw new ArgumentNullException("Function cannot be null");
// ReSharper restore NotResolvedInText
            }
            return "Saved";
        }

        public event EventHandler ItemAdded;

        protected void OnItemAdded()
        {
            if(ItemAdded != null)
            {
                ItemAdded(this, new EventArgs());
            }
        }

        #region Implementation of IDisposable

        ~FunctionRepository()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if(!_isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if(disposing)
                {
                    // Dispose managed resources.                    
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                _isDisposed = true;
            }
        }

        #endregion
    }
}
