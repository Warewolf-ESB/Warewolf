using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Infragistics.Windows.Editors
{
	/// <summary>
	/// Custom <see cref="VirtualizingStackPanel"/>
	/// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class VirtualizingStackPanelEx : VirtualizingStackPanel
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="VirtualizingStackPanelEx"/>
		/// </summary>
		public VirtualizingStackPanelEx()
		{
		} 
		#endregion //Constructor

		#region Base class overrides

		#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			double viewPortExtentBefore = this.ScrollOwner == null ? 0 : (this.Orientation == Orientation.Vertical ? this.ViewportHeight : this.ViewportWidth);

			Size size = base.MeasureOverride(availableSize);

			double viewPortExtentAfter = this.ScrollOwner == null ? 0 : (this.Orientation == Orientation.Vertical ? this.ViewportHeight : this.ViewportWidth);

			if (!CoreUtilities.AreClose(viewPortExtentBefore, viewPortExtentAfter))
			{
				UIElement parent = VisualTreeHelper.GetParent(this) as UIElement;

				// AS 8/25/11 TFS83698
				// At least in this scenario what was happening is that the arrange of the combobox 
				// and all of its descendants (within the popup anyway down to the ItemsPresenter) 
				// were being invalidated (I believe due to the Foreground/SnapsToDevicePixels) 
				// being changed. When the items were added the VSP correctly had its measure 
				// invalidated and also correctly the parent's measure was not (since it may not 
				// need a resize since it may already be filling the available viewport). However 
				// when the popup was opened next and the HwndSource called the SetLayoutSize, that 
				// routine called Measure and Arrange on the root visual. Again this is ok but in 
				// this case since the arrange was invalidated down the chain it got all the way 
				// to the ItemsPresenter's Arrange. When it tried to arrange the VSP within it 
				// the UIElement Arrange method first called Measure on the VSP because its 
				// measure was invalid. The VSP correctly returned a larger desired size and 
				// while that was stored, the OnChildDesiredSizeChanged of the parent was not 
				// invoked because WPF specifically will not do that when the Measure is called 
				// from within the Arrange so the measure of the parent ItemsPresenter was not 
				// invalidated and so the element remained the same size. To try and get around 
				// this issue in the framework we can explicitly invalidate the measure of the 
				// parent ItemsPresenter when the ViewportSize changes. 
				// 
				if (parent != null && parent.IsMeasureValid)
					parent.InvalidateMeasure();
			}

			return size;
		}

		#endregion // MeasureOverride

		#endregion //Base class overrides
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