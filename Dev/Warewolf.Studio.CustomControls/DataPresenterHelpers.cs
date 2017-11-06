using System.Linq;
using System.Windows;
using Infragistics.Windows.Controls;
using Infragistics.Windows.DataPresenter;

namespace Warewolf.Studio.CustomControls
{
    public class DataPresenterHelpers
    {
        protected DataPresenterHelpers()
        {
        }

        public static readonly DependencyProperty FilterTextProperty =
            DependencyProperty.RegisterAttached("FilterText", typeof(string), typeof(DataPresenterHelpers),
                new FrameworkPropertyMetadata(null, OnFilterTextChanged));
        
        public static string GetFilterText(DependencyObject d)
        {
            return (string)d.GetValue(FilterTextProperty);
        }
        
        public static void SetFilterText(DependencyObject d, string value)
        {
            d.SetValue(FilterTextProperty, value);
        }
        
        private static void OnFilterTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dp = d as DataPresenterBase;

            if (dp?.DefaultFieldLayout != null)
            {
                dp.DefaultFieldLayout.RecordFilters.Clear();
                dp.DefaultFieldLayout.Settings.RecordFiltersLogicalOperator = LogicalOperator.Or;

                foreach (var field in dp.DefaultFieldLayout.Fields.Where(o => o.Name == "DisplayName"))
                {
                    var filter = new RecordFilter { Field = field };
                    filter.Conditions.Add(new ComparisonCondition(ComparisonOperator.Contains, e.NewValue));
                    dp.DefaultFieldLayout.RecordFilters.Add(filter);
                }
            }
        }
    }
}
