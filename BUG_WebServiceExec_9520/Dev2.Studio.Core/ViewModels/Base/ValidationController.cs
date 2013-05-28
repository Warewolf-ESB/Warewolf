using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace Dev2.Studio.Core.ViewModels.Base
{
    public class ValidationController : SimpleBaseViewModel
    {
        protected Dictionary<string, string> validationErrors = new Dictionary<string, string>();

        protected void AddError(string key, string value)
        {
            KeyValuePair<string, string> errorInfo = new KeyValuePair<string, string>(key, value);

            if (!validationErrors.ContainsKey(key))
            {
                if (!validationErrors.Contains(errorInfo))
                {
                    validationErrors.Add(key, value);
                }
            }
        }

        protected void RemoveError(string key)
        {
            validationErrors.Remove(key);
        }

        protected string ValidateStringCannotBeNullOrEmpty(string parameterName, string parameterValue)
        {
            string error = null;


            if (string.IsNullOrEmpty(parameterValue))
            {
                error = string.Format("{0} is required", parameterName);
                AddError(parameterName, error);
            }

            if (error == null)
            {
                RemoveError(parameterName);
            }

            return error;

        }

    }
}
