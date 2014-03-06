using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Data;
using System.Windows.Markup;
using System.ComponentModel;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// An element that is used to represent a single key tip for an item within the <see cref="XamRibbon"/>
	/// </summary>
	/// <remarks>
	/// <p class="body">The KeyTip is used to represent the key tip of a single item (e.g. a <see cref="ButtonTool.KeyTip"/>) within the 
	/// <see cref="XamRibbon"/>. Keytips are displayed by the Ribbon when you press and release the <b>Alt</b> key and are used to navigate within the 
	/// Ribbon by typing the character(s) of the keytip when they are visible.</p>
	/// </remarks>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class KeyTip : Control
	{
		#region Member Variables

		private KeyTipManager _keyTipManager;
		private Rect _rect = Rect.Empty;
		private IKeyTipProvider _provider;
		// AS 12/19/07 BR29204
		//private string _value;
		private const string DefaultKeyTip = "O";
		private string _value = DefaultKeyTip;

		#endregion Member Variables

		#region Constructor

		static KeyTip()
		{
			KeyTip.DefaultStyleKeyProperty.OverrideMetadata(typeof(KeyTip), new FrameworkPropertyMetadata(typeof(KeyTip)));
			FrameworkElement.IsHitTestVisibleProperty.OverrideMetadata(typeof(KeyTip), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
			Control.HorizontalContentAlignmentProperty.OverrideMetadata(typeof(KeyTip), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentCenterBox));
			Control.VerticalContentAlignmentProperty.OverrideMetadata(typeof(KeyTip), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentCenterBox));

			// AS 10/9/07
			// Keytips should not receive focus.
			//
			UIElement.FocusableProperty.OverrideMetadata(typeof(KeyTip), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
		}

		internal KeyTip(KeyTipManager keyTipManager, IKeyTipProvider provider)
		{
			this._keyTipManager = keyTipManager;
			this._provider = provider;
			this.IsEnabled = provider.IsEnabled;

			// AS 12/19/07 BR29204
			// The default value is "O" so if a tool wants to use that character
			// we will not get a property change notification and therefore the 
			// _value will not get set - which is what is returned from the Value
			// property. So now _value starts off as "O" but we actually need to 
			// start with null for the internally created ones or everything will 
			// have a keytip starting with "O" if not specifically set.
			//
			this.Value = null;
		}

		/// <summary>
		/// Initializes a new <see cref="KeyTip"/>. This constructor is provided for styling purposes and is not used at runtime.
		/// </summary>
		public KeyTip()
		{
		}

		#endregion Constructor

		#region Methods

		#region Public Methods

		#endregion Public Methods

		#region Static Methods

		#region GetRect

		private static Rect GetRect(Point location, Size size, KeyTipAlignment alignment)
		{
			Point rectLocation = new Point();
			int intAlignment = (int)alignment;

			if ((intAlignment & 0xF) != 0)	// TopXXX
				rectLocation.Y = location.Y;
			else if ((intAlignment & 0xF0) != 0) //MiddleXXX
			{
				rectLocation.Y = location.Y - (size.Height / 2);
				intAlignment >>= 4;
			}
			else
			{
				Debug.Assert((intAlignment & 0xF00) != 0);
				rectLocation.Y = location.Y - size.Height;
				intAlignment >>= 8;
			}

			int horzAlign = (int)intAlignment & 0xF;

			if (horzAlign == 0x1) // Left
				rectLocation.X = location.X;
			else if (horzAlign == 0x2) // Center
				rectLocation.X = location.X - (size.Width / 2);
			else
			{
				Debug.Assert(horzAlign == 0x4); // Right
				rectLocation.X = location.X - size.Width;
			}

			return new Rect(rectLocation, size);
		}

		#endregion GetRect

		#endregion Static Methods

		#endregion Methods

		#region Properties

		#region Internal

		#region IsKeyTipVisible

		internal bool IsKeyTipVisible
		{
			get
			{
				if (this._provider.IsVisible == false)
					return false;

				if (string.IsNullOrEmpty(this._value))
					return false;

				return this._value.StartsWith(this._keyTipManager.CurrentKeyTipValue);
			}
		}

		#endregion IsKeyTipVisible

		#region Provider

		internal IKeyTipProvider Provider
		{
			get { return this._provider; }
		}

		#endregion Provider

		#region Rect

		internal Rect Rect
		{
			get
			{
				if (this._rect.IsEmpty || this.IsMeasureValid == false)
				{
					this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

					this._rect = KeyTip.GetRect(this.Provider.Location, this.DesiredSize, this.Provider.Alignment);

					Adorner parent = VisualTreeHelper.GetParent(this) as Adorner;

					Debug.Assert(parent != null);

					if (null != parent)
					{
						// AS 3/23/11 TFS69892
						//this._rect = this.Provider.AdornedElement.TransformToVisual(parent.AdornedElement).TransformBounds(this._rect);
						_rect = XamRibbon.TryTransformToVisual(this.Provider.AdornedElement, parent.AdornedElement, _rect) ?? new Rect();
					}
				}

				return this._rect;
			}
		}

		#endregion Rect

		#endregion //Internal

		#region Public

		#region Value

		/// <summary>
		/// Identifies the <see cref="Value"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value",
			// AS 12/19/07 BR29204
			//typeof(string), typeof(KeyTip), new FrameworkPropertyMetadata("O", FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnValueChanged), new CoerceValueCallback(CoerceValue)));
			typeof(string), typeof(KeyTip), new FrameworkPropertyMetadata(DefaultKeyTip, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnValueChanged), new CoerceValueCallback(CoerceValue)));

		private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			KeyTip tip = d as KeyTip;

			if (tip != null)
				tip._value = (string)e.NewValue;
		}

		private static object CoerceValue(DependencyObject d, object newValue)
		{
			string strValue = newValue as String;

			if (strValue == null)
				return string.Empty;

			CultureInfo culture = CultureInfo.CurrentCulture;
			XmlLanguage lang = d.GetValue(FrameworkElement.LanguageProperty) as XmlLanguage;

			if (null != lang)
			{
				try
				{
					culture = lang.GetEquivalentCulture();
				}
				catch (InvalidOperationException)
				{
				}
			}

			return strValue.ToUpper(culture);
		}

		/// <summary>
		/// Returns/sets the value of the keytip
		/// </summary>
		/// <remarks>
		/// <p class="body">The Value property is the string to be displayed within the element and represents the text that must be 
		/// typed in order to activate the keytip and initiate the action of the associated object. For example, typing in the keytip of a <see cref="ButtonTool"/> 
		/// will trigger its <see cref="System.Windows.Controls.Primitives.ButtonBase.Click"/> event.</p>
		/// <p class="note">The KeyTip class itself does not invoke any actions or respond to the keyboard. The <see cref="XamRibbon"/> creates and 
		/// positions instance of the KeyTip class to represent the text keytip (e.g. <see cref="ButtonTool.KeyTip"/>) of the elements within the ribbon.</p>
		/// </remarks>
		/// <seealso cref="ValueProperty"/>
		//[Description("Returns/sets the value of the keytip")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public string Value
		{
			get
			{
				return this._value;
			}
			set
			{
				this.SetValue(KeyTip.ValueProperty, value);
			}
		}

		#endregion //Value

		#endregion //Public		

		#endregion Properties
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