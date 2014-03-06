using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics.Controls.Charts.Messaging;
using System.Collections;
using System.ComponentModel;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a sparkline chart.
    /// </summary>

    
    

    public class XamSparkline : Control
    {
        /// <summary>
        /// Creates a new instance of XamSparkLine.
        /// </summary>
        public XamSparkline()
        {

            Infragistics.Windows.Utilities.ValidateLicense(typeof(XamSparkline), this);


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

            DefaultStyleKey = typeof(XamSparkline);
        }

        #region Public Properties
        #region Brush Properties
        #region Brush
        internal const string BrushPropertyName = "Brush";

        /// <summary>
        /// Identifies the Brush dependency property.
        /// </summary>
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(BrushPropertyName, typeof(Brush), typeof(XamSparkline),
            new PropertyMetadata(null, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(BrushPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the sparkline brush.
        /// </summary>
        public Brush Brush
        {
            get { return (Brush)GetValue(BrushProperty); }
            set { SetValue(BrushProperty, value); }
        }
        #endregion

        #region NegativeBrush
        internal const string NegativeBrushPropertyName = "NegativeBrush";

        /// <summary>
        /// Identifies the NegativeBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty NegativeBrushProperty = DependencyProperty.Register(NegativeBrushPropertyName, typeof(Brush), typeof(XamSparkline),
            new PropertyMetadata(null, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(NegativeBrushPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the negative brush of the sparkline.
        /// </summary>
        public Brush NegativeBrush
        {
            get { return (Brush)GetValue(NegativeBrushProperty); }
            set { SetValue(NegativeBrushProperty, value); }
        }
        #endregion

        #region MarkerBrush
        internal const string MarkerBrushPropertyName = "MarkerBrush";

        /// <summary>
        /// Identifies the MarkerBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty MarkerBrushProperty = DependencyProperty.Register(MarkerBrushPropertyName, typeof(Brush), typeof(XamSparkline),
            new PropertyMetadata(null, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(MarkerBrushPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the marker brush of the sparkline.
        /// </summary>
        public Brush MarkerBrush
        {
            get { return (Brush)GetValue(MarkerBrushProperty); }
            set { SetValue(MarkerBrushProperty, value); }
        }
        #endregion

        #region NegativeMarkerBrush
        internal const string NegativeMarkerBrushPropertyName = "NegativeMarkerBrush";

        /// <summary>
        /// Identifies the NegativeMarkerBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty NegativeMarkerBrushProperty = DependencyProperty.Register(NegativeMarkerBrushPropertyName, typeof(Brush), typeof(XamSparkline),
            new PropertyMetadata(null, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(NegativeMarkerBrushPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the negative marker brush of the sparkline.
        /// </summary>
        public Brush NegativeMarkerBrush
        {
            get { return (Brush)GetValue(NegativeMarkerBrushProperty); }
            set { SetValue(NegativeMarkerBrushProperty, value); }
        }
        #endregion

        #region FirstMarkerBrush
        internal const string FirstMarkerBrushPropertyName = "FirstMarkerBrush";

        /// <summary>
        /// Identifies the FirstMarkerBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty FirstMarkerBrushProperty = DependencyProperty.Register(FirstMarkerBrushPropertyName, typeof(Brush), typeof(XamSparkline),
            new PropertyMetadata(null, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(FirstMarkerBrushPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the first marker brush of the sparkline.
        /// </summary>
        public Brush FirstMarkerBrush
        {
            get { return (Brush)GetValue(FirstMarkerBrushProperty); }
            set { SetValue(FirstMarkerBrushProperty, value); }
        }
        #endregion

        #region LastMarkerBrush
        internal const string LastMarkerBrushPropertyName = "LastMarkerBrush";

        /// <summary>
        /// Identifies the LastMarkerBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty LastMarkerBrushProperty = DependencyProperty.Register(LastMarkerBrushPropertyName, typeof(Brush), typeof(XamSparkline),
            new PropertyMetadata(null, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(LastMarkerBrushPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the last marker brush of the sparkline.
        /// </summary>
        public Brush LastMarkerBrush
        {
            get { return (Brush)GetValue(LastMarkerBrushProperty); }
            set { SetValue(LastMarkerBrushProperty, value); }
        }
        #endregion

        #region HighMarkerBrush
        internal const string HighMarkerBrushPropertyName = "HighMarkerBrush";

        /// <summary>
        /// Identifies the HighMarkerBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty HighMarkerBrushProperty = DependencyProperty.Register(HighMarkerBrushPropertyName, typeof(Brush), typeof(XamSparkline),
            new PropertyMetadata(null, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(HighMarkerBrushPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the high marker brush of the sparkline.
        /// </summary>
        public Brush HighMarkerBrush
        {
            get { return (Brush)GetValue(HighMarkerBrushProperty); }
            set { SetValue(HighMarkerBrushProperty, value); }
        }
        #endregion

        #region LowMarkerBrush
        internal const string LowMarkerBrushPropertyName = "LowMarkerBrush";

        /// <summary>
        /// Identifies the LowMarkerBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty LowMarkerBrushProperty = DependencyProperty.Register(LowMarkerBrushPropertyName, typeof(Brush), typeof(XamSparkline),
            new PropertyMetadata(null, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(LowMarkerBrushPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the low marker brush of the sparkline.
        /// </summary>
        public Brush LowMarkerBrush
        {
            get { return (Brush)GetValue(LowMarkerBrushProperty); }
            set { SetValue(LowMarkerBrushProperty, value); }
        }
        #endregion

        #region TrendLineBrush
        internal const string TrendLineBrushPropertyName = "TrendLineBrush";

        /// <summary>
        /// Identifies the TrendLineBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty TrendLineBrushProperty = DependencyProperty.Register(TrendLineBrushPropertyName, typeof(Brush), typeof(XamSparkline),
            new PropertyMetadata(null, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(TrendLineBrushPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the trendline brush of the sparkline.
        /// </summary>
        public Brush TrendLineBrush
        {
            get { return (Brush)GetValue(TrendLineBrushProperty); }
            set { SetValue(TrendLineBrushProperty, value); }
        }
        #endregion



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        #region NormalRangeFill
        internal const string NormalRangeFillPropertyName = "NormalRangeFill";

        /// <summary>
        /// Identifies the NormalRangeFill dependency property.
        /// </summary>
        public static readonly DependencyProperty NormalRangeFillProperty = DependencyProperty.Register(NormalRangeFillPropertyName, typeof(Brush), typeof(XamSparkline),
            new PropertyMetadata(null, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(NormalRangeFillPropertyName, e.OldValue, e.NewValue)));


        /// <summary>
        /// Gets or sets the normal range brush of the sparkline.
        /// </summary>
        public Brush NormalRangeFill
        {
            get { return (Brush)GetValue(NormalRangeFillProperty); }
            set { SetValue(NormalRangeFillProperty, value); }
        }
        #endregion
        #endregion

        #region Visibility Properties
        #region HorizontalAxisVisibility
        internal const string HorizontalAxisVisibilityPropertyName = "HorizontalAxisVisibility";

        /// <summary>
        /// Identifies the HorizontalAxisVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalAxisVisibilityProperty = DependencyProperty.Register(HorizontalAxisVisibilityPropertyName, typeof(Visibility), typeof(XamSparkline),
            new PropertyMetadata(Visibility.Collapsed, (o, e) => 
                (o as XamSparkline).OnPropertyChanged(HorizontalAxisVisibilityPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the display state of the horizontal axis.
        /// </summary>
        public Visibility HorizontalAxisVisibility
        {
            get { return (Visibility)GetValue(HorizontalAxisVisibilityProperty); }
            set { SetValue(HorizontalAxisVisibilityProperty, value); }
        }
        #endregion

        #region VerticalAxisVisibility
        internal const string VerticalAxisVisibilityPropertyName = "VerticalAxisVisibility";

        /// <summary>
        /// Identifies the VerticalAxisVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalAxisVisibilityProperty = DependencyProperty.Register(VerticalAxisVisibilityPropertyName, typeof(Visibility), typeof(XamSparkline),
            new PropertyMetadata(Visibility.Collapsed, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(VerticalAxisVisibilityPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the display state of the vertical axis.
        /// </summary>
        public Visibility VerticalAxisVisibility
        {
            get { return (Visibility)GetValue(VerticalAxisVisibilityProperty); }
            set { SetValue(VerticalAxisVisibilityProperty, value); }
        }
        #endregion

        #region MarkerVisibility
        internal const string MarkerVisibilityPropertyName = "MarkerVisibility";

        /// <summary>
        /// Identifies the MarkerVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty MarkerVisibilityProperty = DependencyProperty.Register(MarkerVisibilityPropertyName, typeof(Visibility), typeof(XamSparkline),
            new PropertyMetadata(Visibility.Collapsed, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(MarkerVisibilityPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the marker visibility of the sparkline.
        /// </summary>
        public Visibility MarkerVisibility
        {
            get { return (Visibility)GetValue(MarkerVisibilityProperty); }
            set { SetValue(MarkerVisibilityProperty, value); }
        }
        #endregion

        #region NegativeMarkerVisibility
        internal const string NegativeMarkerVisibilityPropertyName = "NegativeMarkerVisibility";

        /// <summary>
        /// Identifies the NegativeMarkerVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty NegativeMarkerVisibilityProperty = DependencyProperty.Register(NegativeMarkerVisibilityPropertyName, typeof(Visibility), typeof(XamSparkline),
            new PropertyMetadata(Visibility.Collapsed, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(NegativeMarkerVisibilityPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the negative marker visibility of the sparkline.
        /// </summary>
        public Visibility NegativeMarkerVisibility
        {
            get { return (Visibility)GetValue(NegativeMarkerVisibilityProperty); }
            set { SetValue(NegativeMarkerVisibilityProperty, value); }
        }
        #endregion

        #region FirstMarkerVisibility
        internal const string FirstMarkerVisibilityPropertyName = "FirstMarkerVisibility";

        /// <summary>
        /// Identifies the FirstMarkerVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty FirstMarkerVisibilityProperty = DependencyProperty.Register(FirstMarkerVisibilityPropertyName, typeof(Visibility), typeof(XamSparkline),
            new PropertyMetadata(Visibility.Collapsed, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(FirstMarkerVisibilityPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the first marker visibility of the sparkline.
        /// </summary>
        public Visibility FirstMarkerVisibility
        {
            get { return (Visibility)GetValue(FirstMarkerVisibilityProperty); }
            set { SetValue(FirstMarkerVisibilityProperty, value); }
        }
        #endregion

        #region LastMarkerVisibility
        internal const string LastMarkerVisibilityPropertyName = "LastMarkerVisibility";

        /// <summary>
        /// Identifies the LastMarkerVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty LastMarkerVisibilityProperty = DependencyProperty.Register(LastMarkerVisibilityPropertyName, typeof(Visibility), typeof(XamSparkline),
            new PropertyMetadata(Visibility.Collapsed, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(LastMarkerVisibilityPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the last marker visibility of the sparkline.
        /// </summary>
        public Visibility LastMarkerVisibility
        {
            get { return (Visibility)GetValue(LastMarkerVisibilityProperty); }
            set { SetValue(LastMarkerVisibilityProperty, value); }
        }
        #endregion

        #region LowMarkerVisibility
        internal const string LowMarkerVisibilityPropertyName = "LowMarkerVisibility";

        /// <summary>
        /// Identifies the LowMarkerVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty LowMarkerVisibilityProperty = DependencyProperty.Register(LowMarkerVisibilityPropertyName, typeof(Visibility), typeof(XamSparkline),
            new PropertyMetadata(Visibility.Collapsed, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(LowMarkerVisibilityPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the low marker visibility of the sparkline.
        /// </summary>
        public Visibility LowMarkerVisibility
        {
            get { return (Visibility)GetValue(LowMarkerVisibilityProperty); }
            set { SetValue(LowMarkerVisibilityProperty, value); }
        }
        #endregion

        #region HighMarkerVisibility
        internal const string HighMarkerVisibilityPropertyName = "HighMarkerVisibility";

        /// <summary>
        /// Identifies the HighMarkerVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty HighMarkerVisibilityProperty = DependencyProperty.Register(HighMarkerVisibilityPropertyName, typeof(Visibility), typeof(XamSparkline),
            new PropertyMetadata(Visibility.Collapsed, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(HighMarkerVisibilityPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the high marker visibility of the sparkline.
        /// </summary>
        public Visibility HighMarkerVisibility
        {
            get { return (Visibility)GetValue(HighMarkerVisibilityProperty); }
            set { SetValue(HighMarkerVisibilityProperty, value); }
        }
        #endregion





        #region NormalRangeVisibility
        internal const string NormalRangeVisibilityPropertyName = "NormalRangeVisibility";

        /// <summary>
        /// Identifies the NormalRangeVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty NormalRangeVisibilityProperty = DependencyProperty.Register(NormalRangeVisibilityPropertyName, typeof(Visibility), typeof(XamSparkline),
            new PropertyMetadata(Visibility.Collapsed, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(NormalRangeVisibilityPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the normal range visibility of the sparkline.
        /// </summary>
        public Visibility NormalRangeVisibility
        {
            get { return (Visibility)GetValue(NormalRangeVisibilityProperty); }
            set { SetValue(NormalRangeVisibilityProperty, value); }
        }
        #endregion
        #endregion

        #region Marker Sizes
        #region MarkerSize
        internal const string MarkerSizePropertyName = "MarkerSize";

        /// <summary>
        /// Identifies the MarkerSize dependency property.
        /// </summary>
        public static readonly DependencyProperty MarkerSizeProperty = DependencyProperty.Register(MarkerSizePropertyName, typeof(double), typeof(XamSparkline),
            new PropertyMetadata(3.0, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(MarkerSizePropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the marker size of the sparkline.
        /// </summary>
        public double MarkerSize
        {
            get { return (double)GetValue(MarkerSizeProperty); }
            set { SetValue(MarkerSizeProperty, value); }
        }
        #endregion

        #region FirstMarkerSize
        internal const string FirstMarkerSizePropertyName = "FirstMarkerSize";

        /// <summary>
        /// Identifies the FirstMarkerSize dependency property.
        /// </summary>
        public static readonly DependencyProperty FirstMarkerSizeProperty = DependencyProperty.Register(FirstMarkerSizePropertyName, typeof(double), typeof(XamSparkline),
            new PropertyMetadata(3.0, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(FirstMarkerSizePropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the first marker size of the sparkline.
        /// </summary>
        public double FirstMarkerSize
        {
            get { return (double)GetValue(FirstMarkerSizeProperty); }
            set { SetValue(FirstMarkerSizeProperty, value); }
        }
        #endregion

        #region LastMarkerSize
        internal const string LastMarkerSizePropertyName = "LastMarkerSize";

        /// <summary>
        /// Identifies the LastMarkerSize dependency property.
        /// </summary>
        public static readonly DependencyProperty LastMarkerSizeProperty = DependencyProperty.Register(LastMarkerSizePropertyName, typeof(double), typeof(XamSparkline),
            new PropertyMetadata(3.0, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(LastMarkerSizePropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the last marker size of the sparkline.
        /// </summary>
        public double LastMarkerSize
        {
            get { return (double)GetValue(LastMarkerSizeProperty); }
            set { SetValue(LastMarkerSizeProperty, value); }
        }
        #endregion

        #region HighMarkerSize
        internal const string HighMarkerSizePropertyName = "HighMarkerSize";

        /// <summary>
        /// Identifies the HighMarkerSize dependency property.
        /// </summary>
        public static readonly DependencyProperty HighMarkerSizeProperty = DependencyProperty.Register(HighMarkerSizePropertyName, typeof(double), typeof(XamSparkline),
            new PropertyMetadata(3.0, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(HighMarkerSizePropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the high marker size of the sparkline.
        /// </summary>
        public double HighMarkerSize
        {
            get { return (double)GetValue(HighMarkerSizeProperty); }
            set { SetValue(HighMarkerSizeProperty, value); }
        }
        #endregion

        #region LowMarkerSize
        internal const string LowMarkerSizePropertyName = "LowMarkerSize";

        /// <summary>
        /// Identifies the LowMarkerSize dependency property.
        /// </summary>
        public static readonly DependencyProperty LowMarkerSizeProperty = DependencyProperty.Register(LowMarkerSizePropertyName, typeof(double), typeof(XamSparkline),
            new PropertyMetadata(3.0, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(LowMarkerSizePropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the low marker size of the sparkline.
        /// </summary>
        public double LowMarkerSize
        {
            get { return (double)GetValue(LowMarkerSizeProperty); }
            set { SetValue(LowMarkerSizeProperty, value); }
        }
        #endregion

        #region NegativeMarkerSize
        internal const string NegativeMarkerSizePropertyName = "NegativeMarkerSize";

        /// <summary>
        /// Identifies the NegativeMarkerSize dependency property.
        /// </summary>
        public static readonly DependencyProperty NegativeMarkerSizeProperty = DependencyProperty.Register(NegativeMarkerSizePropertyName, typeof(double), typeof(XamSparkline),
            new PropertyMetadata(3.0, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(NegativeMarkerSizePropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the negative marker size of the sparkline.
        /// </summary>
        public double NegativeMarkerSize
        {
            get { return (double)GetValue(NegativeMarkerSizeProperty); }
            set { SetValue(NegativeMarkerSizeProperty, value); }
        }
        #endregion
        #endregion


#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

        #region LineThickness
        internal const string LineThicknessPropertyName = "LineThickness";

        /// <summary>
        /// Identifies the LineThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty LineThicknessProperty = DependencyProperty.Register(LineThicknessPropertyName, typeof(double), typeof(XamSparkline),
            new PropertyMetadata(2.0, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(LineThicknessPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the line thickness of the sparkline.
        /// </summary>
        public double LineThickness
        {
            get { return (double)GetValue(LineThicknessProperty); }
            set { SetValue(LineThicknessProperty, value); }
        }
        #endregion

        #region Minimum
        internal const string MinimumPropertyName = "Minimum";

        /// <summary>
        /// Identifies the Minimum dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(MinimumPropertyName, typeof(double), typeof(XamSparkline),
            new PropertyMetadata(double.NaN, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(MinimumPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the minimum value of the y axis.
        /// </summary>
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        #endregion

        #region Maximum
        internal const string MaximumPropertyName = "Maximum";

        /// <summary>
        /// Identifies the Maximum dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(MaximumPropertyName, typeof(double), typeof(XamSparkline),
            new PropertyMetadata(double.NaN, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(MaximumPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the maximum value of the y axis.
        /// </summary>
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        #endregion

        #region ItemsSource
        internal const string ItemsSourcePropertyName = "ItemsSource";

        /// <summary>
        /// Identifies the ItemsSource dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(ItemsSourcePropertyName, typeof(IEnumerable), typeof(XamSparkline),
            new PropertyMetadata(null, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(ItemsSourcePropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the ItemsSource of the sparkline.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        #endregion

        #region ValueMemberPath
        internal const string ValueMemberPathPropertyName = "ValueMemberPath";

        /// <summary>
        /// Identifies the ValueMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueMemberPathProperty = DependencyProperty.Register(ValueMemberPathPropertyName, typeof(string), typeof(XamSparkline),
            new PropertyMetadata(null, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(ValueMemberPathPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the string path to the value column.
        /// </summary>
        public string ValueMemberPath
        {
            get { return (string)GetValue(ValueMemberPathProperty); }
            set { SetValue(ValueMemberPathProperty, value); }
        }
        #endregion

        #region LabelMemberPath
        internal const string LabelMemberPathPropertyName = "LabelMemberPath";
        /// <summary>
        /// Identifies the LabelMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelMemberPathProperty = DependencyProperty.Register(LabelMemberPathPropertyName, typeof(string), typeof(XamSparkline),
            new PropertyMetadata(null, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(LabelMemberPathPropertyName, e.OldValue, e.NewValue)));
        /// <summary>
        /// String identifier of a column or property name to get labels from on each item in the ItemsSource.  These labels will be retrieved from the first and last item, and displayed by the horizontal axis.
        /// </summary>
        public string LabelMemberPath
        {
            get { return (string)GetValue(LabelMemberPathProperty); }
            set { SetValue(LabelMemberPathProperty, value); }
        }
        #endregion

        #region ToolTip
        internal const string ToolTipPropertyName = "ToolTip";

        /// <summary>
        /// Identifies the ToolTip dependency property.
        /// </summary>
        public static readonly DependencyProperty ToolTipProperty = DependencyProperty.Register(ToolTipPropertyName, typeof(object), typeof(XamSparkline),
            new PropertyMetadata(null, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(ToolTipPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the tooltip of the sparkline.
        /// </summary>
        /// <remarks>
        /// The tooltip applies to the entire sparkline control, not the data portion of it.
        /// </remarks>
        public object ToolTip
        {
            get { return GetValue(ToolTipProperty); }
            set { SetValue(ToolTipProperty, value); }
        }
        #endregion

        #region ToolTipVisibility
        internal const string ToolTipVisibilityPropertyName = "ToolTipVisibility";
        /// <summary>
        /// Identifies the ToolTipVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty ToolTipVisibilityProperty = DependencyProperty.Register(ToolTipVisibilityPropertyName, typeof(Visibility), typeof(XamSparkline),
            new PropertyMetadata(Visibility.Collapsed, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(ToolTipVisibilityPropertyName, e.OldValue, e.NewValue)));
        /// <summary>
        /// Determines the visibility of tooltips.
        /// </summary>
        public Visibility ToolTipVisibility
        {
            get { return (Visibility)GetValue(ToolTipVisibilityProperty); }
            set { SetValue(ToolTipVisibilityProperty, value); }
        }
        #endregion

        #region TrendLineType
        internal const string TrendLineTypePropertyName = "TrendLineType";

        /// <summary>
        /// Identifies the TrendLineType dependency property.
        /// </summary>
        public static readonly DependencyProperty TrendLineTypeProperty = DependencyProperty.Register(TrendLineTypePropertyName, typeof(TrendLineType), typeof(XamSparkline),
            new PropertyMetadata(TrendLineType.None, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(TrendLineTypePropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the type of trendline used by the sparkline.
        /// </summary>
        public TrendLineType TrendLineType
        {
            get { return (TrendLineType)GetValue(TrendLineTypeProperty); }
            set { SetValue(TrendLineTypeProperty, value); }
        }
        #endregion

        #region TrendLinePeriod
        internal const string TrendLinePeriodPropertyName = "TrendLinePeriod";

        /// <summary>
        /// Identifies the TrendLinePeriod dependency property.
        /// </summary>
        public static readonly DependencyProperty TrendLinePeriodProperty = DependencyProperty.Register(TrendLinePeriodPropertyName, typeof(int), typeof(XamSparkline),
           new PropertyMetadata(7, (o, e) =>
           {
               (o as XamSparkline).OnPropertyChanged(TrendLinePeriodPropertyName, e.OldValue, e.NewValue);
           }));

        /// <summary>
        /// Gets or sets the trendline period used by the sparkline.
        /// </summary>
        public int TrendLinePeriod
        {
            get
            {
                return (int)GetValue(TrendLinePeriodProperty);
            }
            set
            {
                SetValue(TrendLinePeriodProperty, value);
            }
        }
        #endregion

        #region TrendLineThickness
        internal const string TrendLineThicknessPropertyName = "TrendLineThickness";

        /// <summary>
        /// Identifies the TrendLineThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty TrendLineThicknessProperty = DependencyProperty.Register(TrendLineThicknessPropertyName, typeof(double), typeof(XamSparkline),
            new PropertyMetadata(1.5, (o, e) =>
            {
                (o as XamSparkline).OnPropertyChanged(TrendLineThicknessPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the thickness of the sparkline's trendline.
        /// </summary>
        public double TrendLineThickness
        {
            get
            {
                return (double)GetValue(TrendLineThicknessProperty);
            }
            set
            {
                SetValue(TrendLineThicknessProperty, value);
            }
        }
        #endregion

        #region Resolution
        #endregion

        #region NormalRangeMinimum
        internal const string NormalRangeMinimumPropertyName = "NormalRangeMinimum";

        /// <summary>
        /// Identifies the NormalRangeMinimum dependency property.
        /// </summary>
        public static readonly DependencyProperty NormalRangeMinimumProperty = DependencyProperty.Register(NormalRangeMinimumPropertyName, typeof(double), typeof(XamSparkline),
            new PropertyMetadata((o, e) =>
                (o as XamSparkline).OnPropertyChanged(NormalRangeMinimumPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the minimum value of the normal range.
        /// </summary>
        public double NormalRangeMinimum
        {
            get { return (double)GetValue(NormalRangeMinimumProperty); }
            set { SetValue(NormalRangeMinimumProperty, value); }
        }
        #endregion

        #region NormalRangeMaximum
        internal const string NormalRangeMaximumPropertyName = "NormalRangeMaximum";

        /// <summary>
        /// Identifies the NormalRangeMaximum dependency property.
        /// </summary>
        public static readonly DependencyProperty NormalRangeMaximumProperty = DependencyProperty.Register(NormalRangeMaximumPropertyName, typeof(double), typeof(XamSparkline),
            new PropertyMetadata((o, e) =>
                (o as XamSparkline).OnPropertyChanged(NormalRangeMaximumPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the maximum value of the normal range.
        /// </summary>
        public double NormalRangeMaximum
        {
            get { return (double)GetValue(NormalRangeMaximumProperty); }
            set { SetValue(NormalRangeMaximumProperty, value); }
        }
        #endregion

        #region DisplayType
        internal const string DisplayTypePropertyName = "DisplayType";

        /// <summary>
        /// Identifies the DisplayType dependency property.
        /// </summary>
        public static readonly DependencyProperty DisplayTypeProperty = DependencyProperty.Register(DisplayTypePropertyName, typeof(SparklineDisplayType), typeof(XamSparkline),
            new PropertyMetadata(SparklineDisplayType.Line, (o, e) => 
                (o as XamSparkline).OnPropertyChanged(DisplayTypePropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the display type of the sparkline.
        /// </summary>
        public SparklineDisplayType DisplayType
        {
            get { return (SparklineDisplayType)GetValue(DisplayTypeProperty); }
            set { SetValue(DisplayTypeProperty, value); }
        }
        #endregion

        #region UnknownValuePlotting
        internal const string UnknownValuePlottingPropertyName = "UnknownValuePlotting";

        /// <summary>
        /// Identifies the UnknownValuePlotting dependency property.
        /// </summary>
        public static readonly DependencyProperty UnknownValuePlottingProperty = DependencyProperty.Register(UnknownValuePlottingPropertyName, typeof(UnknownValuePlotting), typeof(XamSparkline),
            new PropertyMetadata(UnknownValuePlotting.DontPlot, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(UnknownValuePlottingPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the way null values are interpreted.
        /// </summary>
        public UnknownValuePlotting UnknownValuePlotting
        {
            get { return (UnknownValuePlotting)GetValue(UnknownValuePlottingProperty); }
            set { SetValue(UnknownValuePlottingProperty, value); }
        }
        #endregion

        #region LineMiterLimit
        internal const string LineMiterLimitPropertyName = "LineMiterLimit";
        /// <summary>
        /// Identifies the LineMiterLimit dependency property.
        /// </summary>
        public static readonly DependencyProperty LineMiterLimitProperty = DependencyProperty.Register(LineMiterLimitPropertyName, typeof(double), typeof(XamSparkline),
            new PropertyMetadata(10.0, (o, e) =>
                (o as XamSparkline).OnPropertyChanged(LineMiterLimitPropertyName, e.OldValue, e.NewValue)));
        /// <summary>
        /// The thickness of the line join on a mitered corner of the series data.
        /// </summary>
        public double LineMiterLimit
        {
            get { return (double)GetValue(LineMiterLimitProperty); }
            set { SetValue(LineMiterLimitProperty, value); }
        }
        #endregion

        #region VerticalAxisLabel
        internal const string VerticalAxisLabelPropertyName = "VerticalAxisLabel";
        /// <summary>
        /// Identifies the VerticalAxisLabel dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalAxisLabelProperty = DependencyProperty.Register(VerticalAxisLabelPropertyName, typeof(object), typeof(XamSparkline),
            new PropertyMetadata("{0:n}", (o, e) =>
                (o as XamSparkline).OnPropertyChanged(VerticalAxisLabelPropertyName, e.OldValue, e.NewValue)));
        /// <summary>
        /// The value or content to display on the vertical axis.
        /// </summary>
        /// <remarks>
        /// This can be set to a formatted string, such as "{0:n}", or it can be set to a DataTemplate.
        /// </remarks>
        [TypeConverter(typeof(ObjectConverter))]
        public object VerticalAxisLabel
        {
            get { return GetValue(VerticalAxisLabelProperty); }
            set { SetValue(VerticalAxisLabelProperty, value); }
        }
        #endregion
        #region HorizontalAxisLabel
        internal const string HorizontalAxisLabelPropertyName = "HorizontalAxisLabel";
        /// <summary>
        /// Identifies the HorizontalAxisLabel dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalAxisLabelProperty = DependencyProperty.Register(HorizontalAxisLabelPropertyName, typeof(object), typeof(XamSparkline),
            new PropertyMetadata("{0}", (o, e) =>
                (o as XamSparkline).OnPropertyChanged(HorizontalAxisLabelPropertyName, e.OldValue, e.NewValue)));
        /// <summary>
        /// The value or content to display on the horizontal axis.
        /// </summary>
        /// <remarks>
        /// This can be set to a formatted string, such as "{0}", or it can be set to a DataTemplate.
        /// </remarks>
        [TypeConverter(typeof(ObjectConverter))]
        public object HorizontalAxisLabel
        {
            get { return GetValue(HorizontalAxisLabelProperty); }
            set { SetValue(HorizontalAxisLabelProperty, value); }
        }
        #endregion


        #endregion

        #region Non-public Properties
        private ServiceProvider _serviceProvider;
        internal ServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
            set
            {
                ServiceProvider oldValue = _serviceProvider;
                _serviceProvider = value;
                OnServiceProviderChanged(oldValue, _serviceProvider);
            }
        }

        private MessageChannel _configurationMessages = new MessageChannel();
        internal MessageChannel ConfigurationMessages
        {
            get { return _configurationMessages; }
            set { _configurationMessages = value; }
        }

        private double _actualMinimum = double.NaN;
        internal double ActualMinimum
        {
            get { return _actualMinimum; }
            set { _actualMinimum = value; }
        }

        private double _actualMaximum = double.NaN;
        internal double ActualMaximum
        {
            get { return _actualMaximum; }
            set { _actualMaximum = value; }
        }

        internal IFastItemColumn<object> LabelColumn { get; set; }
        internal HorizontalAxisView HorizontalAxis { get; set; }
        internal VerticalAxisView VerticalAxis { get; set; }
        #endregion

        internal virtual void OnServiceProviderChanged(ServiceProvider oldValue, ServiceProvider newValue)
        {
            if (oldValue != null)
            {
                ConfigurationMessages.DetachFromNext();
            }

            if (newValue != null)
            {
                ConfigurationMessages.ConnectTo((MessageChannel)newValue.GetService("ConfigurationMessages"));
            }
        }

        /// <summary>
        /// Handles property changes.
        /// </summary>
        /// <param name="propertyName">Property Name</param>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        protected virtual void OnPropertyChanged(string propertyName, object oldValue, object newValue)
        {
            ConfigurationMessages.SendMessage(new PropertyChangedMessage()
            {
                PropertyName = propertyName,
                OldValue = oldValue,
                NewValue = newValue
            });
        }
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