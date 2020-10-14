/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Foreach;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Data.Interfaces.Enums;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;

namespace Dev2.Activities.Designers2.SuspendExecution
{
    public class SuspendExecutionDesignerViewModel : ActivityDesignerViewModel
    {
        public SuspendExecutionDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            SuspendOptions = Dev2EnumConverter.ConvertEnumsTypeToStringList<enSuspendOption>();
            SelectedSuspendOption = SuspendOption.GetDescription();
            AddTitleBarLargeToggle();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Flow_SuspendExecution;
            var dataFunc = modelItem.Properties["DataFunc"]?.ComputedValue as ActivityFunc<string, bool>;
            DataFuncDisplayName = dataFunc?.Handler == null ? "" : dataFunc?.Handler?.DisplayName;
            var type = dataFunc?.Handler?.GetType();
            if (type != null)
            {
                DataFuncIcon = ModelItemUtils.GetImageSourceForToolFromType(type);
            }
        }

        public IList<string> SuspendOptions { get; private set; }

        public string SelectedSuspendOption
        {
            get => (string)GetValue(SelectedSuspendOptionProperty);
            set => SetValue(SelectedSuspendOptionProperty, value);
        }

        public static readonly DependencyProperty SelectedSuspendOptionProperty =
            DependencyProperty.Register("SelectedSuspendOption", typeof(string), typeof(SuspendExecutionDesignerViewModel), new PropertyMetadata("", OnSelectedSuspendOptionChanged));

        public string SuspendOptionWatermark
        {
            get => (string)GetValue(SuspendOptionWatermarkProperty);
            set => SetValue(SuspendOptionWatermarkProperty, value);
        }

        public static readonly DependencyProperty SuspendOptionWatermarkProperty =
            DependencyProperty.Register("SuspendOptionWatermark", typeof(string), typeof(SuspendExecutionDesignerViewModel), new PropertyMetadata( null));

        private static void OnSelectedSuspendOptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (SuspendExecutionDesignerViewModel) d;
            var value = e.NewValue as string;

            if (!string.IsNullOrWhiteSpace(value))
            {
                switch (value)
                {
                    case "Suspend until:":
                        viewModel.SuspendOptionWatermark = "Date Time";
                        break;
                    case "Suspend for Second(s):":
                        viewModel.SuspendOptionWatermark = "Second(s)";
                        break;
                    case "Suspend for Minute(s):":
                        viewModel.SuspendOptionWatermark = "Minute(s)";
                        break;
                    case "Suspend for Hour(s):":
                        viewModel.SuspendOptionWatermark = "Hour(s)";
                        break;
                    case "Suspend for Day(s):":
                        viewModel.SuspendOptionWatermark = "Day(s)";
                        break;
                    case "Suspend for Month(s):":
                        viewModel.SuspendOptionWatermark = "Month(s)";
                        break;

                }

                viewModel.SuspendOption =
                    (enSuspendOption) Dev2EnumConverter.GetEnumFromStringDiscription(value, typeof(enSuspendOption));
            }
        }

        public enSuspendOption SuspendOption
        {
            get => GetProperty<enSuspendOption>();
            set => SetProperty(value);
        }

        public string DataFuncDisplayName
        {
            get => (string)GetValue(DataFuncDisplayNameProperty);
            set => SetValue(DataFuncDisplayNameProperty, value);
        }

        public static readonly DependencyProperty DataFuncDisplayNameProperty =
            DependencyProperty.Register("DataFuncDisplayName", typeof(string), typeof(ForeachDesignerViewModel), new PropertyMetadata(null));

        public ImageSource DataFuncIcon
        {
            get => (ImageSource)GetValue(DataFuncIconProperty);
            set => SetValue(DataFuncIconProperty, value);
        }

        public static readonly DependencyProperty DataFuncIconProperty =
            DependencyProperty.Register("DataFuncIcon", typeof(ImageSource), typeof(ForeachDesignerViewModel), new PropertyMetadata(null));


        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}