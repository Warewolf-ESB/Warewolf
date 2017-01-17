/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.ComponentModel;
using Dev2.Studio.Core.Interfaces;
using Warewolf.Resource.Errors;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Models
{
    public class OperatorType : IDataErrorInfo, IOperatorType
    {
        public OperatorType()
        {

        }

        public OperatorType(string operatorName, string friendlyName, string operatorSymbol, dynamic parent, bool showEndValue = true)
        {
            OperatorName = operatorName;
            FriendlyName = friendlyName;
            OperatorSymbol = operatorSymbol;
            Parent = parent;
            ShowEndValue = showEndValue;
        }

        public string OperatorName { get; set; }
        public string FriendlyName { get; set; }
        public string OperatorSymbol { get; set; }
        public dynamic Parent { get; set; }
        public string TagName { get; set; }
        public object Value { get; set; }
        public object EndValue { get; set; }
        public bool Selected { get; set; }
        public bool ShowEndValue
        {
            get;
            set;

        }
        public string Expression
        {
            get
            {
                if(!string.IsNullOrEmpty(TagName))
                {
                    return string.Format("d.Get(\"{0}\",AmbientDataList)", TagName);
                }
                return string.Empty;
            }
        }
        public bool IsValid => true;

        #region IDataErrorInfo Members

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                string error = null;

                switch(columnName)
                {
                    case "TagName":
                        error = ValidateStringCannotBeNull(columnName, TagName);
                        break;

                    case "Value":
                        if(ShowEndValue)
                        {
                            error = ValidateStringCannotBeNull(columnName, Value?.ToString() ?? string.Empty);
                        }
                        break;

                    case "EndValue":
                        error = ValidateStringCannotBeNull(columnName, EndValue?.ToString() ?? string.Empty);
                        break;

                    default:
                        throw new ArgumentException(ErrorResource.UnexpectedPropertyName, columnName);
                }

                return error;
            }
        }

        private string ValidateStringCannotBeNull(string propertyName, string value)
        {
            string error = null;

            if(Selected)
            {
                if(string.IsNullOrEmpty(value))
                {
                    error = string.Format(ErrorResource.IsRequired, propertyName);

                }

                if(error == null)
                {

                }
            }

            return error;
        }

        #endregion
    }
}
