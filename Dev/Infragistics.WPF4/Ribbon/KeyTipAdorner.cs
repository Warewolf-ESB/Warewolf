using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;
using System.Windows;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.Ribbon
{





	internal class KeyTipAdorner : Adorner
	{
		#region Member Variables

		private List<KeyTip> _keyTips;

		#endregion //Member Variables

		#region Constructor
		internal KeyTipAdorner(UIElement adornedElement)
			: base(adornedElement)
		{
			this._keyTips = new List<KeyTip>();
		}
		#endregion //Constructor

		#region Base class overrides

		#region ArrangeOverride
		protected override Size ArrangeOverride(Size finalSize)
		{
			Rect arrangeRect = new Rect(finalSize);

			foreach (KeyTip keyTip in this._keyTips)
			{
				Rect keyTipRect = keyTip.Rect;

				// adjust the rects so they are in the arrange area
				if (keyTipRect.Bottom > arrangeRect.Bottom)
					keyTipRect.Y -= keyTipRect.Bottom - arrangeRect.Bottom;

				if (keyTipRect.Y < arrangeRect.Top)
					keyTipRect.Y += arrangeRect.Top - keyTipRect.Y;

				// AS 10/24/07 BR27830
				// adjust the rects so they are in the arrange area
				if (keyTipRect.Right > arrangeRect.Right)
					keyTipRect.X -= keyTipRect.Right - arrangeRect.Right;

				if (keyTipRect.X < arrangeRect.Left)
					keyTipRect.X += arrangeRect.Left - keyTipRect.Left;

				keyTip.Arrange(keyTipRect);
			}

			return finalSize;
		}
		#endregion //ArrangeOverride

		#region GetVisualChild
		protected override System.Windows.Media.Visual GetVisualChild(int index)
		{
			if (index < 0 || index >= this._keyTips.Count)
				throw new ArgumentOutOfRangeException();

			return this._keyTips[index];
		}
		#endregion //GetVisualChild

		#region MeasureOverride
		protected override Size MeasureOverride(Size constraint)
		{
			// AS 11/28/07 BR28752
			// Use the render size since the desired size may be smaller.
			//
			//return this.AdornedElement.DesiredSize;
			return this.AdornedElement.RenderSize;
		}
		#endregion //MeasureOverride

		#region VisualChildrenCount
		protected override int VisualChildrenCount
		{
			get
			{
				return this._keyTips.Count;
			}
		}
		#endregion //VisualChildrenCount

		#endregion //Base class overrides

		#region Methods

		#region Add
		internal void Add(KeyTip keyTip)
		{
			this._keyTips.Add(keyTip);
			this.AddVisualChild(keyTip);
		}
		#endregion //Add

		#region RefreshKeyTipVisibility
		internal void RefreshKeyTipVisibility()
		{
			foreach (KeyTip keyTip in this._keyTips)
			{
				if (keyTip.IsKeyTipVisible)
					keyTip.ClearValue(KeyTip.VisibilityProperty);
				else
					keyTip.SetValue(KeyTip.VisibilityProperty, KnownBoxes.VisibilityCollapsedBox);
			}
		}
		#endregion //RefreshKeyTipVisibility

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