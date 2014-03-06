using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Provider;
using Infragistics.Windows.Tiles;
using System.Windows.Automation.Peers;
using System.Windows.Automation;
using System.Windows;

namespace Infragistics.Windows.Automation.Peers.Tiles
{
    /// <summary>
    /// Exposes <see cref="Tile"/> type to UI Automation
    /// </summary>
    public class TileAutomationPeer : FrameworkElementAutomationPeer,
        IExpandCollapseProvider,
        ITransformProvider,
        IScrollItemProvider
    {
		#region Member Variables

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Creates a new instance of the <see cref="TileAutomationPeer"/> class
		/// </summary>
		/// <param name="tile">The <see cref="Tile"/> for which the peer is being created</param>
        public TileAutomationPeer(Tile tile)
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
		/// Returns the name of the <see cref="Tile"/>
		/// </summary>
		/// <returns>A string that contains 'Tile'</returns>
		protected override string GetClassNameCore()
		{
			return "Tile";
		}

		#endregion //GetClassNameCore

		#region GetPattern
		/// <summary>
		/// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the <see cref="UIElement"/> that corresponds with the <see cref="Tile"/> that is associated with this <see cref="TileAutomationPeer"/>.
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

            Tile tile = (Tile)this.Owner;

            if (tile.State != TileState.Minimized)
                Tile.ToggleMinimizedExpansionCommand.Execute(null, tile);
        }

        void IExpandCollapseProvider.Expand()
        {
            if (this.IsEnabled() == false)
                throw new ElementNotEnabledException();

            if (((IExpandCollapseProvider)this).ExpandCollapseState == ExpandCollapseState.LeafNode)
                throw new InvalidOperationException();

            Tile tile = (Tile)this.Owner;

            if (tile.State != TileState.MinimizedExpanded)
                Tile.ToggleMinimizedExpansionCommand.Execute(null, tile);
        }

        ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
        {
            get 
            { 
                Tile tile = (Tile)this.Owner;

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
            Tile tile = (Tile)this.Owner;

            tile.BringIntoView();
        }

        #endregion //IScrollItemProvider

        #region ITransformProvider Members

        bool ITransformProvider.CanMove
        {
            get 
            {
                if (this.IsEnabled() == false)
                    return false;

                Tile tile = this.Owner as Tile;

                TilesPanel panel = tile.Panel;

                return panel != null && panel.CanDragTiles; 
            }
        }

        bool ITransformProvider.CanResize
        {
            get 
            {
                if (this.IsEnabled() == false)
                    return false;

                Tile tile = this.Owner as Tile;

                TilesPanel panel = tile.Panel;

                if (panel == null)
                    return false;

                if (panel.IsInMaximizedMode)
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
                throw new InvalidOperationException(Infragistics.Windows.Resources.GetString("LE_CannotPerformAutomationOperation"));

            if (double.IsInfinity(x) || double.IsNaN(x))
                throw new ArgumentOutOfRangeException("x");

            if (double.IsInfinity(y) || double.IsNaN(y))
                throw new ArgumentOutOfRangeException("y");

            Tile tile = this.Owner as Tile;

            TilesPanel panel = tile.Panel;

            if (panel != null)
            {
               panel.MoveTileHelper(tile, panel.ItemFromTile(tile), new Point(x, y));
            }
        }

        void ITransformProvider.Resize(double width, double height)
        {
             if (this.IsEnabled() == false)
                throw new ElementNotEnabledException();

            if (!((ITransformProvider)this).CanResize)
                throw new InvalidOperationException(Infragistics.Windows.Resources.GetString("LE_CannotPerformAutomationOperation"));

            if (double.IsInfinity(width) || double.IsNaN(width))
                throw new ArgumentOutOfRangeException("width");

            if (double.IsInfinity(height) || double.IsNaN(height))
                throw new ArgumentOutOfRangeException("height");

            Tile tile = this.Owner as Tile;

            TilesPanel panel = tile.Panel;

            if (panel != null && !panel.IsInMaximizedMode)
            {
                switch (panel.NormalModeSettingsSafe.AllowTileSizing)
                {
                    case AllowTileSizing.Synchronized:
                        panel.Manager.SynchronizedItemSize = new Size(width, height);
                        break;

                    case AllowTileSizing.Individual:
                        {
                            XamTilesControl tc = panel.TilesControl;

                            if (tc != null)
                            {
                                ItemInfo info = tc.GetItemInfoFromContainer(tile);
                                if (info != null)
                                    info.SizeOverride = new Size(width, height);
                                
                                panel.InvalidateMeasure();
                            }
                        }
                        break;
                }
            }

        }

        void ITransformProvider.Rotate(double degrees)
        {
            if (this.IsEnabled() == false)
                throw new ElementNotEnabledException();

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