using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;
using System.Windows;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Value converter for converting a <see cref="Color"/> to a <see cref="Brush"/> that may be used for a <see cref="RibbonTabItem"/> that belongs to a <see cref="ContextualTabGroup"/>.
	/// </summary>
	public class ContextualTabBaseColorToHoverBrushConverter : IValueConverter
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ContextualTabBaseColorToHoverBrushConverter"/>
		/// </summary>
		public ContextualTabBaseColorToHoverBrushConverter()
		{
		}
		#endregion //Constructor

		#region IValueConverter Members

		/// <summary>
		/// Converts a <see cref="Color"/> to a <see cref="Brush"/> based upon the specified theme name which is indicated via the <paramref name="parameter"/>.
		/// </summary>
		/// <param name="value">The base color for the contextual tab group that contains the tab item.</param>
		/// <param name="targetType">The type the converter is to create. This must be a <see cref="Brush"/>.</param>
		/// <param name="parameter">The name of the theme for which the converter is being used. This must be Blue, Black or Silver. If some other value is specified, Blue will be used instead.</param>
		/// <param name="culture">This parameter is not used.</param>
		/// <returns>A </returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// targetType must be a brush
			if (targetType == null || typeof(Brush).IsAssignableFrom(targetType) == false)
				return DependencyProperty.UnsetValue;

			// the value must be a color
			if (value is Color == false)
				return DependencyProperty.UnsetValue;

			string themeName = parameter as string ?? "Blue";

			// Initialize GradientStop Colors
			Color baseColor = (Color)value;

			switch (themeName)
			{
				case "Black" :	// Same as Silver
				case "Silver" :
					LinearGradientBrush silverBrush = GenerateSilverBrush(baseColor);
					return silverBrush;						

				case "Blue":						
				default :
					RadialGradientBrush blueBrush = GenerateBlueBrush(baseColor);
					return blueBrush;
			}							
		}

		#region ConvertBack
		/// <summary>
		/// This method is not implemented for this converter.
		/// </summary>
		/// <returns><see cref="Binding.DoNothing"/> is always returned.</returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Binding.DoNothing;
		} 
		#endregion //ConvertBack

		#endregion //IValueConverter Members

		#region Methods

		#region GenerateBlueBrush
		private RadialGradientBrush GenerateBlueBrush(Color baseColor)
		{
			RadialGradientBrush rgb = new RadialGradientBrush();

			// Initialize GradientStop Colors
			Color startColor = baseColor;
			Color endColor = baseColor;

			// Set Alpha Values
			startColor.A = 0;
			endColor.A = 190;

			// Define Gradient Stops
			GradientStop stop1 = new GradientStop(startColor, 0.332);
			GradientStop stop2 = new GradientStop(endColor, 1);

			// Add Stops
			rgb.GradientStops.Add(stop1);
			rgb.GradientStops.Add(stop2);

			// Define Gradient Transform
			ScaleTransform scaleTransform = new ScaleTransform(2.083, 2.06, 0.5, 0.5);
			TranslateTransform translateTransform = new TranslateTransform(-0.008, -0.371);

			TransformGroup transformGroup = new TransformGroup();
			transformGroup.Children.Add(scaleTransform);
			transformGroup.Children.Add(translateTransform);

			rgb.RelativeTransform = transformGroup;
			return rgb;
		}
		#endregion GenerateBlueBrush

		#region GenerateSilverBrush
		private LinearGradientBrush GenerateSilverBrush(Color baseColor)
		{
			LinearGradientBrush lgb = new LinearGradientBrush();
			lgb.StartPoint = new System.Windows.Point(0.5, -0.004);
			lgb.EndPoint = new System.Windows.Point(0.5, 1);

			// Initialize GradientStop Colors
			Color color1 = Color.FromArgb(38, 255, 255, 255);
			Color color2 = baseColor;
			Color color3 = baseColor;

			// Set Alpha Values
			color2.A = 69;
			color3.A = 150;

			// Define Gradient Stops
			GradientStop stop1 = new GradientStop(color1, 0);
			GradientStop stop2 = new GradientStop(color2, 0.477);
			GradientStop stop3 = new GradientStop(color3, 1);

			// Add Stops
			lgb.GradientStops.Add(stop1);
			lgb.GradientStops.Add(stop2);
			lgb.GradientStops.Add(stop3);

			return lgb;
		}
		#endregion GenerateSilverBrush

		#endregion //Methods
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