using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Infragistics.Controls.Grids;

namespace Warewolf.Studio.Views
{
    public class DataGridExtender : Behavior<FrameworkElement>
    {
        static XamGrid _grid;
        static INameValue _context;
        static Row _row;
        public XamGrid ParentGrid
        {
            get { return (XamGrid)GetValue(ParentGridProperty); }
            set { SetValue(ParentGridProperty, value); }
        }

        public INameValue DataContext
        {
            get { return (INameValue)GetValue(DataContextProperty); }
            set { SetValue(DataContextProperty, value); }
        }

        public Row Row
        {
            get { return (Row)GetValue(RowProperty); }
            set { SetValue(RowProperty, value); }
        }


        public static readonly DependencyProperty ParentGridProperty =
           DependencyProperty.Register("ParentGrid", typeof(XamGrid), typeof(DataGridExtender), new PropertyMetadata(PropertyChangedCallback));

        public static readonly DependencyProperty DataContextProperty =
        DependencyProperty.Register("DataContext", typeof(INameValue), typeof(DataGridExtender), new PropertyMetadata(PropertyChangedCallback));


        public static readonly DependencyProperty RowProperty =
           DependencyProperty.Register("Row", typeof(Row), typeof(DataGridExtender), new PropertyMetadata(PropertyChangedCallback));




        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {

            if (dependencyPropertyChangedEventArgs.Property == ParentGridProperty)
            {
                if (dependencyPropertyChangedEventArgs.NewValue != null)
                {
                    _grid = dependencyPropertyChangedEventArgs.NewValue as XamGrid;
                }
            }
            if (dependencyPropertyChangedEventArgs.Property == DataContextProperty)
            {
                if (dependencyPropertyChangedEventArgs.NewValue != null)
                {
                    _context = dependencyPropertyChangedEventArgs.NewValue as INameValue;
                }
            }
            if (dependencyPropertyChangedEventArgs.Property == RowProperty)
            {
                if (dependencyPropertyChangedEventArgs.NewValue != null)
                {
                    _row = dependencyPropertyChangedEventArgs.NewValue as Row;
                }
            }
        }


        protected override void OnAttached()
        {
            if(AssociatedObject != null)
            {
                base.OnAttached();
                Mouse.AddMouseUpHandler(AssociatedObject, Handler);
            }
        }



        private void Handler(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (mouseButtonEventArgs.ClickCount == 1)
            {
                if (_grid != null && _context != null)
                {
                    _grid.EnterEditMode(_row);

                }
            }
        }


        protected override void OnDetaching()
        {
            Mouse.RemoveMouseUpHandler(AssociatedObject, Handler);
            base.OnDetaching();
        }
    }
}
