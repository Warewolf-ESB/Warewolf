using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Markup;
using System.Diagnostics;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Shapes;

using Infragistics.Windows.Selection;
using Infragistics.Windows.Licensing;
using Infragistics.Windows.Tiles.Events;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Commands;
using Infragistics.Windows.Virtualization;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.Tiles
{
	/// <summary>
	/// An element used to represent the caption/header area of a <see cref="Tile"/>
	/// </summary>
    //[Description("An element used to represent the caption/header area of a Tile.")]
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!

    // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,               GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMouseOver,            GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateDisabled,             GroupName = VisualStateUtilities.GroupCommon)]

    [TemplateVisualState(Name = VisualStateUtilities.StateDragging,             GroupName = VisualStateUtilities.GroupDrag)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNotDragging,          GroupName = VisualStateUtilities.GroupDrag)]
    [TemplateVisualState(Name = VisualStateUtilities.StateSwapTarget,           GroupName = VisualStateUtilities.GroupDrag)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateMaximized,            GroupName = VisualStateUtilities.GroupMinimized)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMinimized,            GroupName = VisualStateUtilities.GroupMinimized)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMinimizedExpanded,    GroupName = VisualStateUtilities.GroupMinimized)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNotMinimized,         GroupName = VisualStateUtilities.GroupMinimized)]


	public class TileHeaderPresenter : ContentControl
	{
		#region Member Variables

        private Point?          _mouseDownLocation;
        private bool            _dragAttemptCanceled;

        // JJD 4/23/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;

        
		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="TileHeaderPresenter"/>
		/// </summary>
		public TileHeaderPresenter()
		{
		}

		static TileHeaderPresenter()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TileHeaderPresenter), new FrameworkPropertyMetadata(typeof(TileHeaderPresenter)));
			ContentControl.HorizontalContentAlignmentProperty.OverrideMetadata(typeof(TileHeaderPresenter), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentStretchBox));
			ContentControl.VerticalContentAlignmentProperty.OverrideMetadata(typeof(TileHeaderPresenter), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentCenterBox));
			UIElement.FocusableProperty.OverrideMetadata(typeof(TileHeaderPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

			// we need to register a handler for the MouseDown because we need to get handled
			// events because we want to activate the associated tile even if you click on a 
			// button within the caption
			EventManager.RegisterClassHandler(typeof(TileHeaderPresenter), Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseButtonDown), true);


            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            UIElement.IsEnabledProperty.OverrideMetadata(typeof(TileHeaderPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));

        }
		#endregion //Constructor

		#region Resource Keys

		#region CloseButtonStyleKey

		/// <summary>
		/// The key used to identify the <see cref="FrameworkElement.Style"/> used for the close <see cref="Button"/> within the header of a <see cref="Tile"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">This field represents the <see cref="ResourceKey"/> that should be used as the key in a <see cref="ResourceDictionary"/> 
		/// for a <see cref="FrameworkElement.Style"/> targeting a <see cref="Button"/>.</p>
		/// </remarks>
        /// <seealso cref="Infragistics.Windows.Tiles.Tile.CloseAction"/>
		public static readonly ResourceKey CloseButtonStyleKey = new StaticPropertyResourceKey(typeof(TileHeaderPresenter), "CloseButtonStyleKey");

		#endregion //CloseButtonStyleKey

		#region ExpandButtonStyleKey

		/// <summary>
		/// The key used to identify the <see cref="FrameworkElement.Style"/> used for the expand <see cref="Button"/> within the header of a <see cref="Tile"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">This field represents the <see cref="ResourceKey"/> that should be used as the key in a <see cref="ResourceDictionary"/> 
		/// for a <see cref="FrameworkElement.Style"/> targeting a <see cref="Button"/>.</p>
		/// </remarks>
        /// <seealso cref="Infragistics.Windows.Tiles.MaximizedModeSettings.MinimizedTileExpandButtonVisibility"/>
        /// <seealso cref="Infragistics.Windows.Tiles.Tile.ExpandButtonVisibility"/>
		public static readonly ResourceKey ExpandButtonStyleKey = new StaticPropertyResourceKey(typeof(TileHeaderPresenter), "ExpandButtonStyleKey");

		#endregion //ExpandButtonStyleKey

		#region MaximizeButtonStyleKey

		/// <summary>
		/// The key used to identify the <see cref="FrameworkElement.Style"/> used for the maximize <see cref="Button"/> within the header of a <see cref="Tile"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">This field represents the <see cref="ResourceKey"/> that should be used as the key in a <see cref="ResourceDictionary"/> 
		/// for a <see cref="FrameworkElement.Style"/> targeting a <see cref="Button"/>.</p>
		/// </remarks>
        /// <seealso cref="Infragistics.Windows.Tiles.Tile.AllowMaximize"/>
		public static readonly ResourceKey MaximizeButtonStyleKey = new StaticPropertyResourceKey(typeof(TileHeaderPresenter), "MaximizeButtonStyleKey");

		#endregion //MaximizeButtonStyleKey

		#endregion //Resource Keys

		#region Base class overrides

            #region OnApplyTemplate

        /// <summary>
        /// Called when the template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();


            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);


        }

            #endregion //OnApplyTemplate	

            #region OnLostMouseCapture

        /// <summary>
        /// Called when mouse capture is lost
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);

            // clear the point saved in mouse down
            if (this._mouseDownLocation != null)
            {
                this._mouseDownLocation = null;
                e.Handled = true;
            }
        }

            #endregion //OnLostMouseCapture	
  
            #region OnMouseDoubleClick

        /// <summary>
        /// Called when mouse is double clicked.
        /// </summary>
        /// <param name="e">arguments</param>
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            //Debug.WriteLine(string.Format("DoubleClick: {0}.{1}", DateTime.Now.Second, DateTime.Now.Millisecond));
            Tile tile = this.Tile;

            TilesPanel panel = tile != null ? tile.Panel : null;

            if (panel == null)
            {
                base.OnMouseDoubleClick(e);
                return;
            }

            if (e.Handled == false && e.LeftButton == MouseButtonState.Pressed)
            {
				// MD 5/13/10 - TFS32105
				// If a button was double-clicked, we don't want the header to process the double-click and toggle the maximized state, 
				// because the button should be handling clicks.
				ButtonBase button = (ButtonBase)Utilities.GetAncestorFromType((DependencyObject)e.OriginalSource, typeof(ButtonBase), true, this);
				if (button != null)
					return;

                if (Tile.ToggleMaximizedCommand.CanExecute(null, tile))
                {
                    //Debug.WriteLine(string.Format("DoubleClick toggle maximize: {0}.{1}", DateTime.Now.Second, DateTime.Now.Millisecond));
                    Tile.ToggleMaximizedCommand.Execute(null, tile);
                    e.Handled = true;
                }
            }
        }

            #endregion //OnMouseDoubleClick	

		    #region OnMouseEnter

		/// <summary>
        /// Invoked when an unhandled Mouse.MouseEnter attached event is raised on this element
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> that contains the event data</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }

            #endregion //OnMouseEnter	
    
            #region OnMouseLeave

        /// <summary>
        /// Invoked when an unhandled Mouse.MouseLeave attached event is raised on this element
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> that contains the event data</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }

            #endregion //OnMouseLeave	
    
            #region OnMouseLeftButtonDown

        /// <summary>
        /// Called when the left button is pressed down.
        /// </summary>
        /// <param name="e">arguments</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            //Debug.WriteLine(string.Format("MouseDown: {0}.{1}", DateTime.Now.Second, DateTime.Now.Millisecond));
            Tile tile = this.Tile;

            this._dragAttemptCanceled = false;

            TilesPanel panel = tile != null ? tile.Panel : null;

            if ( panel == null)
            {
                base.OnMouseLeftButtonDown(e);
                return;
            }

            if (e.Handled == false && e.ClickCount == 1)
            {
                if (this.CaptureMouse())
                {
                    this._mouseDownLocation = e.GetPosition(this);
                    e.Handled = true;
                    return;
                }
            }
        }

            #endregion //OnMouseLeftButtonDown	

            #region OnMouseLeftButtonUp

        /// <summary>
        /// Called when the left button is released.
        /// </summary>
        /// <param name="e">arguments</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            //Debug.WriteLine(string.Format("MouseUp: {0}.{1}", DateTime.Now.Second, DateTime.Now.Millisecond));
            Tile tile = this.Tile;

            this._dragAttemptCanceled = false;
          
            TilesPanel panel = tile != null ? tile.Panel : null;

            if ( panel == null)
            {
                base.OnMouseLeftButtonUp(e);
                return;
            }

            if (this._mouseDownLocation != null)
            {
                this._mouseDownLocation = null;

                this.ReleaseMouseCapture();
            }
        }

            #endregion //OnMouseLeftButtonUp	

            #region OnMouseMove

        /// <summary>
        /// Called when the is moved.
        /// </summary>
        /// <param name="e">arguments</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (this._mouseDownLocation != null && !this._dragAttemptCanceled)
            {
                Point mouseLocation = e.GetPosition(this);

                Size dragSize = Utilities.SystemDragSize;
                // if the mouse has moved more than a few pixels in any direction then start a drag 
                // operation if possible
                if (Math.Abs(mouseLocation.X - this._mouseDownLocation.Value.X) > dragSize.Width ||
                    Math.Abs(mouseLocation.Y - this._mouseDownLocation.Value.Y) > dragSize.Height)
                {
                    Tile tile = this.Tile;

                    if (tile != null)
                    {
                        TilesPanel panel = tile.Panel;

                        if (panel != null && panel.CanDragTiles)
                        {
                            if (panel.StartDrag(tile, e))
                                this._mouseDownLocation = null;
                            else
                                this._dragAttemptCanceled = true;

                        }
                    }
                }
            }
        }

            #endregion //OnMouseMove	
    
		#endregion //Base class overrides

		#region Properties

		#region Public Properties

        #region Tile

        /// <summary>
        /// Identifies the <see cref="Tile"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TileProperty = DependencyProperty.Register("Tile",
            typeof(Tile), typeof(TileHeaderPresenter), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnTileChanged)));

        private static void OnTileChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TileHeaderPresenter thp = target as TileHeaderPresenter;

            if (thp != null)
            {
                Tile tile = e.OldValue as Tile;

                if (tile != null)
                    tile.InitializeHeaderPresenter(null);
                
                tile = e.NewValue as Tile;

                if (tile != null)
                    tile.InitializeHeaderPresenter(thp);
            }
        }

        /// <summary>
        /// Gets/sets the associated tile that the header represents.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property needs to be set inside the template of the Tile.</para>
        /// </remarks>
        /// <seealso cref="TileProperty"/>
        [Bindable(true)]
        public Tile Tile
        {
            get
            {
                return (Tile)this.GetValue(TileHeaderPresenter.TileProperty);
            }
            set
            {
                this.SetValue(TileHeaderPresenter.TileProperty, value);
            }
        }

        #endregion //Tile

		#endregion //Public Properties

		#region Private Properties

		#endregion //Private Properties

		#endregion //Properties

		#region Methods

        #region Protected Methods

        #region VisualState... Methods


        // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {
            // Set Common state
            if (this.IsEnabled == false)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateDisabled, useTransitions);
            else
            if (this.IsMouseOver)
                VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);

            Tile tile = this.Tile;

            // Set drag state
            if (tile != null)
            {
                if (TilesPanelBase.GetIsDragging(tile))
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateDragging, useTransitions);
                else
                if (TilesPanelBase.GetIsSwapTarget(tile))
                    VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateSwapTarget, VisualStateUtilities.StateNotDragging);
                else 
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateNotDragging, useTransitions);
            }
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNotDragging, useTransitions);

            string minstate = null;

            // set minimized state
            if (tile == null)
                minstate = VisualStateUtilities.StateNotMinimized;
            else
            {
                switch (tile.State)
                {
                    case TileState.Maximized:
                        minstate = VisualStateUtilities.StateMaximized;
                        break;
                    case TileState.Minimized:
                        minstate = VisualStateUtilities.StateMinimized;
                        break;
                    case TileState.MinimizedExpanded:
                        minstate = VisualStateUtilities.StateMinimizedExpanded;
                        break;
                    case TileState.Normal:
                        minstate = VisualStateUtilities.StateNotMinimized;
                        break;
                }
            }

            if (minstate != null)
                VisualStateManager.GoToState(this, minstate, useTransitions);

        }

        // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
        internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TileHeaderPresenter thp = target as TileHeaderPresenter;

            if (thp != null)
                thp.UpdateVisualStates();
        }

        // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        internal protected void UpdateVisualStates()
        {
            this.UpdateVisualStates(true);
        }

        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        internal protected void UpdateVisualStates(bool useTransitions)
        {
            if (false == this._hasVisualStateGroups)
                return;

            if (!this.IsLoaded)
                useTransitions = false;

            this.SetVisualState(useTransitions);
        }



        #endregion //VisualState... Methods	

        #endregion //Protected Methods	

		#region Private Methods

		#region OnMouseButtonDown
		private static void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
		{
            //TileHeaderPresenter tileHeader = sender as TileHeaderPresenter;
            //Tile tile = null != tileHeader ? tileHeader.Tile : null;

            //if (null != tile)
            //{
            //    bool wasHandled = e.Handled;

            //    if (e.ChangedButton == MouseButton.Left && wasHandled == false && e.ClickCount == 2)
            //    {
            //        XamDockManager dockManager = XamDockManager.GetDockManager(tile);

            //        if (null != dockManager && dockManager.ProcessTileHeaderDoubleClick(tile))
            //        {
            //            e.Handled = true;
            //            return;
            //        }
            //    }

            //    if (tile.ActivateInternal())
            //    {
            //        e.Handled = true;
            //    }

            //    if (wasHandled == false)
            //        DragManager.ProcessMouseDown(tileHeader, e);
            //}
		} 
		#endregion //OnMouseButtonDown

		#endregion //Private Methods 

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