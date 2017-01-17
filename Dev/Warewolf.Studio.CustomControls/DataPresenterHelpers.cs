using System.Linq;
using System.Windows;
using Infragistics.Windows.Controls;
using Infragistics.Windows.DataPresenter;

namespace Warewolf.Studio.CustomControls
{
    public class DataPresenterHelpers
    {
        #region FilterText

        /// <summary>
        /// FilterText Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty FilterTextProperty =
            DependencyProperty.RegisterAttached("FilterText", typeof(string), typeof(DataPresenterHelpers),
                new FrameworkPropertyMetadata(null, OnFilterTextChanged));

        /// <summary>
        /// Gets the text to be used to filter the DataPresenter on which the property was set.
        /// </summary>
        public static string GetFilterText(DependencyObject d)
        {
            return (string)d.GetValue(FilterTextProperty);
        }

        /// <summary>
        /// Sets the filter text on the DataPresenter that should be used to manipulate the RecordFilters of the specified DataPresenter
        /// </summary>
        public static void SetFilterText(DependencyObject d, string value)
        {
            d.SetValue(FilterTextProperty, value);
        }

        /// <summary>
        /// Handles changes to the FilterText property.
        /// </summary>
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

        #endregion //FilterText
    }
}
