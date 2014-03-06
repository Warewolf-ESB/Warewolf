using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using System.Windows.Input;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Themes;
using System.Diagnostics;
using Infragistics.Windows.DockManager.Dragging;
using Infragistics.Windows.Automation.Peers.DockManager;
using System.Windows.Automation.Peers;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// Used to represent a <see cref="ContentPane"/> as a tab header within the <see cref="UnpinnedTabArea"/> and <see cref="TabGroupPane"/>.
	/// </summary>
	//[ToolboxItem(false)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class PaneTabItem : TabItemEx
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="PaneTabItem"/>
		/// </summary>
		public PaneTabItem()
		{
		}

		static PaneTabItem()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(PaneTabItem), new FrameworkPropertyMetadata(typeof(PaneTabItem)));
			FrameworkElement.FocusableProperty.OverrideMetadata(typeof(PaneTabItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
			ContentControl.HorizontalContentAlignmentProperty.OverrideMetadata(typeof(PaneTabItem), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentStretchBox));
			ContentControl.VerticalContentAlignmentProperty.OverrideMetadata(typeof(PaneTabItem), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentStretchBox));

			// we need to show the panes context menu when right clicking on the header or a tab item
			EventManager.RegisterClassHandler(typeof(PaneTabItem), FrameworkElement.ContextMenuOpeningEvent, new ContextMenuEventHandler(ContentPane.OnPaneContextMenuOpening));

			MouseHoverService.Register(typeof(PaneTabItem));
			EventManager.RegisterClassHandler(typeof(PaneTabItem), MouseHoverService.MouseHoverEvent, new MouseEventHandler(OnMouseHover));
		} 
		#endregion //Constructor

		#region Base class overrides

		// AS 2/2/10 TFS27037
		#region OnCreateAutomationPeer

		/// <summary>
		/// Returns <see cref="SplitPane"/> Automation Peer Class <see cref="SplitPaneAutomationPeer"/>
		/// </summary>
		/// <returns>AutomationPeer</returns>
		protected override AutomationPeer OnCreateAutomationPeer()
		{





			return new PaneTabItemWrapperAutomationPeer(this);
		}

		#endregion //OnCreateAutomationPeer

		#region OnContentChanged
		/// <summary>
		/// Invoked when the <see cref="ContentControl.Content"/> property has been changed.
		/// </summary>
		/// <param name="oldContent">The previous value of the Content property</param>
		/// <param name="newContent">The new value of the Content property</param>
		protected override void OnContentChanged(object oldContent, object newContent)
		{
			base.OnContentChanged(oldContent, newContent);

			// set or clear the Pane property based on the content
			this.SetValue(PanePropertyKey, newContent as ContentPane);
		}
		#endregion //OnContentChanged

		#region OnMouseDown
		/// <summary>
		/// Invoked when the mouse is pressed down on the element.
		/// </summary>
		/// <param name="e">Provides data for the mouse event</param>
		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);

			ContentPane pane = this.Pane;

			if (null != pane)
			{
				if (e.ChangedButton == MouseButton.Left)
				{
					if (e.ClickCount == 2)
					{
						if (pane.ExecuteCommand(ContentPaneCommands.ToggleDockedState))
						{
							e.Handled = true;
						}
					}
					else if (e.ClickCount == 1 && this.IsUnpinned && Keyboard.Modifiers == ModifierKeys.Shift)
					{
						// to emulate vs, shift clicking an unpinned tab should pin it
						if (pane.ExecuteCommand(ContentPaneCommands.TogglePinnedState))
						{
							pane.ActivateInternal();
							e.Handled = true;
						}
					}
				}
				else if (e.ChangedButton == MouseButton.Middle && XamDockManager.GetPaneLocation(this) == PaneLocation.Document)
				{
					if (pane.ExecuteCommand(ContentPaneCommands.Close))
					{
						e.Handled = true;
					}
				}

				if (e.Handled == false)
				{
					// while the document tabs select on the mouse up, to be consistent we will 
					// select on the down just like vs does with the non-document tab items
					// AS 5/8/09 TFS15993
					// I thought of a scenario while debugging this whereby the pane may not flyout 
					// when it should have. Actually the scenario could have happened before the fix 
					// so its not specific. Basically if the unpinned pane was active but the flyout 
					// was not shown, clicking on the pane's tab wouldn't have shown the flyout because 
					// we don't force activation by default and since it had keyboard focus nothing 
					// would have happened. The only reason the flyout would show right now is because 
					// we also explicitly show the flyout if you enter/hover over the pane tab item 
					// but if we ever add the capability to disable that then the pane would not show 
					// plus it really shouldn't wait for any other criteria if it was explicitly 
					// clicked.
					//
					//pane.ActivateInternal();
					bool force = false;

					XamDockManager dm = XamDockManager.GetDockManager(pane);

					if (null != dm && !pane.IsPinned && pane != dm.CurrentFlyoutPane)
						force = true;
					
					pane.ActivateInternal(force);

					// let the drag manager process the mouse down in case we need to start a drag
					DragManager.ProcessMouseDown(this, e);
				}
			}
		} 
		#endregion //OnMouseDown

		#region OnMouseEnter
		/// <summary>
		/// Invoked when the mouse enters the bounds of the element.
		/// </summary>
		/// <param name="e">Provides information about the event</param>
		protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			if (this.IsUnpinned)
				UnpinnedTabFlyout.ShowFlyoutOnMouseEnter(this.Pane);
		}
		#endregion //OnMouseEnter

		#region OnMouseLeave
		/// <summary>
		/// Invoked when the mouse leaves the bounds of the element.
		/// </summary>
		/// <param name="e">Provides information about the event</param>
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			// if the pane is shown in the flyout but not active then hide it on leave
			if (this.IsUnpinned)
				UnpinnedTabFlyout.HideFlyoutOnMouseLeave(this.Pane);
		} 
		#endregion //OnMouseLeave
		
		#endregion //Base class overrides

		#region Resource Keys

		#region DockableTabItemTemplateKey

		/// <summary>
		/// The key used to identify the <see cref="ControlTemplate"/> for the <see cref="PaneTabItem"/> when it is not contained within the <see cref="DocumentContentHost"/> area.
		/// </summary>
		/// <seealso cref="DocumentTabItemTemplateKey"/>
		/// <seealso cref="UnpinnedTabItemTemplateKey"/>
		/// <seealso cref="TabGroupPane.DockableTabGroupTemplateKey"/>
		public static readonly ResourceKey DockableTabItemTemplateKey = new StaticPropertyResourceKey(typeof(PaneTabItem), "DockableTabItemTemplateKey");

		#endregion //DockableTabItemTemplateKey

		#region DocumentTabItemTemplateKey

		/// <summary>
		/// The key used to identify the <see cref="ControlTemplate"/> for the <see cref="PaneTabItem"/> when hosted within a <see cref="DocumentContentHost"/>
		/// </summary>
		/// <seealso cref="DockableTabItemTemplateKey"/>
		/// <seealso cref="UnpinnedTabItemTemplateKey"/>
		/// <seealso cref="TabGroupPane.DocumentTabGroupTemplateKey"/>
		public static readonly ResourceKey DocumentTabItemTemplateKey = new StaticPropertyResourceKey(typeof(PaneTabItem), "DocumentTabItemTemplateKey");

		#endregion //DocumentTabItemTemplateKey

		#region UnpinnedTabItemTemplateKey

		/// <summary>
		/// The key used to identify the <see cref="ControlTemplate"/> for the <see cref="PaneTabItem"/> when hosted within a <see cref="UnpinnedTabArea"/>
		/// </summary>
		/// <seealso cref="DockableTabItemTemplateKey"/>
		/// <seealso cref="DocumentTabItemTemplateKey"/>
		/// <seealso cref="UnpinnedTabArea"/>
		public static readonly ResourceKey UnpinnedTabItemTemplateKey = new StaticPropertyResourceKey(typeof(PaneTabItem), "UnpinnedTabItemTemplateKey");

		#endregion //UnpinnedTabItemTemplateKey

		#endregion //Resource Keys

		#region Properties

		#region Public Properties

		#region Pane

		private static readonly DependencyPropertyKey PanePropertyKey =
			DependencyProperty.RegisterReadOnly("Pane",
			typeof(ContentPane), typeof(PaneTabItem), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="Pane"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PaneProperty =
			PanePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the associated ContentPane that the tab item represents.
		/// </summary>
		/// <seealso cref="PaneProperty"/>
		//[Description("Returns the associated ContentPane that the tab item represents.")]
		//[Category("DockManager Properties")] // Data
		[Bindable(true)]
		[ReadOnly(true)]
		public ContentPane Pane
		{
			get
			{
				return (ContentPane)this.GetValue(PaneTabItem.PaneProperty);
			}
		}

		#endregion //Pane

		#endregion //Public Properties 

		#region Private Properties

		#region IsUnpinned
		private bool IsUnpinned
		{
			get { return PaneLocation.Unpinned == XamDockManager.GetPaneLocation(this); }
		}
		#endregion //IsUnpinned 

		#endregion //Private Properties

		#endregion //Properties

		#region Methods
		
		#region Private Methods

		#region OnMouseHover
		private static void OnMouseHover(object sender, MouseEventArgs e)
		{
			PaneTabItem tab = (PaneTabItem)sender;

			if (null != tab && tab.IsUnpinned && null != tab.Pane)
			{
				XamDockManager dockManager = XamDockManager.GetDockManager(tab);

				if (null != dockManager)
					dockManager.ShowFlyout(tab.Pane, false, true);
			}
		}
		#endregion //OnMouseHover

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