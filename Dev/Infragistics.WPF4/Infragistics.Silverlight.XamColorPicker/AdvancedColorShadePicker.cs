using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Infragistics.Controls.Editors.Primitives;
using System.Diagnostics;

using System.Windows.Media.Imaging;


namespace Infragistics.Controls.Editors.Primitives
{
    /// <summary>
    /// A control containing a hue selection area along with sliders for color selection.
    /// </summary>
    [TemplatePart(Name = "HueSelector", Type = typeof(Canvas))]
    [TemplatePart(Name = "LightnessSelector", Type = typeof(Canvas))]
    [TemplatePart(Name = "ShadingCanvas", Type = typeof(Canvas))]
    [TemplatePart(Name = "HueRectangle", Type = typeof(Rectangle))]
    [TemplatePart(Name = "ColorSamplingRectangle", Type = typeof(Rectangle))]
    public class AdvancedColorShadePicker : Control, INotifyPropertyChanged, ICommandTarget, ISupportColorPickerAdvancedEditing
    {
        #region Static
        #region GetString
        internal static string GetString(string input)
        {
#pragma warning disable 436

            return SR.GetString(input);



#pragma warning restore 436
        }
        #endregion // GetString
        #endregion Static

        #region Members

        bool _lightnessMouseDown = false;
        bool _hueMouseDown = false;
        bool _suspendNotifyPropertyChange = false;
        bool _suspendHSLNotify = false;

        ColorSliderView _colorSliderView = ColorSliderView.RGB;

        Rectangle _hueRectangle;
        Rectangle _lightnessRectange;

        Canvas _shadingCanvas;
        Canvas _hueSelector;
        Canvas _lightnessSelector;    

        #endregion // Members

        #region Constructor

        /// <summary>
        /// Static constructor for the <see cref="AdvancedColorShadePicker"/> class.
        /// </summary>
        static AdvancedColorShadePicker()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(AdvancedColorShadePicker), new FrameworkPropertyMetadata(typeof(AdvancedColorShadePicker)));
            UIElement.FocusableProperty.OverrideMetadata(typeof(AdvancedColorShadePicker), new FrameworkPropertyMetadata(false));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvancedColorShadePicker"/> class.
        /// </summary>
        public AdvancedColorShadePicker()
        {




            this.Unloaded += new RoutedEventHandler(AdvancedColorShadePicker_Unloaded);
            this.Loaded += new RoutedEventHandler(AdvancedColorShadePicker_Loaded);

            this.AlphaCaption = AdvancedColorShadePicker.GetString("AlphaCaption");
            this.RedCaption = AdvancedColorShadePicker.GetString("RedCaption");
            this.BlueCaption = AdvancedColorShadePicker.GetString("BlueCaption");
            this.GreenCaption = AdvancedColorShadePicker.GetString("GreenCaption");
            this.HueCaption = AdvancedColorShadePicker.GetString("HueCaption");
            this.SaturationCaption = AdvancedColorShadePicker.GetString("SaturationCaption");
            this.LightnessCaption = AdvancedColorShadePicker.GetString("LightnessCaption");
            this.CyanCaption = AdvancedColorShadePicker.GetString("CyanCaption");
            this.YellowCaption = AdvancedColorShadePicker.GetString("YellowCaption");
            this.MagentaCaption = AdvancedColorShadePicker.GetString("MagentaCaption");
            this.BlackCaption = AdvancedColorShadePicker.GetString("BlackCaption");
            this.OKCaption = AdvancedColorShadePicker.GetString("OKCaption");
            this.CancelCaption = AdvancedColorShadePicker.GetString("CancelCaption");
        }

        #endregion // Constructor

        #region Methods

        #region Private

        #region SetRGBValuesToCurrentColor

        private void SetRGBValuesToCurrentColor()
        {
            this.Red = this.CurrentColor.R;
            this.Blue = this.CurrentColor.B;
            this.Green = this.CurrentColor.G;
            this.Alpha = this.CurrentColor.A;
        }
        #endregion // SetRGBValuesToCurrentColor

        #region SetHSLValuesToCurrentColor
        private void SetHSLValuesToCurrentColor()
        {
            if (!this._suspendHSLNotify)
            {
                HSL hslColor = HSL.FromColor(this.CurrentColor);
                this.H = hslColor.H;
                this.L = hslColor.L;
                this.S = hslColor.S;
            }
        }
        #endregion // SetHSLValuesToCurrentColor

        #region SetCMYKValuesToCurrentColor

        private void SetCMYKValuesToCurrentColor()
        {
            CMYK color = CMYK.FromColor(this.CurrentColor);
            this.C = color.C;
            this.M = color.M;
            this.Y = color.Y;
            this.K = color.K;
        }
        #endregion // SetCMYKValuesToCurrentColor 

        #region SetLightnessMarker

