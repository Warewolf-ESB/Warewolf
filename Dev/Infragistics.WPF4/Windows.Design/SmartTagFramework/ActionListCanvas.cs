using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Infragistics.Windows.Design.SmartTagFramework
{
	/// <summary>
	/// Area where the current ActionListPage is displayed and where old/new pages are animated into and out of view
	/// </summary>
	public class ActionListCanvas : Canvas
	{
		/// <summary>
		/// Arranges the content of an element.
		/// </summary>
		/// <returns>
		/// A Size that represents the arranged size of this element and its descendants.
		/// </returns>
		/// <param name="arrangeSize">
		/// The size that this element should use to arrange its child elements.
		/// </param>
		protected override Size ArrangeOverride(Size arrangeSize)
		{
		    foreach (UIElement element in base.InternalChildren)
		    {
		        if (element == null)
		            continue;

		        double x	= 0.0;
		        double y	= 0.0;
		        double left = GetLeft(element);
		        if (!double.IsNaN(left))
		            x = left;
		        else
		        {
		            double right = GetRight(element);
		            if (!double.IsNaN(right))
		                x = (arrangeSize.Width - element.DesiredSize.Width) - right;
		        }

		        double top = GetTop(element);
		        if (!double.IsNaN(top))
		            y = top;
		        else
		        {
		            double bottom = GetBottom(element);
		            if (!double.IsNaN(bottom))
		                y = (arrangeSize.Height - element.DesiredSize.Height) - bottom;
		        }

		        element.Arrange(new Rect(new Point(x, y), new Size(arrangeSize.Width, element.DesiredSize.Height)));
		    }
		    return arrangeSize;
		}

		/// <summary>
		/// MeasureOverride
		/// </summary>
		/// <param name="constraint"></param>
		/// <returns></returns>
		protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
		{
			double maxWidth		= 0;
			double maxHeight	= 0;

			foreach (UIElement child in this.Children)
			{
				if (child.IsEnabled)
				{
					child.Measure(constraint);

					if (child.DesiredSize.Width > maxWidth)
						maxWidth = child.DesiredSize.Width;

					if (child.DesiredSize.Height > maxHeight)
						maxHeight = child.DesiredSize.Height;
				}
			}

			if (double.IsInfinity(maxWidth) || double.IsInfinity(maxHeight))
				return base.MeasureOverride(constraint);
			else
				return new Size(maxWidth, maxHeight);
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