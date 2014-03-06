using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Provider;
using Infragistics.Controls.Layouts;
using System.Windows.Automation.Peers;
using System.Windows.Automation;
using System.Windows;
using Infragistics.Controls.Layouts.Primitives;
using Infragistics.Controls;

namespace Infragistics.AutomationPeers
{
    /// <summary>
    /// Exposes <see cref="XamTile"/> type to UI Automation
    /// </summary>
    public class XamTileAutomationPeer : FrameworkElementAutomationPeer,
        IExpandCollapseProvider,
        ITransformProvider,
        IScrollItemProvider
    {
		#region Member Variables

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Creates a new instance of the <see cref="XamTileAutomationPeer"/> class
		/// </summary>
		/// <param name="tile">The <see cref="XamTile"/> for which the peer is being created</param>
        public XamTileAutomationPeer(XamTile tile)
			: base(tile)
		{
		}

		#endregion //Constructor

		#region Base class overrides

		#region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>DataItem</b> enumeration value</returns>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
    		return AutomationControlType.ListItem;
		}

		#endregion //GetAutomationControlTypeCore

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="XamTile"/>
		/// </summary>
		/// <returns>A string that contains 'Tile'</returns>
		protected override string GetClassNameCore()
		{
			return "Tile";
		}

		#endregion //GetClassNameCore

		#region GetPattern
		/// <summary>
		/// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the <see cref="UIElement"/> that corresponds with the <see cref="XamTile"/> that is associated with this <see cref="XamTileAutomationPeer"/>.
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

		#region IsOffscreenCore
		/// <summary>
		/// Returns a value that indicates whether the System.Windows.UIElement that corresponds with the object that is associated with this System.Windows.Automation.Peers.AutomationPeer is off the screen.
		/// </summary>
		protected override bool IsOffscreenCore()
		{
			// AS 9/1/09
			// The element for this record may not be on screen but its children
			// in a flat view could so we'll base this on the bounding rectangle.
			//
			return this.GetBoundingRectangleCore().Equals(new Rect());
		} 
		#endregion //IsOffscreenCore

		#endregion //Base class overrides

        #region IExpandCollapseProvider Members

        void IExpandCollapseProvider.Collapse()
        {
            if (this.IsEnabled() == false)
                throw new ElementNotEnabledException();

            if (((IExpandCollapseProvider)this).ExpandCollapseState == ExpandCollapseState.LeafNode)
                throw new InvalidOperationException();

            XamTile tile = (XamTile)this.Owner;

            if (tile.State != TileState.Minimized && 
				tile.CanExecuteCommand(TileCommandType.ToggleMinimizedExpansion, tile))
                tile.ExecuteCommand(TileCommandType.ToggleMinimizedExpansion, null, tile);
        }

        void IExpandCollapseProvider.Expand()
        {
            if (this.IsEnabled() == false)
                throw new ElementNotEnabledException();

            if (((IExpandCollapseProvider)this).ExpandCollapseState == ExpandCollapseState.LeafNode)
                throw new InvalidOperationException();

            XamTile tile = (XamTile)this.Owner;

            if (tile.State != TileState.MinimizedExpanded &&
				tile.CanExecuteCommand(TileCommandType.ToggleMinimizedExpansion, tile))
				tile.ExecuteCommand(TileCommandType.ToggleMinimizedExpansion, null, tile);
		}

        ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
        {
            get 
            { 
                XamTile tile = (XamTile)this.Owner;

                if ( tile.ExpandButtonVisibilityResolved == Visibility.Collapsed || this.IsEnabled() == false)
                    return ExpandCollapseState.LeafNode;

                TileState state = tile.State;

                switch (state)
                {
                    default:
                        return ExpandCollapseState.LeafNode;

                    case TileState.MinimizedExpanded:
                        return ExpandCollapseState.Expanded;

                    case TileState.Minimized:
                        return ExpandCollapseState.Collapsed;
                }
            }
        }

        #endregion

        #region IScrollItemProvider

        void IScrollItemProvider.ScrollIntoView()
        {
            XamTile tile = (XamTile)this.Owner;

			XamTileManager tm = tile.Manager;

			if (tm != null)
			{
				object item = tm.ItemFromTile(tile);

				if (item != null)
					tm.ScrollIntoView(item);
			}
        }