        private void SetLightnessMarker(HSB color)
        {
            if (this._shadingCanvas != null)
            {
                double s = color.S / 100.0;  // x value
                double b = color.B / 100.0; // y value

                if (!double.IsNaN(this._lightnessRectange.Height))
                {
                    double x = this._lightnessRectange.ActualWidth * s;
                    double y = this._lightnessRectange.ActualHeight - this._lightnessRectange.ActualHeight * b;

                    _lightnessSelector.SetValue(Canvas.LeftProperty, x - (_lightnessSelector.Height / 2));
                    _lightnessSelector.SetValue(Canvas.TopProperty, y - (_lightnessSelector.Height / 2));
                }
            }
        }

        #endregion // SetLightnessMarker

        #region SetLightnessBox
        private void SetLightnessBox(double huePosition)
        {
            double height = _hueRectangle.ActualHeight;
            double huePos = huePosition / height;

            LinearGradientBrush fill = this._hueRectangle.Fill as LinearGradientBrush;
            if (fill != null)
            {
                int gradientStops = fill.GradientStops.Count - 1;

                int bytesDown = (int)(gradientStops * huePos * 255);

                byte variation = (byte)(bytesDown % 255);
                byte scale = (byte)(255 - variation);
                byte alpha = 255;

                Color c = Colors.Transparent;

                if (huePos >= 1)
                {
                    bytesDown = 0;
                }

                switch (bytesDown / 255)
                {
                    case 0: { c = Color.FromArgb(alpha, 255, variation, 0); break; }
                    case 1: { c = Color.FromArgb(alpha, scale, 255, 0); break; }
                    case 2: { c = Color.FromArgb(alpha, 0, 255, variation); break; }
                    case 3: { c = Color.FromArgb(alpha, 0, scale, 255); break; }
                    case 4: { c = Color.FromArgb(alpha, variation, 0, 255); break; }
                    case 5: { c = Color.FromArgb(alpha, 255, 0, scale); break; }
                    default: { c = Colors.Black; break; }
                }

                this._lightnessRectange.Fill = new SolidColorBrush(c);
            }
        }
        #endregion // SetLightnessBox

        #region SetLightnessBox
        private void SetLightnessBox(HSB color)
        {
            if (_hueRectangle != null)
            {
                double displacement = (color.H / 360.0) * _hueRectangle.Height;

                this.SetLightnessBox(displacement);
            }
        }
        #endregion // SetLightnessBox

        #region SetHueMarker

        private void SetHueMarker(double hue)
        {
            if (_hueRectangle != null)
            {
                double rectHeight = double.IsNaN(this._hueRectangle.Height) ? 0 : this._hueRectangle.Height;
                double selectorHeight = double.IsNaN(this._hueSelector.Height) ? 0 : this._hueSelector.Height;

                double displacement = ((hue / 360.0) * rectHeight);

                _hueSelector.SetValue(Canvas.TopProperty, displacement - (selectorHeight / 2));
            }
        }

        #endregion // SetHueMarker

        #region UpdateDisplayBoxes
        internal void UpdateDisplayBoxes()
        {
            if (this._lightnessSelector != null)
            {
                this._lightnessSelector.Visibility = System.Windows.Visibility.Collapsed;                
                this._lightnessSelector.Visibility = System.Windows.Visibility.Visible;
                HSB hslColor = HSB.FromColor(this.CurrentColor);
                this.SetHueMarker(hslColor.H);
                this.SetLightnessBox(hslColor);
                this.SetLightnessMarker(hslColor);
            }
        }
        #endregion // UpdateDisplayBoxes

        #region GenerateColors

        #region GenerateNewColor
        private Color GenerateNewColor()
        {
            return Color.FromArgb(this.Alpha, this.Red, this.Green, this.Blue);
        }
        #endregion // GenerateNewColor

        #region GenerateNewHSLColor

        private HSL GenerateNewHSLColor()
        {
            return HSL.FromHSL(this.H, this.S, this.L, this.Alpha);
        }
        #endregion // GenerateNewHSLColor

        #region GenerateNewCMYKColor

        private CMYK GenerateNewCMYKColor()
        {
            return CMYK.FromCMYK(this.C, this.M, this.Y, this.K, this.Alpha);
        }
        #endregion // GenerateNewCMYKColor

        #endregion // GenerateColors

        #region EnsureVisualStates

        private void EnsureVisualStates()
        {
            if (this.ColorSliderView == ColorSliderView.RGB)
            {
                VisualStateManager.GoToState(this, "RGB", false);
            }
            else if (this.ColorSliderView == ColorSliderView.HSL)
            {
                VisualStateManager.GoToState(this, "HSL", false);
            }
            else if (this.ColorSliderView == ColorSliderView.CMYK)
            {
                VisualStateManager.GoToState(this, "CMYK", false);
            }
        }

        #endregion //EnsureVisualStates

        #region SetShadingCanvasPointer

        private void SetShadingCanvasPointer(double x , double y)
        {                        
            int xPos = (int)x;
            int yPos = (int)y;                                    
            _lightnessSelector.SetValue(Canvas.LeftProperty, xPos - (_lightnessSelector.Height / 2));
            _lightnessSelector.SetValue(Canvas.TopProperty, yPos - (_lightnessSelector.Height / 2));
        }

        #endregion // SetShadingCanvasPointer

