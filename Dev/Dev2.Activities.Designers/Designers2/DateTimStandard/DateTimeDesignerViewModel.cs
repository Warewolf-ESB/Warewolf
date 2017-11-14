/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Common;
using Dev2.Common.DateAndTime;
using Dev2.Studio.Interfaces;

namespace Dev2.Activities.Designers2.DateTimStandard
{
    public class DateTimeDesignerViewModel : ActivityDesignerViewModel
    {
        public DateTimeDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            TimeModifierTypes = new List<string>(DateTimeFormatter.TimeModifierTypes);
            SelectedTimeModifierType = string.IsNullOrEmpty(TimeModifierType) ? TimeModifierTypes[0] : TimeModifierType;
            AddTitleBarLargeToggle();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Utility_Date_Time;
            if (string.IsNullOrEmpty(InputFormat))
            {
                InputFormat = GlobalConstants.Dev2DotNetDefaultDateTimeFormat;

            }
            if (string.IsNullOrEmpty(OutputFormat))
            {
                OutputFormat = GlobalConstants.Dev2DotNetDefaultDateTimeFormat;
            }
        }

        public List<string> TimeModifierTypes { get; private set; }


        public string SelectedTimeModifierType
        {
            get { return (string)GetValue(SelectedTimeModifierTypeProperty); }
            set { SetValue(SelectedTimeModifierTypeProperty, value); }
        }

        public static readonly DependencyProperty SelectedTimeModifierTypeProperty =
            DependencyProperty.Register("SelectedTimeModifierType", typeof(string), typeof(DateTimeDesignerViewModel), new PropertyMetadata(null, OnSelectedTimeModifierTypeChanged));

        static void OnSelectedTimeModifierTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (DateTimeDesignerViewModel)d;
            var value = e.NewValue as string;

            if (string.IsNullOrWhiteSpace(value))
            {
                viewModel.SetTimeModifierAmountDisplay(value);
            }
            viewModel.TimeModifierType = value;
        }

        string TimeModifierType { set => SetProperty(value); get { return GetProperty<string>(); } }

        private void SetTimeModifierAmountDisplay(string value) { SetProperty(value); }
        string InputFormat { set => SetProperty(value); get => GetProperty<string>(); }
        string OutputFormat { set => SetProperty(value); get => GetProperty<string>(); }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        public override void Validate()
        {
            throw new System.NotImplementedException();
        }
    }
}