        #endregion //IScrollItemProvider

        #region ITransformProvider Members

        bool ITransformProvider.CanMove
        {
            get 
            {
                if (this.IsEnabled() == false)
                    return false;

                XamTile tile = this.Owner as XamTile;

                XamTileManager tm = tile.Manager;

                return tm != null && tm.CanDragTiles; 
            }
        }

        bool ITransformProvider.CanResize
        {
            get 
            {
                if (this.IsEnabled() == false)
                    return false;

                XamTile tile = this.Owner as XamTile;

                TileAreaPanel panel = tile.Panel;

                if (panel == null)
                    return false;

                if (tile.Manager.IsInMaximizedMode)
                    return false;

                return panel.NormalModeSettingsSafe.AllowTileSizing != AllowTileSizing.No;
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

            if (!((ITransformProvider)this).CanMove)
                throw new InvalidOperationException(TileUtilities.GetString("LE_CannotPerformAutomationOperation"));

            if (double.IsInfinity(x) || double.IsNaN(x))
                throw new ArgumentOutOfRangeException("x");

            if (double.IsInfinity(y) || double.IsNaN(y))
                throw new ArgumentOutOfRangeException("y");

            XamTile tile = this.Owner as XamTile;

			TileAreaPanel panel = tile.Panel;

			if ( panel != null )
				panel.MoveTileHelper(tile, tile.Manager.ItemFromTile(tile), new Point(x, y));
        }

        void ITransformProvider.Resize(double width, double height)
        {
             if (this.IsEnabled() == false)
                throw new ElementNotEnabledException();

            if (!((ITransformProvider)this).CanResize)
                throw new InvalidOperationException(TileUtilities.GetString("LE_CannotPerformAutomationOperation"));

			CoreUtilities.ValidateIsNotInfinity(width ); 	
			CoreUtilities.ValidateIsNotNan(width); 	
			CoreUtilities.ValidateIsNotNegative(width); 
	
			CoreUtilities.ValidateIsNotInfinity(height ); 	
			CoreUtilities.ValidateIsNotNan(height); 	
			CoreUtilities.ValidateIsNotNegative(height); 	

            XamTile tile = this.Owner as XamTile;

            TileAreaPanel panel = tile.Panel;
			XamTileManager tm = tile.Manager;

            if (panel != null && tm != null && !tm.IsInMaximizedMode)
            {
				// JJD 03/29/12 - TFS106851 
				// Use the logic below instead so resizing behavior matches the UI behavior
				//switch (panel.NormalModeSettingsSafe.AllowTileSizing)
				//{
				//    case AllowTileSizing.Synchronized:
				//        tm.LayoutManager.SynchronizedItemSize = new Size(width, height);
				//        break;

				//    case AllowTileSizing.Individual:
				//        {
				//            ItemTileInfo info = tm.GetItemInfoFromContainer(tile);
				//            if (info != null)
				//                info.SizeOverride = new Size(width, height);
                                
				//            panel.InvalidateMeasure();
				//        }
				//        break;
				//}

				// JJD 03/29/12 - TFS106851 
				// Calculate the delta X and Y
				// Then call the appropriate 'Resize...' method on the IResizeHostMulti
				// interface that the XamTileManager implements
				// JJD 04/13/12 - TFS107688
				// Treat zero values as the equivalent of no delta
				double deltaX = width >= 1 ?  width - tile.ActualWidth : 0;
				double deltaY = height >= 1 ?  height - tile.ActualHeight : 0;

				// JJD 04/13/12 - TFS107688
 				// If the deltaX is less than 1 then don't resize in the X dimension
				if (Math.Abs(deltaX) < 1)
					deltaX = 0;

				// JJD 04/13/12 - TFS107688
 				// If the deltaY is less than 1 then don't resize in the Y dimension
				if (Math.Abs(deltaY) < 1)
					deltaY = 0;

				IResizeHostMulti resizeHost = tm as IResizeHostMulti;

				if ( deltaX == 0 )
				{
					if ( deltaY != 0 )
						resizeHost.Resize(tile, false, deltaY);
				}
				else
				{
					if (deltaY != 0)
						resizeHost.ResizeBothDimensions(tile, deltaX, deltaY);
					else
						resizeHost.Resize(tile, true, deltaX);
				}
            }

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