        #region UpdateColorSystem
        private void UpdateColorSystem(ColorSliderView colorSystem)
        {
            switch (colorSystem)
            {
                case (ColorSliderView.CMYK):
                    {
                        this.SetHSLValuesToCurrentColor();
                        this.SetRGBValuesToCurrentColor();
                        break;
                    }

                case (ColorSliderView.RGB):
                    {
                        this.SetHSLValuesToCurrentColor();
                        this.SetCMYKValuesToCurrentColor();
                        break;
                    }

                case (ColorSliderView.HSL):
                    {
                        this.SetCMYKValuesToCurrentColor();
                        this.SetRGBValuesToCurrentColor();
                        break;
                    }
            }
        }
        #endregion // UpdateColorSystem

        #region RangeCheck
        private void RangeCheck(double lowValue, double highValue, string errorMsg, double currentValue)
        {
            if (currentValue < lowValue || currentValue > highValue)
            {
                throw new ArgumentException(GetString(errorMsg));
            }
        }
        #endregion // RangeCheck

        #endregion // Private

        #endregion // Methods

        #region Properties

        #region CurrentColor

        /// <summary>
        /// Identifies the <see cref="CurrentColor"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CurrentColorProperty = DependencyProperty.Register("CurrentColor", typeof(Color), typeof(AdvancedColorShadePicker), new PropertyMetadata(Colors.Transparent, new PropertyChangedCallback(CurrentColorChanged)));

        /// <summary>
        /// Gets / sets the <see cref="Color"/> which is currently being built in the editor.
        /// </summary>
        public Color CurrentColor
        {
            get { return (Color)this.GetValue(CurrentColorProperty); }
            set { this.SetValue(CurrentColorProperty, value); }
        }

