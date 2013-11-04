using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Common;
using Dev2.Data.Enums;

namespace Dev2.Activities.Designers2.Foreach
{
    public class ForeachDesignerViewModel : ActivityDesignerViewModel
    {
        public ForeachDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();
            ForeachTypes = Dev2EnumConverter.ConvertEnumsTypeToStringList<enForEachType>();
            SelectedForeachType = Dev2EnumConverter.ConvertEnumValueToString(ForEachType);
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

        // DO NOT bind to these properties - these are here for convenience only!!!
        enForEachType ForEachType { set { SetProperty(value); } get { return GetProperty<enForEachType>(); } }

        public override void Validate()
        {
        }
    }
}