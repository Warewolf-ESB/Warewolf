using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Common;
using Dev2.Converters.DateAndTime;

namespace Dev2.Activities.Designers2.Random
{
    public class RandomDesignerViewModel : ActivityDesignerViewModel
    {
        public RandomDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();
            TimeModifierTypes = new List<string>(RandomFormatter.TimeModifierTypes);
        }

        public List<string> TimeModifierTypes { get; private set; }

        public string Dev2DefaultRandom { get { return GlobalConstants.Dev2CustomDefaultRandomFormat; } }

        public string SelectedTimeModifierType
        {
            get { return (string)GetValue(SelectedTimeModifierTypeProperty); }
            set { SetValue(SelectedTimeModifierTypeProperty, value); }
        }

        public static readonly DependencyProperty SelectedTimeModifierTypeProperty =
            DependencyProperty.Register("SelectedTimeModifierType", typeof(string), typeof(RandomDesignerViewModel), new PropertyMetadata(null, OnSelectedTimeModifierTypeChanged));

        static void OnSelectedTimeModifierTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (RandomDesignerViewModel)d;
            var value = e.NewValue as string;

            if(string.IsNullOrWhiteSpace(value))
            {
                viewModel.TimeModifierAmountDisplay = value;
            }
            viewModel.TimeModifierType = value;
        }

        // DO NOT bind to these properties - these are here for convenience only!!!
        string TimeModifierType { set { SetProperty(value); } }
        string TimeModifierAmountDisplay { set { SetProperty(value); } }

        public override void Validate()
        {
        }
    }
}