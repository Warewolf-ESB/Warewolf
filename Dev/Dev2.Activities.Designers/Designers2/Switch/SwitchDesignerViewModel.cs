using System.Activities.Presentation.Model;
using System.Linq;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Common;
using Dev2.Data.SystemTemplates;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Interfaces;
using Dev2.Utilities;

namespace Dev2.Activities.Designers2.Switch
{
    public class SwitchDesignerViewModel : ActivityDesignerViewModel
    {
        string _switchVariable;

        public SwitchDesignerViewModel(ModelItem mi, string display):base(mi)
        {
            Initialize(display);
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Flow_Switch;
        }

        void Initialize(string display)
        {
            var expressionText = ModelItem.Properties[GlobalConstants.SwitchExpressionTextPropertyText];
            ModelProperty switchCaseValue = ModelItem.Properties["Case"];
            Dev2Switch ds;
            if (expressionText?.Value != null)
            {
                ds = new Dev2Switch();
                var val = ActivityHelper.ExtractData(expressionText.Value.ToString());
                if (!string.IsNullOrEmpty(val))
                {
                    ds.SwitchVariable = val;
                }
            }
            else
            {
                ds = DataListConstants.DefaultSwitch;
            }
            if (string.IsNullOrEmpty(display))
            {
                var displayName = ModelItem.Properties[GlobalConstants.DisplayNamePropertyText];
                if (displayName?.Value != null)
                {
                    ds.DisplayText = displayName.Value.ToString();
                }
            }
            else
            {
                ds.DisplayText = display; 
            }
            if (switchCaseValue != null)
            {
                string val = switchCaseValue.ComputedValue.ToString();
                ds.SwitchExpression = val;
            }
          
            SwitchVariable = ds.SwitchVariable;
            SwitchExpression = ds.SwitchExpression;
            DisplayText = ds.DisplayText;
            if(DisplayText!= SwitchVariable && DisplayText != "Switch")
            {
                _hascustomeDisplayText = true;
            }
        }

        public string SwitchExpression { get; set; }
        public string SwitchVariable
        {
            get
            {
                return _switchVariable;
            }
            set
            {
                _switchVariable = value;
                if(string.IsNullOrEmpty(value))
                {
                    DisplayText = "Switch";
                    DisplayName = "Switch";
                }
                else if(!_hascustomeDisplayText ||string.IsNullOrEmpty(DisplayText) )
                {
                    DisplayText = value;
                    DisplayName = value;
                }
            }
        }
        public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register("DisplayText", typeof(string), typeof(SwitchDesignerViewModel), new PropertyMetadata(default(string)));
        bool _hascustomeDisplayText;

        public string DisplayText
        {
            get { return (string)GetValue(DisplayTextProperty); }
            set
            {
                if(string.IsNullOrEmpty(value))
                {
                    _hascustomeDisplayText = false;
                }
                SetValue(DisplayTextProperty, value);
            }
        }

        public Dev2Switch Switch
        {
            get
            {
                var dev2Switch = new Dev2Switch
                {
                    DisplayText = DisplayText,
                    SwitchVariable = SwitchVariable,
                    SwitchExpression = SwitchExpression
                };
                return dev2Switch;
            }
        }
        
        public override void Validate()
        {
            ValidExpression = true;
            if (ModelItem?.Parent?.Source?.Collection != null)
            {
                ValidateProperties();
            }
            else
            {
                if (ModelItem != null)
                {
                    if (ModelItem.Properties.Any())
                    {
                        foreach (var property in ModelItem.Properties)
                        {
                            if (property?.Name == "Case")
                            {
                                var modelItem = property.ComputedValue;
                                if (modelItem?.ToString() == SwitchExpression)
                                {
                                    ValidExpression = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ValidateProperties()
        {
            if (ModelItem?.Parent?.Source?.Collection != null)
            {
                foreach (var value in ModelItem.Parent.Source.Collection)
                {
                    if (value?.Properties.Any(property => property.Name == "Key") ?? false)
                    {
                        var modelItem = value.Properties["Key"]?.ComputedValue;
                        if (modelItem?.ToString() == SwitchExpression)
                        {
                            ValidExpression = false;
                            break;
                        }
                    }
                }
            }
        }

        public bool ValidExpression;

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
