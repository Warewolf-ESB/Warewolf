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
using System.ComponentModel;
using System.Windows.Media.Effects;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the class for the axis labels settings and behaviors.
    /// </summary>
    public class AxisLabelSettings : DependencyObject, INotifyPropertyChanged
    {
        /// <summary>
        /// Constructs a new AxisLabelSettings instance.
        /// </summary>
        public AxisLabelSettings()
        {
            TextBlock text = new TextBlock();
            this.FontFamily = text.FontFamily;
            this.FontSize = text.FontSize;
            this.Foreground = text.Foreground;
            this.OpacityMask = text.OpacityMask;



            this.TextDecorations = text.TextDecorations;
            this.ActualLocation = AxisLabelsLocation.OutsideBottom;
            PropertyUpdated += (o, e) => PropertyUpdatedOverride(o, e.PropertyName, e.OldValue, e.NewValue);
        }


        #region Effect Dependency Property
        internal const string EffectPropertyName = "Effect";

        /// <summary>
        /// Identifies the Effect dependency property.
        /// </summary>
        public static readonly DependencyProperty EffectProperty = DependencyProperty.Register(EffectPropertyName, typeof(Effect), typeof(AxisLabelSettings),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as AxisLabelSettings).RaisePropertyChanged(EffectPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the Effet property.
        /// </summary>
        public Effect Effect
        {
            get
            {
                return (Effect)this.GetValue(EffectProperty);
            }
            set
            {
                this.SetValue(EffectProperty, value);
            }
        }

        #endregion


        #region Foreground Dependency Property
        internal const string ForegroundPropertyName = "Foreground";

        /// <summary>
        /// Identifies the Foreground dependency property.
        /// </summary>
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(ForegroundPropertyName, typeof(Brush), typeof(AxisLabelSettings),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as AxisLabelSettings).RaisePropertyChanged(ForegroundPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the Foreground property.
        /// </summary>
        public Brush Foreground
        {
            get
            {
                return (Brush)this.GetValue(ForegroundProperty);
            }
            set
            {
                this.SetValue(ForegroundProperty, value);
            }
        }
        #endregion

        #region FontFamily Dependency Property
        internal const string FontFamilyPropertyName = "FontFamily";

        /// <summary>
        /// Identifies the FontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(FontFamilyPropertyName, typeof(FontFamily), typeof(AxisLabelSettings),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as AxisLabelSettings).RaisePropertyChanged(FontFamilyPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the FontFamily property
        /// </summary>
        public FontFamily FontFamily
        {
            get
            {
                return (FontFamily)this.GetValue(FontFamilyProperty);
            }
            set
            {
                this.SetValue(FontFamilyProperty, value);
            }
        }
        #endregion

        #region FontSize Dependency Property
        internal const string FontSizePropertyName = "FontSize";

        /// <summary>
        /// Identifies the FontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(FontSizePropertyName, typeof(double), typeof(AxisLabelSettings),
            new PropertyMetadata(11.0, (sender, e) =>
                {
                    (sender as AxisLabelSettings).RaisePropertyChanged(FontSizePropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets or sets the FontSize property
        /// </summary>
        public double FontSize
        {
            get
            {
                return (double)this.GetValue(FontSizeProperty);
            }
            set
            {
                this.SetValue(FontSizeProperty, value);
            }
        }
        #endregion

        #region FontStretch Dependency Property
        internal const string FontStretchPropertyName = "FontStretch";

        /// <summary>
        /// Identifies the FontStretch dependency property.
        /// </summary>
        public static readonly DependencyProperty FontStretchProperty = DependencyProperty.Register(FontStretchPropertyName, typeof(FontStretch), typeof(AxisLabelSettings),
            new PropertyMetadata(FontStretches.Normal, (sender, e) =>
                {
                    (sender as AxisLabelSettings).RaisePropertyChanged(FontStretchPropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets or sets the FontStretch property.
        /// </summary>
        public FontStretch FontStretch
        {
            get
            {
                return (FontStretch)this.GetValue(FontStretchProperty);
            }
            set
            {
                this.SetValue(FontStretchProperty, value);
            }
        }
        #endregion

        #region FontStyle Dependency Property
        internal const string FontStylePropertyName = "FontStyle";

        /// <summary>
        /// Identifies the FontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty FontStyleProperty = DependencyProperty.Register(FontStylePropertyName, typeof(FontStyle), typeof(AxisLabelSettings),
            new PropertyMetadata(FontStyles.Normal, (sender, e) =>
                {
                    (sender as AxisLabelSettings).RaisePropertyChanged(FontStylePropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets or sets the FontStyle property.
        /// </summary>
        public FontStyle FontStyle
        {
            get
            {
                return (FontStyle)this.GetValue(FontStyleProperty);
            }
            set
            {
                this.SetValue(FontStyleProperty, value);
            }
        }
        #endregion

        #region FontWeight Dependency Property
        internal const string FontWeightPropertyName = "FontWeight";

        /// <summary>
        /// Identifies the FontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register(FontWeightPropertyName, typeof(FontWeight), typeof(AxisLabelSettings),
            new PropertyMetadata(FontWeights.Normal, (sender, e) =>
            {
                (sender as AxisLabelSettings).RaisePropertyChanged(FontWeightPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the FontWeight property.
        /// </summary>
        public FontWeight FontWeight
        {
            get
            {
                return (FontWeight)this.GetValue(FontWeightProperty);
            }
            set
            {
                this.SetValue(FontWeightProperty, value);
            }
        }
        #endregion

        #region HorizontalAlignment Dependency Property
        internal const string HorizontalAlignmentPropertyName = "HorizontalAlignment";

        /// <summary>
        /// Identifies the HorizontalAlignment dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalAlignmentProperty = DependencyProperty.Register(HorizontalAlignmentPropertyName, typeof(HorizontalAlignment), typeof(AxisLabelSettings),
            new PropertyMetadata(HorizontalAlignment.Center, (sender, e) =>
            {
                (sender as AxisLabelSettings).RaisePropertyChanged(HorizontalAlignmentPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the HorizontalAlignment property.
        /// </summary>
        public HorizontalAlignment HorizontalAlignment
        {
            get
            {
                return (HorizontalAlignment)this.GetValue(HorizontalAlignmentProperty);
            }
            set
            {
                this.SetValue(HorizontalAlignmentProperty, value);
            }
        }
        #endregion

        #region VerticalAlignment Dependency Property
        internal const string VerticalAlignmentPropertyName = "VerticalAlignment";

        /// <summary>
        /// Identifies the VerticalAlignment dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalAlignmentProperty = DependencyProperty.Register(VerticalAlignmentPropertyName, typeof(VerticalAlignment), typeof(AxisLabelSettings),
            new PropertyMetadata(VerticalAlignment.Center, (sender, e) =>
            {
                (sender as AxisLabelSettings).RaisePropertyChanged(VerticalAlignmentPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the VerticalAlignment property.
        /// </summary>
        public VerticalAlignment VerticalAlignment
        {
            get
            {
                return (VerticalAlignment)this.GetValue(VerticalAlignmentProperty);
            }
            set
            {
                this.SetValue(VerticalAlignmentProperty, value);
            }
        }
        #endregion

        #region IsHitTestVisible Dependency Property
        internal const string IsHitTestVisiblePropertyName = "IsHitTestVisible";

        /// <summary>
        /// Identifies the IsHitTestVisible dependency property.
        /// </summary>
        public static readonly DependencyProperty IsHitTestVisibleProperty = DependencyProperty.Register(IsHitTestVisiblePropertyName, typeof(bool), typeof(AxisLabelSettings),
            new PropertyMetadata(false, (sender, e) =>
            {
                (sender as AxisLabelSettings).RaisePropertyChanged(IsHitTestVisiblePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the IsHitTestVisible property.
        /// </summary>
        public bool IsHitTestVisible
        {
            get
            {
                return (bool)this.GetValue(IsHitTestVisibleProperty);
            }
            set
            {
                this.SetValue(IsHitTestVisibleProperty, value);
            }
        }
        #endregion

        #region OpacityMask Dependency Property
        internal const string OpacityMaskPropertyName = "OpacityMask";

        /// <summary>
        /// Identifies the OpacityMask dependency property.
        /// </summary>
        public static readonly DependencyProperty OpacityMaskProperty = DependencyProperty.Register(OpacityMaskPropertyName, typeof(Brush), typeof(AxisLabelSettings),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as AxisLabelSettings).RaisePropertyChanged(OpacityMaskPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the OpacityMask property.
        /// </summary>
        public Brush OpacityMask
        {
            get
            {
                return (Brush)this.GetValue(OpacityMaskProperty);
            }
            set
            {
                this.SetValue(OpacityMaskProperty, value);
            }
        }
        #endregion

        #region Opacity Dependency Property
        internal const string OpacityPropertyName = "Opacity";

        /// <summary>
        /// Identifies the Opacity dependency property.
        /// </summary>
        public static readonly DependencyProperty OpacityProperty = DependencyProperty.Register(OpacityPropertyName, typeof(double), typeof(AxisLabelSettings),
            new PropertyMetadata(1.0, (sender, e) =>
            {
                (sender as AxisLabelSettings).RaisePropertyChanged(OpacityPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the Opacity property
        /// </summary>
        public double Opacity
        {
            get
            {
                return (double)this.GetValue(OpacityProperty);
            }
            set
            {
                this.SetValue(OpacityProperty, value);
            }
        }
        #endregion

        #region Padding Dependency Property
        internal const string PaddingPropertyName = "Padding";

        /// <summary>
        /// Identifies the Padding dependency property.
        /// </summary>
        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(PaddingPropertyName, typeof(Thickness), typeof(AxisLabelSettings),
            new PropertyMetadata(new Thickness(), (sender, e) =>
            {
                (sender as AxisLabelSettings).RaisePropertyChanged(PaddingPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the Padding property.
        /// </summary>
        public Thickness Padding
        {
            get
            {
                return (Thickness)this.GetValue(PaddingProperty);
            }
            set
            {
                this.SetValue(PaddingProperty, value);
            }
        }
        #endregion



#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)


        #region TextDecorations Dependency Property
        internal const string TextDecorationsPropertyName = "TextDecorations";

        /// <summary>
        /// Identifies the TextDecorations dependency property.
        /// </summary>
        public static readonly DependencyProperty TextDecorationsProperty = DependencyProperty.Register(TextDecorationsPropertyName, typeof(TextDecorationCollection), typeof(AxisLabelSettings),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as AxisLabelSettings).RaisePropertyChanged(TextDecorationsPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the TextDecorations property.
        /// </summary>
        public TextDecorationCollection TextDecorations
        {
            get
            {
                return (TextDecorationCollection)this.GetValue(TextDecorationsProperty);
            }
            set
            {
                this.SetValue(TextDecorationsProperty, value);
            }
        }
        #endregion

        #region TextWrapping Dependency Property
        internal const string TextWrappingPropertyName = "TextWrapping";

        /// <summary>
        /// Identifies the TextWrapping dependency property.
        /// </summary>
        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(TextWrappingPropertyName, typeof(TextWrapping), typeof(AxisLabelSettings),
            new PropertyMetadata(TextWrapping.NoWrap, (sender, e) =>
            {
                (sender as AxisLabelSettings).RaisePropertyChanged(TextWrappingPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the TextWrapping property.
        /// </summary>
        public TextWrapping TextWrapping
        {
            get
            {
                return (TextWrapping)this.GetValue(TextWrappingProperty);
            }
            set
            {
                this.SetValue(TextWrappingProperty, value);
            }
        }
        #endregion

        #region Visibility Dependency Property
        internal const string VisibilityPropertyName = "Visibility";

        /// <summary>
        /// Identifies the Visibility dependency property.
        /// </summary>
        public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register(VisibilityPropertyName, typeof(Visibility), typeof(AxisLabelSettings),
            new PropertyMetadata(Visibility.Visible, (sender, e) =>
            {
                (sender as AxisLabelSettings).RaisePropertyChanged(VisibilityPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the Visibility property.
        /// </summary>
        public Visibility Visibility
        {
            get
            {
                return (Visibility)this.GetValue(VisibilityProperty);
            }
            set
            {
                this.SetValue(VisibilityProperty, value);
            }
        }
        #endregion

        #region Extent Dependency Property
        internal const string ExtentPropertyName = "Extent";
        internal const double ExtentPropertyDefault = 50.0;
        /// <summary>
        /// Identifies the Extent dependency property.
        /// </summary>
        public static readonly DependencyProperty ExtentProperty = DependencyProperty.Register(ExtentPropertyName, typeof(double), typeof(AxisLabelSettings),
            new PropertyMetadata(ExtentPropertyDefault, (sender, e) =>
            {
                (sender as AxisLabelSettings).RaisePropertyChanged(ExtentPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the Extent property
        /// </summary>
        public double Extent
        {
            get
            {
                return (double)this.GetValue(ExtentProperty);
            }
            set
            {
                if (this.Axis != null && this.Axis.LabelPanel != null)
                {
                    this.Axis.LabelPanel.SetValue(AxisLabelPanelBase.ExtentProperty, value);
                    this.Axis.RenderAxis();
                }
                this.SetValue(ExtentProperty, value);
            }
        }
        #endregion

        #region Angle Dependency Property
        internal const string AnglePropertyName = "Angle";

        /// <summary>
        /// Identifies the Angle dependency property.
        /// </summary>
        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register(AnglePropertyName, typeof(double), typeof(AxisLabelSettings),
            new PropertyMetadata(0.0, (sender, e) =>
            {
                (sender as AxisLabelSettings).RaisePropertyChanged(AnglePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the Angle property
        /// </summary>
        public double Angle
        {
            get
            {
                return (double)this.GetValue(AngleProperty);
            }
            set
            {
                this.SetValue(AngleProperty, value);
            }
        }
        #endregion

        #region Location Dependency Property
        internal const string LocationPropertyName = "Location";

        /// <summary>
        /// Identifies the Location dependency property.
        /// </summary>
        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(LocationPropertyName, typeof(AxisLabelsLocation), typeof(AxisLabelSettings),
            new PropertyMetadata(AxisLabelsLocation.OutsideBottom, (sender, e) =>
            {
                (sender as AxisLabelSettings).RaisePropertyChanged(LocationPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the Location property.
        /// </summary>
        public AxisLabelsLocation Location
        {
            get
            {
                return (AxisLabelsLocation)this.GetValue(LocationProperty);
            }
            set
            {
                this.SetValue(LocationProperty, value);
            }
        }
        #endregion

        #region TextAlignment Dependency Property
        internal const string TextAlignmentPropertyName = "TextAlignment";

        /// <summary>
        /// Identifies the TextAlignment dependency property.
        /// </summary>
        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(TextAlignmentPropertyName, typeof(TextAlignment), typeof(AxisLabelSettings),
            new PropertyMetadata(TextAlignment.Center, (sender, e) =>
            {
                (sender as AxisLabelSettings).RaisePropertyChanged(TextAlignmentPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the TextAlignment property.
        /// </summary>
        public TextAlignment TextAlignment
        {
            get
            {
                return (TextAlignment)this.GetValue(TextAlignmentProperty);
            }
            set
            {
                this.SetValue(TextAlignmentProperty, value);
            }
        }
        #endregion

        //private Axis _Axis;
        /// <summary>
        /// a reference back to the axis containing these label settings.
        /// </summary>
        public Axis Axis { get; internal set; }
        //{
        //    get
        //    {
        //        return this._Axis;
        //    }
        //    set
        //    {
        //        bool changed = value != null && value != this._Axis;
        //        this._Axis = value;
        //        if (changed && this.Axis.Chart != null && this.Axis.Chart.CentralArea != null)
        //        {
        //            this.Axis.Chart.CentralArea.InvalidateArrange();
        //            this.Axis.RenderAxis();
        //        }
        //    }
        //}
        
        /// <summary>
        /// Location where the labels are displayed.
        /// </summary>
        internal AxisLabelsLocation ActualLocation { get; set; }

        /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated
        /// on the AxisLabels. Gives the axis labels a chance to respond to the various property updates.
        /// </summary>
        /// <param name="sender">The object being updated.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected virtual void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {

            switch (propertyName)
            {
                case LocationPropertyName:
                    this.ActualLocation = this.Location;             
                    break;
            }

            if (this.Axis == null || this.Axis.SeriesViewer == null)
            {
                //we either have a set of axis labels without an axis or an axis without a parent chart.
                //no point in handling property changes.
                return;
            }

            switch (propertyName)
            {
                case AnglePropertyName:
                    this.Axis.MustInvalidateLabels = true;
                    this.Axis.RenderAxis();
                    break;

                case VerticalAlignmentPropertyName:
                case HorizontalAlignmentPropertyName:
                    this.Axis.MustInvalidateLabels = true;
                    this.Axis.RenderAxis();
                    break;

                case LocationPropertyName:
                    this.Axis.MustInvalidateLabels = true;
                    this.Axis.SeriesViewer.CentralArea.InvalidateArrange();
                    this.Axis.RenderAxis();
                    break;
                case VisibilityPropertyName:
                    this.Axis.MustInvalidateLabels = true;
                    this.Axis.SeriesViewer.CentralArea.InvalidateArrange();
                    this.Axis.RenderAxis();
                    break;
                case ExtentPropertyName:
                    this.Axis.MustInvalidateLabels = true;
                    this.Axis.SeriesViewer.CentralArea.InvalidateArrange();
                    this.Axis.RenderAxis();
                    break;

                //these properties need to trigger axis refresh to re-render the labels,
                //because this can affect anti-collision logic.
                case FontFamilyPropertyName:
                case FontSizePropertyName:
                case FontStretchPropertyName:
                case FontStylePropertyName:
                case FontWeightPropertyName:
                    this.Axis.RenderAxis();
                    break;
            }
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Notifies clients that a property value has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when a property value has been updated.
        /// </summary>
        public event PropertyUpdatedEventHandler PropertyUpdated;

        private void RaisePropertyChanged(string name, object oldValue, object newValue)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }

            if (PropertyUpdated != null)
            {
                PropertyUpdated(this, new PropertyUpdatedEventArgs(name, oldValue, newValue));
            }
        }

        #endregion

        internal bool HasUserAngle()
        {
            return ReadLocalValue(AxisLabelSettings.AngleProperty) != DependencyProperty.UnsetValue;
        }

        internal bool HasUserExtent()
        {
            return ReadLocalValue(AxisLabelSettings.ExtentProperty) != DependencyProperty.UnsetValue;
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