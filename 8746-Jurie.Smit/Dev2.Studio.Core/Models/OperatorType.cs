using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Framework;

namespace Dev2.Studio.Core.Models {
    public class OperatorType : IDataErrorInfo, IOperatorType {
        public OperatorType() {
            
        }

        public OperatorType(string operatorName, string friendlyName, string operatorSymbol, dynamic parent, bool showEndValue=true) {
            this.OperatorName = operatorName;
            this.FriendlyName = friendlyName;
            this.OperatorSymbol = operatorSymbol;
            this.Parent = parent;
            this.ShowEndValue = showEndValue;
        }

        public string OperatorName { get; set; }
        public string FriendlyName { get; set; }
        public string OperatorSymbol { get; set; }
        public dynamic Parent { get; set; }
        public string TagName {get;set;}
        public object Value { get; set; }
        public object EndValue { get; set; }
        public bool Selected { get; set; }
        public bool ShowEndValue {
            get;
            set;

        }
        public string Expression {
            get {
                if (!string.IsNullOrEmpty(TagName)) {
                    return string.Format("d.Get(\"{0}\",AmbientDataList)", TagName);
                }
                return string.Empty;
            }
        }
        public bool IsValid {
            get {
                return true;
            }

        }

        #region IDataErrorInfo Members

        public string Error {
            get { return null; }
        }

        public string this[string columnName] {
            get {
                string error = null;

                switch (columnName) {
                    case "TagName":
                    error = ValidateStringCannotBeNull(columnName, TagName);
                    break;

                    case "Value":
                    if (ShowEndValue) {
                        error = ValidateStringCannotBeNull(columnName, Value == null ? string.Empty : Value.ToString());
                    }
                    break;

                    case "EndValue":
                    error = ValidateStringCannotBeNull(columnName, EndValue == null ? string.Empty : EndValue.ToString());
                    break;

                    default:
                    throw new ArgumentException("Unexpected property name encountered", columnName);
                }

                return error;
            }
        }

        private string ValidateStringCannotBeNull(string propertyName, string value ) {
            string error = null;

            if (this.Selected) {
                if (string.IsNullOrEmpty(value)) {
                    error = string.Format("{0} is required", propertyName);

                }

                if (error == null) {
                   
                }
            }

            return error;
        }

        #endregion
    }
}
