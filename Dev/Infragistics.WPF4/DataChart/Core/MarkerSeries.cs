using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading;
using Infragistics.Controls.Charts.Util;
using System.Runtime.CompilerServices;
using Infragistics.Controls.Charts.VisualData;
using System.ComponentModel;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the base class for series containing markers.
    /// </summary>
    [StyleTypedProperty(Property = "MarkerStyle", StyleTargetType = typeof(Marker))]
    public abstract class MarkerSeries
        : Series
    {

        internal MarkerSeriesView MarkerView { get; set; }

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);
            MarkerView = (MarkerSeriesView)view;
        }

        #region MarkerType
        /// <summary>
        /// Gets or sets the marker type for the current series object.
        /// </summary>
        /// <remarks>
        /// If the MarkerTemplate property is set, the setting of the MarkerType property will be ignored.
        /// </remarks>
        [WidgetDefaultString("none")]
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
        public static readonly DependencyProperty MarkerTypeProperty = DependencyProperty.Register(MarkerTypePropertyName, typeof(MarkerType), typeof(MarkerSeries),
            new PropertyMetadata(MarkerType.None, (sender, e) =>
            {
                (sender as MarkerSeries).RaisePropertyChanged(MarkerTypePropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region MarkerTemplate and ActualMarkerTemplate Dependency Properties
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
        public static readonly DependencyProperty MarkerTemplateProperty = DependencyProperty.Register(MarkerTemplatePropertyName, typeof(DataTemplate), typeof(MarkerSeries),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as MarkerSeries).RaisePropertyChanged(MarkerTemplatePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets the effective marker template for the current series object.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public DataTemplate ActualMarkerTemplate
        {
            get
            {
                return (DataTemplate)GetValue(ActualMarkerTemplateProperty);
            }
            internal set
            {
                SetValue(ActualMarkerTemplateProperty, value);
            }
        }

        internal const string ActualMarkerTemplatePropertyName = "ActualMarkerTemplate";

        /// <summary>
        /// Identifies the ActualMarkerTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualMarkerTemplateProperty = DependencyProperty.Register(ActualMarkerTemplatePropertyName, typeof(DataTemplate), typeof(MarkerSeries),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as MarkerSeries).RaisePropertyChanged(ActualMarkerTemplatePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// The marker template to use if the MarkerTemplate property is set to null.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DataTemplate NullMarkerTemplate = new DataTemplate();
        #endregion

        #region MarkerBrush and ActualMarkerBrush Dependency Properties
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
        public static readonly DependencyProperty MarkerBrushProperty = DependencyProperty.Register(MarkerBrushPropertyName, typeof(Brush), typeof(MarkerSeries),
            new PropertyMetadata((sender, e) =>
            {
                var markerSeries = (sender as MarkerSeries);
                markerSeries.RaisePropertyChanged(MarkerBrushPropertyName, e.OldValue, e.NewValue);




            }));



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


        /// <summary>
        /// Gets the effective marker brush for the current series object.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public Brush ActualMarkerBrush
        {
            get
            {
                return (Brush)GetValue(ActualMarkerBrushProperty);
            }
            internal set
            {
                SetValue(ActualMarkerBrushProperty, value);
            }
        }

        internal const string ActualMarkerBrushPropertyName = "ActualMarkerBrush";

        /// <summary>
        /// Identifies the ActualMarkerBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualMarkerBrushProperty = DependencyProperty.Register(ActualMarkerBrushPropertyName, typeof(Brush), typeof(MarkerSeries),
            new PropertyMetadata((sender, e) =>
            {
                (sender as MarkerSeries).RaisePropertyChanged(ActualMarkerBrushPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region MarkerOutline and ActualMarkerOutline Dependency Properties
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
        public static readonly DependencyProperty MarkerOutlineProperty = DependencyProperty.Register(MarkerOutlinePropertyName, typeof(Brush), typeof(MarkerSeries),
            new PropertyMetadata((sender, e) =>
            {
                (sender as MarkerSeries).RaisePropertyChanged(MarkerOutlinePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets the effective marker outline for the current series object.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public Brush ActualMarkerOutline
        {
            get
            {
                return (Brush)GetValue(ActualMarkerOutlineProperty);
            }
            internal set
            {
                SetValue(ActualMarkerOutlineProperty, value);
            }
        }

        internal const string ActualMarkerOutlinePropertyName = "ActualMarkerOutline";

        /// <summary>
        /// Identifies the ActualMarkerOutline dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualMarkerOutlineProperty = DependencyProperty.Register(ActualMarkerOutlinePropertyName, typeof(Brush), typeof(MarkerSeries),
            new PropertyMetadata((sender, e) =>
            {
                (sender as MarkerSeries).RaisePropertyChanged(ActualMarkerOutlinePropertyName, e.OldValue, e.NewValue);
            }));
        #endregion


        #region MarkerStyle Dependency Property
        /// <summary>
        /// Gets or sets the Style to be used for the markers.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [SuppressWidgetMember]
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
        public static readonly DependencyProperty MarkerStyleProperty = DependencyProperty.Register(MarkerStylePropertyName, typeof(Style), typeof(MarkerSeries),
            new PropertyMetadata((sender, e) =>
            {
                (sender as MarkerSeries).RaisePropertyChanged(MarkerStylePropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region UseLightweightMarkers
        /// <summary>
        /// Gets or sets the marker type for the current series object.
        /// </summary>
        [SuppressWidgetMember]
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
        public static readonly DependencyProperty UseLightweightMarkersProperty = 
            DependencyProperty.Register(
            UseLightweightMarkersPropertyName, 
            typeof(bool), typeof(MarkerSeries),
            new PropertyMetadata(false, (sender, e) =>
            {
                (sender as MarkerSeries)
                    .RaisePropertyChanged(
                    UseLightweightMarkersPropertyName, 
                    e.OldValue, 
                    e.NewValue);
            }));
        #endregion

        /// <summary>
        /// Returns true if the markers should be displayed.
        /// </summary>
        /// <returns>True if the marker should be displayed.</returns>
        internal protected virtual bool ShouldDisplayMarkers()
        {
            return ActualMarkerTemplate != null &&
                ((MarkerType != MarkerType.None && MarkerType != MarkerType.Unset) ||
                MarkerTemplate != null); // [DN 2/5/2010] if the MarkerTemplate property is set, we should render markers even if MarkerType is set to None.
        }

        /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated
        /// on the series or owning chart. Gives the series a chance to respond to the various property updates.
        /// </summary>
        /// <param name="sender">The object being updated.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected override void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);

            switch (propertyName)
            {
                case MarkerSeries.MarkerBrushPropertyName:
                case MarkerSeries.MarkerTypePropertyName:
                case MarkerSeries.MarkerOutlinePropertyName:
                case MarkerSeries.MarkerTemplatePropertyName:
                    UpdateIndexedProperties();
                    break;
                case MarkerSeries.ActualMarkerTemplatePropertyName:
                    if (oldValue == MarkerSeries.NullMarkerTemplate ||
                        newValue == MarkerSeries.NullMarkerTemplate ||
                        (oldValue == null || newValue != null))
                    {
                        MarkerView.DoUpdateMarkerTemplates();
                        MarkerSeriesView thumbnailView = this.ThumbnailView as MarkerSeriesView;
                        if (thumbnailView != null)
                        {
                            thumbnailView.DoUpdateMarkerTemplates();
                        }
                        RenderSeries(false);
                    }
                    this.NotifyThumbnailAppearanceChanged();
                    break;
                case MarkerSeries.UseLightweightMarkersPropertyName:
                    MarkerView.SetUseLightweightMode(UseLightweightMarkers);
                    RenderSeries(false);
                    break;
            }
        }

        /// <summary>
        /// Provides the property name of the marker template on the chart that should be bound to based on a MarkerType enumeration value.
        /// </summary>
        /// <param name="markerType">The enumeration value to map to a property name.</param>
        /// <returns>The proeprty name.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static string GetMarkerTemplatePropertyName(MarkerType markerType)
        {
            switch (markerType)
            {
                case MarkerType.Circle:
                    return XamDataChart.CircleMarkerTemplatePropertyName;
                case MarkerType.Triangle:
                    return XamDataChart.TriangleMarkerTemplatePropertyName;
                case MarkerType.Pyramid:
                    return XamDataChart.PyramidMarkerTemplatePropertyName;
                case MarkerType.Square:
                    return XamDataChart.SquareMarkerTemplatePropertyName;
                case MarkerType.Diamond:
                    return XamDataChart.DiamondMarkerTemplatePropertyName;
                case MarkerType.Pentagon:
                    return XamDataChart.PentagonMarkerTemplatePropertyName;
                case MarkerType.Hexagon:
                    return XamDataChart.HexagonMarkerTemplatePropertyName;
                case MarkerType.Tetragram:
                    return XamDataChart.TetragramMarkerTemplatePropertyName;
                case MarkerType.Pentagram:
                    return XamDataChart.PentagramMarkerTemplatePropertyName;
                case MarkerType.Hexagram:
                    return XamDataChart.HexagramMarkerTemplatePropertyName;
                default:
                case MarkerType.Unset:
                case MarkerType.None:
                    return null;
                
            }
        }

        /// <summary>
        /// Returnes the marker type that should be used for a series.
        /// </summary>
        /// <param name="series">The series to resolve for.</param>
        /// <param name="seriesMarkerType">The input marker type.</param>
        /// <returns>The resolved marker type.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static MarkerType ResolveMarkerType(Series series, MarkerType seriesMarkerType)
        {
            MarkerType markerType = series.SeriesViewer != null ? seriesMarkerType : MarkerType.None;

            if (markerType == MarkerType.Automatic)
            {
                MarkerType[] markerTypes = new MarkerType[] {
                        MarkerType.Circle,
                        MarkerType.Triangle,
                        MarkerType.Pentagon,
                        MarkerType.Tetragram,
                        MarkerType.Diamond,
                        MarkerType.Square,
                        MarkerType.Hexagon,
                        MarkerType.Pentagram,
                        MarkerType.Pyramid,
                        MarkerType.Hexagram
                    };

                markerType = series.Index >= 0 ? markerTypes[series.Index % markerTypes.Length] : MarkerType.None;
            }
            return markerType;
        }
        /// <summary>
        /// Updates properties that are based on the index of the series in the series collection.
        /// </summary>
        protected override void UpdateIndexedProperties()
        {
            base.UpdateIndexedProperties();

            if (Index < 0)
            {
                return;
            }

            #region ActualMarkerTemplate

            if (MarkerView.HasCustomMarkerTemplate())
            {
                MarkerView.ClearActualMarkerTemplate();
                MarkerView.BindActualToCustomMarkerTemplate();
            }
            else
            {

                MarkerType markerType = MarkerSeries.ResolveMarkerType(this, this.MarkerType);
                string markerTemplatePropertyName = MarkerSeries.GetMarkerTemplatePropertyName(markerType);
                if (markerTemplatePropertyName == null)
                {
                    this.ActualMarkerTemplate = MarkerSeries.NullMarkerTemplate;
                }
                else
                {
                    this.MarkerView.BindActualToMarkerTemplate(markerTemplatePropertyName);
                }
            }
            #endregion
            #region ActualMarkerBrush
            if (MarkerBrush != null)
            {
                MarkerView.ClearActualMarkerBrush();
                MarkerView.BindActualToMarkerBrush();
            }
            else
            {
                this.ActualMarkerBrush = this.SeriesViewer == null ? null : this.SeriesViewer.GetMarkerBrushByIndex(this.Index);
            }
            #endregion
            #region ActualMarkerOutline
            if (MarkerOutline != null)
            {
                MarkerView.ClearActualMarkerOutline();
                MarkerView.BindActualToMarkerOutline();
            }
            else
            {
                this.ActualMarkerOutline = this.SeriesViewer == null ? null : this.SeriesViewer.GetMarkerOutlineByIndex(this.Index);
            }
            #endregion
        }

        /// <summary>
        /// Exports visual information about the series for use by external tools and functionality.
        /// </summary>
        /// <param name="svd">The data container.</param>
        protected override void ExportVisualDataOverride(SeriesVisualData svd)
        {
            base.ExportVisualDataOverride(svd);

            MarkerView.DoToAllMarkers((m) =>
                {
                    MarkerVisualData mvd = new MarkerVisualData();




                    var tt = m.RenderTransform as TranslateTransform;
                    if (tt != null)
                    {
                        mvd.X = tt.X;
                        mvd.Y = tt.Y;
                    }

                    mvd.Index = -1;
                    mvd.ContentTemplate = m.ContentTemplate;
                    if (m.Content != null && m.Content is DataContext && m.Visibility == Visibility.Visible)
                    {
                        var dataContext = (DataContext)m.Content;
                        if (dataContext.Item != null)
                        {
                            mvd.Index = FastItemsSource.IndexOf(dataContext.Item);
                        }
                    }

                    mvd.Visibility = m.Visibility;

                    svd.MarkerShapes.Add(mvd);
                });
        }



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

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