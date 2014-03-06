using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.Specialized;
using Infragistics.Windows.Helpers;
using System.Collections;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using Infragistics.Shared;
using System.ComponentModel;
using Infragistics.Collections;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// Represents a custom tabcontrol displayed on each outer edge of the <see cref="XamDockManager"/> that is used to display the unpinned <see cref="ContentPane"/> instances.
	/// </summary>
	//[ToolboxItem(false)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class UnpinnedTabArea : TabControl
		, IPaneContainer
	{
		#region Member Variables

		private List<object> _logicalChildren;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="UnpinnedTabArea"/>
		/// </summary>
		public UnpinnedTabArea()
		{
			this._logicalChildren = new List<object>();
		}

		static UnpinnedTabArea()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(UnpinnedTabArea), new FrameworkPropertyMetadata(typeof(UnpinnedTabArea)));
			FrameworkElement.FocusableProperty.OverrideMetadata(typeof(UnpinnedTabArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

			#region Default ItemsPanel

			FrameworkElementFactory fef = new FrameworkElementFactory(typeof(UnpinnedTabItemPanel));
			// AS 10/12/10 TFS57232
			// FindAncestor is asynchronous so by the first measure the values haven't gotten to the panel.
			//
			//RelativeSource relativeSourceTab = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(UnpinnedTabArea), 1);
			//fef.SetBinding(UnpinnedTabItemPanel.TabStripPlacementProperty, Utilities.CreateBindingObject(UnpinnedTabArea.TabStripPlacementProperty, BindingMode.OneWay, relativeSourceTab));
			fef.SetBinding(UnpinnedTabItemPanel.TabStripPlacementProperty, new Binding("TemplatedParent.TabStripPlacement") { Mode = BindingMode.OneWay, RelativeSource = RelativeSource.TemplatedParent });
			fef.SetValue(Panel.IsItemsHostProperty, KnownBoxes.TrueBox);

			ItemsPanelTemplate template = new ItemsPanelTemplate(fef);
			template.Seal();
			ItemsControl.ItemsPanelProperty.OverrideMetadata(typeof(UnpinnedTabArea), new FrameworkPropertyMetadata(template));

			#endregion //Default ItemsPanel

			// we need to show the flyout for a child if requested
			EventManager.RegisterClassHandler(typeof(UnpinnedTabArea), FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(OnRequestBringIntoView));

			// the unpinned tab area shows a context menu with a list of the visible panes
			EventManager.RegisterClassHandler(typeof(UnpinnedTabArea), FrameworkElement.ContextMenuOpeningEvent, new ContextMenuEventHandler(OnContextMenuOpening));

			// we need to change the visible to collapsed if all children are collapsed
			UIElement.VisibilityProperty.OverrideMetadata(typeof(UnpinnedTabArea), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceVisibility)));
			EventManager.RegisterClassHandler(typeof(UnpinnedTabArea), DockManagerUtilities.VisibilityChangedEvent, new RoutedEventHandler(OnChildVisibilityChanged));
		}
		#endregion //Constructor

		#region Base class overrides

		#region ClearContainerForItemOverride
		/// <summary>
		/// Called to clear the effects of the PrepareContainerForItemOverride method. 
		/// </summary>
		/// <param name="element">The container being cleared.</param>
		/// <param name="item">The item contained by the container being cleared.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.Windows.Controls.Primitives.Selector.#ClearContainerForItemOverride(System.Windows.DependencyObject,System.Object)")]
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
		{
			PaneTabItem tabItem = element as PaneTabItem;

			BindingOperations.ClearBinding(tabItem, PaneTabItem.HeaderProperty);
			BindingOperations.ClearBinding(tabItem, PaneTabItem.HeaderTemplateProperty);
			BindingOperations.ClearBinding(tabItem, PaneTabItem.HeaderTemplateSelectorProperty);

			base.ClearContainerForItemOverride(element, item);
		}
		#endregion //ClearContainerForItemOverride

		#region GetContainerForItemOverride
		/// <summary>
		/// Returns an instance of element used to display an item within the tab control.
		/// </summary>
		/// <returns>An instance of the <see cref="PaneTabItem"/> class</returns>
		protected override DependencyObject GetContainerForItemOverride()
		{
			return new PaneTabItem();
		}
		#endregion //GetContainerForItemOverride

		#region IsItemItsOwnContainerOverride
		/// <summary>
		/// Determines if the specified item is (or is eligible to be) its own container. 
		/// </summary>
		/// <param name="item">The item to evaluate</param>
		/// <returns>True if the specified item is its own container.</returns>
		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			return item is PaneTabItem;
		}
		#endregion //IsItemItsOwnContainerOverride

		#region LogicalChildren
		/// <summary>
		/// Returns an enumerator of the logical children
		/// </summary>
		protected override IEnumerator LogicalChildren
		{
			get
			{
				return this._logicalChildren.Count == 0
					? base.LogicalChildren
					: new MultiSourceEnumerator(base.LogicalChildren, this._logicalChildren.GetEnumerator());
			}
		}
		#endregion //LogicalChildren

		#region OnInitialized
		/// <summary>
		/// Invoked after the element has been initialized.
		/// </summary>
		/// <param name="e">Provides information for the event.</param>
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			// hide the element if it doesn't have any visible children
			this.CoerceValue(FrameworkElement.VisibilityProperty);

			// make sure a containing toolwindow knows it has new children
			DockManagerUtilities.VerifyOwningToolWindow(this);
		} 
		#endregion //OnInitialized

		#region OnItemsChanged
		/// <summary>
		/// Invoked when the <see cref="ItemsControl.Items"/> collection has been changed.
		/// </summary>
		protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
		{
			// hide the element if it doesn't have any visible children
			this.CoerceValue(FrameworkElement.VisibilityProperty);

			base.OnItemsChanged(e);
		}
		#endregion //OnItemsChanged

		#region PrepareContainerForItemOverride
		/// <summary>
		/// Prepares the specified container element to display the specified item. 
		/// </summary>
		/// <param name="element">The container element to prepare.</param>
		/// <param name="item">The item contained by the specified container element.</param>
		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			if (item is ContentPane == false)
				throw new InvalidOperationException(XamDockManager.GetString("LE_InvalidUnpinnedTabAreaChild"));

			PaneTabItem tabItem = element as PaneTabItem;
			ContentPane pane = item as ContentPane;

			tabItem.SetBinding(PaneTabItem.HeaderProperty, Utilities.CreateBindingObject(ContentPane.TabHeaderProperty, System.Windows.Data.BindingMode.OneWay, pane));
			tabItem.SetBinding(PaneTabItem.HeaderTemplateProperty, Utilities.CreateBindingObject(ContentPane.TabHeaderTemplateProperty, System.Windows.Data.BindingMode.OneWay, pane));
			tabItem.SetBinding(PaneTabItem.HeaderTemplateSelectorProperty, Utilities.CreateBindingObject(ContentPane.TabHeaderTemplateSelectorProperty, System.Windows.Data.BindingMode.OneWay, pane));

			BindingOperations.SetBinding(element, UIElement.VisibilityProperty, Utilities.CreateBindingObject(UIElement.VisibilityProperty, System.Windows.Data.BindingMode.OneWay, item));

			base.PrepareContainerForItemOverride(element, item);
		}
		#endregion //PrepareContainerForItemOverride

		#endregion //Base class overrides

		#region Methods

		#region Internal Methods

		#region AddLogicalChildInternal
		internal void AddLogicalChildInternal(object child)
		{
			this.AddLogicalChild(child);

			this._logicalChildren.Add(child);
		}
		#endregion //AddLogicalChildInternal

		#region RemoveLogicalChildInternal
		internal void RemoveLogicalChildInternal(object child)
		{
			this.RemoveLogicalChild(child);
			this._logicalChildren.Remove(child);
		}
		#endregion //RemoveLogicalChildInternal

		#endregion //Internal Methods

		#region Private Methods

		#region CoerceVisibility
		private static object CoerceVisibility(DependencyObject d, object newValue)
		{
			return DockManagerUtilities.ProcessCoerceVisibility(d, newValue);
		}
		#endregion //CoerceVisibility

		#region OnChildVisibilityChanged
		private static void OnChildVisibilityChanged(object sender, RoutedEventArgs e)
		{
			UnpinnedTabArea tabArea = sender as UnpinnedTabArea;

			// if the element's own visibility is changing then don't do anything
			if (sender != e.OriginalSource)
			{
				tabArea.CoerceValue(UIElement.VisibilityProperty);
				e.Handled = true;
			}

			DockManagerUtilities.VerifyOwningToolWindow(tabArea);
		}
		#endregion //OnChildVisibilityChanged

		#region OnContextMenuOpening

		private static void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			UnpinnedTabArea tabArea = sender as UnpinnedTabArea;

			// make sure the mouse wasn't over a tab item.
			if (null != Utilities.GetAncestorFromType(e.OriginalSource as DependencyObject, typeof(TabItem), true, tabArea))
				return;

			if (null != tabArea &&
				tabArea.ContextMenu == null &&
				tabArea.ReadLocalValue(FrameworkElement.ContextMenuProperty) != null)
			{
				List<ContentPane> visiblePanes = new List<ContentPane>();

				foreach (object item in tabArea.Items)
				{
					ContentPane pane = item as ContentPane;

					Debug.Assert(null != pane);

					if (null != pane && pane.Visibility == Visibility.Visible)
						visiblePanes.Add(pane);
				}

				if (visiblePanes.Count == 0)
					return;

				ContextMenu menu = new ContextMenu();

				menu.SetValue(DefaultStyleKeyProperty, XamDockManager.ContextMenuStyleKey);
				menu.PlacementTarget = tabArea;
				menu.Placement = PlacementMode.RelativePoint;

				Point pt = new Point();

				DependencyObject relativeElement = e.OriginalSource as DependencyObject;

				while (relativeElement is ContentElement)
					relativeElement = LogicalTreeHelper.GetParent(relativeElement);

				if (relativeElement is UIElement)
					pt = tabArea.TranslatePoint(pt, (UIElement)relativeElement);

				menu.HorizontalOffset = e.CursorLeft - pt.X;
				menu.VerticalOffset = e.CursorTop - pt.Y;

				// build a list of menu items
				foreach (ContentPane pane in visiblePanes)
				{
					MenuItem mi = new MenuItem();
					mi.Command = ContentPaneCommands.ActivatePane;
					mi.CommandTarget = pane;
					// AS 3/25/08
					// See notes in TabGroupPane.OnSubmenuOpened
					//
					mi.CommandParameter = pane;
					mi.Header = pane.TabHeader;
					mi.HeaderTemplate = pane.TabHeaderTemplate;
					mi.HeaderTemplateSelector = pane.TabHeaderTemplateSelector;
					mi.SetValue(DefaultStyleKeyProperty, XamDockManager.MenuItemStyleKey);

					menu.Items.Add(mi);
				}

				if (menu.Items.Count > 0)
				{
					menu.IsOpen = true;
					e.Handled = true;
				}
			}
		}

		#endregion //OnContextMenuOpening

		#region OnRequestBringIntoView
		private static void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
		{
			UnpinnedTabArea tabCtrl = sender as UnpinnedTabArea;

			tabCtrl.OnRequestBringIntoView(e);
		}

		private void OnRequestBringIntoView(RequestBringIntoViewEventArgs e)
		{
            
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

            DependencyObject item = DockManagerUtilities.GetTabControlChild(e.TargetObject, this);

			if (item != null && item is TabItem == false)
			{
				int index = this.Items.IndexOf(item);

				// then select the tab so its content is in view
				if (index >= 0)
				{
					// AS 5/15/08 BR32762
					// Do not try to select the tab of a non-visible item.
					//
					FrameworkElement element = item as FrameworkElement;
					Debug.Assert(null == element || element.Visibility == Visibility.Visible, "We should not be trying to make a hidden tab the selected item");

					if (element != null && element.Visibility != Visibility.Visible)
						return;

					this.SelectedIndex = index;

					ContentPane pane = item as ContentPane;

					Debug.Assert(null != pane);

					if (null != pane)
					{
						XamDockManager dockManager = XamDockManager.GetDockManager(this);

						// AS 5/5/10 TFS29178
						//if (null != dockManager)
						// AS 7/1/10 TFS34388
						//if (null != dockManager && !dockManager.IsShowFlyoutSuspended)
						if (null != dockManager && !dockManager.IsShowFlyoutSuspended && dockManager == XamDockManager.GetDockManager(this))
							dockManager.ShowFlyout(pane, false, false);
					}
				}
			}
		}
		#endregion //OnRequestBringIntoView

		#endregion //Private Methods

		#endregion //Methods

		#region IPaneContainer Members

		IList IPaneContainer.Panes
		{
			get { return this.Items; }
		}

		bool IPaneContainer.RemovePane(object pane)
		{
			return false;
		}

		bool IPaneContainer.CanBeRemoved
		{
			get { return false; }
		}
		#endregion //IPaneContainer
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