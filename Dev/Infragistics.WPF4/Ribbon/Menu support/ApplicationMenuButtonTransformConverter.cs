using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Converter used by the <see cref="ApplicationMenuPresenter"/> style to transform the location of an element in the popup to overlay the button in the ribbon header.
	/// </summary>
	public class ApplicationMenuButtonTransformConverter : IMultiValueConverter
	{
		#region IMultiValueConverter Members

		object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			bool isOpen = false;

			if (values == null || values.Length != 3 || targetType != typeof(Transform))
				return DependencyProperty.UnsetValue;

			if (values[0] is bool)
				isOpen = (bool)values[0];
			else
				return DependencyProperty.UnsetValue;

			// JM BR28857 12-4-07
			//if (isOpen == false)
			//    return DependencyProperty.UnsetValue;

			FrameworkElement baseButton = values[1] as FrameworkElement;
			FrameworkElement overlayButton = values[2] as FrameworkElement;

			if (baseButton == null ||
				 overlayButton == null)
				return DependencyProperty.UnsetValue;

			// AS 12/7/07 RightToLeft
			// I noticed while testing for RLT (although its not specific to RTL) that 
			// this code would cause us to always use the original transform and therefore
			// whenever the app menu was shifted horizontally differently (because it was
			// or is intersecting with the screen), the button would be in the wrong spot.
			//
			//// JM BR28857 12-4-07
			//if (overlayButton.RenderTransform != Transform.Identity)
			//	return overlayButton.RenderTransform;

			// JM BR28857 12-4-07
			if (isOpen == false && (baseButton.IsInitialized == false || baseButton.IsLoaded == false))
				return overlayButton.RenderTransform;

            // JJD 11/06/07 - Call the Utilities.Point...Safe methods so we don't get an exception throw
            // in XBAP semi-trust applications
            //Point pt = baseButton.PointToScreen(new Point());
			//pt = overlayButton.PointFromScreen(pt);
			// AS 6/23/08 BR33893
			// Its possible that the baseButton is not in the visual tree.
			// In that case, don't return a value.
			//
			//Point pt = Utilities.PointToScreenSafe( baseButton, new Point());
			Point pt;

			try
			{
				pt = Utilities.PointToScreenSafe(baseButton, new Point());
			}
			catch (InvalidOperationException)
			{
				return DependencyProperty.UnsetValue; 
			}

			// AS 12/7/07 RightToLeft
			// Do not clear the transform since this is the value converter that will be setting
			// the transform.
			//
			//// JM BR28857 12-4-07
			//overlayButton.ClearValue(FrameworkElement.RenderTransformProperty);
			pt = Utilities.PointFromScreenSafe( overlayButton, pt);

			// AS 12/7/07 RightToLeft
			// Instead of bailing out above shift pull out the adjustment caused by the render transform.
			//
			if (overlayButton.RenderTransform != Transform.Identity)
			{
				pt = overlayButton.RenderTransform.Transform(pt);
			}

			TranslateTransform transform = new TranslateTransform(pt.X, pt.Y);

			return transform;
		}

		object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			return new object[] { Binding.DoNothing };
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