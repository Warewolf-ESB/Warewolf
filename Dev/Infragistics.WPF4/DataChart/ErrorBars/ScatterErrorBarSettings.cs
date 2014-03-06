using System;
using System.ComponentModel;
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

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the class for the scatter error bar settings
    /// </summary>

    [DesignTimeVisible(false)]


    public class ScatterErrorBarSettings : ErrorBarSettingsBase
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public ScatterErrorBarSettings()
        {
            this.PropertyUpdated += new PropertyUpdatedEventHandler(ScatterErrorBarSettings_PropertyUpdated);
        }

        #endregion Constructor

        #region Properties

        #region Public

        #region Horizontal

        #region EnableErrorBarsHorizontal

        /// <summary>
        /// Determines which horizontal error bars are enabled - none, the plus ones, the minus ones or both.
        /// </summary>
        public EnableErrorBars EnableErrorBarsHorizontal
        {
            get { return (EnableErrorBars)GetValue(EnableErrorBarsHorizontalProperty); }
            set { SetValue(EnableErrorBarsHorizontalProperty, value); }
        }

        private const string EnableErrorBarsHorizontalPropertyName = "EnableErrorBarsHorizontal";

        /// <summary>
        /// Identifies the ErrorErrorBarsHorizontal dependency property.
        /// </summary>
        public static readonly DependencyProperty EnableErrorBarsHorizontalProperty =
            DependencyProperty.Register(EnableErrorBarsHorizontalPropertyName, typeof(EnableErrorBars), typeof(ScatterErrorBarSettings),
            new PropertyMetadata(EnableErrorBars.None, (sender, e) =>
                    {
                        (sender as ScatterErrorBarSettings).RaisePropertyChanged(EnableErrorBarsHorizontalPropertyName, e.OldValue, e.NewValue);
                    }));

        #endregion EnableErrorBarsHorizontal

        #region HorizontalCalculatorReference

        /// <summary>
        /// Specifies the reference value for any dependent horizontal calculators.
        /// </summary>
        public ErrorBarCalculatorReference HorizontalCalculatorReference
        {
            get { return (ErrorBarCalculatorReference)GetValue(HorizontalCalculatorReferenceProperty); }
            set { SetValue(HorizontalCalculatorReferenceProperty, value); }
        }

        private const string HorizontalCalculatorReferencePropertyName = "HorizontalCalculatorReference";

        /// <summary>
        /// Identifies the HorizontalCalculatorReference dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalCalculatorReferenceProperty =
            DependencyProperty.Register(HorizontalCalculatorReferencePropertyName, typeof(ErrorBarCalculatorReference), typeof(ScatterErrorBarSettings),
            new PropertyMetadata(ErrorBarCalculatorReference.X, (sender, e) =>
            {
                (sender as ScatterErrorBarSettings).RaisePropertyChanged(HorizontalCalculatorReferencePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion HorizontalCalculatorReference

        #region HorizontalCalculator

        /// <summary>
        /// Specifies the calculator for the horizontal error bars.
        /// </summary>
        public IErrorBarCalculator HorizontalCalculator
        {
            get { return (IErrorBarCalculator)GetValue(HorizontalCalculatorProperty); }
            set { SetValue(HorizontalCalculatorProperty, value); }
        }

        private const string HorizontalCalculatorPropertyName = "HorizontalCalculator";

        /// <summary>
        /// Identifies the HorizontalCalculator dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalCalculatorProperty =
            DependencyProperty.Register(HorizontalCalculatorPropertyName, typeof(IErrorBarCalculator), typeof(ScatterErrorBarSettings),
            new PropertyMetadata(null, (sender, e) =>
                    {
                        (sender as ScatterErrorBarSettings).RaisePropertyChanged(HorizontalCalculatorPropertyName, e.OldValue, e.NewValue);
                    }));

        #endregion HorizontalCalculator

        #region HorizontalErrorBarCapLength

        /// <summary>
        /// Determines the length, in pixels, of each horizontal error bar's cap.
        /// </summary>
        public int HorizontalErrorBarCapLength
        {
            get { return (int)GetValue(HorizontalErrorBarCapLengthProperty); }
            set { SetValue(HorizontalErrorBarCapLengthProperty, value); }
        }

        /// <summary>
        /// String used to declare HorizontalErrorBarCepLength dependency property.
        /// </summary>
        protected const string HorizontalErrorBarCapLengthPropertyName = "HorizontalErrorBarCapLength";

        /// <summary>
        /// Identifies the HorizontalErrorBarCapLength dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalErrorBarCapLengthProperty =
            DependencyProperty.Register(HorizontalErrorBarCapLengthPropertyName, typeof(int), typeof(ScatterErrorBarSettings),
            new PropertyMetadata(6, (sender, e) =>
            {
                (sender as ScatterErrorBarSettings).RaisePropertyChanged(HorizontalErrorBarCapLengthPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion // HorizontalErrorBarCapLength

        #region HorizontalStroke

        /// <summary>
        /// Determines the stroke of the horizontal error bars.
        /// </summary>
        public Brush HorizontalStroke
        {
            get { return (Brush)GetValue(HorizontalStrokeProperty); }
            set { SetValue(HorizontalStrokeProperty, value); }
        }

        private const string HorizontalStrokePropertyName = "HorizontalStroke";

        /// <summary>
        /// Identifies the HorizontalStroke dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalStrokeProperty =
            DependencyProperty.Register(HorizontalStrokePropertyName, typeof(Brush), typeof(ScatterErrorBarSettings),
            new PropertyMetadata(

                Path.StrokeProperty.GetMetadata(typeof(Path)).DefaultValue as Brush,




                (sender, e) =>
                    {
                        (sender as ScatterErrorBarSettings).RaisePropertyChanged(HorizontalStrokePropertyName, e.OldValue, e.NewValue);
                    }));

        #endregion HorizontalStroke

        #region HorizontalStrokeThickness

        /// <summary>
        /// Determines the stroke thickness of the horizontal error bars.
        /// </summary>
        public double HorizontalStrokeThickness
        {
            get { return (double)GetValue(HorizontalStrokeThicknessProperty); }
            set { SetValue(HorizontalStrokeThicknessProperty, value); }
        }

        /// <summary>
        /// Identifies the HorizontalStrokeThickness dependency property.
        /// </summary>
        private const string HorizontalStrokeThicknessPropertyName = "HorizontalStrokeThickness";

        /// <summary>
        /// Identifies the HorizontalStrokeThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalStrokeThicknessProperty =
            DependencyProperty.Register(HorizontalStrokeThicknessPropertyName, typeof(double), typeof(ScatterErrorBarSettings),
            new PropertyMetadata(

                (double)Path.StrokeThicknessProperty.GetMetadata(typeof(Path)).DefaultValue,



                (sender, e) =>
                    {
                        (sender as ScatterErrorBarSettings).RaisePropertyChanged(HorizontalStrokeThicknessPropertyName, e.OldValue, e.NewValue);
                    }));

        #endregion HorizontalStrokeThickness

        #region HorizontalErrorBarStyle

        internal const string HorizontalErrorBarStylePropertyName = "HorizontalErrorBarStyle";

        /// <summary>
        /// Identifies the HorizontalErrorBarStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalErrorBarStyleProperty = DependencyProperty.Register(HorizontalErrorBarStylePropertyName, typeof(Style), typeof(ScatterErrorBarSettings),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as ScatterErrorBarSettings).RaisePropertyChanged(HorizontalErrorBarStylePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the HorizontalErrorBarStyle property.
        /// </summary>
        public Style HorizontalErrorBarStyle
        {
            get
            {
                return (Style)GetValue(HorizontalErrorBarStyleProperty);
            }
            set
            {
                SetValue(HorizontalErrorBarStyleProperty, value);
            }
        }

        #endregion ErrorBarStyle Dependency Property

        #endregion Horizontal

        #region Vertical

        #region EnableErrorBarsVertical

        /// <summary>
        /// Determines which vertical error bars are enabled - none, the plus ones, the minus ones or both.
        /// </summary>
        public EnableErrorBars EnableErrorBarsVertical
        {
            get { return (EnableErrorBars)GetValue(EnableErrorBarsVerticalProperty); }
            set { SetValue(EnableErrorBarsVerticalProperty, value); }
        }

        private const string EnableErrorBarsVerticalPropertyName = "EnableErrorBarsVertical";

        /// <summary>
        /// Identifies the EnableErrorBarsVertical dependency property.
        /// </summary>
        public static readonly DependencyProperty EnableErrorBarsVerticalProperty =
            DependencyProperty.Register(EnableErrorBarsVerticalPropertyName, typeof(EnableErrorBars), typeof(ScatterErrorBarSettings),
            new PropertyMetadata(EnableErrorBars.None, (sender, e) =>
                    {
                        (sender as ScatterErrorBarSettings).RaisePropertyChanged(EnableErrorBarsVerticalPropertyName, e.OldValue, e.NewValue);
                    }));

        #endregion EnableErrorBarsVertical

        #region VerticalCalculatorReference

        /// <summary>
        /// Specifies the reference value for any dependent vertical calculators.
        /// </summary>
        public ErrorBarCalculatorReference VerticalCalculatorReference
        {
            get { return (ErrorBarCalculatorReference)GetValue(VerticalCalculatorReferenceProperty); }
            set { SetValue(VerticalCalculatorReferenceProperty, value); }
        }

        private const string VerticalCalculatorReferencePropertyName = "VerticalCalculatorReference";

        /// <summary>
        /// Identifies the VerticalCalculatorReference dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalCalculatorReferenceProperty =
            DependencyProperty.Register(VerticalCalculatorReferencePropertyName, typeof(ErrorBarCalculatorReference), typeof(ScatterErrorBarSettings),
            new PropertyMetadata(ErrorBarCalculatorReference.Y, (sender, e) =>
            {
                (sender as ScatterErrorBarSettings).RaisePropertyChanged(VerticalCalculatorReferencePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion HorizontalCalculatorReference

        #region VerticalCalculator

        /// <summary>
        /// Specifies the calculator for the vertical error bars.
        /// </summary>
        public IErrorBarCalculator VerticalCalculator
        {
            get { return (IErrorBarCalculator)GetValue(VerticalCalculatorProperty); }
            set { SetValue(VerticalCalculatorProperty, value); }
        }

        private const string VerticalCalculatorPropertyName = "VerticalCalculator";

        /// <summary>
        /// Identifies the VerticalCalculator dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalCalculatorProperty =
            DependencyProperty.Register(VerticalCalculatorPropertyName, typeof(IErrorBarCalculator), typeof(ScatterErrorBarSettings),
            new PropertyMetadata(null, (sender, e) =>
                    {
                        (sender as ScatterErrorBarSettings).RaisePropertyChanged(VerticalCalculatorPropertyName, e.OldValue, e.NewValue);
                    }));

        #endregion VerticalCalculator

        #region VerticalErrorBarCapLength

        /// <summary>
        /// Determines the length, in pixels, of each vertical error bar's cap.
        /// </summary>
        public int VerticalErrorBarCapLength
        {
            get { return (int)GetValue(VerticalErrorBarCapLengthProperty); }
            set { SetValue(VerticalErrorBarCapLengthProperty, value); }
        }

        /// <summary>
        /// String used to identify the VerticalErrorBarCapLength property.
        /// </summary>
        protected const string VerticalErrorBarCapLengthPropertyName = "VerticalErrorBarCapLength";

        /// <summary>
        /// Identifies the VerticalErrorBarCapLength dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalErrorBarCapLengthProperty =
            DependencyProperty.Register(VerticalErrorBarCapLengthPropertyName, typeof(int), typeof(ScatterErrorBarSettings),
            new PropertyMetadata(6, (sender, e) =>
            {
                (sender as ScatterErrorBarSettings).RaisePropertyChanged(VerticalErrorBarCapLengthPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion // VerticalErrorBarCapLength

        #region VerticalStroke

        /// <summary>
        /// Determines the stroke of the vertical error bars.
        /// </summary>
        public Brush VerticalStroke
        {
            get { return (Brush)GetValue(VerticalStrokeProperty); }
            set { SetValue(VerticalStrokeProperty, value); }
        }

        private const string VerticalStrokePropertyName = "VerticalStroke";

        /// <summary>
        /// Identifies the VerticalStroke dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalStrokeProperty =
            DependencyProperty.Register(VerticalStrokePropertyName, typeof(Brush), typeof(ScatterErrorBarSettings),
            new PropertyMetadata(

                Path.StrokeProperty.GetMetadata(typeof(Path)).DefaultValue as Brush,




                (sender, e) =>
                    {
                        (sender as ScatterErrorBarSettings).RaisePropertyChanged(VerticalStrokePropertyName, e.OldValue, e.NewValue);
                    }));

        #endregion VerticalStroke

        #region VerticalStrokeThickness

        /// <summary>
        /// Determines the stroke thickness of the vertical error bars.
        /// </summary>
        public double VerticalStrokeThickness
        {
            get { return (double)GetValue(VerticalStrokeThicknessProperty); }
            set { SetValue(VerticalStrokeThicknessProperty, value); }
        }

        private const string VerticalStrokeThicknessPropertyName = "VerticalStrokeThickness";

        /// <summary>
        /// Identifies the VerticalStrokeThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalStrokeThicknessProperty =
            DependencyProperty.Register(VerticalStrokeThicknessPropertyName, typeof(double), typeof(ScatterErrorBarSettings),
            new PropertyMetadata(

                (double)Path.StrokeThicknessProperty.GetMetadata(typeof(Path)).DefaultValue,




                (sender, e) =>
                    {
                        (sender as ScatterErrorBarSettings).RaisePropertyChanged(VerticalStrokeThicknessPropertyName, e.OldValue, e.NewValue);
                    }));

        #endregion VerticalStrokeThickness

        #region VerticalErrorBarStyle

        internal const string VerticalErrorBarStylePropertyName = "VerticalErrorBarStyle";

        /// <summary>
        /// Identifies the VerticalErrorBarStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalErrorBarStyleProperty = DependencyProperty.Register(VerticalErrorBarStylePropertyName, typeof(Style), typeof(ScatterErrorBarSettings),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as ScatterErrorBarSettings).RaisePropertyChanged(VerticalErrorBarStylePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the VerticalErrorBarStyle property.
        /// </summary>
        public Style VerticalErrorBarStyle
        {
            get
            {
                return (Style)GetValue(VerticalErrorBarStyleProperty);
            }
            set
            {
                SetValue(VerticalErrorBarStyleProperty, value);
            }
        }

        #endregion ErrorBarStyle Dependency Property

        #endregion Vertical

        #endregion Public

        #region Internal

        #region Series

        Series _series;

        internal Series Series
        {
            get { return _series; }
            set { _series = value; }
        }
        #endregion Series

        #endregion Internal

        #endregion Properties

        #region INotifyPropertyChanged implementation

        private void ScatterErrorBarSettings_PropertyUpdated(object sender, PropertyUpdatedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case HorizontalCalculatorPropertyName:
                case VerticalCalculatorPropertyName:
                    IErrorBarCalculator oldCalc = e.OldValue as IErrorBarCalculator;
                    if (oldCalc != null)
                    {
                        oldCalc.Changed -= Calculator_Changed;
                    }
                    if (this.Series != null)
                    {
                        this.Series.RenderSeries(false);
                        if (this.Series.SeriesViewer != null)
                        {
                            this.Series.NotifyThumbnailAppearanceChanged();
                        }
                    }
                    IErrorBarCalculator newCalc = e.NewValue as IErrorBarCalculator;
                    if (newCalc != null)
                    {
                        newCalc.Changed += Calculator_Changed;
                    }
                    break;
                case EnableErrorBarsHorizontalPropertyName:
                case EnableErrorBarsVerticalPropertyName:
                case HorizontalCalculatorReferencePropertyName:
                case HorizontalErrorBarCapLengthPropertyName:
                case HorizontalErrorBarStylePropertyName:
                case HorizontalStrokePropertyName:
                case HorizontalStrokeThicknessPropertyName:
                case VerticalCalculatorReferencePropertyName:
                case VerticalErrorBarCapLengthPropertyName:
                case VerticalErrorBarStylePropertyName:
                case VerticalStrokePropertyName:
                case VerticalStrokeThicknessPropertyName:
                    if (this.Series != null)
                    {
                        this.Series.RenderSeries(false);
                        if (this.Series.SeriesViewer != null)
                        {
                            this.Series.NotifyThumbnailAppearanceChanged();
                        }
                    }
                    break;
            }
        }

        private void Calculator_Changed(object sender, EventArgs e)
        {
            IErrorBarCalculator calculator = sender as IErrorBarCalculator;
            if (calculator != null)
            {
                calculator.Changed -= Calculator_Changed;
                if (this.Series != null)
                {
                    this.Series.RenderSeries(false);
                }
                calculator.Changed += Calculator_Changed;
            }
        }

        #endregion INotifyPropertyChanged implementation
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