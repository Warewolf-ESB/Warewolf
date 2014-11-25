
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Converters.DateAndTime;

namespace Dev2.Activities.Designers2.DateTimeDifference
{
    public class DateTimeDifferenceDesignerViewModel : ActivityDesignerViewModel
    {
        public DateTimeDifferenceDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();
            OutputTypes = new List<string>(DateTimeComparer.OutputFormatTypes);
            SelectedOutputType = string.IsNullOrEmpty(OutputType) ? OutputTypes[0] : OutputType;
        }

        public List<string> OutputTypes { get; private set; }

        public string SelectedOutputType
        {
            get { return (string)GetValue(SelectedOutputTypeProperty); }
            set { SetValue(SelectedOutputTypeProperty, value); }
        }

        public static readonly DependencyProperty SelectedOutputTypeProperty =
            DependencyProperty.Register("SelectedOutputType", typeof(string), typeof(DateTimeDifferenceDesignerViewModel), new PropertyMetadata(null, OnSelectedOutputTypeChanged));

        static void OnSelectedOutputTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (DateTimeDifferenceDesignerViewModel)d;
            var value = e.NewValue as string;
            viewModel.OutputType = value;
        }

        // DO NOT bind to these properties - these are here for convenience only!!!
        string OutputType { set { SetProperty(value); } get { return GetProperty<string>(); } }

        public override void Validate()
        {
        }
    }
}
