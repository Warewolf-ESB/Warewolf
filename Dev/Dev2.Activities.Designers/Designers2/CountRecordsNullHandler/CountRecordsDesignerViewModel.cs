/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Utils;
using Dev2.Studio.Interfaces;

namespace Dev2.Activities.Designers2.CountRecordsNullHandler
{
    public class CountRecordsDesignerViewModel : ActivityDesignerViewModel
    {
        public CountRecordsDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            RecordsetNameValue = RecordsetName;
            AddTitleBarLargeToggle();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Recordset_Count;
        }
        
        public string RecordsetNameValue { get => (string)GetValue(RecordsetNameValueProperty); set => SetValue(RecordsetNameValueProperty, value); }

        public static readonly DependencyProperty RecordsetNameValueProperty =
            DependencyProperty.Register("RecordsetNameValue", typeof(string), typeof(CountRecordsDesignerViewModel), new PropertyMetadata(null, OnRecordsetNameValueChanged));
        
        string RecordsetName { set => SetProperty(value); get => GetProperty<string>(); }

        static void OnRecordsetNameValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (CountRecordsDesignerViewModel)d;
            var value = e.NewValue as string;

            if(!string.IsNullOrWhiteSpace(value))
            {
                viewModel.RecordsetName = ActivityDesignerLanuageNotationConverter.ConvertToTopLevelRSNotation(value); 
            }
        }
        
        public override void Validate()
        {
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
