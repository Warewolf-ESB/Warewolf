using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.ViewModels.Base
{
    public class ValidationController : SimpleBaseViewModel
    {
        protected Dictionary<string, string> ValidationErrors = new Dictionary<string, string>();

        protected void AddError(string key, string value)
        {
            KeyValuePair<string, string> errorInfo = new KeyValuePair<string, string>(key, value);

            if(!ValidationErrors.ContainsKey(key))
            {
                if(!ValidationErrors.Contains(errorInfo))
                {
                    ValidationErrors.Add(key, value);
                }
            }
        }

        protected void RemoveError(string key)
        {
            ValidationErrors.Remove(key);
        }

        protected string ValidateStringCannotBeNullOrEmpty(string parameterName, string parameterValue)
        {
            string error = null;


            if(string.IsNullOrEmpty(parameterValue))
            {
                error = string.Format("{0} is required", parameterName);
                AddError(parameterName, error);
            }

            if(error == null)
            {
                RemoveError(parameterName);
            }

            return error;

        }

    }
}