        private static void CurrentColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.OnPropertyChanged("CurrentColor");
            if (!cs._suspendNotifyPropertyChange)
            {
                cs._suspendNotifyPropertyChange = true;
                cs.SetCMYKValuesToCurrentColor();
                cs.SetHSLValuesToCurrentColor();
                cs.SetRGBValuesToCurrentColor();
                cs._suspendNotifyPropertyChange = false;
            }
        }

        #endregion // CurrentColor

        #region SelectedColor

        /// <summary>
        /// Identifies the <see cref="SelectedColor"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color?), typeof(AdvancedColorShadePicker), new PropertyMetadata(Colors.Transparent, new PropertyChangedCallback(SelectedColorChanged)));

        /// <summary>
        /// Gets / sets the value of the <see cref="AdvancedColorShadePicker"/>.
        /// </summary>
        public Color? SelectedColor
        {
            get { return (Color?)this.GetValue(SelectedColorProperty); }
            set { this.SetValue(SelectedColorProperty, value); }
        }

        private static void SelectedColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.OnPropertyChanged("SelectedColor");

            if (cs.SelectedColor == null)
                cs.CurrentColor = Colors.Transparent;
            else
                cs.CurrentColor = (Color)cs.SelectedColor;

            HSL hslColor = HSL.FromColor(cs.CurrentColor);
            cs.UpdateDisplayBoxes();
            cs.SetHueMarker(hslColor.H);
        }

        #endregion // SelectedColor

        #region ColorPieces

        #region Alpha

        /// <summary>
        /// Identifies the <see cref="Alpha"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AlphaProperty = DependencyProperty.Register("Alpha", typeof(byte), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(AlphaChanged)));

        /// <summary>
        /// Gets / sets the Alpha channel value for the <see cref="CurrentColor"/>.
        /// </summary>
        public byte Alpha
        {
            get { return (byte)this.GetValue(AlphaProperty); }
            set { this.SetValue(AlphaProperty, value); }
        }

        private static void AlphaChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.OnPropertyChanged("Alpha");
            cs.RGB_ValueChanged();
        }

        #endregion // Alpha

        private void RGB_ValueChanged()
        {
            if (!this._suspendNotifyPropertyChange)
            {
                Color c = this.GenerateNewColor();
                this.CurrentColor = c;
                this._suspendNotifyPropertyChange = true;
                this.UpdateDisplayBoxes();
                this.UpdateColorSystem(ColorSliderView.RGB);
                this._suspendNotifyPropertyChange = false;
            }
        }

        private void HSL_ValueChanged()
        {
            if (!this._suspendNotifyPropertyChange && !this._suspendHSLNotify)
            {
                HSL hsl = this.GenerateNewHSLColor();
                this._suspendNotifyPropertyChange = true;
                this.CurrentColor = hsl.ToColor();
                this.UpdateDisplayBoxes();
                this.UpdateColorSystem(ColorSliderView.HSL);
                this._suspendNotifyPropertyChange = false;
            }
        }

        private void CMYK_ValueChanged()
        {
            if (!this._suspendNotifyPropertyChange)
            {
                CMYK cmyk = this.GenerateNewCMYKColor();
                this._suspendNotifyPropertyChange = true;
                this.CurrentColor = cmyk.ToColor();
                this.UpdateDisplayBoxes();
                this.UpdateColorSystem(ColorSliderView.CMYK);
                this._suspendNotifyPropertyChange = false;
            }
        }

        #region RGB

        #region Red

        /// <summary>
        /// Identifies the <see cref="Red"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty RedProperty = DependencyProperty.Register("Red", typeof(byte), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(RedChanged)));

        /// <summary>
        /// Gets / sets the Red channel value for the <see cref="CurrentColor"/>.
        /// </summary>
        public byte Red
        {
            get { return (byte)this.GetValue(RedProperty); }
            set { this.SetValue(RedProperty, value); }
        }

        private static void RedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;            
            cs.OnPropertyChanged("Red");
            cs.RGB_ValueChanged();
        }

        #endregion // Red

        #region Blue

        /// <summary>
        /// Identifies the <see cref="Blue"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty BlueProperty = DependencyProperty.Register("Blue", typeof(byte), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(BlueChanged)));

        /// <summary>
        /// Gets / sets the Blue channel value for the <see cref="CurrentColor"/>.
        /// </summary>
        public byte Blue
        {
            get { return (byte)this.GetValue(BlueProperty); }
            set { this.SetValue(BlueProperty, value); }
        }

        private static void BlueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.OnPropertyChanged("Blue");
            cs.RGB_ValueChanged();
        }

        #endregion // Blue

        #region Green

        /// <summary>
        /// Identifies the <see cref="Green"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty GreenProperty = DependencyProperty.Register("Green", typeof(byte), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(GreenChanged)));

        /// <summary>
        /// Gets / sets the Green channel value for the <see cref="CurrentColor"/>.
        /// </summary>
        public byte Green
        {
            get { return (byte)this.GetValue(GreenProperty); }
            set { this.SetValue(GreenProperty, value); }
        }

        private static void GreenChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.OnPropertyChanged("Green");
            cs.RGB_ValueChanged();
        }

        #endregion // Green

        #endregion // RGB

        #region HSL

        #region H

        /// <summary>
        /// Identifies the <see cref="H"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HProperty = DependencyProperty.Register("H", typeof(double), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(HChanged)));

        /// <summary>
        /// Gets / sets the H channel value for the <see cref="CurrentColor"/>. This is from the HSL color space.
        /// </summary>        
        public double H
        {
            get { return (double)this.GetValue(HProperty); }
            set { this.SetValue(HProperty, value); }
        }

        private static void HChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.RangeCheck(0.0, 360.0, "HueOutOfRange", (double)e.NewValue);
            cs.OnPropertyChanged("H");
            cs.HSL_ValueChanged();
        }

        #endregion // H

        #region S

        /// <summary>
        /// Identifies the <see cref="S"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty SProperty = DependencyProperty.Register("S", typeof(double), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(SChanged)));

        /// <summary>
        /// Gets / sets the S channel value for the <see cref="CurrentColor"/>. This is from the HSL color space.
        /// </summary>        
        public double S
        {
            get { return (double)this.GetValue(SProperty); }
            set { this.SetValue(SProperty, value); }
        }

        private static void SChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.RangeCheck(0.0, 100.0, "SaturationOutOfRange", (double)e.NewValue);
            cs.OnPropertyChanged("S");
            cs.HSL_ValueChanged();
        }

        #endregion // S

        #region L

        /// <summary>
        /// Identifies the <see cref="L"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty LProperty = DependencyProperty.Register("L", typeof(double), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(LChanged)));

        /// <summary>
        /// Gets / sets the L channel value for the <see cref="CurrentColor"/>. This is from the HSL color space.
        /// </summary>        
        public double L
        {
            get { return (double)this.GetValue(LProperty); }
            set { this.SetValue(LProperty, value); }
        }

        private static void LChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.RangeCheck(0.0, 100.0, "LightnessOutOfRange", (double)e.NewValue);
            cs.OnPropertyChanged("L");
            cs.HSL_ValueChanged();
        }

        #endregion // L

        #endregion // HSL

        #region CMYK

        #region C

        /// <summary>
        /// Identifies the <see cref="C"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CProperty = DependencyProperty.Register("C", typeof(double), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(CChanged)));

        /// <summary>
        /// Gets / sets the C channel value for the <see cref="CurrentColor"/>. This is from the CMYK color space.
        /// </summary>        
        public double C
        {
            get { return (double)this.GetValue(CProperty); }
            set { this.SetValue(CProperty, value); }
        }

        private static void CChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.RangeCheck(0.0, 100.0, "CyanOutOfRange", (double)e.NewValue);
            cs.OnPropertyChanged("C");
            cs.CMYK_ValueChanged();
        }

        #endregion // C

        #region M

        /// <summary>
        /// Identifies the <see cref="M"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MProperty = DependencyProperty.Register("M", typeof(double), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(MChanged)));

        /// <summary>
        /// Gets / sets the M channel value for the <see cref="CurrentColor"/>. This is from the CMYK color space.
        /// </summary>        
        public double M
        {
            get { return (double)this.GetValue(MProperty); }
            set { this.SetValue(MProperty, value); }
        }

        private static void MChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.RangeCheck(0.0, 100.0, "MagentaOutOfRange", (double)e.NewValue);
            cs.OnPropertyChanged("M");
            cs.CMYK_ValueChanged();
        }

        #endregion // M

        #region Y

        /// <summary>
        /// Identifies the <see cref="Y"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(double), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(YChanged)));

        /// <summary>
        /// Gets / sets the Y channel value for the <see cref="CurrentColor"/>. This is from the CMYK color space.
        /// </summary>        
        public double Y
        {
            get { return (double)this.GetValue(YProperty); }
            set { this.SetValue(YProperty, value); }
        }

        private static void YChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.RangeCheck(0.0, 100.0, "YellowOutOfRange", (double)e.NewValue);
            cs.OnPropertyChanged("Y");
            cs.CMYK_ValueChanged();
        }

        #endregion // Y

        #region K

        /// <summary>
        /// Identifies the <see cref="K"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty KProperty = DependencyProperty.Register("K", typeof(double), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(KChanged)));

        /// <summary>
        /// Gets / sets the K channel value for the <see cref="CurrentColor"/>. This is from the CMYK color space.
        /// </summary>        
        public double K
        {
            get { return (double)this.GetValue(KProperty); }
            set { this.SetValue(KProperty, value); }
        }

        private static void KChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.RangeCheck(0.0, 100.0, "BlackOutOfRange", (double)e.NewValue);
            cs.OnPropertyChanged("K");
            cs.CMYK_ValueChanged();
        }

        #endregion // K

        #endregion // CMYK

        #endregion // ColorPieces

        #region ColorSliderView
        /// <summary>
        /// Gets / sets which group of sliders will be visible
        /// </summary>
        protected internal ColorSliderView ColorSliderView
        {
            get
            {
                return this._colorSliderView;
            }
            set
            {
                if (this._colorSliderView != value)
                {
                    this._colorSliderView = value;
                    this.EnsureVisualStates();
                }
            }
        }
        #endregion // ColorSliderView

        #region ColorPicker

        /// <summary>
        /// Gets / sets the <see cref="XamColorPicker"/> which is associated with this <see cref="AdvancedColorShadePicker"/>.
        /// </summary>
        protected XamColorPicker ColorPicker
        {
            get;
            set;
        }

        #endregion // ColorPicker

        #region Captions

        #region AlphaCaption

        /// <summary>
        /// Identifies the <see cref="AlphaCaption"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AlphaCaptionProperty = DependencyProperty.Register("AlphaCaption", typeof(string), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(AlphaCaptionChanged)));

        /// <summary>
        /// Gets / sets the caption for the <see cref="Alpha"/> slider.
        /// </summary>
        public string AlphaCaption
        {
            get { return (string)this.GetValue(AlphaCaptionProperty); }
            set { this.SetValue(AlphaCaptionProperty, value); }
        }

        private static void AlphaCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.OnPropertyChanged("AlphaCaption");
        }

        #endregion // AlphaCaption

        #region RedCaption

        /// <summary>
        /// Identifies the <see cref="RedCaption"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty RedCaptionProperty = DependencyProperty.Register("RedCaption", typeof(string), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(RedCaptionChanged)));

        /// <summary>
        /// Gets / sets the caption for the <see cref="Red"/> slider.
        /// </summary>
        public string RedCaption
        {
            get { return (string)this.GetValue(RedCaptionProperty); }
            set { this.SetValue(RedCaptionProperty, value); }
        }

        private static void RedCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.OnPropertyChanged("RedCaption");
        }

        #endregion // RedCaption

        #region BlueCaption

        /// <summary>
        /// Identifies the <see cref="BlueCaption"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty BlueCaptionProperty = DependencyProperty.Register("BlueCaption", typeof(string), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(BlueCaptionChanged)));

        /// <summary>
        /// Gets / sets the caption for the <see cref="Blue"/> slider.
        /// </summary>
        public string BlueCaption
        {
            get { return (string)this.GetValue(BlueCaptionProperty); }
            set { this.SetValue(BlueCaptionProperty, value); }
        }

        private static void BlueCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.OnPropertyChanged("BlueCaption");
        }

        #endregion // BlueCaption

        #region GreenCaption

        /// <summary>
        /// Identifies the <see cref="GreenCaption"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty GreenCaptionProperty = DependencyProperty.Register("GreenCaption", typeof(string), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(GreenCaptionChanged)));

        /// <summary>
        /// Gets / sets the caption for the <see cref="Green"/> slider.
        /// </summary>
        public string GreenCaption
        {
            get { return (string)this.GetValue(GreenCaptionProperty); }
            set { this.SetValue(GreenCaptionProperty, value); }
        }

        private static void GreenCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.OnPropertyChanged("GreenCaption");
        }

        #endregion // GreenCaption

        #region HueCaption

        /// <summary>
        /// Identifies the <see cref="HueCaption"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HueCaptionProperty = DependencyProperty.Register("HueCaption", typeof(string), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(HueCaptionChanged)));

        /// <summary>
        /// Gets / sets the caption for the <see cref="H"/> slider.
        /// </summary>
        public string HueCaption
        {
            get { return (string)this.GetValue(HueCaptionProperty); }
            set { this.SetValue(HueCaptionProperty, value); }
        }

        private static void HueCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.OnPropertyChanged("HueCaption");
        }

        #endregion // HueCaption

        #region SaturationCaption

        /// <summary>
        /// Identifies the <see cref="SaturationCaption"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty SaturationCaptionProperty = DependencyProperty.Register("SaturationCaption", typeof(string), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(SaturationCaptionChanged)));

        /// <summary>
        /// Gets / sets the caption for the <see cref="S"/> slider.
        /// </summary>
        public string SaturationCaption
        {
            get { return (string)this.GetValue(SaturationCaptionProperty); }
            set { this.SetValue(SaturationCaptionProperty, value); }
        }

        private static void SaturationCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.OnPropertyChanged("SaturationCaption");
        }

        #endregion // SaturationCaption

        #region LightnessCaption

        /// <summary>
        /// Identifies the <see cref="LightnessCaption"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty LightnessCaptionProperty = DependencyProperty.Register("LightnessCaption", typeof(string), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(LightnessCaptionChanged)));

        /// <summary>
        /// Gets / sets the caption for the <see cref="L"/> slider.
        /// </summary>
        public string LightnessCaption
        {
            get { return (string)this.GetValue(LightnessCaptionProperty); }
            set { this.SetValue(LightnessCaptionProperty, value); }
        }

        private static void LightnessCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.OnPropertyChanged("LightnessCaption");
        }

        #endregion // LightnessCaption

        #region CyanCaption

        /// <summary>
        /// Identifies the <see cref="CyanCaption"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CyanCaptionProperty = DependencyProperty.Register("CyanCaption", typeof(string), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(CyanCaptionChanged)));

        /// <summary>
        /// Gets / sets the caption for the <see cref="C"/> slider.
        /// </summary>
        public string CyanCaption
        {
            get { return (string)this.GetValue(CyanCaptionProperty); }
            set { this.SetValue(CyanCaptionProperty, value); }
        }

        private static void CyanCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.OnPropertyChanged("CyanCaption");
        }

        #endregion // CyanCaption

        #region YellowCaption

        /// <summary>
        /// Identifies the <see cref="YellowCaption"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty YellowCaptionProperty = DependencyProperty.Register("YellowCaption", typeof(string), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(YellowCaptionChanged)));

        /// <summary>
        /// Gets / sets the caption for the <see cref="Y"/> slider.
        /// </summary>
        public string YellowCaption
        {
            get { return (string)this.GetValue(YellowCaptionProperty); }
            set { this.SetValue(YellowCaptionProperty, value); }
        }

        private static void YellowCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.OnPropertyChanged("YellowCaption");
        }

        #endregion // YellowCaption

        #region MagentaCaption

        /// <summary>
        /// Identifies the <see cref="MagentaCaption"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MagentaCaptionProperty = DependencyProperty.Register("MagentaCaption", typeof(string), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(MagentaCaptionChanged)));

        /// <summary>
        /// Gets / sets the caption for the <see cref="M"/> slider.
        /// </summary>
        public string MagentaCaption
        {
            get { return (string)this.GetValue(MagentaCaptionProperty); }
            set { this.SetValue(MagentaCaptionProperty, value); }
        }

        private static void MagentaCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.OnPropertyChanged("MagentaCaption");
        }

        #endregion // MagentaCaption

        #region BlackCaption

        /// <summary>
        /// Identifies the <see cref="BlackCaption"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty BlackCaptionProperty = DependencyProperty.Register("BlackCaption", typeof(string), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(BlackCaptionChanged)));

        /// <summary>
        /// Gets / sets the caption for the <see cref="K"/> slider.
        /// </summary>
        public string BlackCaption
        {
            get { return (string)this.GetValue(BlackCaptionProperty); }
            set { this.SetValue(BlackCaptionProperty, value); }
        }

        private static void BlackCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.OnPropertyChanged("BlackCaption");
        }

        #endregion // BlackCaption

        #region OKCaption

        /// <summary>
        /// Identifies the <see cref="OKCaption"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty OKCaptionProperty = DependencyProperty.Register("OKCaption", typeof(string), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(OKCaptionChanged)));

        /// <summary>
        /// Gets / sets a string that can be used for acceptance text.
        /// </summary>
        public string OKCaption
        {
            get { return (string)this.GetValue(OKCaptionProperty); }
            set { this.SetValue(OKCaptionProperty, value); }
        }

        private static void OKCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.OnPropertyChanged("OKCaption");
        }

        #endregion // OKCaption

        #region CancelCaption

        /// <summary>
        /// Identifies the <see cref="CancelCaption"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CancelCaptionProperty = DependencyProperty.Register("CancelCaption", typeof(string), typeof(AdvancedColorShadePicker), new PropertyMetadata(new PropertyChangedCallback(CancelCaptionChanged)));

        /// <summary>
        /// Gets / sets a string that can be used for cancel text.
        /// </summary>
        public string CancelCaption
        {
            get { return (string)this.GetValue(CancelCaptionProperty); }
            set { this.SetValue(CancelCaptionProperty, value); }
        }

        private static void CancelCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AdvancedColorShadePicker cs = (AdvancedColorShadePicker)obj;
            cs.OnPropertyChanged("CancelCaption");
        }

        #endregion // CancelCaption

        #endregion // Captions

        #endregion // Properties

        #region Overrides
        /// <summary>
        /// Builds the visual tree for the <see cref="AdvancedColorShadePicker"/> when a new template is applied. 
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (this._hueRectangle != null)
            {
                this._hueRectangle.MouseLeftButtonDown -= HueRectangle_MouseLeftButtonDown;
                this._hueRectangle.MouseLeftButtonUp -= HueRectangle_MouseLeftButtonUp;
                this._hueRectangle.MouseMove -= HueRectangle_MouseMove;
            }

            if (this._shadingCanvas != null)
            {
                this._shadingCanvas.MouseLeftButtonDown -= ShadingCanvas_MouseLeftButtonDown;
                this._shadingCanvas.MouseLeftButtonUp -= ShadingCanvas_MouseLeftButtonUp;
                this._shadingCanvas.MouseMove -= ShadingCanvas_MouseMove;
                this._shadingCanvas.SizeChanged -= ShadingCanvas_SizeChanged;
            }

            base.OnApplyTemplate();

            this._hueRectangle = base.GetTemplateChild("HueRectangle") as Rectangle;

            if (this._hueRectangle != null)
            {
                this._hueRectangle.MouseLeftButtonDown += HueRectangle_MouseLeftButtonDown;
                this._hueRectangle.MouseLeftButtonUp += HueRectangle_MouseLeftButtonUp;
                this._hueRectangle.MouseMove += HueRectangle_MouseMove;
            }

            this._shadingCanvas = base.GetTemplateChild("ShadingCanvas") as Canvas;

            if (this._shadingCanvas != null)
            {
                this._shadingCanvas.MouseLeftButtonDown += ShadingCanvas_MouseLeftButtonDown;
                this._shadingCanvas.MouseLeftButtonUp += ShadingCanvas_MouseLeftButtonUp;
                this._shadingCanvas.MouseMove += ShadingCanvas_MouseMove;
                this._shadingCanvas.SizeChanged += ShadingCanvas_SizeChanged;
            }

            this._lightnessRectange = base.GetTemplateChild("ColorSamplingRectangle") as Rectangle;
            this._hueSelector = base.GetTemplateChild("HueSelector") as Canvas;
            this._lightnessSelector = base.GetTemplateChild("LightnessSelector") as Canvas;

            Color currentColor = Colors.Transparent;

            if (this.SelectedColor != null)
            {
                currentColor = (Color)this.SelectedColor;
            }
            this._suspendNotifyPropertyChange = true;
            HSB hslColor = HSB.FromColor(currentColor);
            this.SetHueMarker(hslColor.H);
            this.SetLightnessBox(hslColor);
            this.SetLightnessMarker(hslColor);
            this.SetCMYKValuesToCurrentColor();
            this.SetHSLValuesToCurrentColor();
            this.SetRGBValuesToCurrentColor();
            this._suspendNotifyPropertyChange = false;
        }

        #endregion // Overrides

        #region Event Handlers

        #region HueRectangle_MouseLeftButtonUp

        void HueRectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ProcessHueCanvasMouseEvent(e);
            _hueMouseDown = false;
            _hueRectangle.ReleaseMouseCapture();
        }

        #endregion // HueRectangle_MouseLeftButtonUp

        #region HueRectangle_MouseLeftButtonDown

        void HueRectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this._lightnessMouseDown = false;
            this._hueMouseDown = true;
            ProcessHueCanvasMouseEvent(e);
        }

        #endregion // HueRectangle_MouseLeftButtonDown

        #region HueRectangle_MouseMove

        void HueRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (_hueMouseDown)
            {
                _hueRectangle.CaptureMouse();
                this.ProcessHueCanvasMouseEvent(e);
            }
        }

        #endregion // HueRectangle_MouseMove

        #region ShadingCanvas_MouseLeftButtonUp

        void ShadingCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this._lightnessMouseDown = false;
            this._shadingCanvas.ReleaseMouseCapture();
        }

        #endregion // ShadingCanvas_MouseLeftButtonUp

        #region ShadingCanvas_MouseLeftButtonDown

        void ShadingCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this._lightnessMouseDown = true;

            this._hueMouseDown = false;

            this.ProcessShadingCanvasMouseEvent(e);
        }

        #endregion // ShadingCanvas_MouseLeftButtonDown

        #region ShadingCanvas_SizeChanged

        void ShadingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.SelectedColor != null)
            {
                HSB hslColor = HSB.FromColor((Color)this.SelectedColor);
                this.SetHueMarker(hslColor.H);
                this.SetLightnessBox(hslColor);
                this.SetLightnessMarker(hslColor);
            }
        }

        #endregion // ShadingCanvas_SizeChanged

        #region ProcessHueCanvasMouseEvent

        private void ProcessHueCanvasMouseEvent(MouseEventArgs e)
        {
            Point pointInHueRectangle = e.GetPosition(this._hueRectangle);

            double y = pointInHueRectangle.Y < 0 ? 0 : (pointInHueRectangle.Y > _hueRectangle.ActualHeight ? _hueRectangle.ActualHeight : pointInHueRectangle.Y);

            int yPos = (int)y;

            this.SetLightnessBox(yPos);

            _hueSelector.SetValue(Canvas.TopProperty, yPos - (_hueSelector.Height / 2));

            //this._suspendNotifyPropertyChange = true;
            this._suspendHSLNotify = true;
            this.H = (yPos / this._hueRectangle.ActualHeight) * 360.0;
            HSL hsl = new HSL();
            hsl.Alpha = this.Alpha;
            hsl.H = this.H;
            hsl.S = this.S;
            hsl.L = this.L;
            Color c1 = hsl.ToColor();
            this.CurrentColor = c1;
            this._suspendHSLNotify = false;
            //this._suspendNotifyPropertyChange = false;            
        }

        #endregion // ProcessHueCanvasMouseEvent

        #region ProcessShadingCanvasMouseEvent

        private void ProcessShadingCanvasMouseEvent(MouseEventArgs e)
        {
            Point pointInHueRectangle = e.GetPosition(this._hueRectangle);
            double yHue = pointInHueRectangle.Y < 0 ? 0 : (pointInHueRectangle.Y > _hueRectangle.ActualHeight ? _hueRectangle.ActualHeight : pointInHueRectangle.Y);

            Point pointInShadingCanvas = e.GetPosition(this._shadingCanvas);
            double x = pointInShadingCanvas.X < 0 ? 0 : (pointInShadingCanvas.X > _shadingCanvas.ActualWidth ? _shadingCanvas.ActualWidth : pointInShadingCanvas.X);
            double y = pointInShadingCanvas.Y < 0 ? 0 : (pointInShadingCanvas.Y > _shadingCanvas.ActualHeight ? _shadingCanvas.ActualHeight : pointInShadingCanvas.Y);
            
            HSL hsl = new HSL();
            hsl.Alpha = this.CurrentColor.A;
            hsl.H = this.H;
            
            this._suspendHSLNotify = true;

            double s = (x / this._shadingCanvas.ActualWidth);
            double v = (1.0 - (y / this._shadingCanvas.ActualHeight));
            double ll = (2.0 - s) * v;
            double ss = s * v;
            double saturation = 0;
            double lightness = ll / 2.0;

            if (ss != 0)
            {
                saturation = ss / ((ll <= 1) ? ll : (2.0 - ll));
            }

            this.S = hsl.S = Math.Min(1, Math.Max(0, saturation)) * 100;
            this.L = hsl.L = Math.Min(1, Math.Max(0, lightness)) * 100;

            this._shadingCanvas.CaptureMouse();
            this.SetShadingCanvasPointer(x, y);
            Color c1 = hsl.ToColor();
            this.CurrentColor = c1;

            this._suspendHSLNotify = false;
        }

        #endregion // ProcessShadingCanvasMouseEvent

        #region ShadingCanvas_MouseMove

        void ShadingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_lightnessMouseDown)
            {
                this.ProcessShadingCanvasMouseEvent(e);
            }
        }

        #endregion // ShadingCanvas_MouseMove

        #region AdvancedColorShadePicker_Unloaded
        void AdvancedColorShadePicker_Unloaded(object sender, RoutedEventArgs e)
        {
            this._hueMouseDown = false;
            this._lightnessMouseDown = false;
            if (this.SelectedColor == null)
            {
                this.CurrentColor = Colors.Transparent;
            }
            else
            {
                this.CurrentColor = Colors.Transparent;
                this.CurrentColor = (Color)this.SelectedColor;
            }
        }

        #endregion // AdvancedColorShadePicker_Unloaded

        #region AdvancedColorShadePicker_Loaded

        void AdvancedColorShadePicker_Loaded(object sender, RoutedEventArgs e)
        {
            Color currentColor = Colors.Transparent;

            if (this.SelectedColor != null)
            {
                currentColor = (Color)this.SelectedColor;
            }

            this.CurrentColor = currentColor;

            this._suspendNotifyPropertyChange = true;
            HSB hslColor = HSB.FromColor(currentColor);
            this.SetHueMarker(hslColor.H);
            this.SetLightnessBox(hslColor);
            this.SetLightnessMarker(hslColor);
            this.SetCMYKValuesToCurrentColor();
            this.SetHSLValuesToCurrentColor();
            this.SetRGBValuesToCurrentColor();
            this._suspendNotifyPropertyChange = false;

            this.Dispatcher.BeginInvoke((Action)UpdateDisplayBoxes, null);

        }

        #endregion // AdvancedColorShadePicker_Loaded

        #endregion // Event Handlers

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Fired when a property changes on the <see cref="XamColorPicker"/>.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invoked when a property changes on the <see cref="XamColorPicker"/> object.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region ICommandTarget Members

        bool ICommandTarget.SupportsCommand(ICommand command)
        {
            return command is ColorSlidersCommandBase;
        }

        object ICommandTarget.GetParameter(CommandSource source)
        {
            return this;
        }

        #endregion

        #region ISupportColorPickerAdvancedEditing Members

        XamColorPicker ISupportColorPickerAdvancedEditing.ColorPicker
        {
            get
            {
                return this.ColorPicker;
            }
            set
            {
                this.ColorPicker = value;
            }
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