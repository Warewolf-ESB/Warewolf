using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Provider;
using Infragistics.Controls.Layouts;
using System.Windows.Automation.Peers;
using System.Windows.Automation;
using System.Windows;
using Infragistics.Controls.Layouts.Primitives;
using System.Windows.Controls;

namespace Infragistics.AutomationPeers
{
	/// <summary>
	/// Exposes <see cref="ItemTileInfo"/> type to UI Automation
	/// </summary>
	public class TileItemAutomationPeer : AutomationPeerProxy,
		IExpandCollapseProvider,
		IScrollItemProvider,
		ITransformProvider
	{
		#region Private Members

		private ItemTileInfo _itemInfo;

		#endregion //Private Members	
 
		#region Constructor

		internal TileItemAutomationPeer(ItemTileInfo itemInfo)
		{
			_itemInfo = itemInfo;
		}

		#endregion //Constructor	
    
		#region Base class overrides

		#region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>ListItem</b> enumeration value</returns>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.ListItem;
		}

		#endregion //GetAutomationControlTypeCore

		#region GetClassNameCore
		/// <summary>
		/// Returns the class name
		/// </summary>
		/// <returns>A string that contains 'TileItem'</returns>
		protected override string GetClassNameCore()
		{
			return "TileItem";
		}

		#endregion //GetClassNameCore

		#region GetNameCore
		/// <summary>
		/// Returns the text label for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The text label</returns>
		protected override string GetNameCore()
		{
			string name = base.GetNameCore();

			if (String.IsNullOrEmpty(name))
			{
				object item = _itemInfo.Item;

				if (item == null)
					return name;

				if (item is string)
					return item as string;

				ContentControl cc = item as ContentControl;

				if (cc != null)
				{
					object content = cc.Content;

					if (content != null)
					{
						return content.ToString();
					}

				}
				return item.ToString();
			}

			return name;
		}
		#endregion //GetNameCore

		#region GetPattern
		/// <summary>
		/// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the <see cref="UIElement"/> that corresponds with the <see cref="ItemTileInfo"/> that is associated with this <see cref="TileItemAutomationPeer"/>.
		/// </summary>
		/// <param name="patternInterface">The pattern being requested</param>
		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.ExpandCollapse)
				return this;

			if (patternInterface == PatternInterface.Transform)
				return this;

			if (patternInterface == PatternInterface.ScrollItem)
				return this;

			return base.GetPattern(patternInterface);
		}
		#endregion //GetPattern

		#region GetUnderlyingPeer

		/// <summary>
		/// Returns the automation peer for which this proxy is associated.
		/// </summary>
		/// <returns>A <see cref="XamTileAutomationPeer"/></returns>
		protected override AutomationPeer GetUnderlyingPeer()
		{
			object item = _itemInfo.Item;

			if ( item == null )
				return null;

			XamTile tile = _itemInfo.LayoutManager.OwningManager.TileFromItem(item);

			AutomationPeer peer = null != tile
								? FrameworkElementAutomationPeer.CreatePeerForElement(tile)
								: null;

			if (null != peer)
				peer.EventsSource = this;

			return peer;
		}
		#endregion //GetUnderlyingPeer

		#region IsEnabledCore

		/// <summary>
		/// Returns true if the underlying item is enabled
		/// </summary>
		/// <returns></returns>
		protected override bool IsEnabledCore()
		{
			XamTileAutomationPeer tap = this.GetUnderlyingPeer() as XamTileAutomationPeer;

			if (tap != null)
				return tap.IsEnabled();

			return _itemInfo.Index >= 0 && false == _itemInfo.IsClosed;
		}

		#endregion //IsEnabledCore	
    
		#region IsOffscreenCore
		/// <summary>
		/// Returns a value that indicates whether the System.Windows.UIElement that corresponds with the object that is associated with this System.Windows.Automation.Peers.AutomationPeer is off the screen.
		/// </summary>
		protected override bool IsOffscreenCore()
		{
			// The element for this item may not be on screen but its children
			// in a flat view could so we'll base this on the bounding rectangle.
			//
			return this.GetBoundingRectangleCore().Equals(new Rect());
		}
		#endregion //IsOffscreenCore

		#endregion //Base class overrides

		#region Properties

		#region ItemInfo

		/// <summary>
		/// Returns the associated <see cref="ItemTileInfo"/> object (read-only).
		/// </summary>
		public ItemTileInfo ItemInfo { get { return _itemInfo; } }

		#endregion //ItemInfo	
    
		#endregion //Properties	
    
		#region Methods

		#region GetTilePeer

		internal XamTileAutomationPeer GetTilePeer()
		{
			return this.GetUnderlyingPeer() as XamTileAutomationPeer;
		}

		#endregion //GetTilePeer

		#endregion //Methods	
        
		#region IExpandCollapseProvider Members

		void IExpandCollapseProvider.Collapse()
		{
			if (this.IsEnabled() == false)
				throw new ElementNotEnabledException();

			if (((IExpandCollapseProvider)this).ExpandCollapseState == ExpandCollapseState.LeafNode)
				throw new InvalidOperationException();

			IExpandCollapseProvider tap = this.GetUnderlyingPeer() as IExpandCollapseProvider;

			if (tap == null)
			{
				_itemInfo.BringIntoView();
				_itemInfo.LayoutManager.OwningManager.UpdateLayout();
				tap = this.GetUnderlyingPeer() as IExpandCollapseProvider;
			}

			if (tap != null)
				tap.Collapse();
			else
				throw new ElementNotEnabledException();
		}

		void IExpandCollapseProvider.Expand()
		{
			if (this.IsEnabled() == false)
				throw new ElementNotEnabledException();

			if (((IExpandCollapseProvider)this).ExpandCollapseState == ExpandCollapseState.LeafNode)
				throw new InvalidOperationException();

			IExpandCollapseProvider tap = this.GetUnderlyingPeer() as IExpandCollapseProvider;

			if (tap == null)
			{
				_itemInfo.BringIntoView();
				_itemInfo.LayoutManager.OwningManager.UpdateLayout();
				tap = this.GetUnderlyingPeer() as IExpandCollapseProvider;
			}

			if (tap != null)
				tap.Expand();
			else
				throw new ElementNotEnabledException();

		}

		ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
		{
			get
			{
				IExpandCollapseProvider tap = this.GetUnderlyingPeer() as IExpandCollapseProvider;

				if (tap != null)
					return tap.ExpandCollapseState;

				if ( _itemInfo.IsClosed || 
					 _itemInfo.IsMaximized || 
					 false == _itemInfo.LayoutManager.OwningManager.IsInMaximizedMode)
					return ExpandCollapseState.LeafNode;

				if (_itemInfo.IsExpandedWhenMinimizedResolved )
					return ExpandCollapseState.Expanded;
				else
					return ExpandCollapseState.Collapsed;
			}
		}

		#endregion

		#region IScrollItemProvider

		void IScrollItemProvider.ScrollIntoView()
		{
			_itemInfo.BringIntoView();
		}

		#endregion //IScrollItemProvider

		#region ITransformProvider Members

		bool ITransformProvider.CanMove
		{
			get
			{
				ITransformProvider tap = this.GetUnderlyingPeer() as ITransformProvider;

				if (tap != null)
					return tap.CanMove;

				return false;
			}
		}

		bool ITransformProvider.CanResize
		{
			get
			{
				ITransformProvider tap = this.GetUnderlyingPeer() as ITransformProvider;

				if (tap != null)
					return tap.CanResize;

				return false;
			}
		}

		bool ITransformProvider.CanRotate
		{
			get
			{
				return false;
			}
		}

		void ITransformProvider.Move(double x, double y)
		{
			if (this.IsEnabled() == false)
				throw new ElementNotEnabledException();
			
			ITransformProvider tap = this.GetUnderlyingPeer() as ITransformProvider;

			if (tap != null)
				tap.Move(x, y);
		}

		void ITransformProvider.Resize(double width, double height)
		{
			if (this.IsEnabled() == false)
				throw new ElementNotEnabledException();

			ITransformProvider tap = this.GetUnderlyingPeer() as ITransformProvider;

			if (tap != null)
				tap.Resize(width, height);

		}

		void ITransformProvider.Rotate(double degrees)
		{
			throw new InvalidOperationException();
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