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
using System.Windows.Controls.Primitives;
using System.Collections.Generic;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Color picker used in the ActivityCategoryDialogs.  For internal use only.
	/// </summary>
	[TemplatePart(Name = PartFixedColorsGrid, Type = typeof(Grid))]
	[TemplatePart(Name = PartColorPickerPopup, Type = typeof(Popup))]
	[TemplatePart(Name = PartColorPickerDropDownButton, Type = typeof(DropDownToggleButton))]
	[TemplatePart(Name = PartNoneColorButton, Type = typeof(Grid))]
	[DesignTimeVisible(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class ActivityCategoryColorPicker : Control
	{
		#region Member Variables

		// Template part names
		private const string PartFixedColorsGrid			= "FixedColorsGrid";
		private const string PartColorPickerPopup			= "ColorPickerPopup";
		private const string PartColorPickerDropDownButton	= "ColorPickerDropDownButton";
		private const string PartNoneColorButton			= "NoneColorButton";

		private bool								_initialized;
		private ColorWell							_selectedColorWell;
		private Grid								_fixedColorsGrid;
		private Grid								_noneColorButton;
		private Popup								_colorPickerPopup;
		private DropDownToggleButton				_colorPickerDropDownButton;
		private bool								_selectedColorSetByRGBChange;
		private bool								_selectedRGBSetBySelectedColorChange;
		private bool								_isMouseOverNoneColorButton;
		private bool								_isLeftMouseButtonDownOverNoneColorButton;





		private Color?[,]							_fixedColorsArray = new Color?[5, 5];

		// JM 04-08-11 TFS72027
		private Dictionary<string, string>			_localizedStrings;

		#endregion //Member  Variables

		#region Constructors
		static ActivityCategoryColorPicker()
		{

			ActivityCategoryColorPicker.DefaultStyleKeyProperty.OverrideMetadata(typeof(ActivityCategoryColorPicker), new FrameworkPropertyMetadata(typeof(ActivityCategoryColorPicker)));

		}

		/// <summary>
		/// Creates an instance of the ActivityCategoryColorPicker.  For internal use only.
		/// </summary>
		public ActivityCategoryColorPicker()
		{



		}
		#endregion //Constructors

		#region Base Class Overrides

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			// Initialize.
			this.Dispatcher.BeginInvoke(new ScheduleUtilities.MethodInvoker(this.Initialize));
		}
		#endregion //OnApplyTemplate

		#endregion //Base Class Overrides

		#region Properties

		#region Public Properties

		#region AreCustomColorsAllowed

		/// <summary>
		/// Identifies the <see cref="AreCustomColorsAllowed"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AreCustomColorsAllowedProperty = DependencyPropertyUtilities.Register("AreCustomColorsAllowed",
			typeof(bool), typeof(ActivityCategoryColorPicker),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox, null)
			);

		/// <summary>
		/// Returns/sets whether the custom color portion of the picker should be visible.
		/// </summary>
		/// <seealso cref="FixedColors"/>
		public bool AreCustomColorsAllowed
		{
			get { return (bool)this.GetValue(ActivityCategoryColorPicker.AreCustomColorsAllowedProperty); }
			set { this.SetValue(ActivityCategoryColorPicker.AreCustomColorsAllowedProperty, value); }
		}

		#endregion //AreCustomColorsAllowed

		#region ColorSwatchExtent

		/// <summary>
		/// Identifies the <see cref="ColorSwatchExtent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ColorSwatchExtentProperty = DependencyPropertyUtilities.Register("ColorSwatchExtent",
			typeof(double), typeof(ActivityCategoryColorPicker),
			DependencyPropertyUtilities.CreateMetadata(15d, null)
			);

		/// <summary>
		/// Returns/sets the extent used for the width and height of the color swatches in the color picker dropdown.
		/// </summary>
		public double ColorSwatchExtent
		{
			get { return (double)this.GetValue(ActivityCategoryColorPicker.ColorSwatchExtentProperty); }
			set { this.SetValue(ActivityCategoryColorPicker.ColorSwatchExtentProperty, value); }
		}

		#endregion //ColorSwatchExtent

		#region FixedColors

		/// <summary>
		/// Identifies the <see cref="FixedColors"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FixedColorsProperty = DependencyPropertyUtilities.Register("FixedColors",
			typeof(IList<Color>), typeof(ActivityCategoryColorPicker),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnFixedColorsChanged))
			);

		private static void OnFixedColorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityCategoryColorPicker colorPicker = d as ActivityCategoryColorPicker;
			if (null == colorPicker)
				return;

			if (colorPicker._initialized)
				colorPicker.InitializeFixedColorsGrid();
		}

		/// <summary>
		/// Returns/sets a list of up to 25 Colors that should be used for the fixed colors portion of the picker.
		/// </summary>
		/// <remarks>
		/// If the list contains more than 25 colors, only the first 25 are used.
		/// </remarks>
		/// <seealso cref="AreCustomColorsAllowed"/>
		public IList<Color> FixedColors
		{
			get { return (IList<Color>)this.GetValue(ActivityCategoryColorPicker.FixedColorsProperty); }
			set { this.SetValue(ActivityCategoryColorPicker.FixedColorsProperty, value); }
		}

		#endregion //FixedColors

		// JM 04-08-11 TFS72027 Added.
		#region LocalizedStrings
		/// <summary>
		/// Returns a dictionary of localized strings for use by the controls in the template.
		/// </summary>
		public Dictionary<string, string> LocalizedStrings
		{
			get
			{
				if (this._localizedStrings == null)
				{
					this._localizedStrings = new Dictionary<string, string>(10);

					this._localizedStrings.Add("CTL_ActivityCategoryColorPicker_None", ScheduleUtilities.GetString("CTL_ActivityCategoryColorPicker_None"));
					this._localizedStrings.Add("CTL_ActivityCategoryColorPicker_RGBValues", ScheduleUtilities.GetString("CTL_ActivityCategoryColorPicker_RGBValues"));
					this._localizedStrings.Add("CTL_ActivityCategoryColorPicker_R", ScheduleUtilities.GetString("CTL_ActivityCategoryColorPicker_R"));
					this._localizedStrings.Add("CTL_ActivityCategoryColorPicker_G", ScheduleUtilities.GetString("CTL_ActivityCategoryColorPicker_G"));
					this._localizedStrings.Add("CTL_ActivityCategoryColorPicker_B", ScheduleUtilities.GetString("CTL_ActivityCategoryColorPicker_B"));
				}

				return this._localizedStrings;
			}
		}
		#endregion //LocalizedStrings

		#region NoneColorButtonBackgroundBrush

		private static readonly DependencyPropertyKey NoneColorButtonBackgroundBrushPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("NoneColorButtonBackgroundBrush",
			typeof(Brush), typeof(ActivityCategoryColorPicker), ScheduleUtilities.GetBrush(Colors.Transparent), null);

		/// <summary>
		/// Identifies the <see cref="NoneColorButtonBackgroundBrush"/> dependency property
		/// </summary>
		public static readonly DependencyProperty NoneColorButtonBackgroundBrushProperty = NoneColorButtonBackgroundBrushPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the background of the 'None' color button. (read only)
		/// </summary>
		public Brush NoneColorButtonBackgroundBrush
		{
			get { return (Brush)this.GetValue(ActivityCategoryColorPicker.NoneColorButtonBackgroundBrushProperty); }
			internal set { this.SetValue(ActivityCategoryColorPicker.NoneColorButtonBackgroundBrushPropertyKey, value); }
		}

		#endregion //NoneColorButtonBackgroundBrush

		#region NoneColorButtonBorderBrush

		private static readonly DependencyPropertyKey NoneColorButtonBorderBrushPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("NoneColorButtonBorderBrush",
			typeof(Brush), typeof(ActivityCategoryColorPicker), ScheduleUtilities.GetBrush(Colors.Transparent), null);

		/// <summary>
		/// Identifies the <see cref="NoneColorButtonBorderBrush"/> dependency property
		/// </summary>
		public static readonly DependencyProperty NoneColorButtonBorderBrushProperty = NoneColorButtonBorderBrushPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the border of the 'None' color button. (read only)
		/// </summary>
		public Brush NoneColorButtonBorderBrush
		{
			get { return (Brush)this.GetValue(ActivityCategoryColorPicker.NoneColorButtonBorderBrushProperty); }
			internal set { this.SetValue(ActivityCategoryColorPicker.NoneColorButtonBorderBrushPropertyKey, value); }
		}

		#endregion //NoneColorButtonBorderBrush

		#region NoneColorButtonForegroundBrush

		private static readonly DependencyPropertyKey NoneColorButtonForegroundBrushPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("NoneColorButtonForegroundBrush",
			typeof(Brush), typeof(ActivityCategoryColorPicker), ScheduleUtilities.GetBrush(Colors.Transparent), null);

		/// <summary>
		/// Identifies the <see cref="NoneColorButtonForegroundBrush"/> dependency property
		/// </summary>
		public static readonly DependencyProperty NoneColorButtonForegroundBrushProperty = NoneColorButtonForegroundBrushPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the foreground of the 'None' color button. (read only)
		/// </summary>
		public Brush NoneColorButtonForegroundBrush
		{
			get { return (Brush)this.GetValue(ActivityCategoryColorPicker.NoneColorButtonForegroundBrushProperty); }
			internal set { this.SetValue(ActivityCategoryColorPicker.NoneColorButtonForegroundBrushPropertyKey, value); }
		}

		#endregion //NoneColorButtonForegroundBrush

		#region SelectedColor

		/// <summary>
		/// Identifies the <see cref="SelectedColor"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedColorProperty = DependencyPropertyUtilities.Register("SelectedColor",
			typeof(Color?), typeof(ActivityCategoryColorPicker),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnSelectedColorChanged))
			);

		private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityCategoryColorPicker colorPicker = d as ActivityCategoryColorPicker;
			if (null == colorPicker)
				return;

			Color? newColor = (Color?)e.NewValue;
			colorPicker.UpdateUIWithSelectedColor(newColor);

			if (false == colorPicker._selectedColorSetByRGBChange)
			{
				colorPicker._selectedRGBSetBySelectedColorChange = true;

				if (newColor.HasValue)
				{
					colorPicker.SelectedColorRedValue	= Convert.ToInt32(newColor.Value.R);
					colorPicker.SelectedColorGreenValue = Convert.ToInt32(newColor.Value.G);
					colorPicker.SelectedColorBlueValue	= Convert.ToInt32(newColor.Value.B);
				}
				else
				{
					colorPicker.SelectedColorRedValue	= 0;
					colorPicker.SelectedColorGreenValue = 0;
					colorPicker.SelectedColorBlueValue	= 0;
				}

				colorPicker._selectedRGBSetBySelectedColorChange = false;
			}
		}

		/// <summary>
		/// Returns the currently selected color
		/// </summary>
		public Color? SelectedColor
		{
			get
			{
				return (Color?)this.GetValue(ActivityCategoryColorPicker.SelectedColorProperty);
			}
			// JM 07-20-11 TFS81257 - Make this public to support 2-way binding from our templates
			//internal set
			set
			{
				this.SetValue(ActivityCategoryColorPicker.SelectedColorProperty, value);
			}
		}

		#endregion //SelectedColor

		#region SelectedColorRedValue

		/// <summary>
		/// Identifies the <see cref="SelectedColorRedValue"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedColorRedValueProperty = DependencyPropertyUtilities.Register("SelectedColorRedValue",
			typeof(int), typeof(ActivityCategoryColorPicker),
			DependencyPropertyUtilities.CreateMetadata((int)0, new PropertyChangedCallback(OnSelectedColorRedValueChanged))
			);

		private static void OnSelectedColorRedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityCategoryColorPicker colorPicker = d as ActivityCategoryColorPicker;
			if (null == colorPicker)
				return;

			if (false == colorPicker._selectedRGBSetBySelectedColorChange)
			{
				colorPicker._selectedColorSetByRGBChange	= true;
				colorPicker.SelectedColor					= Color.FromArgb(Convert.ToByte(255), Convert.ToByte((int)e.NewValue), Convert.ToByte(colorPicker.SelectedColorGreenValue), Convert.ToByte(colorPicker.SelectedColorBlueValue));
				colorPicker._selectedColorSetByRGBChange	= false;
			}
		}

		/// <summary>
		/// Returns/sets the currently selected color's red value
		/// </summary>
		public int SelectedColorRedValue
		{
			get
			{
				return (int)this.GetValue(ActivityCategoryColorPicker.SelectedColorRedValueProperty);
			}
			set
			{
				this.SetValue(ActivityCategoryColorPicker.SelectedColorRedValueProperty, value);
			}
		}

		#endregion //SelectedColorRedValue

		#region SelectedColorGreenValue

		/// <summary>
		/// Identifies the <see cref="SelectedColorGreenValue"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedColorGreenValueProperty = DependencyPropertyUtilities.Register("SelectedColorGreenValue",
			typeof(int), typeof(ActivityCategoryColorPicker),
			DependencyPropertyUtilities.CreateMetadata((int)0, new PropertyChangedCallback(OnSelectedColorGreenValueChanged))
			);

		private static void OnSelectedColorGreenValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityCategoryColorPicker colorPicker = d as ActivityCategoryColorPicker;
			if (null == colorPicker)
				return;

			if (false == colorPicker._selectedRGBSetBySelectedColorChange)
			{
				colorPicker._selectedColorSetByRGBChange	= true;
				colorPicker.SelectedColor					= Color.FromArgb(Convert.ToByte(255), Convert.ToByte(colorPicker.SelectedColorRedValue), Convert.ToByte((int)e.NewValue), Convert.ToByte(colorPicker.SelectedColorBlueValue));
				colorPicker._selectedColorSetByRGBChange	= false;
			}
		}

		/// <summary>
		/// Returns/sets the currently selected color's green value
		/// </summary>
		public int SelectedColorGreenValue
		{
			get
			{
				return (int)this.GetValue(ActivityCategoryColorPicker.SelectedColorGreenValueProperty);
			}
			set
			{
				this.SetValue(ActivityCategoryColorPicker.SelectedColorGreenValueProperty, value);
			}
		}

		#endregion //SelectedColorGreenValue

		#region SelectedColorBlueValue

		/// <summary>
		/// Identifies the <see cref="SelectedColorBlueValue"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedColorBlueValueProperty = DependencyPropertyUtilities.Register("SelectedColorBlueValue",
			typeof(int), typeof(ActivityCategoryColorPicker),
			DependencyPropertyUtilities.CreateMetadata((int)0, new PropertyChangedCallback(OnSelectedColorBlueValueChanged))
			);

		private static void OnSelectedColorBlueValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityCategoryColorPicker colorPicker = d as ActivityCategoryColorPicker;
			if (null == colorPicker)
				return;

			if (false == colorPicker._selectedRGBSetBySelectedColorChange)
			{
				colorPicker._selectedColorSetByRGBChange	= true;
				colorPicker.SelectedColor					= Color.FromArgb(Convert.ToByte(255), Convert.ToByte(colorPicker.SelectedColorRedValue), Convert.ToByte(colorPicker.SelectedColorGreenValue), Convert.ToByte((int)e.NewValue));
				colorPicker._selectedColorSetByRGBChange	= false;
			}
		}

		/// <summary>
		/// Returns/sets the currently selected color's blue value
		/// </summary>
		public int SelectedColorBlueValue
		{
			get
			{
				return (int)this.GetValue(ActivityCategoryColorPicker.SelectedColorBlueValueProperty);
			}
			set
			{
				this.SetValue(ActivityCategoryColorPicker.SelectedColorBlueValueProperty, value);
			}
		}

		#endregion //SelectedColorBlueValue

		#region SelectedColorSwatch

		private static readonly DependencyPropertyKey SelectedColorSwatchPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("SelectedColorSwatch",
			typeof(UIElement), typeof(ActivityCategoryColorPicker), null, null);

		/// <summary>
		/// Identifies the <see cref="SelectedColorSwatch"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedColorSwatchProperty = SelectedColorSwatchPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the currently selected color
		/// </summary>
		public UIElement SelectedColorSwatch
		{
			get { return (UIElement)this.GetValue(ActivityCategoryColorPicker.SelectedColorSwatchProperty); }
			internal set { this.SetValue(ActivityCategoryColorPicker.SelectedColorSwatchPropertyKey, value); }
		}

		#endregion //SelectedColorSwatch

		#endregion //Public Properties

		#region Private Properties

		#region DefaultFixedColors
		private static IList<Color> DefaultFixedColors
		{
			get
			{
				List<Color> list = new List<Color>(25);
				list.Add(Color.FromArgb(255, 231,161, 162));
				list.Add(Color.FromArgb(255, 249, 186, 137)); 
				list.Add(Color.FromArgb(255, 247, 221, 143)); 
				list.Add(Color.FromArgb(255, 252, 250, 144)); 
				list.Add(Color.FromArgb(255, 120, 209, 104)); 
				list.Add(Color.FromArgb(255, 159, 220, 201)); 
				list.Add(Color.FromArgb(255, 198, 210, 176)); 
				list.Add(Color.FromArgb(255, 157, 183, 232)); 
				list.Add(Color.FromArgb(255, 181, 161, 226)); 
				list.Add(Color.FromArgb(255, 218, 174, 194));
				list.Add(Color.FromArgb(255, 218, 217, 220)); 
				list.Add(Color.FromArgb(255, 107, 121, 148)); 
				list.Add(Color.FromArgb(255, 191, 191, 191)); 
				list.Add(Color.FromArgb(255, 111, 111, 111)); 
				list.Add(Color.FromArgb(255, 79, 79, 79));
				list.Add(Color.FromArgb(255, 193, 26, 37)); 
				list.Add(Color.FromArgb(255,226, 98, 13 )); 
				list.Add(Color.FromArgb(255, 199, 153, 48)); 
				list.Add(Color.FromArgb(255, 185, 179, 0)); 
				list.Add(Color.FromArgb(255, 54, 143, 43));
				list.Add(Color.FromArgb(255, 50, 155, 122)); 
				list.Add(Color.FromArgb(255, 119, 139, 69)); 
				list.Add(Color.FromArgb(255, 40, 88, 165)); 
				list.Add(Color.FromArgb(255, 92, 63, 163));
				list.Add(Color.FromArgb(255, 147, 68, 107));

				return list;
			}
		}
		#endregion //DefaultFixedColors

		#endregion //Private Properties

		#endregion //Properties

		#region Methods

		#region Private Methods

		#region ClosePopup
		private void ClosePopup()
		{
			if (null != this._colorPickerPopup && this._colorPickerPopup.IsOpen)
				this._colorPickerPopup.IsOpen = false;
		}
		#endregion //ClosePopup

		#region GetColorWell
		private ColorWell GetColorWell(int row, int col)
		{
			if (null == this._fixedColorsGrid)
				return null;

			foreach (UIElement element in this._fixedColorsGrid.Children)
			{
				if (row == (int)element.GetValue(Grid.RowProperty) &&
					col == (int)element.GetValue(Grid.ColumnProperty))
					return element as ColorWell;
			}

			return null;
		}
		#endregion //GetColorWell

		#region Initialize
		private void Initialize()
		{
			if (true == this._initialized)
			{
				if (null != this._noneColorButton)
				{
					this._noneColorButton.MouseEnter			-= new MouseEventHandler(OnNoneColorButtonMouseEnter);
					this._noneColorButton.MouseLeave			-= new MouseEventHandler(OnNoneColorButtonMouseLeave);
					this._noneColorButton.MouseLeftButtonDown	-= new MouseButtonEventHandler(OnNoneColorButtonMouseLeftButtonDown);
					this._noneColorButton.MouseLeftButtonUp		-= new MouseButtonEventHandler(OnNoneColorButtonMouseLeftButtonUp);
				}

				if (null != this._colorPickerPopup)
					this._colorPickerPopup.Closed				-= new EventHandler(OnColorPickerPopupClosed);
			}
			
			// Find the parts we need.
			//
			// The FixedColors grid
			this._fixedColorsGrid = this.GetTemplateChild(PartFixedColorsGrid) as Grid;
			this.InitializeFixedColorsGrid();

			// The color picker dropdown button
			this._colorPickerDropDownButton = this.GetTemplateChild(PartColorPickerDropDownButton) as DropDownToggleButton;

			// The color picker Popup.
			this._colorPickerPopup = this.GetTemplateChild(PartColorPickerPopup) as Popup;
			if (null != this._colorPickerPopup)
			{


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

				this._colorPickerPopup.StaysOpen = false;
				if (null != this._colorPickerDropDownButton)
					this._colorPickerPopup.PlacementTarget = this._colorPickerDropDownButton;


				if (null != this._colorPickerDropDownButton)
					this._colorPickerDropDownButton.Popup = this._colorPickerPopup;

				this._colorPickerPopup.Closed += new EventHandler(OnColorPickerPopupClosed);
			}

			// The 'None' color button
			this._noneColorButton = this.GetTemplateChild(PartNoneColorButton) as Grid;
			if (null != this._noneColorButton)
			{
				this._noneColorButton.MouseEnter			+= new MouseEventHandler(OnNoneColorButtonMouseEnter);
				this._noneColorButton.MouseLeave			+= new MouseEventHandler(OnNoneColorButtonMouseLeave);
				this._noneColorButton.MouseLeftButtonDown	+= new MouseButtonEventHandler(OnNoneColorButtonMouseLeftButtonDown);
				this._noneColorButton.MouseLeftButtonUp		+= new MouseButtonEventHandler(OnNoneColorButtonMouseLeftButtonUp);
			}

			this.UpdateUIWithSelectedColor(this.SelectedColor);

			this._initialized = true;
		}

		#endregion //Initialize

		#region InitializeFixedColorsGrid
		private void InitializeFixedColorsGrid()
		{
			if (null != this._fixedColorsGrid)
			{
				// Remove any existing child elements from the fixed colors grid.
				if (this._fixedColorsGrid.Children.Count > 0)
					this._fixedColorsGrid.Children.Clear();

				// If our FixedColors property has been set use those colors, else use a default set of fixed colors.
				IList<Color> fixedColors;
				if (this.FixedColors != null)
					fixedColors = this.FixedColors;
				else
					fixedColors = DefaultFixedColors;

				// Copy the fixed colors specified in the list to our 2 dimensional (5,5) array of colors.
				int r = 0, c = 0;
				for (int i = 0; i < Math.Min(25, fixedColors.Count); i++)
				{
					this._fixedColorsArray[r, c] = fixedColors[i];
					c++;
					if (c > 4)
					{
						c = 0;
						r++;
					}
				}

				// Create 1 color well for each fixed color.
				ColorWell colorWell;
				for (int row = 0; row < 5; row++)
				{
					for (int col = 0; col < 5; col++)
					{
						if (false == this._fixedColorsArray[row, col].HasValue)
							break;

						colorWell = new ColorWell(this,
												  ScheduleUtilities.ColorFromBaseColor(_fixedColorsArray[row, col].Value, 0.6f, 0.7f),
												  _fixedColorsArray[row, col].Value);
						colorWell.SetValue(Grid.RowProperty, row);
						colorWell.SetValue(Grid.ColumnProperty, col);
						this._fixedColorsGrid.Children.Add(colorWell);
					}
				}
			}
		}
		#endregion //InitializeFixedColorsGrid

		#region OnColorPickerPopupClosed
		void OnColorPickerPopupClosed(object sender, EventArgs e)
		{
			this._isLeftMouseButtonDownOverNoneColorButton	= false;
			this._isMouseOverNoneColorButton				= false;
		}
		#endregion //OnColorPickerPopupClosed

		#region OnNoneColorButtonMouseLeftButtonUp
		void OnNoneColorButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			this._isLeftMouseButtonDownOverNoneColorButton = false;

			if (this.SelectedColor != null)
			{
				this.SelectedColor = null;
				this.ClosePopup();
			}

			this.UpdateNoneColorButtonBrushes();
		}
		#endregion //OnNoneColorButtonMouseLeftButtonUp	
    
		#region OnNoneColorButtonMouseLeftButtonDown
		void OnNoneColorButtonMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this._isLeftMouseButtonDownOverNoneColorButton = true;
			this.UpdateNoneColorButtonBrushes();
		}
		#endregion //OnNoneColorButtonMouseLeftButtonDown	
    
		#region OnNoneColorButtonMouseLeave
		void OnNoneColorButtonMouseLeave(object sender, MouseEventArgs e)
		{
			this._isMouseOverNoneColorButton = false;
			this.UpdateNoneColorButtonBrushes();
		}
		#endregion //OnNoneColorButtonMouseLeave	
    
		#region OnNoneColorButtonMouseEnter
		void OnNoneColorButtonMouseEnter(object sender, MouseEventArgs e)
		{
			this._isMouseOverNoneColorButton = true;
			this.UpdateNoneColorButtonBrushes();
		}
		#endregion //OnNoneColorButtonMouseEnter	
    
		#region UpdateNoneColorButtonBrushes
		private void UpdateNoneColorButtonBrushes()
		{
			if (null == this._noneColorButton)
				return;

			bool isMouseOver = this._isMouseOverNoneColorButton;
			bool isMouseDown = this._isLeftMouseButtonDownOverNoneColorButton;

			isMouseOver = this._noneColorButton.IsMouseOver;
			isMouseDown = Mouse.LeftButton == MouseButtonState.Pressed;


			bool isSelected	 = null == this.SelectedColor;

			// JM 02-24-11 TFS66905 - Always set the Forecolor so the text is visible.
			this.NoneColorButtonForegroundBrush = ScheduleUtilities.GetBrush(Colors.Black);

			if (false == isMouseOver)
			{
				if (isSelected)
				{
					this.NoneColorButtonBackgroundBrush = ScheduleUtilities.GetBrush(Color.FromArgb(255, 255, 189, 105));
					this.NoneColorButtonBorderBrush		= ScheduleUtilities.GetBrush(Color.FromArgb(255, 255, 171, 63));
					// JM 02-24-11 TFS66905
//					this.NoneColorButtonForegroundBrush = ScheduleUtilities.GetBrush(Colors.Black);
				}
				else
				{
					// AS 6/13/12 TFS105402
					// Technically we probably shouldn't make these colors hard coded but the default (non-selected/non-mouseover)
					// color should be transparent so the background shows through which previously was white but could be something 
					// else.
					//
					//this.NoneColorButtonBackgroundBrush = ScheduleUtilities.GetBrush(Colors.White);
					//this.NoneColorButtonBorderBrush		= ScheduleUtilities.GetBrush(Colors.White);
					this.NoneColorButtonBackgroundBrush = ScheduleUtilities.GetBrush(Colors.Transparent);
					this.NoneColorButtonBorderBrush		= ScheduleUtilities.GetBrush(Colors.Transparent);
					// JM 02-24-11 TFS66905
//					this.NoneColorButtonForegroundBrush = ScheduleUtilities.GetBrush(Colors.White);
				}
			}
			else
			{
				// JM 02-24-11 TFS66905
//				this.NoneColorButtonForegroundBrush = ScheduleUtilities.GetBrush(Colors.Black);

				if (isMouseDown)
				{
					this.NoneColorButtonBackgroundBrush = ScheduleUtilities.GetBrush(Color.FromArgb(255, 251, 140, 60));
					this.NoneColorButtonBorderBrush		= ScheduleUtilities.GetBrush(Color.FromArgb(255, 251, 140, 60));
				}
				else
				{
					if (isSelected)
					{
						this.NoneColorButtonBackgroundBrush = ScheduleUtilities.GetBrush(Color.FromArgb(255, 255, 171, 63));
						this.NoneColorButtonBorderBrush		= ScheduleUtilities.GetBrush(Color.FromArgb(255, 251, 140, 60));
					}
					else
					{
						this.NoneColorButtonBackgroundBrush = ScheduleUtilities.GetBrush(Color.FromArgb(255, 255, 231, 162));
						this.NoneColorButtonBorderBrush		= ScheduleUtilities.GetBrush(Color.FromArgb(255, 255, 189, 105));
					}
				}
			}
		}
		#endregion //UpdateNoneColorButtonBrushes	
    
		#region UpdateUIWithSelectedColor
		private void UpdateUIWithSelectedColor(Color? color)
		{
			// Update the ColorSwatch
			Brush borderBrush, backgroundBrush;
			if (false == color.HasValue)
			{
				backgroundBrush = ScheduleUtilities.GetBrush(ActivityCategoryPresenter.NullColorBackground);
				borderBrush		= ScheduleUtilities.GetBrush(ActivityCategoryPresenter.NullColorBorder);
			}
			else
			{
				backgroundBrush = ScheduleUtilities.GetActivityCategoryBrush(color.Value, ActivityCategoryBrushId.Background);
				borderBrush		= ScheduleUtilities.GetActivityCategoryBrush(color.Value, ActivityCategoryBrushId.Border);
			}
			this.SelectedColorSwatch = new Border
			{
				Width			= 15,
				Height			= 15,
				BorderBrush		= borderBrush,
				BorderThickness = new Thickness(1),
				Background		= backgroundBrush
			};


			this.UpdateNoneColorButtonBrushes();


			// If the color is not null, see if the color is a fixed color.  If so,
			// select the appropriate ColorWell
			if (color.HasValue)
			{
				for (int row = 0; row < 5; row++)
				{
					for (int col = 0; col < 5; col++)
					{
						if (_fixedColorsArray[row, col] == color.Value)
						{
							ColorWell colorWell = this.GetColorWell(row, col);
							if (null != colorWell)
							{
								this.SelectColorWell(colorWell, true, false);
								return;
							}
						}
					}
				}
			}


			if (null != this._selectedColorWell)
				this.SelectColorWell(this._selectedColorWell, false, false);
		}
		#endregion //UpdateUIWithSelectedColor

		#endregion //Private Methods

		#region Internal Methods

		#region SelectColorWell
		internal void SelectColorWell(ColorWell colorWell, bool select, bool closePopup)
		{
			if (null != this._selectedColorWell)
				this._selectedColorWell.IsSelected = false;

			colorWell.IsSelected = select;
			if (select)
			{
				this._selectedColorWell = colorWell;
				this.SelectedColor		= colorWell.Color;

				if (closePopup)
					this.ClosePopup();
			}
		}
		#endregion //SelectColorWell

		#endregion //Internal Methods

		#endregion //Methods
	}

	#region ColorWell Class
	internal class ColorWell : Grid
	{
		#region Member Variables
		
		private ActivityCategoryColorPicker						_colorPicker;
		private Border											_border;
		private bool											_isSelected;

		private static SolidColorBrush							SelectedBorderBrush = ScheduleUtilities.GetBrush(Color.FromArgb(255, 255, 171, 63));
		private static SolidColorBrush							TransparentBrush = ScheduleUtilities.GetBrush(Colors.Transparent);
		
		#endregion //Member Variables

		#region Constructor
		internal ColorWell(ActivityCategoryColorPicker colorPicker, Color borderColor, Color fillColor)
		{
			CoreUtilities.ValidateNotNull(colorPicker, "colorPicker");

			this._colorPicker	= colorPicker;
			this.Color			= fillColor;

			this._border					= new Border();
			this._border.BorderThickness	= new Thickness(2);
			this._border.BorderBrush		= TransparentBrush;


			this._border.Child = new Rectangle
			{
				Stroke	= ScheduleUtilities.GetBrush(borderColor),
				Fill	= ScheduleUtilities.GetBrush(fillColor),
				Width	= Math.Min(100, Math.Max(15, colorPicker.ColorSwatchExtent)),
				Height  = Math.Min(100, Math.Max(15, colorPicker.ColorSwatchExtent))
			};
			this._border.Child.MouseLeftButtonUp += new MouseButtonEventHandler(OnColorMouseLeftButtonUp);

			this.Children.Add(this._border);
		}
		#endregion //Constructor

		#region Event Handlers

		#region OnColorMouseLeftButtonUp
		void OnColorMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			this._colorPicker.SelectColorWell(this, true, true);
		}
		#endregion //OnColorMouseLeftButtonUp

		#endregion //EventHandlers

		#region Properties

		#region Color
		internal Color Color { get; private set; }
		#endregion //Color

		#region IsSelected
		internal bool IsSelected 
		{
			get { return this._isSelected; }
			set
			{
				this._isSelected = value;

				this._border.BorderBrush = (value == true ? SelectedBorderBrush : TransparentBrush); 
			}
		}
		#endregion //IsSelected

		#endregion //Properties
	}
	#endregion //ColorWell Class

	#region PopupManager Class


