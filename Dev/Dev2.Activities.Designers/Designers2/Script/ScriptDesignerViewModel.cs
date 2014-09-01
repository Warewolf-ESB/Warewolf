using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Enums.Enums;

namespace Dev2.Activities.Designers2.Script
{
    public class ScriptDesignerViewModel : ActivityDesignerViewModel
    {
        public ScriptDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();
            ScriptTypes = Dev2EnumConverter.ConvertEnumsTypeToStringList<enScriptType>();
            SelectedScriptType = Dev2EnumConverter.ConvertEnumValueToString(ScriptType);
        }

        public IList<string> ScriptTypes { get; private set; }

        public string SelectedScriptType
        {
            get { return (string)GetValue(SelectedScriptTypeProperty); }
            set { SetValue(SelectedScriptTypeProperty, value); }
        }

        public static readonly DependencyProperty SelectedScriptTypeProperty =
            DependencyProperty.Register("SelectedScriptType", typeof(string), typeof(ScriptDesignerViewModel), new PropertyMetadata(null, OnSelectedScriptTypeChanged));

        static void OnSelectedScriptTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (ScriptDesignerViewModel)d;
            var value = e.NewValue as string;

            if(!string.IsNullOrWhiteSpace(value))
            {
                switch(value)
                {
                    case "Ruby":
                        viewModel.ScriptTypeDefaultText = "Ruby Syntax";
                        break;
                    case "Python":
                        viewModel.ScriptTypeDefaultText = "Python Syntax";
                        break;
                    default:
                        viewModel.ScriptTypeDefaultText = "JavaScript Syntax";
                        break;
                }
                viewModel.ScriptType = (enScriptType)Dev2EnumConverter.GetEnumFromStringDiscription(value, typeof(enScriptType));
            }
        }


        public string ScriptTypeDefaultText
        {
            get { return (string)GetValue(ScriptTypeTextProperty); }
            set { SetValue(ScriptTypeTextProperty, value); }
        }

        public static readonly DependencyProperty ScriptTypeTextProperty =
            DependencyProperty.Register("ScriptTypeDefaultText", typeof(string), typeof(ScriptDesignerViewModel), new PropertyMetadata(null));

        // DO NOT bind to these properties - these are here for convenience only!!!
        enScriptType ScriptType { set { SetProperty(value); } get { return GetProperty<enScriptType>(); } }

        public override void Validate()
        {
        }
    }
}