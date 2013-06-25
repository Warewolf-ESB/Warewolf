using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using System.Windows.Input;
using Dev2.Studio.Core.ViewModels.Base;

namespace Dev2.Studio.Core.ViewModels {
    public class ConfigureDecisionViewModel : IConfigureDecisionViewModel {

        RelayCommand _okCommand;
        public List<DecisionType> decisionTypes = new List<DecisionType>{new NormalDecisionType(),  new StringDecisionType(), new DateDecisionType()};

        public event OperatorTypeEventHandler OnExpressionBuilt;
        public event EventHandler OnUserClose;



        public dynamic DecisionTypes {
            get {

                return decisionTypes;
            }
        }


        private DecisionType _selectedDecisionType;
        public DecisionType SelectedDecisionType {
            get {
                if (_selectedDecisionType == null) {
                    var select = decisionTypes.Where(c => c.GetType() == typeof(NormalDecisionType));
                    if (select.Count() > 0) {
                        _selectedDecisionType = select.FirstOrDefault();
                    }
                }

                return _selectedDecisionType;
            }
            set {
                _selectedDecisionType = value;
            }
        }



                public ICommand OkCommand {
            get {
                if (_okCommand == null) {
                    _okCommand = new RelayCommand(param => UserOK(), param => CanSelect);
                }
                return _okCommand;
            }
        }



        private void UserOK() {

            if (SelectedDecisionType != null) {
                if (OnExpressionBuilt != null) {
                    OnExpressionBuilt(SelectedDecisionType.GetExpression());
                }
            }
            else {
                if (OnUserClose != null) {
                    OnUserClose(this, null);
                }
            }


        }

        public bool CanSelect {
            get {
                if(SelectedDecisionType is NormalDecisionType){
                    return true;
                }
                var test = SelectedDecisionType.OperatorTypes.Where(c => c.Selected);
                if (test.Any()) {
                    string tag = test.FirstOrDefault().TagName;
                    object value = test.FirstOrDefault().Value;
                    value = value == null ? string.Empty : value.ToString();
                    object endvalue = test.FirstOrDefault().EndValue;
                    endvalue = endvalue == null ? string.Empty : endvalue.ToString();
                    if (test is BetweenOperatorType) {
                        if (string.IsNullOrEmpty(tag)) {
                            return false;
                        }
                        if (string.IsNullOrEmpty(value.ToString())) {
                            return false;
                        }
                        if(string.IsNullOrEmpty(endvalue.ToString())){
                            return false;
                        }
                        return true;
                    
                    }
                    else{
                        if(string.IsNullOrEmpty(tag)){
                            return false;
                        }
                        if(string.IsNullOrEmpty(value.ToString())){
                            if (test.FirstOrDefault().ShowEndValue) {
                                return false;
                            }
                        }
                        return true;
                    }
                }
                return false;

            }
        }
    }
}
