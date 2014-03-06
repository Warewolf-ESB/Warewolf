using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a DataContext for tooltips, markerItems, cursors and legend items
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DataContext : DependencyObject, INotifyPropertyChanged
    {
        /// <summary>
        /// Identifies the Series dependency property.
        /// </summary>
        public static readonly DependencyProperty SeriesProperty =
            DependencyProperty.Register(
            "Series",
            typeof(Control),
            typeof(DataContext),
            new PropertyMetadata(null,
                (o, e) => (o as DataContext).RaisePropertyChanged("Series", e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets the current DataContext's series.
        /// </summary>
        public Control Series
        {
            get { return (Control)GetValue(SeriesProperty); }
            set { SetValue(SeriesProperty, value); }
        }

        /// <summary>
        /// Identifies the Item dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register(
            "Item",
            typeof(object),
            typeof(DataContext),
            new PropertyMetadata(null,
                (o, e) => (o as DataContext).RaisePropertyChanged("Item", e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets the current DataContext's item.
        /// </summary>
        public object Item
        {
            get { return GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        /// <summary>
        /// Identifies the ItemBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemBrushProperty =
            DependencyProperty.Register(
            "ItemBrush",
            typeof(Brush),
            typeof(DataContext),
            new PropertyMetadata(null,
                (o, e) => (o as DataContext).RaisePropertyChanged("ItemBrush", e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the brush to use to color the item.
        /// </summary>
        public Brush ItemBrush
        {
            get
            {
                return (Brush)GetValue(ItemBrushProperty);
            }
            internal set
            {
                SetValue(ItemBrushProperty, value);
            }
        }

        /// <summary>
        /// Identifies the ActualItemBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualItemBrushProperty =
            DependencyProperty.Register(
            "ActualItemBrush",
            typeof(Brush),
            typeof(DataContext),
            new PropertyMetadata(null,
                (o, e) => (o as DataContext).RaisePropertyChanged("ActualItemBrush", e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets the calculated brush to use to color the item.
        /// </summary>
        public Brush ActualItemBrush
        {
            get
            {
                return (Brush)GetValue(ActualItemBrushProperty);
            }
            private set
            {
                SetValue(ActualItemBrushProperty, value);
            }
        }

        #region ItemLabel dependency property

        internal const string ItemLabelPropertyName = "ItemLabel";

        /// <summary>
        /// Identifies the ItemLabel dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemLabelProperty = DependencyProperty.Register(ItemLabelPropertyName, typeof(string), typeof(DataContext),
            new PropertyMetadata(string.Empty, (sender, e) =>
            {
                (sender as DataContext).RaisePropertyChanged(ItemLabelPropertyName, e.OldValue, e.NewValue);
            }
            ));

        /// <summary>
        /// Gets or sets the item label for the current data context.
        /// </summary>
        public string ItemLabel
        {
            get
            {
                return (string)GetValue(ItemLabelProperty);
            }
            set
            {
                SetValue(ItemLabelProperty, value);
            }
        }

        #endregion ItemLabel dependency property

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raises PropertyChanged event for a specified property name.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="oldValue">Old property value</param>
        /// <param name="newValue">New property value</param>
        protected virtual void RaisePropertyChanged(string propertyName, object oldValue, object newValue)
        {
            switch (propertyName)
            {
                case "Series":
                case "ItemBrush":
                    this.BindActualItemBrush();
                    break;
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged Members

        private void BindActualItemBrush()
        {
            if (this.ItemBrush == null)
            {
                if (this.Series is MarkerSeries)
                {
                    BindingOperations.SetBinding(
                              this,
                              ActualItemBrushProperty,
                              new Binding("ActualMarkerBrush") { Source = this.Series });
                }
            }
            else
            {
                BindingOperations.SetBinding(
                           this,
                           ActualItemBrushProperty,
                           new Binding("ItemBrush") { Source = this });
            }
        }
    }

    /// <summary>
    /// Represents a data context for an item on an axis.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AxisItemDataContext : INotifyPropertyChanged
    {
        private Axis axis;

        /// <summary>
        /// Gets or sets the axis which owns this item.
        /// </summary>
        public Axis Axis
        {
            get
            {
                return this.axis;
            }
            set
            {
                this.axis = value;
                this.RaisePropertyChanged("Axis");
            }
        }

        /// <summary>
        /// Gets the current DataContext's item.
        /// </summary>
        public object Item
        {
            get { return item; }
            internal set
            {
                if (item != value)
                {
                    item = value;
                    RaisePropertyChanged("Item");
                }
            }
        }

        private object item;
        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property which was changed.</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Raised when a property value on this object changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// The data context that will be provided for a funnel slice.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class FunnelSliceDataContext : DataContext
    {
        /// <summary>
        /// Identifies the ItemOutline dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemOutlineProperty =
            DependencyProperty.Register(
            "ItemOutline",
            typeof(Brush),
            typeof(FunnelSliceDataContext),
            new PropertyMetadata(null,
                (o, e) => (o as FunnelSliceDataContext).RaisePropertyChanged("ItemOutline", e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the Outline to use to color the item.
        /// </summary>
        public Brush ItemOutline
        {
            get
            {
                return (Brush)GetValue(ItemOutlineProperty);
            }
            internal set
            {
                SetValue(ItemOutlineProperty, value);
            }
        }
    }

    /// <summary>
    /// Represents a data context for a pie chart slice.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class PieSliceDataContext : DataContext
    {
        /// <summary>
        /// Identifies Slice dependency property.
        /// </summary>
        public static readonly DependencyProperty SliceProperty =
            DependencyProperty.Register(
            "Slice",
            typeof(FrameworkElement),
            typeof(PieSliceDataContext),
            new PropertyMetadata(null,
                (o, e) => (o as PieSliceDataContext).RaisePropertyChanged("Slice", e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets the current DataContext's pie slice.
        /// </summary>
        public FrameworkElement Slice
        {
            get { return (FrameworkElement)GetValue(SliceProperty); }
            internal set { SetValue(SliceProperty, value); }
        }

        private const string PercentValuePropertyName = "PercentValue";
        /// <summary>
        /// Identifies the PercentValue dependency property.
        /// </summary>
        public static readonly DependencyProperty PercentValueProperty = DependencyProperty.Register(PercentValuePropertyName, typeof(double), typeof(PieSliceDataContext), new PropertyMetadata(double.NaN, (sender, e) =>
        {
            (sender as PieSliceDataContext).RaisePropertyChanged(PercentValuePropertyName, e.OldValue, e.NewValue);
        }));
        /// <summary>
        /// The percent value of the pie slice in context, proportional to the entire pie.
        /// </summary>
        public double PercentValue
        {
            get
            {
                return (double)this.GetValue(PercentValueProperty);
            }
            set
            {
                this.SetValue(PercentValueProperty, value);
            }
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raises PropertyChanged event for a specified property name.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="oldValue">Old property value</param>
        /// <param name="newValue">New property value</param>
        protected override void RaisePropertyChanged(string propertyName, object oldValue, object newValue)
        {
            switch (propertyName)
            {
                case "Series":
                    if (newValue != null && ItemBrush == null)
                    {
                        BindingOperations.SetBinding(
                            this,
                            ActualItemBrushProperty,
                            new Binding("Slice.Background") { Source = this });
                    }
                    else
                    {
                        BindingOperations.SetBinding(
                            this,
                            ActualItemBrushProperty,
                            new Binding("ItemBrush") { Source = this });
                    }
                    break;
                case "ItemBrush":
                    if (newValue == null && Series != null)
                    {
                        BindingOperations.SetBinding(
                            this,
                            ActualItemBrushProperty,
                            new Binding("Slice.Background") { Source = this });
                    }
                    else
                    {
                        BindingOperations.SetBinding(
                            this,
                            ActualItemBrushProperty,
                            new Binding("ItemBrush") { Source = this });
                    }
                    break;
                case "Slice":
                    if (newValue != null && ItemBrush == null)
                    {
                        BindingOperations.SetBinding(
                            this,
                            ActualItemBrushProperty,
                            new Binding("Slice.Background") { Source = this });
                    }
                    else
                    {
                        BindingOperations.SetBinding(
                            this,
                            ActualItemBrushProperty,
                            new Binding("ItemBrush") { Source = this });
                    }
                    break;
            }
        }

        #endregion INotifyPropertyChanged Members
    }
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved