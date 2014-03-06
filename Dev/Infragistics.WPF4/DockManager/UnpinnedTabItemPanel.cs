using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using System.Diagnostics;


namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// A custom panel used to arrange tab items within an <see cref="UnpinnedTabArea"/>
	/// </summary>
	//[ToolboxItem(false)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class UnpinnedTabItemPanel : Panel
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="UnpinnedTabItemPanel"/>
		/// </summary>
		public UnpinnedTabItemPanel()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			Rect tabRect = new Rect(finalSize);
			UIElementCollection children = this.Children;
			Dock dock = this.TabStripPlacement;
			bool isHorizontal = dock == Dock.Top || dock == Dock.Bottom;

			for (int i = 0, count = children.Count; i < count; i++)
			{
				UIElement element = children[i];

				if (isHorizontal)
					tabRect.Width = element.DesiredSize.Width;
				else // vertical
					tabRect.Height = element.DesiredSize.Height;

				element.Arrange(tabRect);

				if (isHorizontal)
					tabRect.X += tabRect.Width;
				else
					tabRect.Y += tabRect.Height;
			}

			return finalSize;
		}

		#endregion //ArrangeOverride

		#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			bool isHorizontal = this.IsHorizontal;
			UIElementCollection children = this.Children;
			Size childMeasureSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
			double desiredExtent = 0;
			double desiredHeight = 0;
			IContentPaneContainer currentGroup = null;

			for (int i = 0, count = children.Count; i < count; i++)
			{
				UIElement child = children[i];

				if (child.Visibility == Visibility.Collapsed)
					continue;

				PaneTabItem tab = child as PaneTabItem;

				Debug.Assert(null != tab);

				ContentPane pane = null != tab ? tab.Pane : null;
				IContentPaneContainer childGroup = null != pane ? pane.PlacementInfo.DockedContainer : null;

				child.SetValue(IsFirstInGroupPropertyKey, childGroup != currentGroup ? KnownBoxes.TrueBox : DependencyProperty.UnsetValue);
				currentGroup = childGroup;

				child.Measure(childMeasureSize);
				Size childSize = child.DesiredSize;

				double childExtent = isHorizontal ? childSize.Width : childSize.Height;
				double childHeight = isHorizontal ? childSize.Height : childSize.Width;

				desiredExtent += childExtent;
				desiredHeight = Math.Max(childHeight, desiredHeight);
			}

			Size desiredSize;

			if (isHorizontal)
				desiredSize = new Size(desiredExtent, desiredHeight);
			else
				desiredSize = new Size(desiredHeight, desiredExtent);

			return desiredSize;
		}

		#endregion //MeasureOverride

		#region OnVisualChildrenChanged
		/// <summary>
		/// Invoked when a child element has been added or removed.
		/// </summary>
		/// <param name="visualAdded">Visual being added</param>
		/// <param name="visualRemoved">Visual being removed</param>
		protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
		{
			base.OnVisualChildrenChanged(visualAdded, visualRemoved);

			if (null != visualRemoved)
				visualRemoved.ClearValue(IsFirstInGroupPropertyKey);
		} 
		#endregion //OnVisualChildrenChanged

		#endregion //Base class overrides

		#region Properties

		#region Private Properties

		#region IsHorizontal
		private bool IsHorizontal
		{
			get
			{
				switch (this.TabStripPlacement)
				{
					default:
					case Dock.Bottom:
					case Dock.Top:
						return true;
					case Dock.Left:
					case Dock.Right:
						return false;
				}
			}
		}
		#endregion //IsHorizontal

		#endregion //Private Properties

		#region Public Properties

		#region IsFirstInGroup

		private static readonly DependencyPropertyKey IsFirstInGroupPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("IsFirstInGroup",
			typeof(bool), typeof(UnpinnedTabItemPanel), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the IsFirstInGroup" attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetIsFirstInGroup"/>
		public static readonly DependencyProperty IsFirstInGroupProperty =
			IsFirstInGroupPropertyKey.DependencyProperty;


		/// <summary>
		/// Gets the value of the 'IsFirstInGroup' attached readonly property
		/// </summary>
		/// <seealso cref="IsFirstInGroupProperty"/>
		public static bool GetIsFirstInGroup(DependencyObject d)
		{
			return (bool)d.GetValue(UnpinnedTabItemPanel.IsFirstInGroupProperty);
		}

		#endregion //IsFirstInGroup

		#region TabStripPlacement

		/// <summary>
		/// Identifies the <see cref="TabStripPlacement"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TabStripPlacementProperty = TabControl.TabStripPlacementProperty.AddOwner(typeof(UnpinnedTabItemPanel), new FrameworkPropertyMetadata(KnownBoxes.DockTopBox, FrameworkPropertyMetadataOptions.AffectsMeasure));

		/// <summary>
		/// Determines the orientation and placement of the tab items.
		/// </summary>
		/// <seealso cref="TabStripPlacementProperty"/>
		//[Description("Determines the orientation and placement of the tab items.")]
		//[Category("DockManager Properties")] // Layout
		[Bindable(true)]
		public Dock TabStripPlacement
		{
			get
			{
				return (Dock)this.GetValue(DocumentTabPanel.TabStripPlacementProperty);
			}
			set
			{
				this.SetValue(DocumentTabPanel.TabStripPlacementProperty, value);
			}
		}

		#endregion //TabStripPlacement

		#endregion //Public Properties

		#endregion //Properties

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