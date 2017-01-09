/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Data.Enums;
using Dev2.Interfaces;

namespace Dev2.Activities.Designers2.Foreach
{
    public class ForeachDesignerViewModel : ActivityDesignerViewModel
    {
        public ForeachDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            ForeachTypes = Dev2EnumConverter.ConvertEnumsTypeToStringList<enForEachType>();
            SelectedForeachType = Dev2EnumConverter.ConvertEnumValueToString(ForEachType);
            AddTitleBarLargeToggle();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_LoopConstruct_For_Each;
        }

        public IList<string> ForeachTypes { get; private set; }

        public Visibility Visibility
        {
            get { return (Visibility)GetValue(VisibilityProperty); }
            set { SetValue(VisibilityProperty, value); }
        }

        public static readonly DependencyProperty VisibilityProperty =
            DependencyProperty.Register("Visibility", typeof(Visibility), typeof(ForeachDesignerViewModel), new PropertyMetadata(null));

        public Visibility FromVisibility
        {
            get { return (Visibility)GetValue(FromVisibilityProperty); }
            set { SetValue(FromVisibilityProperty, value); }
        }

        public static readonly DependencyProperty FromVisibilityProperty =
            DependencyProperty.Register("FromVisibility", typeof(Visibility), typeof(ForeachDesignerViewModel), new PropertyMetadata(null));

        public Visibility ToVisibility
        {
            get { return (Visibility)GetValue(ToVisibilityProperty); }
            set { SetValue(ToVisibilityProperty, value); }
        }

        public static readonly DependencyProperty ToVisibilityProperty =
            DependencyProperty.Register("ToVisibility", typeof(Visibility), typeof(ForeachDesignerViewModel), new PropertyMetadata(null));

        public Visibility CsvIndexesVisibility
        {
            get { return (Visibility)GetValue(CsvIndexesVisibilityProperty); }
            set { SetValue(CsvIndexesVisibilityProperty, value); }
        }

        public static readonly DependencyProperty CsvIndexesVisibilityProperty =
            DependencyProperty.Register("CsvIndexesVisibility", typeof(Visibility), typeof(ForeachDesignerViewModel), new PropertyMetadata(null));

        public Visibility NumberVisibility
        {
            get { return (Visibility)GetValue(NumberVisibilityProperty); }
            set { SetValue(NumberVisibilityProperty, value); }
        }

        public static readonly DependencyProperty NumberVisibilityProperty =
            DependencyProperty.Register("NumberVisibility", typeof(Visibility), typeof(ForeachDesignerViewModel), new PropertyMetadata(null));

        public Visibility RecordsetVisibility
        {
            get { return (Visibility)GetValue(RecordsetVisibilityProperty); }
            set { SetValue(RecordsetVisibilityProperty, value); }
        }

        public static readonly DependencyProperty RecordsetVisibilityProperty =
            DependencyProperty.Register("RecordsetVisibility", typeof(Visibility), typeof(ForeachDesignerViewModel), new PropertyMetadata(null));

        public string SelectedForeachType
        {
            get { return (string)GetValue(SelectedForeachTypeProperty); }
            set { SetValue(SelectedForeachTypeProperty, value); }
        }

        public static readonly DependencyProperty SelectedForeachTypeProperty =
            DependencyProperty.Register("SelectedForeachType", typeof(string), typeof(ForeachDesignerViewModel), new PropertyMetadata("", OnSelectedForeachTypeChanged));

        static void OnSelectedForeachTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (ForeachDesignerViewModel)d;
            var value = e.NewValue as string;

            if(!string.IsNullOrWhiteSpace(value))
            {
                switch(value)
                {
                    case "* in Range":
                        viewModel.FromVisibility = Visibility.Visible;
                        viewModel.ToVisibility = Visibility.Visible;
                        viewModel.CsvIndexesVisibility = Visibility.Hidden;
                        viewModel.NumberVisibility = Visibility.Hidden;
                        viewModel.RecordsetVisibility = Visibility.Hidden;
                        break;

                    case "* in CSV":
                        viewModel.FromVisibility = Visibility.Hidden;
                        viewModel.ToVisibility = Visibility.Hidden;
                        viewModel.CsvIndexesVisibility = Visibility.Visible;
                        viewModel.NumberVisibility = Visibility.Hidden;
                        viewModel.RecordsetVisibility = Visibility.Hidden;
                        break;

                    case "* in Recordset":
                        viewModel.FromVisibility = Visibility.Hidden;
                        viewModel.ToVisibility = Visibility.Hidden;
                        viewModel.CsvIndexesVisibility = Visibility.Visible;
                        viewModel.NumberVisibility = Visibility.Hidden;
                        viewModel.RecordsetVisibility = Visibility.Visible;
                        break;

                    default:
                        viewModel.FromVisibility = Visibility.Hidden;
                        viewModel.ToVisibility = Visibility.Hidden;
                        viewModel.CsvIndexesVisibility = Visibility.Hidden;
                        viewModel.NumberVisibility = Visibility.Visible;
                        viewModel.RecordsetVisibility = Visibility.Hidden;
                        break;

                }
                viewModel.ForEachType = (enForEachType)Dev2EnumConverter.GetEnumFromStringDiscription(value, typeof(enForEachType));
            }
        }

        public bool MultipleItemsToSequence(IDataObject dataObject)
        {
            if(dataObject != null)
            {
                var formats = dataObject.GetFormats();
                if(!formats.Any())
                {
                    return false;
                }
                var modelItemString = formats.FirstOrDefault(s => s.IndexOf("ModelItemsFormat", StringComparison.Ordinal) >= 0);
                if(!String.IsNullOrEmpty(modelItemString))
                {
                    var objectData = dataObject.GetData(modelItemString);
                    var data = objectData as List<ModelItem>;

                    if(data != null && data.Count > 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // DO NOT bind to these properties - these are here for convenience only!!!
        enForEachType ForEachType { set { SetProperty(value); } get { return GetProperty<enForEachType>(); } }


        public override void Validate()
        {
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
