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
    /// Represents the base class for error bar settings
    /// </summary>
    public abstract class ErrorBarSettingsBase : DependencyObject, INotifyPropertyChanged
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ErrorBarSettingsBase()
        {
            DefaultErrorBarStyle = new Style();

            DefaultErrorBarStyle.TargetType = typeof(Path);
            DefaultErrorBarStyle.Setters.Add(new Setter(Path.StrokeProperty, new SolidColorBrush(Colors.Black)));
            DefaultErrorBarStyle.Setters.Add(new Setter(Path.StrokeThicknessProperty, 1.0));

        }

        #region Internal

        #region DefaultErrorBarStyle

        internal const string DefaultErrorBarStylePropertyName = "DefaultErrorBarStyle";

        /// <summary>
        /// identifies the DefaultErrorBarStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty DefaultErrorBarStyleProperty = DependencyProperty.Register(DefaultErrorBarStylePropertyName, typeof(Style), typeof(ErrorBarSettingsBase),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as ErrorBarSettingsBase).RaisePropertyChanged(DefaultErrorBarStylePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the DefaultErrorBarStyle property.
        /// </summary>
        protected internal Style DefaultErrorBarStyle
        {
            get
            {
                return (Style)GetValue(DefaultErrorBarStyleProperty);
            }
            set
            {
                SetValue(DefaultErrorBarStyleProperty, value);
            }
        }

        #endregion DefaultErrorBarStyle

        #endregion Internal

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
        /// Raises the property changed and updated events.
        /// </summary>
        /// <param name="name">The name of the property being changed.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected void RaisePropertyChanged(string name, object oldValue, object newValue)
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

        #endregion INotifyPropertyChanged implementation
    }

    /// <summary>
    /// Represents the class for the category error bar settings
    /// </summary>

    [DesignTimeVisible(false)]

    public class CategoryErrorBarSettings : ErrorBarSettingsBase
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public CategoryErrorBarSettings()
        {
            this.PropertyUpdated += new PropertyUpdatedEventHandler(CategoryErrorBarSettings_PropertyUpdated);
        }

        #endregion Constructor

        #region Properties

        #region Public

        #region EnableErrorBars

        /// <summary>
        /// Determines which error bars are enabled - none, the plus ones, the minus ones or both.
        /// </summary>
        public EnableErrorBars EnableErrorBars
        {
            get { return (EnableErrorBars)GetValue(EnableErrorBarsProperty); }
            set { SetValue(EnableErrorBarsProperty, value); }
        }

        private const string EnableErrorBarsPropertyName = "EnableErrorBars";

        /// <summary>
        /// Identifies the EnableErrorBars dependency property.
        /// </summary>
        public static readonly DependencyProperty EnableErrorBarsProperty =
            DependencyProperty.Register(EnableErrorBarsPropertyName, typeof(EnableErrorBars), typeof(CategoryErrorBarSettings),
            new PropertyMetadata(EnableErrorBars.None, (sender, e) =>
                    {
                        (sender as CategoryErrorBarSettings).RaisePropertyChanged(EnableErrorBarsPropertyName, e.OldValue, e.NewValue);
                    }));

        #endregion EnableErrorBars

        #region ErrorBarCapLength

        /// <summary>
        /// Determines the length, in pixels, of each error bar's cap.
        /// </summary>
        public int ErrorBarCapLength
        {
            get { return (int)GetValue(ErrorBarCapLengthProperty); }
            set { SetValue(ErrorBarCapLengthProperty, value); }
        }

        private const string ErrorBarCapLengthPropertyName = "ErrorBarCapLength";

        /// <summary>
        /// Identifies the ErrorBarCapLength dependency property.
        /// </summary>
        public static readonly DependencyProperty ErrorBarCapLengthProperty =
            DependencyProperty.Register(ErrorBarCapLengthPropertyName, typeof(int), typeof(CategoryErrorBarSettings),
            new PropertyMetadata(6, (sender, e) =>
            {
                (sender as CategoryErrorBarSettings).RaisePropertyChanged(ErrorBarCapLengthPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion // ErrorBarCapLength

        #region Calculator

        /// <summary>
        /// Specifies the calculator for the error bars
        /// </summary>
        public IErrorBarCalculator Calculator
        {
            get { return (IErrorBarCalculator)GetValue(CalculatorProperty); }
            set { SetValue(CalculatorProperty, value); }
        }

        private const string CalculatorPropertyName = "Calculator";

        /// <summary>
        /// Identifies the Calculator dependency property.
        /// </summary>
        public static readonly DependencyProperty CalculatorProperty =
            DependencyProperty.Register(CalculatorPropertyName, typeof(IErrorBarCalculator), typeof(CategoryErrorBarSettings),
            new PropertyMetadata(null, (sender, e) =>
                    {
                        (sender as CategoryErrorBarSettings).RaisePropertyChanged(CalculatorPropertyName, e.OldValue, e.NewValue);
                    }));

        #endregion Calculator

        #region Stroke

        /// <summary>
        /// Determines the stroke of the error bars
        /// </summary>
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        private const string StrokePropertyName = "Stroke";

        /// <summary>
        /// Identifies the Stroke dependency property.
        /// </summary>
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register(StrokePropertyName, typeof(Brush), typeof(CategoryErrorBarSettings),
            new PropertyMetadata(

                Path.StrokeProperty.GetMetadata(typeof(Path)).DefaultValue as Brush,




                (sender, e) =>
                    {
                        (sender as CategoryErrorBarSettings).RaisePropertyChanged(StrokePropertyName, e.OldValue, e.NewValue);
                    }));

        #endregion Stroke

        #region StrokeThickness

        /// <summary>
        /// Determines the stroke thickness of the error bars
        /// </summary>
        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        private const string StrokeThicknessPropertyName = "StrokeThickness";

        /// <summary>
        /// Identifies the StrokeThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(StrokeThicknessPropertyName, typeof(double), typeof(CategoryErrorBarSettings),
            new PropertyMetadata(

                (double)Path.StrokeThicknessProperty.GetMetadata(typeof(Path)).DefaultValue,



                (sender, e) =>
                    {
                        (sender as CategoryErrorBarSettings).RaisePropertyChanged(StrokeThicknessPropertyName, e.OldValue, e.NewValue);
                    }));

        #endregion StrokeThickness

        #region ErrorBarStyle

        internal const string ErrorBarStylePropertyName = "ErrorBarStyle";

        /// <summary>
        /// Identifies the ErrorBarStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty ErrorBarStyleProperty = DependencyProperty.Register(ErrorBarStylePropertyName, typeof(Style), typeof(CategoryErrorBarSettings),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as CategoryErrorBarSettings).RaisePropertyChanged(ErrorBarStylePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the ErrorBarStyle property.
        /// </summary>
        public Style ErrorBarStyle
        {
            get
            {
                return (Style)GetValue(ErrorBarStyleProperty);
            }
            set
            {
                SetValue(ErrorBarStyleProperty, value);
            }
        }

        #endregion ErrorBarStyle Dependency Property

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

        private void CategoryErrorBarSettings_PropertyUpdated(object sender, PropertyUpdatedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case CalculatorPropertyName:
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
                case EnableErrorBarsPropertyName:
                case ErrorBarCapLengthPropertyName:
                case ErrorBarStylePropertyName:
                case StrokePropertyName:
                case StrokeThicknessPropertyName:
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