#region Infragistics Source Cleanup (Region)






















































































#endregion // Infragistics Source Cleanup (Region)

	#endregion //PopupManager Class

	#region DropDownToggleButton Class
	/// <summary>
	/// ToggleButton used in conjunction with a Popup to simulate dropdown functionality.  For internal use only.
	/// </summary>
	public class DropDownToggleButton : ToggleButton
	{
		#region Member Variables

		private bool					_ignoreNextMouseUp;
		private bool					_ignoreNextPopupEvent;
		private Popup					_popup;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Creates an instance of a custom ToggleButton used in conjunction with a Popup to simulate dropdown functionality.  For internal use only.
		/// </summary>
		public DropDownToggleButton()
		{
		}
		#endregion //Constructor

		#region Base Class Overrides

		#region OnMouseLeftButtonDown
		/// <summary>
		/// Called when the left mouse button is pressed.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			if (this.IsChecked.HasValue &&  this.IsChecked.Value && this.Popup.IsOpen == false)
				this.IsChecked = false;
			else
				base.OnMouseLeftButtonDown(e);

			if (this.Popup.IsOpen)
			{
				this._ignoreNextMouseUp = true;
				return;
			}
		}
		#endregion //OnMouseLeftButtonDown

		#region OnMouseLeftButtonUp
		/// <summary>
		/// Called when the left mouse button is released.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonUp(e);

			if (true == this._ignoreNextMouseUp)
			{
				this._ignoreNextMouseUp = false;



				return;
			}

			if (this.IsChecked.HasValue && this.Popup.IsOpen != this.IsChecked.Value)
			{
				this._ignoreNextPopupEvent	= true;
				this.Popup.IsOpen			= this.IsChecked.Value;
			}
		}
		#endregion //OnMouseLeftButtonUp

		#endregion //Base Class Overrides

		#region Properties

		#region Popup
		internal Popup Popup
		{
			get	{ return this._popup; }
			set
			{
				if (null != this._popup)
				{
					this._popup.Opened -= new EventHandler(OnPopupOpened);
					this._popup.Closed -= new EventHandler(OnPopupClosed);
				}

				this._popup			= value;
				this._popup.Opened += new EventHandler(OnPopupOpened);
				this._popup.Closed += new EventHandler(OnPopupClosed);
			}
		}
		#endregion //Popup

		#endregion //Properties

		#region Methods
    
		#region OnPopupClosed
		void OnPopupClosed(object sender, EventArgs e)
		{
			if (this._ignoreNextPopupEvent)
				this._ignoreNextPopupEvent = false;
			else
				if (false == this.IsChecked.HasValue || this.IsChecked.Value == true)
					this.IsChecked = false;
		}
		#endregion //OnPopupClosed	
    
		#region OnPopupOpened
		void OnPopupOpened(object sender, EventArgs e)
		{
			if (this._ignoreNextPopupEvent)
				this._ignoreNextPopupEvent = false;
			else
				if (false == this.IsChecked.HasValue || this.IsChecked.Value == false)
					this.IsChecked = true;
		}
		#endregion //OnPopupOpened	
    
		#endregion //Methods
	}
	#endregion //DropDownToggleButton class
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