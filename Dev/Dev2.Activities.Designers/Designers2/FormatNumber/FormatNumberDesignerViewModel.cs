using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Common;
using Dev2.DataList.Contract;

namespace Dev2.Activities.Designers2.FormatNumber
{
    public class FormatNumberDesignerViewModel : ActivityDesignerViewModel
    {
        public static readonly DependencyProperty SelectedRoundingTypeProperty =
            DependencyProperty.Register("SelectedRoundingType", typeof(string), typeof(FormatNumberDesignerViewModel), new PropertyMetadata(null, OnSelectedRoundingTypeChanged));
        public static readonly DependencyProperty IsRoundingEnabledProperty =
            DependencyProperty.Register("IsRoundingEnabled", typeof(bool), typeof(FormatNumberDesignerViewModel), new PropertyMetadata(false));

        public FormatNumberDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();
        }

        public List<string> RoundingTypes { get { return new List<string>(Dev2EnumConverter.ConvertEnumsTypeToStringList<enRoundingType>()); } }

        public string SelectedRoundingType { get { return (string)GetValue(SelectedRoundingTypeProperty); } set { SetValue(SelectedRoundingTypeProperty, value); } }

        // DO NOT bind to these properties - these are here for convenience only!!!
        string RoundingType { set { SetProperty(value); } }
        string RoundingDecimalPlaces { set { SetProperty(value); } }

        public bool IsRoundingEnabled { get { return (bool)GetValue(IsRoundingEnabledProperty); } set { SetValue(IsRoundingEnabledProperty, value); } }

        static void OnSelectedRoundingTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (FormatNumberDesignerViewModel)d;
            var value = e.NewValue as string;

            enRoundingType roundingType;
            if(Enum.TryParse(value, out roundingType))
            {
                if(roundingType == enRoundingType.None)
                {
                    //RoundingDecimalPlaces = string.Empty;
                    //IsRoundingEnabled = false;
                }
                else
                {
                    // IsRoundingEnabled = true;
                }
            }
        }

        public override void Validate()
        {
        }
    }
}