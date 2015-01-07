using System;
using System.Collections.Generic;
using Dev2;
using Dev2.Common.Interfaces.ErrorHandling;

namespace Warewolf.Core
{
    public class WarewolfExceptionHandler : IExceptionHandler
    {
        #region Implementation of IExceptionHandler

        private readonly IDictionary<Type, Action> _errorHandlers;

        public WarewolfExceptionHandler(IDictionary<Type, Action> errorHandlers)
        {
            VerifyArgument.IsNotNull("errorHandlers",errorHandlers);
            _errorHandlers = errorHandlers;
        }

        public void AddHandler(Type t, Action a)
        {
            _errorHandlers.Add(t,a);
        }
        public void Handle(Exception error)
        {
            if(_errorHandlers.ContainsKey(error.GetType()))
            {
                _errorHandlers[error.GetType()].Invoke();
            }
            else
            {
                throw error;
            }
        }

        #endregion
    }

}
