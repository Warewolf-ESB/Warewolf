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
using System.Windows.Media.Effects;
using System.ComponentModel;
using System.Collections.Generic;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a non-visual child of StackedSeriesBase.
    /// </summary>
    public class StackedFragmentSeries : DependencyObject, INotifyPropertyChanged
    {
        /// <summary>
        /// Creates a new instance of StackedFragmentSeries.
        /// </summary>
        public StackedFragmentSeries() 
        {
            HighValues = new List<double>();
            LowValues = new List<double>();
            Buckets = new List<float[]>();

            PropertyUpdated += (o, e) => { PropertyUpdatedOverride(o, e.PropertyName, e.OldValue, e.NewValue); };
        }

        #region Public Properties
        #region Brush Dependency Property
        internal const string BrushPropertyName = "Brush";

        /// <summary>
        /// Identifies the Brush dependency property.
        /// </summary>
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(BrushPropertyName, typeof(Brush), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) => 
            {
                (o as StackedFragmentSeries).RaisePropertyChanged(BrushPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the brush of the stacked fragment.
        /// </summary>
        public Brush Brush
        {
            get { return (Brush)GetValue(BrushProperty); }
            set { SetValue(BrushProperty, value); }
        }
        #endregion

        #region ActualBrush Dependency Property
        internal const string ActualBrushPropertyName = "ActualBrush";

        /// <summary>
        /// Identifies the ActualBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualBrushProperty = DependencyProperty.Register(ActualBrushPropertyName, typeof(Brush), typeof(StackedFragmentSeries),
        new PropertyMetadata((o, e) =>
        {
            (o as StackedFragmentSeries).RaisePropertyChanged(ActualBrushPropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets the actual brush used by the series.
        /// </summary>
        public Brush ActualBrush
        {
            get { return (Brush)GetValue(ActualBrushProperty); }
            internal set { SetValue(ActualBrushProperty, value); }
        }
        #endregion

        #region Cursor Dependency Property
        internal const string CursorPropertyName = "Cursor";

        /// <summary>
        /// Identifies the Cursor dependency property.
        /// </summary>
        public static readonly DependencyProperty CursorProperty = DependencyProperty.Register(CursorPropertyName, typeof(Cursor), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
            {
                (o as StackedFragmentSeries).RaisePropertyChanged(CursorPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the cursor of the stacked fragment.
        /// </summary>
        public Cursor Cursor
        {
            get { return (Cursor)GetValue(CursorProperty); }
            set { SetValue(CursorProperty, value); }
        }
        #endregion

        #region ActualCursor Dependency Property
        internal const string ActualCursorPropertyName = "ActualCursor";

        /// <summary>
        /// Identifies the ActualCursor dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualCursorProperty = DependencyProperty.Register(ActualCursorPropertyName, typeof(Cursor), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
                {
                    (o as StackedFragmentSeries).RaisePropertyChanged(ActualCursorPropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets the actual cursor used by the series.
        /// </summary>
        public Cursor ActualCursor
        {
            get { return (Cursor)GetValue(ActualCursorProperty); }
            internal set { SetValue(ActualCursorProperty, value); }
        }
        #endregion

        #region DashArray Property
        /// <summary>
        /// Gets or sets a collection of Double values that indicate the pattern of dashes and gaps that
        /// is used to outline the current series object. 
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public DoubleCollection DashArray
        {
            get
            {
                return (DoubleCollection)GetValue(DashArrayProperty);
            }
            set
            {
                SetValue(DashArrayProperty, value);
            }
        }

        internal const string DashArrayPropertyName = "DashArray";

        /// <summary>
        /// Identifies the StrokeDashArray dependency property.
        /// </summary>
        public static readonly DependencyProperty DashArrayProperty = DependencyProperty.Register(DashArrayPropertyName, typeof(DoubleCollection), typeof(StackedFragmentSeries),
            new PropertyMetadata((sender, e) =>
            {
                (sender as StackedFragmentSeries).RaisePropertyChanged(DashArrayPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region ActualDashArray Dependency Property
        internal const string ActualDashArrayPropertyName = "ActualDashArray";

        /// <summary>
        /// Identifies the ActualDashArray dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualDashArrayProperty = DependencyProperty.Register(ActualDashArrayPropertyName, typeof(DoubleCollection), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
                {
                    (o as StackedFragmentSeries).RaisePropertyChanged(ActualDashArrayPropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets the actual dash array used by the series.
        /// </summary>
        public DoubleCollection ActualDashArray
        {
            get { return (DoubleCollection)GetValue(ActualDashArrayProperty); }
            internal set { SetValue(ActualDashArrayProperty, value); }
        }
        #endregion

        #region DashCap Property
        /// <summary>
        /// Gets or sets the PenLineCap enumeration value that specifies how the current series object's dash ends are drawn. 
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public PenLineCap DashCap
        {
            get
            {
                return (PenLineCap)GetValue(DashCapProperty);
            }
            set
            {
                SetValue(DashCapProperty, value);
            }
        }

        internal const string DashCapPropertyName = "DashCap";

        /// <summary>
        /// Identifies the DashCap dependency property.
        /// </summary>
        public static readonly DependencyProperty DashCapProperty = DependencyProperty.Register(DashCapPropertyName, typeof(PenLineCap), typeof(StackedFragmentSeries),
            new PropertyMetadata((sender, e) =>
            {
                (sender as StackedFragmentSeries).RaisePropertyChanged(DashCapPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region ActualDashCap Dependency Property
        internal const string ActualDashCapPropertyName = "ActualDashCap";

        /// <summary>
        /// Identifies the ActualDashCap dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualDashCapProperty = DependencyProperty.Register(ActualDashCapPropertyName, typeof(PenLineCap), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
                {
                    (o as StackedFragmentSeries).RaisePropertyChanged(ActualDashCapPropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets the actual dash cap used by the series.
        /// </summary>
        public PenLineCap ActualDashCap
        {
            get { return (PenLineCap)GetValue(ActualDashCapProperty); }
            internal set { SetValue(ActualDashCapProperty, value); }
        }
        #endregion


        #region Effect Dependency Property
        internal const string EffectPropertyName = "Effect";

        /// <summary>
        /// Identifies the Effect dependency property.
        /// </summary>
        public static readonly DependencyProperty EffectProperty = DependencyProperty.Register(EffectPropertyName, typeof(Effect), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
            {
                (o as StackedFragmentSeries).RaisePropertyChanged(EffectPropertyName, e.OldValue, e.NewValue);
            }));


        /// <summary>
        /// Gets or sets the Effect of the stacked fragment.
        /// </summary>
        public Effect Effect
        {
            get { return (Effect)GetValue(EffectProperty); }
            set { SetValue(EffectProperty, value); }
        }

        #endregion

        #region ActualEffect Dependency Property
        internal const string ActualEffectPropertyName = "ActualEffect";

        /// <summary>
        /// Identifies the ActualEffect dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualEffectProperty = DependencyProperty.Register(ActualEffectPropertyName, typeof(Effect), typeof(StackedFragmentSeries),
        new PropertyMetadata((o, e) => 
        {
            (o as StackedFragmentSeries).RaisePropertyChanged(ActualEffectPropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets the actual effect used by the series.
        /// </summary>
        public Effect ActualEffect
        {
            get { return (Effect)GetValue(ActualEffectProperty); }
            internal set { SetValue(ActualEffectProperty, value); }
        }
        #endregion


        #region EndCap Dependency Property
        internal const string EndCapPropertyName = "EndCap";
        /// <summary>
        /// Identifies the EndCap dependency property.
        /// </summary>
        public static readonly DependencyProperty EndCapProperty = DependencyProperty.Register(EndCapPropertyName, typeof(PenLineCap), typeof(StackedFragmentSeries),
            new PropertyMetadata(PenLineCap.Round, (sender, e) =>
            {
                (sender as StackedFragmentSeries).RaisePropertyChanged(EndCapPropertyName, e.OldValue, e.NewValue);
            }));
        /// <summary>
        /// The style of the end point of any lines or polylines representing this series.
        /// </summary>
        /// <remarks>
        /// Not every series type has a line at which it would be appropriate to display an end cap, so this property does not affect every series type.  LineSeries, for example, is affected by EndCap, but ColumnSeries is not.
        /// </remarks>
        public PenLineCap EndCap
        {
            get
            {
                return (PenLineCap)this.GetValue(EndCapProperty);
            }
            set
            {
                this.SetValue(EndCapProperty, value);
            }
        }
        #endregion

        #region ActualEndCap Dependency Property
        internal const string ActualEndCapPropertyName = "ActualEndCap";

        /// <summary>
        /// Identifies the ActualEndCap dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualEndCapProperty = DependencyProperty.Register(ActualEndCapPropertyName, typeof(PenLineCap), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
                {
                    (o as StackedFragmentSeries).RaisePropertyChanged(ActualEndCapPropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets the actual end cap used by the series.
        /// </summary>
        public PenLineCap ActualEndCap
        {
            get { return (PenLineCap)GetValue(ActualEndCapProperty); }
            internal set { SetValue(ActualEndCapProperty, value); }
        }
        #endregion

        #region IsHitTestVisible Dependency Property
        internal const string IsHitTestVisiblePropertyName = "IsHitTestVisible";

        /// <summary>
        /// Identifies the IsHitTestVisible dependency property.
        /// </summary>
        public static readonly DependencyProperty IsHitTestVisibleProperty = DependencyProperty.Register(IsHitTestVisiblePropertyName, typeof(bool), typeof(StackedFragmentSeries),
            new PropertyMetadata(true, (o, e) =>
            {
                (o as StackedFragmentSeries).RaisePropertyChanged(IsHitTestVisiblePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets whether the series receives mouse events.
        /// </summary>
        public bool IsHitTestVisible
        {
            get { return (bool)GetValue(IsHitTestVisibleProperty); }
            set { SetValue(IsHitTestVisibleProperty, value); }
        }
        #endregion

        #region ActualIsHitTestVisible Dependency Property
        internal const string ActualIsHitTestVisiblePropertyName = "ActualIsHitTestVisible";


        /// <summary>
        /// Identifies the ActualIsHitTestVisible dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualIsHitTestVisibleProperty = DependencyProperty.Register(ActualIsHitTestVisiblePropertyName, typeof(bool), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
                {
                    (o as StackedFragmentSeries).RaisePropertyChanged(ActualIsHitTestVisiblePropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets the actual value of whether or not the series receives mouse events.
        /// </summary>
        public bool ActualIsHitTestVisible
        {
            get { return (bool)GetValue(ActualIsHitTestVisibleProperty); }
            internal set { SetValue(ActualIsHitTestVisibleProperty, value); }
        }
        #endregion

        #region LegendItemBadgeTemplate Dependency Property
        /// <summary>
        /// Gets or sets the LegendItemBadgeTemplate property.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The legend item badge is created according to the LegendItemBadgeTemplate on-demand by 
        /// the series object itself.
        /// </remarks>
        public DataTemplate LegendItemBadgeTemplate
        {
            get
            {
                return (DataTemplate)GetValue(LegendItemBadgeTemplateProperty);
            }
            set
            {
                SetValue(LegendItemBadgeTemplateProperty, value);
            }
        }

        internal const string LegendItemBadgeTemplatePropertyName = "LegendItemBadgeTemplate";

        /// <summary>
        /// Identifies the LegendItemBadgeTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty LegendItemBadgeTemplateProperty = DependencyProperty.Register(LegendItemBadgeTemplatePropertyName, typeof(DataTemplate), typeof(StackedFragmentSeries),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as StackedFragmentSeries).RaisePropertyChanged(LegendItemBadgeTemplatePropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region ActualLegendItemBadgeTemplate Dependency Property
        internal const string ActualLegendItemBadgeTemplatePropertyName = "ActualLegendItemBadgeTemplate";

        /// <summary>
        /// Identifies the ActualLegendItemBadgeTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualLegendItemBadgeTemplateProperty = DependencyProperty.Register(ActualLegendItemBadgeTemplatePropertyName, typeof(DataTemplate), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
                {
                    (o as StackedFragmentSeries).RaisePropertyChanged(ActualLegendItemBadgeTemplatePropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets the actual legend item badge template used by the series.
        /// </summary>
        public DataTemplate ActualLegendItemBadgeTemplate
        {
            get { return (DataTemplate)GetValue(ActualLegendItemBadgeTemplateProperty); }
            internal set { SetValue(ActualLegendItemBadgeTemplateProperty, value); }
        }
        #endregion

        #region LegendItemTemplate Dependency Property
        /// <summary>
        /// Gets or sets the LegendItemTemplate property.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The legend item control content is created according to the LegendItemTemplate on-demand by 
        /// the series object itself.
        /// </remarks>
        public DataTemplate LegendItemTemplate
        {
            get
            {
                return (DataTemplate)GetValue(LegendItemTemplateProperty);
            }
            set
            {
                SetValue(LegendItemTemplateProperty, value);
            }
        }

        internal const string LegendItemTemplatePropertyName = "LegendItemTemplate";

        /// <summary>
        /// Identifies the LegendItemTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty LegendItemTemplateProperty = DependencyProperty.Register(LegendItemTemplatePropertyName, typeof(DataTemplate), typeof(StackedFragmentSeries),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as StackedFragmentSeries).RaisePropertyChanged(LegendItemTemplatePropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region ActualLegendItemTemplate Dependency Property
        internal const string ActualLegendItemTemplatePropertyName = "ActualLegendItemTemplate";

        /// <summary>
        /// Identifies the ActualLegendItemTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualLegendItemTemplateProperty = DependencyProperty.Register(ActualLegendItemTemplatePropertyName, typeof(DataTemplate), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
                {
                    (o as StackedFragmentSeries).RaisePropertyChanged(ActualLegendItemTemplatePropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets the actual legend item template used by the series.
        /// </summary>
        public DataTemplate ActualLegendItemTemplate
        {
            get { return (DataTemplate)GetValue(ActualLegendItemTemplateProperty); }
            internal set { SetValue(ActualLegendItemTemplateProperty, value); }
        }
        #endregion

        #region LegendItemVisibility Dependency Property
        /// <summary>
        /// Gets or sets the legend item visibility for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Visibility LegendItemVisibility
        {
            get
            {
                return (Visibility)GetValue(LegendItemVisibilityProperty);
            }
            set
            {
                SetValue(LegendItemVisibilityProperty, value);
            }
        }

        internal const string LegendItemVisibilityPropertyName = "LegendItemVisibility";

        /// <summary>
        /// Identifies the LegendItemVisibility Dependency Property.
        /// </summary>
        public static readonly DependencyProperty LegendItemVisibilityProperty = DependencyProperty.Register(LegendItemVisibilityPropertyName, typeof(Visibility), typeof(StackedFragmentSeries),
            new PropertyMetadata(Visibility.Visible, (sender, e) =>
            {
                (sender as StackedFragmentSeries).RaisePropertyChanged(LegendItemVisibilityPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion

        #region ActualLegendItemVisibility Dependency Property
        internal const string ActualLegendItemVisibilityPropertyName = "ActualLegendItemVisibility";

        /// <summary>
        /// Identifies the ActualLegendItemVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualLegendItemVisibilityProperty = DependencyProperty.Register(ActualLegendItemVisibilityPropertyName, typeof(Visibility), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
                {
                    (o as StackedFragmentSeries).RaisePropertyChanged(ActualLegendItemVisibilityPropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets the actual visibility of the legend items in the series.
        /// </summary>
        public Visibility ActualLegendItemVisibility
        {
            get { return (Visibility)GetValue(ActualLegendItemVisibilityProperty); }
            internal set { SetValue(ActualLegendItemVisibilityProperty, value); }
        }
        #endregion

        #region MarkerBrush Dependency Property
        /// <summary>
        /// Gets or sets the brush that specifies how the current series object's marker interiors are painted.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Brush MarkerBrush
        {
            get
            {
                return (Brush)GetValue(MarkerBrushProperty);
            }
            set
            {
                SetValue(MarkerBrushProperty, value);
            }
        }

        internal const string MarkerBrushPropertyName = "MarkerBrush";

        /// <summary>
        /// Identifies the MarkerBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty MarkerBrushProperty = DependencyProperty.Register(MarkerBrushPropertyName, typeof(Brush), typeof(StackedFragmentSeries),
            new PropertyMetadata((sender, e) =>
            {
                (sender as StackedFragmentSeries).RaisePropertyChanged(MarkerBrushPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion

        #region ActualMarkerBrush Dependency Property
        internal const string ActualMarkerBrushPropertyName = "ActualMarkerBrush";

        /// <summary>
        /// Identifies the ActualMarkerBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualMarkerBrushProperty = DependencyProperty.Register(ActualMarkerBrushPropertyName, typeof(Brush), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
                {
                    (o as StackedFragmentSeries).RaisePropertyChanged(ActualMarkerBrushPropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets the actual marker brush of the series.
        /// </summary>
        public Brush ActualMarkerBrush
        {
            get { return (Brush)GetValue(ActualMarkerBrushProperty); }
            internal set { SetValue(ActualMarkerBrushProperty, value); }
        }
        #endregion

        #region MarkerOutline Dependency Property
        /// <summary>
        /// Gets or sets the brush that specifies how the current series object's marker outlines are painted.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Brush MarkerOutline
        {
            get
            {
                return (Brush)GetValue(MarkerOutlineProperty);
            }
            set
            {
                SetValue(MarkerOutlineProperty, value);
            }
        }

        internal const string MarkerOutlinePropertyName = "MarkerOutline";

        /// <summary>
        /// Identifies the MarkerOutline dependency property.
        /// </summary>
        public static readonly DependencyProperty MarkerOutlineProperty = DependencyProperty.Register(MarkerOutlinePropertyName, typeof(Brush), typeof(StackedFragmentSeries),
            new PropertyMetadata((sender, e) =>
            {
                (sender as StackedFragmentSeries).RaisePropertyChanged(MarkerOutlinePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion

        #region ActualMarkerOutline Dependency Property
        internal const string ActualMarkerOutlinePropertyName = "ActualMarkerOutline";

        /// <summary>
        /// Identifies the ActualMarkerOutline dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualMarkerOutlineProperty = DependencyProperty.Register(ActualMarkerOutlinePropertyName, typeof(Brush), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
                {
                    (o as StackedFragmentSeries).RaisePropertyChanged(ActualMarkerOutlinePropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets the actual marker outline of the series.
        /// </summary>
        public Brush ActualMarkerOutline
        {
            get { return (Brush)GetValue(ActualMarkerOutlineProperty); }
            internal set { SetValue(ActualMarkerOutlineProperty, value); }
        }
        #endregion

        #region MarkerStyle Dependency Property
        /// <summary>
        /// Gets or sets the Style to be used for the markers.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Style MarkerStyle
        {
            get
            {
                return (Style)GetValue(MarkerStyleProperty);
            }
            set
            {
                SetValue(MarkerStyleProperty, value);
            }
        }

        internal const string MarkerStylePropertyName = "MarkerStyle";

        /// <summary>
        /// Identifies the MarkerStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty MarkerStyleProperty = DependencyProperty.Register(MarkerStylePropertyName, typeof(Style), typeof(StackedFragmentSeries),
            new PropertyMetadata((sender, e) =>
            {
                (sender as StackedFragmentSeries).RaisePropertyChanged(MarkerStylePropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region ActualMarkerStyle Dependency Property
        internal const string ActualMarkerStylePropertyName = "ActualMarkerStyle";

        /// <summary>
        /// Identifies the ActualMarkerStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualMarkerStyleProperty = DependencyProperty.Register(ActualMarkerStylePropertyName, typeof(Style), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
                {
                    (o as StackedFragmentSeries).RaisePropertyChanged(ActualMarkerStylePropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets the actual marker style used by the series.
        /// </summary>
        public Style ActualMarkerStyle
        {
            get { return (Style)GetValue(ActualMarkerStyleProperty); }
            internal set { SetValue(ActualMarkerStyleProperty, value); }
        }
        #endregion

        #region MarkerTemplate Dependency Property
        /// <summary>
        /// Gets or sets the MarkerTemplate for the current series object.
        /// </summary>
        public DataTemplate MarkerTemplate
        {
            get
            {
                return (DataTemplate)GetValue(MarkerTemplateProperty);
            }
            set
            {
                SetValue(MarkerTemplateProperty, value);
            }
        }

        internal const string MarkerTemplatePropertyName = "MarkerTemplate";

        /// <summary>
        /// Identifies the MarkerTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty MarkerTemplateProperty = DependencyProperty.Register(MarkerTemplatePropertyName, typeof(DataTemplate), typeof(StackedFragmentSeries),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as StackedFragmentSeries).RaisePropertyChanged(MarkerTemplatePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion

        #region ActualMarkerTemplate Dependency Property
        internal const string ActualMarkerTemplatePropertyName = "ActualMarkerTemplate";

        /// <summary>
        /// Identifies the ActualMarkerTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualMarkerTemplateProperty = DependencyProperty.Register(ActualMarkerTemplatePropertyName, typeof(DataTemplate), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
                {
                    (o as StackedFragmentSeries).RaisePropertyChanged(ActualMarkerTemplatePropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets the actual marker template used by the series.
        /// </summary>
        public DataTemplate ActualMarkerTemplate
        {
            get { return (DataTemplate)GetValue(ActualMarkerTemplateProperty); }
            internal set { SetValue(ActualMarkerTemplateProperty, value); }
        }
        #endregion

        #region MarkerType
        /// <summary>
        /// Gets or sets the marker type for the current series object.
        /// </summary>
        /// <remarks>
        /// If the MarkerTemplate property is set, the setting of the MarkerType property will be ignored.
        /// </remarks>
        public MarkerType MarkerType
        {
            get
            {
                return (MarkerType)GetValue(MarkerTypeProperty);
            }
            set
            {
                SetValue(MarkerTypeProperty, value);
            }
        }

        internal const string MarkerTypePropertyName = "MarkerType";

        /// <summary>
        /// Identifies the MarkerType dependency property.
        /// </summary>
        public static readonly DependencyProperty MarkerTypeProperty = DependencyProperty.Register(MarkerTypePropertyName, typeof(MarkerType), typeof(StackedFragmentSeries),
            new PropertyMetadata(MarkerType.Unset, (sender, e) =>
            {
                (sender as StackedFragmentSeries).RaisePropertyChanged(MarkerTypePropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region ActualMarkerType Dependency Property
        internal const string ActualMarkerTypePropertyName = "ActualMarkerType";

        /// <summary>
        /// Identifies the ActualMarkerType dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualMarkerTypeProperty = DependencyProperty.Register(ActualMarkerTypePropertyName, typeof(MarkerType), typeof(StackedFragmentSeries),
        new PropertyMetadata((o, e) =>
        {
            (o as StackedFragmentSeries).RaisePropertyChanged(ActualMarkerTypePropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets the actual marker type set used in the series.
        /// </summary>
        public MarkerType ActualMarkerType
        {
            get { return (MarkerType)GetValue(ActualMarkerTypeProperty); }
            internal set { SetValue(ActualMarkerTypeProperty, value); }
        }
        #endregion

        #region Name Dependency Property
        internal const string NamePropertyName = "Name";

        /// <summary>
        /// Identifies the Name dependency property.
        /// </summary>
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register(NamePropertyName, typeof(string), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
            {
                (o as StackedFragmentSeries).RaisePropertyChanged(NamePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the Name of the stacked fragment.
        /// </summary>
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        #endregion

        #region Opacity Dependency Property
        internal const string OpacityPropertyName = "Opacity";

        /// <summary>
        /// Identifies the Opacity dependency property.
        /// </summary>
        public static readonly DependencyProperty OpacityProperty = DependencyProperty.Register(OpacityPropertyName, typeof(double), typeof(StackedFragmentSeries),
            new PropertyMetadata(1.0, (o, e) =>
            {
                (o as StackedFragmentSeries).RaisePropertyChanged(OpacityPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the Opacity of the stacked fragment.
        /// </summary>
        public double Opacity
        {
            get { return (double)GetValue(OpacityProperty); }
            set { SetValue(OpacityProperty, value); }
        }
        #endregion

        #region ActualOpacity Dependency Property
        internal const string ActualOpacityPropertyName = "ActualOpacity";

        /// <summary>
        /// Identifies the ActualOpacity depenedency property.
        /// </summary>
        public static readonly DependencyProperty ActualOpacityProperty = DependencyProperty.Register(ActualOpacityPropertyName, typeof(double), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
                {
                    (o as StackedFragmentSeries).RaisePropertyChanged(ActualOpacityPropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets the series opacity.
        /// </summary>
        public double ActualOpacity
        {
            get { return (double)GetValue(ActualOpacityProperty); }
            internal set { SetValue(ActualOpacityProperty, value); }
        }
        #endregion

        #region OpacityMask Dependency Property
        internal const string OpacityMaskPropertyName = "OpacityMask";

        /// <summary>
        /// Identifies the OpacityMask dependency property.
        /// </summary>
        public static readonly DependencyProperty OpacityMaskProperty = DependencyProperty.Register(OpacityMaskPropertyName, typeof(Brush), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
            {
                (o as StackedFragmentSeries).RaisePropertyChanged(OpacityMaskPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the OpacityMask of the stacked fragment.
        /// </summary>
        public Brush OpacityMask
        {
            get { return (Brush)GetValue(OpacityMaskProperty); }
            set { SetValue(OpacityMaskProperty, value); }
        }
        #endregion

        #region ActualOpacityMask Dependency Property
        internal const string ActualOpacityMaskPropertyName = "ActualOpacityMask";

        /// <summary>
        /// Identifies the ActualOpacityMask dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualOpacityMaskProperty = DependencyProperty.Register(ActualOpacityMaskPropertyName, typeof(Brush), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) => 
                {
                    (o as StackedFragmentSeries).RaisePropertyChanged(ActualOpacityMaskPropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets the series opacity mask.
        /// </summary>
        public Brush ActualOpacityMask
        {
                get { return (Brush)GetValue(ActualOpacityMaskProperty); }
                internal set { SetValue(ActualOpacityMaskProperty, value); }
        }
        #endregion

        #region Outline Dependency Property
        /// <summary>
        /// Gets or sets the brush to use for the outline of the series.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>Some series types, such as LineSeries, do not display outlines.  Therefore, this property does not affect some charts.</remarks>
        public Brush Outline
        {
            get
            {
                return (Brush)GetValue(OutlineProperty);
            }
            set
            {
                SetValue(OutlineProperty, value);
            }
        }

        internal const string OutlinePropertyName = "Outline";
        /// <summary>
        /// Identifies the Outline dependency property.
        /// </summary>
        public static readonly DependencyProperty OutlineProperty = DependencyProperty.Register(OutlinePropertyName, typeof(Brush), typeof(StackedFragmentSeries),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as StackedFragmentSeries).RaisePropertyChanged(OutlinePropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region ActualOutline Dependency Property
        internal const string ActualOutlinePropertyName = "ActualOutline";

        /// <summary>
        /// Identifies the ActualOutline dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualOutlineProperty = DependencyProperty.Register(ActualOutlinePropertyName, typeof(Brush), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
                {
                    (o as StackedFragmentSeries).RaisePropertyChanged(ActualOutlinePropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets the series outline.
        /// </summary>
        public Brush ActualOutline
        {
            get { return (Brush)GetValue(ActualOutlineProperty); }
            internal set { SetValue(ActualOutlineProperty, value); }
        }
        #endregion

        #region RadiusX Dependency Property
        /// <summary>
        /// Gets or sets the x-radius of the ellipse that is used to round the corners of the column. This only applies to Bar and Column series.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>  
        public double RadiusX
        {
            get
            {
                return (double)GetValue(RadiusXProperty);
            }
            set
            {
                SetValue(RadiusXProperty, value);
            }
        }

        internal const string RadiusXPropertyName = "RadiusX";

        /// <summary>
        /// Identifies the RadiusX dependency property.
        /// </summary>
        public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register(RadiusXPropertyName, typeof(double), typeof(StackedFragmentSeries),
            new PropertyMetadata(2.0, (sender, e) =>
            {
                (sender as StackedFragmentSeries).RaisePropertyChanged(RadiusXPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region ActualRadiusX Dependency Property
        internal const string ActualRadiusXPropertyName = "ActualRadiusX";

        /// <summary>
        /// Identifies the ActualRadiusX dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualRadiusXProperty = DependencyProperty.Register(ActualRadiusXPropertyName, typeof(double), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
                {
                    (o as StackedFragmentSeries).RaisePropertyChanged(ActualRadiusXPropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets the actual corner radius of the series
        /// </summary>
        public double ActualRadiusX
        {
            get { return (double)GetValue(ActualRadiusXProperty); }
            internal set { SetValue(ActualRadiusXProperty, value); }
        }
        #endregion

        #region RadiusY Dependency Property
        /// <summary>
        /// Gets or sets the y-radius of the ellipse that is used to round the corners of the column. This only applies to Bar and Column series.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public double RadiusY
        {
            get
            {
                return (double)GetValue(RadiusYProperty);
            }
            set
            {
                SetValue(RadiusYProperty, value);
            }
        }

        internal const string RadiusYPropertyName = "RadiusY";

        /// <summary>
        /// Identifies the RadiusY dependency property.
        /// </summary>
        public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register(RadiusYPropertyName, typeof(double), typeof(StackedFragmentSeries),
            new PropertyMetadata(2.0, (sender, e) =>
            {
                (sender as StackedFragmentSeries).RaisePropertyChanged(RadiusYPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region ActualRadiusY Dependency Property
        internal const string ActualRadiusYPropertyName = "ActualRadiusY";

        /// <summary>
        /// Identifies the ActualRadiusY dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualRadiusYProperty = DependencyProperty.Register(ActualRadiusYPropertyName, typeof(double), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
                {
                    (o as StackedFragmentSeries).RaisePropertyChanged(ActualRadiusYPropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets the actual corner radius of the series
        /// </summary>
        public double ActualRadiusY
        {
            get { return (double)GetValue(ActualRadiusYProperty); }
            internal set { SetValue(ActualRadiusYProperty, value); }
        }
        #endregion

        #region StartCap Dependency Property
        internal const string StartCapPropertyName = "StartCap";
        /// <summary>
        /// Identifies the StartCap dependency property.
        /// </summary>
        public static readonly DependencyProperty StartCapProperty = DependencyProperty.Register(StartCapPropertyName, typeof(PenLineCap), typeof(StackedFragmentSeries),
            new PropertyMetadata(PenLineCap.Round, (sender, e) =>
            {
                (sender as StackedFragmentSeries).RaisePropertyChanged(StartCapPropertyName, e.OldValue, e.NewValue);
            }));
        /// <summary>
        /// The style of the starting point of any lines or polylines representing this series.
        /// </summary>
        /// <remarks>
        /// Not every series type has a line at which it would be appropriate to display a start cap, so this property does not affect every series type.  LineSeries, for example, is affected by StartCap, but ColumnSeries is not.
        /// </remarks>
        public PenLineCap StartCap
        {
            get
            {
                return (PenLineCap)this.GetValue(StartCapProperty);
            }
            set
            {
                this.SetValue(StartCapProperty, value);
            }
        }
        #endregion

        #region ActualStartCap Dependency Property
        internal const string ActualStartCapPropertyName = "ActualStartCap";

        /// <summary>
        /// Identifies the ActualStartCap dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualStartCapProperty = DependencyProperty.Register(ActualStartCapPropertyName, typeof(PenLineCap), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
                {
                    (o as StackedFragmentSeries).RaisePropertyChanged(ActualStartCapPropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets the series start cap.
        /// </summary>
        public PenLineCap ActualStartCap
        {
            get { return (PenLineCap)GetValue(ActualStartCapProperty); }
            internal set { SetValue(ActualStartCapProperty, value); }
        }
        #endregion

        #region Thickness Property
        /// <summary>
        /// Gets or sets the width of the current series object's line thickness.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public double Thickness
        {
            get
            {
                return (double)GetValue(ThicknessProperty);
            }
            set
            {
                SetValue(ThicknessProperty, value);
            }
        }

        internal const string ThicknessPropertyName = "Thickness";

        /// <summary>
        /// Identifies the Thickness dependency property.
        /// </summary>
        /// <remarks>
        /// There is a problematic behavior in Silverlight 3 where changing the StrokeThickness property of many shapes is not reflected at runtime.  If changing this property seems to have no effect, please use the workaround of making another change to the UI to force a refresh.
        /// <code>
        /// theChart.RenderTransform = new RotateTransform() { Angle = 0.01 };
        /// Dispatcher.BeginInvoke( () => theChart.RenderTransform = null);
        /// </code>
        /// </remarks>
        public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register(ThicknessPropertyName, typeof(double), typeof(StackedFragmentSeries),
            new PropertyMetadata(1.5, (sender, e) =>
            {
                (sender as StackedFragmentSeries).RaisePropertyChanged(ThicknessPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region ActualThickness Dependency Property
        internal const string ActualThicknessPropertyName = "ActualThickness";

        /// <summary>
        /// Identifies the ActualThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualThicknessProperty = DependencyProperty.Register(ActualThicknessPropertyName, typeof(double), typeof(StackedFragmentSeries),
        new PropertyMetadata((o, e) =>
        {
            (o as StackedFragmentSeries).RaisePropertyChanged(ActualThicknessPropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets or sets the thickness of this stacked fragment.
        /// </summary>
        public double ActualThickness
        {
            get { return (double)GetValue(ActualThicknessProperty); }
            internal set { SetValue(ActualThicknessProperty, value); }
        }
        #endregion

        #region Title Dependency Property
        /// <summary>
        /// Gets or sets the Title property.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The legend item control is created according to the Title on-demand by 
        /// the series object itself.
        /// </remarks>
        [TypeConverter(typeof(ObjectConverter))]
        public object Title
        {
            get
            {
                return (object)GetValue(TitleProperty);
            }
            set
            {
                SetValue(TitleProperty, value);
            }
        }

        internal const string TitlePropertyName = "Title";

        /// <summary>
        /// Identifies the Title dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(TitlePropertyName, typeof(object), typeof(StackedFragmentSeries),
            new PropertyMetadata("Series Title", (sender, e) =>
            {
                (sender as StackedFragmentSeries).RaisePropertyChanged(TitlePropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region ToolTip Dependency Property
        /// <summary>
        /// Gets or sets the ToolTip for the current series object.
        /// <para>This is a dependency property.</para>
        /// </summary>
        [TypeConverter(typeof(ObjectConverter))]
        public object ToolTip
        {
            get
            {
                return (object)GetValue(ToolTipProperty);
            }
            set
            {
                SetValue(ToolTipProperty, value);
            }
        }

        internal const string ToolTipPropertyName = "ToolTip";

        /// <summary>
        /// Identifies the ToolTip dependency property.
        /// </summary>
        public static readonly DependencyProperty ToolTipProperty = DependencyProperty.Register(ToolTipPropertyName, typeof(object), typeof(StackedFragmentSeries),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as StackedFragmentSeries).RaisePropertyChanged(ToolTipPropertyName, e.OldValue, e.NewValue);
            }));

        internal StringFormatter ToolTipFormatter = new StringFormatter();
        #endregion

        #region ActualToolTip Dependency Property
        internal const string ActualToolTipPropertyName = "ActualToolTip";

        /// <summary>
        /// Identifies the ActualToolTip dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualToolTipProperty = DependencyProperty.Register(ActualToolTipPropertyName, typeof(object), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
                {
                    (o as StackedFragmentSeries).RaisePropertyChanged(ActualToolTipPropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets the series tooltip.
        /// </summary>
        public object ActualToolTip
        {
            get { return (object)GetValue(ActualToolTipProperty); }
            internal set { SetValue(ActualToolTipProperty, value); }
        }
        #endregion


#region Infragistics Source Cleanup (Region)
































































































#endregion // Infragistics Source Cleanup (Region)

        #region UseLightweightMarkers Dependency Property
        /// <summary>
        /// Gets or sets the marker type for the current series object.
        /// </summary>
        public bool UseLightweightMarkers
        {
            get
            {
                return (bool)GetValue(UseLightweightMarkersProperty);
            }
            set
            {
                SetValue(UseLightweightMarkersProperty, value);
            }
        }

        internal const string UseLightweightMarkersPropertyName = "UseLightweightMarkers";

        /// <summary>
        /// Identifies the UseLightweightMarkers dependency property.
        /// </summary>
        public static readonly DependencyProperty UseLightweightMarkersProperty = DependencyProperty.Register(UseLightweightMarkersPropertyName, typeof(bool), typeof(StackedFragmentSeries),
            new PropertyMetadata(false, (sender, e) =>
            {
                (sender as StackedFragmentSeries).RaisePropertyChanged(UseLightweightMarkersPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region ActualUseLightweightMarkers Dependency Property
        internal const string ActualUseLightweightMarkersPropertyName = "ActualUseLightweightMarkers";

        /// <summary>
        /// Identifies the ActualUseLightweightMarkers dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualUseLightweightMarkersProperty = DependencyProperty.Register(ActualUseLightweightMarkersPropertyName, typeof(bool), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
                {
                    (o as StackedFragmentSeries).RaisePropertyChanged(ActualUseLightweightMarkersPropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets whether lightweight markers are used by the series.
        /// </summary>
        public bool ActualUseLightweightMarkers
        {
            get { return (bool)GetValue(ActualUseLightweightMarkersProperty); }
            internal set { SetValue(ActualUseLightweightMarkersProperty, value); }
        }
        #endregion

        #region ValueMemberPath Dependency Property
        /// <summary>
        /// Gets or sets the value mapping property for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public string ValueMemberPath
        {
            get
            {
                return (string)GetValue(ValueMemberPathProperty);
            }
            set
            {
                SetValue(ValueMemberPathProperty, value);
            }
        }

        internal const string ValueMemberPathPropertyName = "ValueMemberPath";

        /// <summary>
        /// Identifies the ValueMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueMemberPathProperty = DependencyProperty.Register(ValueMemberPathPropertyName, typeof(string), typeof(StackedFragmentSeries),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as StackedFragmentSeries).RaisePropertyChanged(ValueMemberPathPropertyName, e.OldValue, e.NewValue);
        }));
        #endregion

        #region Visibility Dependency Property
        internal const string VisibilityPropertyName = "Visibility";

        /// <summary>
        /// Identifies the Visibility dependency property.
        /// </summary>
        public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register(VisibilityPropertyName, typeof(Visibility), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
            {
                (o as StackedFragmentSeries).RaisePropertyChanged(VisibilityPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the Visibility of the stacked fragment.
        /// </summary>
        public Visibility Visibility
        {
            get { return (Visibility)GetValue(VisibilityProperty); }
            set { SetValue(VisibilityProperty, value); }
        }
        #endregion

        #region ActualVisibility Dependency Property
        internal const string ActualVisibilityPropertyName = "ActualVisibility";
        
       /// <summary>
        /// Identifies the ActualVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualVisibilityProperty = DependencyProperty.Register(ActualVisibilityPropertyName, typeof(Visibility), typeof(StackedFragmentSeries),
            new PropertyMetadata((o, e) =>
            {
                (o as StackedFragmentSeries).RaisePropertyChanged(ActualVisibilityPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets the actual visibility of the stacked fragment.
        /// </summary>
        public Visibility ActualVisibility
        {
            get { return (Visibility)GetValue(ActualVisibilityProperty); }
            internal set { SetValue(ActualVisibilityProperty, value); }
        }
        #endregion
        #endregion

        #region Non-public Properties
        internal StackedSeriesBase ParentSeries { get; set; }
        internal int Index { get; set; }
        internal SeriesViewer Chart { get; set; }
        internal IFastItemColumn<double> ValueColumn { get; set; }
        internal AnchoredCategorySeries VisualSeriesLink { get; set; }
        internal List<double> HighValues { get; set; }
        internal List<double> LowValues { get; set; }
        internal List<float[]> Buckets { get; set; }
        internal bool Positive { get; set; }
        #endregion

        #region Methods
        internal void UpdateVisibility()
        {
            if (ParentSeries == null) return;
            ActualVisibility = ParentSeries.Visibility != Visibility.Visible ? Visibility.Collapsed : Visibility;
        }

        internal void UpdateMarkerTemplate()
        {
            if (ParentSeries == null) return;
            ActualMarkerTemplate = MarkerTemplate ?? ParentSeries.MarkerTemplate;
        }

        internal void UpdateMarkerType()
        {
            if (ParentSeries == null) return;
            MarkerType localMarkerType = MarkerType == MarkerType.Unset ? MarkerType.None : MarkerType;
            ActualMarkerType = ReadLocalValue(MarkerTypeProperty) == DependencyProperty.UnsetValue ? ParentSeries.MarkerType : localMarkerType;
        }

        internal void UpdateBrush()
        {
            if (ParentSeries == null) return;
            ActualBrush = Brush ?? ParentSeries.Brush;
        }

        internal void UpdateDashArray()
        {
            if (ParentSeries == null) return;
            ActualDashArray = DashArray ?? ParentSeries.DashArray;
        }

        internal void UpdateDashCap()
        {
            if (ParentSeries == null) return;
            ActualDashCap = ReadLocalValue(DashCapProperty) == DependencyProperty.UnsetValue ? ParentSeries.DashCap : DashCap;
        }


        internal void UpdateCursor()
        {
            if (ParentSeries == null) return;
            ActualCursor = Cursor ?? ParentSeries.Cursor;
        }

        internal void UpdateEffect()
        {
            if (ParentSeries == null) return;
            ActualEffect = Effect ?? ParentSeries.Effect;
        }


        internal void UpdateEndCap()
        {
            if (ParentSeries == null) return;
            ActualEndCap = ReadLocalValue(EndCapProperty) == DependencyProperty.UnsetValue ? ParentSeries.EndCap : EndCap;
        }

        internal void UpdateIsHitTestVisible()
        {
            if (ParentSeries == null) return;
            ActualIsHitTestVisible = ReadLocalValue(IsHitTestVisibleProperty) == DependencyProperty.UnsetValue ? ParentSeries.IsHitTestVisible : IsHitTestVisible;
        }
        #endregion

        internal void UpdateLegendItemBadgeTemplate()
        {
            if (ParentSeries == null) return;
            ActualLegendItemBadgeTemplate = LegendItemBadgeTemplate ?? ParentSeries.LegendItemBadgeTemplate;

            if (VisualSeriesLink != null)
            {
                if (ActualLegendItemBadgeTemplate != null)
                {
                    VisualSeriesLink.LegendItemBadgeTemplate = ActualLegendItemBadgeTemplate;
                }
                else
                {
                    VisualSeriesLink.ClearValue(Series.LegendItemBadgeTemplateProperty);
                }
            }
        }

        internal void UpdateLegendItemTemplate()
        {
            if (ParentSeries == null) return;
            ActualLegendItemTemplate = LegendItemTemplate ?? ParentSeries.LegendItemTemplate;

            if (VisualSeriesLink != null)
            {
                if (ActualLegendItemTemplate != null)
                {
                    VisualSeriesLink.LegendItemTemplate = ActualLegendItemTemplate;
                }
                else
                {
                    VisualSeriesLink.ClearValue(Series.LegendItemTemplateProperty);
                }
            }
        }

        internal void UpdateLegendItemVisibility()
        {
            if (ParentSeries == null) return;
            ActualLegendItemVisibility = ParentSeries.LegendItemVisibility != Visibility.Visible ? Visibility.Collapsed : LegendItemVisibility;
        }

        internal void UpdateMarkerBrush()
        {
            if (ParentSeries == null) return;
            ActualMarkerBrush = MarkerBrush ?? ParentSeries.MarkerBrush;
        }

        internal void UpdateMarkerOutline()
        {
            if (ParentSeries == null) return;
            ActualMarkerOutline = MarkerOutline ?? ParentSeries.MarkerOutline;
        }

        internal void UpdateMarkerStyle()
        {
            if (ParentSeries == null) return;
            ActualMarkerStyle = MarkerStyle ?? ParentSeries.MarkerStyle;

            if (VisualSeriesLink != null)
            {
                if (ActualMarkerStyle != null)
                {
                    VisualSeriesLink.MarkerStyle = ActualMarkerStyle;
                }
                else
                {
                    VisualSeriesLink.ClearValue(MarkerSeries.MarkerStyleProperty);
                }
            }
        }

        internal void UpdateOpacity()
        {
            if (ParentSeries == null) return;
            ActualOpacity = ReadLocalValue(OpacityProperty) == DependencyProperty.UnsetValue ? ParentSeries.Opacity : Opacity;
        }

        internal void UpdateOpacityMask()
        {
            if (ParentSeries == null) return;
            ActualOpacityMask = OpacityMask ?? ParentSeries.OpacityMask;
        }

        internal void UpdateOutline()
        {
            if (ParentSeries == null) return;
            ActualOutline = Outline ?? ParentSeries.Outline;
        }

        internal void UpdateRadiusX()
        {
            if (ParentSeries == null) return;
            
            double radiusX = double.NaN;
            
            if (ParentSeries is StackedColumnSeries)
            {
                radiusX = (ParentSeries as StackedColumnSeries).RadiusX;
            }
           
            if (ParentSeries is StackedBarSeries)
            {
                radiusX = (ParentSeries as StackedBarSeries).RadiusX;
            }

            ActualRadiusX = ReadLocalValue(RadiusXProperty) == DependencyProperty.UnsetValue 
                && !double.IsNaN(radiusX) ? radiusX : RadiusX;
        }

        internal void UpdateRadiusY()
        {
            if (ParentSeries == null) return;

            double radiusY = double.NaN;

            if (ParentSeries is StackedColumnSeries)
            {
                radiusY = (ParentSeries as StackedColumnSeries).RadiusY;
            }

            if (ParentSeries is StackedBarSeries)
            {
                radiusY = (ParentSeries as StackedBarSeries).RadiusY;
            }

            ActualRadiusY = ReadLocalValue(RadiusYProperty) == DependencyProperty.UnsetValue
                && !double.IsNaN(radiusY) ? radiusY : RadiusY;
        }

        internal void UpdateStartCap()
        {
            if (ParentSeries == null) return;
            ActualStartCap = ReadLocalValue(StartCapProperty) == DependencyProperty.UnsetValue ? ParentSeries.StartCap : StartCap;
        }

        internal void UpdateThickness()
        {
            if (ParentSeries == null) return;
            ActualThickness = ReadLocalValue(ThicknessProperty) == DependencyProperty.UnsetValue ? ParentSeries.Thickness : Thickness;
        }

        internal void UpdateToolTip()
        {
            if (ParentSeries == null) return;
            ActualToolTip = ToolTip ?? ParentSeries.ToolTip;
        }


#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

        internal void UpdateUseLightweightMarkers()
        {
            if (ParentSeries == null) return;
            ActualUseLightweightMarkers = ReadLocalValue(UseLightweightMarkersProperty) == DependencyProperty.UnsetValue ? ParentSeries.UseLightweightMarkers : UseLightweightMarkers;
        }

        #region Events
        /// <summary>
        /// Occurs when the mouse enters the visual fragment series.
        /// </summary>
        [Obsolete("This event is unused.")]
        public event MouseEventHandler MouseEnter;

        /// <summary>
        /// Occurs when the mouse leaves the visual fragment series.
        /// </summary>
        [Obsolete("This event is unused.")]
        public event MouseEventHandler MouseLeave;

        /// <summary>
        /// Occurs when the left button of the mouse is pressed over the visual fragment series.
        /// </summary>
        [Obsolete("This event is unused.")]
        public event MouseButtonEventHandler MouseLeftButtonDown;

        /// <summary>
        /// Occurs when the left button of the mouse is released over the visual fragment series.
        /// </summary>
        [Obsolete("This event is unused.")]
        public event MouseButtonEventHandler MouseLeftButtonUp;

        /// <summary>
        /// Occus when the mouse is moved within the visual fragment series.
        /// </summary>
        [Obsolete("This event is unused.")]
        public event MouseEventHandler MouseMove;

        /// <summary>
        /// Occurs when the right button of the mouse is pressed over the visual fragment series.
        /// </summary>
        [Obsolete("This event is unused.")]
        public event MouseButtonEventHandler MouseRightButtonDown;

        /// <summary>
        /// Occurs when the right button of the mouse is released over the visual fragment series.
        /// </summary>
        [Obsolete("This event is unused.")]
        public event MouseButtonEventHandler MouseRightButtonUp;

        /// <summary>
        /// Occurs when the mouse wheel is used on a visual fragment series.
        /// </summary>
        [Obsolete("This event is unused.")]
        public event MouseWheelEventHandler MouseWheel;
        #endregion

        #region INotifyPropertyChanged implementation
        /// <summary>
        /// Occurs when a property (including "effective" and non-dependency property) value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when a property (including "effective" and non-dependency property) value changes.
        /// </summary>
        public event PropertyUpdatedEventHandler PropertyUpdated;

        /// <summary>
        /// Raises PropertyChanged and/or PropertyUpdated events if any listeners have been registered.
        /// </summary>
        /// <param name="propertyName">Name of property whos value changed.</param>
        /// <param name="oldValue">Property value before change.</param>
        /// <param name="newValue">Property value after change.</param>
        protected void RaisePropertyChanged(string propertyName, object oldValue, object newValue)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            if (PropertyUpdated != null)
            {
                PropertyUpdated(this, new PropertyUpdatedEventArgs(propertyName, oldValue, newValue));
            }
        }
        #endregion

        #region Property Updates
         /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated
        /// on the series or owning chart. Gives the series a chance to respond to the various property updates.
        /// </summary>
        /// <param name="sender">The object being updated.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected virtual void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            if (ParentSeries == null) return;

            switch (propertyName)
            {
                case VisibilityPropertyName:
                    UpdateVisibility();
                    break;

                case BrushPropertyName:
                    UpdateBrush();
                    break;

                case DashArrayPropertyName:
                    UpdateDashArray();
                    break;

                case DashCapPropertyName:
                    UpdateDashCap();
                    break;


                case CursorPropertyName:
                    UpdateCursor();
                    break;

                case EffectPropertyName:
                    UpdateEffect();
                    break;


                case EndCapPropertyName:
                    UpdateEndCap();
                    break;

                case IsHitTestVisiblePropertyName:
                    UpdateIsHitTestVisible();
                    break;

                case MarkerTemplatePropertyName:
                    UpdateMarkerTemplate();
                    break;

                case MarkerTypePropertyName:
                    UpdateMarkerType();
                    break;

                case LegendItemBadgeTemplatePropertyName:
                    UpdateLegendItemBadgeTemplate();
                    break;

                case LegendItemTemplatePropertyName:
                    UpdateLegendItemTemplate();
                    break;

                case LegendItemVisibilityPropertyName:
                    UpdateLegendItemVisibility();
                    break;

                case MarkerBrushPropertyName:
                    UpdateMarkerBrush();
                    break;

                case MarkerOutlinePropertyName:
                    UpdateMarkerOutline();
                    break;

                case MarkerStylePropertyName:
                    UpdateMarkerStyle();
                    break;

                case OpacityPropertyName:
                    UpdateOpacity();
                    break;

                case OpacityMaskPropertyName:
                    UpdateOpacityMask();
                    break;

                case OutlinePropertyName:
                    UpdateOutline();
                    break;

                case RadiusXPropertyName:
                    UpdateRadiusX();
                    break;

                case RadiusYPropertyName:
                    UpdateRadiusY();
                    break;

                case StartCapPropertyName:
                    UpdateStartCap();
                    break;

                case ThicknessPropertyName:
                    UpdateThickness();
                    break;

                case ToolTipPropertyName:
                    UpdateToolTip();
                    break;


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

                case UseLightweightMarkersPropertyName:
                    UpdateUseLightweightMarkers();
                    break;
                        
            }

            ParentSeries.RenderSeries(false);
        }
        #endregion
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