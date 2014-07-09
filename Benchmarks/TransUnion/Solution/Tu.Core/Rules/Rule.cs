using System;
using System.Collections.Generic;

namespace Tu.Rules
{
    public class Rule : IRule
    {
        readonly string _fieldName;

        public Rule(IValidator validator, string fieldName)
        {
            if(validator == null)
            {
                throw new ArgumentNullException("validator");
            }
            if(string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            Errors = new List<string>();
            Validator = validator;
            _fieldName = fieldName;
        }

        public IValidator Validator { get; private set; }

        #region IRule Members

        public List<string> Errors { get; private set; }

        public virtual bool IsValid(object value)
        {
            return false;
        }

        #endregion

        public bool IsValid(Func<bool> validator, string errorMessage)
        {
            var isValid = validator();
            if(!isValid)
            {
                if(!string.IsNullOrEmpty(errorMessage))
                {
                    Errors.Add(_fieldName + " " + errorMessage);
                }
            }
            return isValid;
        }
    }
}