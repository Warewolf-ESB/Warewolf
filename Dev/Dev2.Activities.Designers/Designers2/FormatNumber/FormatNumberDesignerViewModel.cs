using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.DataList.Contract;

namespace Dev2.Activities.Designers2.FormatNumber
{
    public class FormatNumberDesignerViewModel : ActivityDesignerViewModel
    {
        public FormatNumberDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();
            RoundingTypes = new List<string>(Dev2EnumConverter.ConvertEnumsTypeToStringList<enRoundingType>());
            SelectedRoundingType = string.IsNullOrEmpty(RoundingType) ? RoundingTypes[0] : RoundingType;
           
        }

        public List<string> RoundingTypes { get; set; }

        public string SelectedRoundingType { get { return (string)GetValue(SelectedRoundingTypeProperty); } set { SetValue(SelectedRoundingTypeProperty, value); } }
        public static readonly DependencyProperty SelectedRoundingTypeProperty =
        DependencyProperty.Register("SelectedRoundingType", typeof(string), typeof(FormatNumberDesignerViewModel), new PropertyMetadata(null, OnSelectedRoundingTypeChanged));

        // DO NOT bind to these properties - these are here for convenience only!!!
        string RoundingType { set { SetProperty(value); } get { return GetProperty<string>(); } }
        string RoundingDecimalPlaces { set { SetProperty(value); } }

        public bool IsRoundingEnabled { get { return (bool)GetValue(IsRoundingEnabledProperty); } set { SetValue(IsRoundingEnabledProperty, value); } }

        public static readonly DependencyProperty IsRoundingEnabledProperty =
           DependencyProperty.Register("IsRoundingEnabled", typeof(bool), typeof(FormatNumberDesignerViewModel), new PropertyMetadata(false));

        static void OnSelectedRoundingTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (FormatNumberDesignerViewModel)d;
            var value = e.NewValue as string;

            enRoundingType roundingType;
            if(Enum.TryParse(value, out roundingType))
            {
                if(roundingType == enRoundingType.None)
                {
                    viewModel.RoundingDecimalPlaces = string.Empty;
                    viewModel.IsRoundingEnabled = false;
                }
                else
                {
                    viewModel.IsRoundingEnabled = true;
                }
            }
            viewModel.RoundingType = value;
        }

        public override void Validate()
        {
        }
    }
}