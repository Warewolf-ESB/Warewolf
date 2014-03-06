using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using Infragistics.Shared;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Controls;

namespace Infragistics.Windows.DockManager.Dragging
{
	#region PaneDragAction
	/// <summary>
	/// Base class for an object that describes an action that would be taken as a result of a drag operation within the <see cref="XamDockManager"/>
	/// </summary>
	public abstract class PaneDragAction
	{
		/// <summary>
		/// Initializes a new <see cref="PaneDragAction"/>
		/// </summary>
		protected PaneDragAction()
		{
		}

		/// <summary>
		/// Used to perform the specified operation.
		/// </summary>
		/// <param name="dragManager">The class managing the drag operation.</param>
		/// <param name="isPreview">True if this is occuring during the drag operation or false if operation should be performed.</param>
		internal abstract void PerformAction(DragManager dragManager, bool isPreview);

		internal virtual int GetDragHashCode()
		{
			return this.GetHashCode();
		}

		internal virtual bool IsSameResult(PaneDragAction action)
		{
			return this.Equals(action);
		}

		internal abstract bool IsActionAllowed(DragManager dragManager);
	} 
	#endregion //PaneDragAction

	#region MoveWindowAction
	/// <summary>
	/// Drag action that describes the repositioning of a <see cref="PaneToolWindow"/>
	/// </summary>
	public class MoveWindowAction : PaneDragAction
	{
		#region Member Variables

		private Point _oldLocation;
		private Point _newLocation;
		private PaneToolWindow _window;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="MoveWindowAction"/>
		/// </summary>
		/// <param name="window">The window being moved</param>
		/// <param name="oldLocation">The old position of the window</param>
		/// <param name="newLocation">The new position of the window</param>
		public MoveWindowAction(PaneToolWindow window, Point oldLocation, Point newLocation)
		{
            // AS 3/13/09 FloatingWindowDragMode
            // When the FloatingWindowDragMode is Deferred then the Window will be null.
            //
            //DockManagerUtilities.ThrowIfNull(window, "window");

			this._window = window;
			this._oldLocation = oldLocation;
			this._newLocation = newLocation;
		} 
		#endregion //Constructor

		#region Properties

		/// <summary>
		/// Returns the window being repositioned or null if the <see cref="XamDockManager.FloatingWindowDragMode"/> is deferred.
		/// </summary>
		public PaneToolWindow Window
		{
			get { return this._window; }
		}

		/// <summary>
		/// Returns the old location of the window
		/// </summary>
		public Point OldLocation
		{
			get { return this._oldLocation; }
		}

		/// <summary>
		/// Returns the new location of the window.
		/// </summary>
		public Point NewLocation
		{
			get { return this._newLocation; }
			// AS 9/29/09 NA 2010.1 - PaneDragAction
			[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_MiscDockManagerFeatures_10_1)]
			set { this._newLocation = value; }
		}

		#endregion //Properties

		#region Base class overrides

		internal override int GetDragHashCode()
		{
            // AS 3/13/09 FloatingWindowDragMode
            if (_window == null)
                return _newLocation.GetHashCode();

            return this._newLocation.GetHashCode() | this._window.GetHashCode();
		}

		internal override bool IsActionAllowed(DragManager dragManager)
		{
			return dragManager.IsDropLocationAllowed(AllowedDropLocations.Floating);
		}

		internal override bool IsSameResult(PaneDragAction action)
		{
			MoveWindowAction dragAction = action as MoveWindowAction;

			return null != dragAction &&
				dragAction._newLocation == this._newLocation &&
				dragAction._window == this._window;
		}

		internal override void PerformAction(DragManager dragManager, bool isPreview)
		{
			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// If the OS is performing the drag then we don't need to do anything during the 
			// preview or even during the drop.
			//
			if (dragManager.IsInWindowDragMode)
				return;

            
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


            // AS 3/13/09 FloatingWindowDragMode
            if (dragManager.DeferFloatingWindowPositioning)
            {
                if (isPreview)
                {
                    ToolWindow floatingWindow = dragManager.FloatingWindow;

                    if (floatingWindow == null)
                    {
                        PaneToolWindow windowBeingDragged = dragManager.WindowBeingDragged;

                        Debug.Assert(windowBeingDragged != null);

                        // when dragging around the preview create it if it hasn't been 
                        // created. This would happen with the first move of a drag 
                        // operation involving an already floating window 
                        if (windowBeingDragged != null)
                        {
                            Size windowSize = new Size(windowBeingDragged.ActualWidth, windowBeingDragged.ActualHeight);

							// AS 6/8/11 TFS76337
							// If we end up keeping the source window in a maximized state then don't use that size. Use 
							// the restore bounds instead.
							//
							//windowSize = windowBeingDragged.AddNonClientSize(windowSize);
							Rect restoreBounds = windowBeingDragged.WindowState == WindowState.Maximized 
								? windowBeingDragged.GetRestoreBounds() 
								: Rect.Empty;
							if (!restoreBounds.IsEmpty)
								windowSize = restoreBounds.Size;
							// AS 6/24/11 FloatingWindowCaptionSource
							// Found this while debugging. We shouldn't add the non-client area if the area is part 
							// of the toolwindow itself - only if we're in a window showing the non-client area.
							//
							//else
							else if (windowBeingDragged.Host != null && windowBeingDragged.UseOSNonClientArea)
	                            windowSize = windowBeingDragged.AddNonClientSize(windowSize);

                            dragManager.CreateFloatingPreviewWindow(this.NewLocation, windowSize);
                        }
                    }
                    else
                    {
                        // otherwise just reposition the window
                        DockManagerUtilities.MoveToolWindow(floatingWindow, this.NewLocation);

                        if (floatingWindow.IsVisible == false)
                        {
                            // AS 3/30/09 TFS16355 - WinForms Interop
                            //floatingWindow.Show(dragManager.DockManager, false);
                            DockManagerUtilities.ShowToolWindow(floatingWindow, dragManager.DockManager, false);
                            floatingWindow.BringToFront();
                        }
                    }
                }
                else
                {
                    // when commiting we either need to position the 
                    // window being dragged or create one
                    if (dragManager.WindowBeingDragged == null)
                    {
                        ToolWindow floatingWindow = dragManager.FloatingWindow;
                        Debug.Assert(null != floatingWindow);
                        Size floatingSize = floatingWindow == null ? new Size(double.PositiveInfinity, double.PositiveInfinity) : floatingWindow.RenderSize;
                        dragManager.CreateWindowBeingDragged(this.NewLocation, floatingSize);
                    }
                    else
                    {
						// AS 6/8/11 TFS76337
						// If the window is maximized then restore it to normal so the location can take effect.
						//
                        //DockManagerUtilities.MoveToolWindow(dragManager.WindowBeingDragged, this.NewLocation);
						ToolWindow window = dragManager.WindowBeingDragged;
						if (window.WindowState == WindowState.Maximized)
							window.WindowState = WindowState.Normal;

                        DockManagerUtilities.MoveToolWindow(window, this.NewLocation);
                    }
                }
            }
            else
            {
                Debug.Assert(null != _window);

                if (null != _window)
                    DockManagerUtilities.MoveToolWindow(_window, this.NewLocation);
            }
		}



#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


		#endregion //Base class overrides
	}

	#endregion //MoveWindowAction

	#region AddToGroupActionBase
	/// <summary>
	/// Base class for a drag action used when the <see cref="ContentPane"/> instances being dragged will be added to an existing <see cref="TabGroupPane"/> or <see cref="SplitPane"/>
	/// </summary>
	public abstract class AddToGroupActionBase : PaneDragAction
	{
		#region Member Variables

		private FrameworkElement _group;
		private int _index;

		// AS 11/12/09 TFS24789 - TabItemDragMode
		private TabItemDragBehavior _tabItemDragBehavior;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="AddToGroupActionBase"/>
		/// </summary>
		/// <param name="group">The group to which the panes are being added</param>
		/// <param name="index">The index at which the elements will be inserted</param>
		protected AddToGroupActionBase(FrameworkElement group, int index)
		{
			DockManagerUtilities.ThrowIfNull(group, "group");

			if (group is IContentPaneContainer == false)
				throw new ArgumentException(XamDockManager.GetString("LE_InvalidAddToGroupElement"), "group");

			this._group = group;
			this._index = index;
		}

		#endregion //Constructor

		#region Properties

		/// <summary>
		/// Returns the group to which the panes will be added.
		/// </summary>
		/// <seealso cref="TabGroupPane"/>
		/// <seealso cref="SplitPane"/>
		public FrameworkElement Group
		{
			get { return this._group; }
		}

		/// <summary>
		/// Returns the index at which the items will be inserted.
		/// </summary>
		public int Index
		{
			get { return this._index; }
		}

		// AS 11/12/09 TFS24789 - TabItemDragMode
		// Note this doesn't affect the result so the IsSameResult doesn't need to check this.
		//
		internal TabItemDragBehavior TabItemDragBehavior
		{
			get { return _tabItemDragBehavior; }
			set { _tabItemDragBehavior = value; }
		}

		#endregion //Properties

		#region Base class overrides

		internal override int GetDragHashCode()
		{
			return this._group.GetHashCode();
		}

		internal override bool IsActionAllowed(DragManager dragManager)
		{
			if (false == dragManager.IsDropLocationAllowed(XamDockManager.GetPaneLocation(this._group)))
				return false;

			// we don't need to check the panes in the group since they are in a tab group 
			// but we don't want a pane that doesn't want to be in a group being added to 
			// a tab group
			// AS 5/15/08 BR32634
			// AllowDockingInTabGroup only applies to dockable areas.
			//
			//if (this._group is TabGroupPane)
			TabGroupPane tabGroup = this._group as TabGroupPane;
			if (null != tabGroup && DockManagerUtilities.IsDockable(XamDockManager.GetPaneLocation(tabGroup)))
			{
				ItemCollection items = tabGroup.Items;
				foreach (ContentPane pane in dragManager.PanesBeingDragged)
				{
					if (pane.AllowDockingInTabGroup == false &&
						DockManagerUtilities.IndexOf(items, pane, true) < 0)
					{
						return false;
					}
				}

				// AS 5/15/08 BR32634
				foreach (object item in tabGroup.Items)
				{
					ContentPane cp = item as ContentPane;

					// also check the placeholder
					if (null == cp && item is ContentPanePlaceholder)
						cp = ((ContentPanePlaceholder)item).Pane;

					// if the pane doesn't allow docking in a tab group and
					// doesn't already belong to the tab group then also block
					// the action
					if (null != cp 
						&& cp.AllowDockingInTabGroup == false
						&& dragManager.PanesBeingDragged.IndexOf(cp) < 0)
					{
						return false;
					}
				}
			}

			return true;
		}

		internal override bool IsSameResult(PaneDragAction action)
		{
			AddToGroupActionBase dragAction = action as AddToGroupActionBase;

			return null != dragAction &&
				dragAction._group == this._group &&
				dragAction._index == this._index;
		}

		internal override void PerformAction(DragManager dragManager, bool isPreview)
		{
			PaneLocation paneLocation = XamDockManager.GetPaneLocation(this._group);

			if (this._group is TabGroupPane)
			{
				#region TabGroupPane
				TabGroupPane tabGroup = (TabGroupPane)this._group;

				if (isPreview)
				{
					// AS 11/12/09 TFS24789 - TabItemDragBehavior
					// When possible (and allowed) we need to show a tab item insertion bar instead of the 
					// tab item drop preview.
					//
					//DropPreviewTabLocation location = DockManagerUtilities.GetPreviewLocation(tabGroup.TabStripPlacement);
					//dragManager.ShowDropPreview(tabGroup, location, Size.Empty, XamDockManager.GetPaneLocation(tabGroup));
					bool showDropPreview = true;

					if (_tabItemDragBehavior == TabItemDragBehavior.DisplayInsertionBar)
					{
						showDropPreview = !dragManager.ShowTabItemPreview(tabGroup, this.Index);
					}

					if (showDropPreview)
					{
						DropPreviewTabLocation location = DockManagerUtilities.GetPreviewLocation(tabGroup.TabStripPlacement);
						dragManager.ShowDropPreview(tabGroup, location, Size.Empty, XamDockManager.GetPaneLocation(tabGroup));
					}
				}
				else
				{
					#region Add To TabGroup

					// AS 12/9/09 TFS25268
					// Cache the selected item so we can try to retain that when the 
					// drop is complete.
					//
					object selectedItem = tabGroup.SelectedItem;

					// for a tab group, we'll flatten all the panes into a single list and
					// insert them in the existing tab group

					int index = this.Index;

					// AS 5/21/08
					// If the item is already in the collection or has a placeholder
					// in the collection before the index at which we will be inserting
					// then we need to decrement the index to account for that
					DockManagerUtilities.AdjustIndexForMove(tabGroup.Items, dragManager.PanesBeingDragged, ref index);

					// AS 5/21/08
					// get the current containers so we can remove them if needed after the move
					IList<IContentPaneContainer> oldContainers = dragManager.GetDragPaneContainers();

                    // AS 10/15/08 TFS8068
                    using (DockManagerUtilities.CreateMoveReplacement(dragManager.PanesBeingDragged))
                    {
                        // first remove all the panes from the group. this will ensure that there
                        // are no placeholders to mess up our positioning logic
                        foreach (ContentPane pane in dragManager.PanesBeingDragged)
                        {
                            IContentPaneContainer oldPaneContainer = pane.PlacementInfo.CurrentContainer;
                            oldPaneContainer.RemoveContentPane(pane, DockManagerUtilities.NeedsPlaceholder(oldPaneContainer.PaneLocation, paneLocation));
                        }

                        IContentPaneContainer newContainer = this._group as IContentPaneContainer;
                        Debug.Assert(index <= tabGroup.Items.Count);

                        index = Math.Min(index, tabGroup.Items.Count);

                        foreach (ContentPane pane in dragManager.PanesBeingDragged)
                        {
                            newContainer.InsertContentPane(index++, pane);
                        }

                        // AS 5/21/08
                        DockManagerUtilities.RemoveContainersIfNeeded(oldContainers);
                    }

					// AS 12/9/09 TFS25268
					int selectionIndex = selectedItem == null ? -1 : tabGroup.Items.IndexOf(selectedItem);

					if (selectionIndex >= 0)
						tabGroup.SelectedIndex = selectionIndex;

					#endregion //Add To TabGroup
				} 
				#endregion //TabGroupPane
			}
			else if (this._group is SplitPane)
			{
				#region SplitPane
				SplitPane split = (SplitPane)this._group;

				if (isPreview)
				{
					#region Preview

					Dock side = split.SplitterOrientation == Orientation.Vertical ? Dock.Left : Dock.Top;

					// assume the default relative size...
					Size relativeSize = (Size)SplitPane.RelativeSizeProperty.DefaultMetadata.DefaultValue;

					// if we're dropping onto documents then we'll create a tab group but
					// otherwise we'll drop the panes being dragged so get that from the drag manager
					if (paneLocation != PaneLocation.Document)
						relativeSize = dragManager.GetMoveToSplitPaneRelativeSize(this._group);

					Rect dropRect = split.PredictDrop(relativeSize, this.Index);
					double offset = split.SplitterOrientation == Orientation.Vertical ? dropRect.X : dropRect.Y;
					dragManager.ShowDropPreview(split, side, dropRect.Size, offset, XamDockManager.GetPaneLocation(split));

					#endregion //Preview
				}
				else
				{
					#region Drop

					// AS 5/21/08
					// get the current containers so we can remove them if needed after the move
					IList<IContentPaneContainer> oldContainers = dragManager.GetDragPaneContainers();

					if (paneLocation == PaneLocation.Document)
					{
						#region Document

						// when adding to an existing document split pane we need to create a new
						// tab group to contain the pane(s) being dragged and insert that

						int index = this.Index;

                        // AS 10/15/08 TFS8068
                        using (DockManagerUtilities.CreateMoveReplacement(dragManager.PanesBeingDragged))
                        {
                            // first remove all the panes from the group. this will ensure that there
                            // are no placeholders to mess up our positioning logic
                            foreach (ContentPane pane in dragManager.PanesBeingDragged)
                            {
                                IContentPaneContainer oldPaneContainer = pane.PlacementInfo.CurrentContainer;
                                oldPaneContainer.RemoveContentPane(pane, DockManagerUtilities.NeedsPlaceholder(oldPaneContainer.PaneLocation, paneLocation));
                            }

                            TabGroupPane group = DockManagerUtilities.CreateTabGroup(dragManager.DockManager);
                            IContentPaneContainer newContainer = group;

							// AS 4/28/11 TFS73532
							// Moved this up so the tabgroup is in the tree before the panes are added to it.
							//
							Debug.Assert(index >= 0 && index <= split.Panes.Count);
                            index = Math.Min(index, split.Panes.Count);
                            split.Panes.Insert(index, group);

                            foreach (ContentPane pane in dragManager.PanesBeingDragged)
                            {
                                newContainer.InsertContentPane(null, pane);
                            }

							// AS 4/28/11 TFS73532
							// Moved up
							//Debug.Assert(index >= 0 && index <= split.Panes.Count);
							//index = Math.Min(index, split.Panes.Count);
							//split.Panes.Insert(index, group);
                        }
						#endregion //Document
					}
					else
					{
						int index = this.Index;

						// AS 5/21/08
						// if the item is already in the collection or has a placeholder
						// in the collection before the index at which we will be inserting
						// then we need to decrement the index to account for that
						DockManagerUtilities.AdjustIndexForMove(split.Panes, dragManager.PanesBeingDragged, ref index);

						// non-document - just add to split
						dragManager.MoveToSplitPane(split, index);
					}


					// AS 5/21/08
					DockManagerUtilities.RemoveContainersIfNeeded(oldContainers);

					#endregion //Drop
				} 
				#endregion //SplitPane
			}
		}

		#endregion //Base class overrides
	} 
	#endregion //AddToGroupActionBase

	#region AddToGroupAction
	/// <summary>
	/// Drag action used when the <see cref="ContentPane"/> instances being dragged will be added to an existing <see cref="TabGroupPane"/> or <see cref="SplitPane"/>
	/// </summary>
	public class AddToGroupAction : AddToGroupActionBase
	{
		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="AddToGroupAction"/>
		/// </summary>
		/// <param name="group">The group to which the panes are being added</param>
		/// <param name="index">The index at which the elements will be inserted</param>
		public AddToGroupAction(FrameworkElement group, int index) : base(group, index)
		{
		}

		#endregion //Constructor

		#region Base class overrides



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


		#endregion //Base class overrides
	} 
	#endregion //AddToGroupActionBase

	#region NewTabGroupAction
	/// <summary>
	/// Drag action used when a new <see cref="TabGroupPane"/> will be created to contain a pane within the <see cref="XamDockManager"/> and the panes being dragged.
	/// </summary>
	/// <seealso cref="TabGroupPane"/>
	public class NewTabGroupAction : PaneDragAction
	{
		#region Member Variables

		private ContentPane _pane;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="NewTabGroupAction"/>
		/// </summary>
		/// <param name="pane">The pane that will be moved into the new <see cref="TabGroupPane"/> with the panes being dragged.</param>
		public NewTabGroupAction(ContentPane pane)
		{
			DockManagerUtilities.ThrowIfNull(pane, "pane");

			if (DockManagerUtilities.GetParentPane(pane) is TabGroupPane)
				throw new InvalidOperationException(XamDockManager.GetString("LE_InvalidNewTabGroupActionPane"));

			this._pane = pane;
		}

		#endregion //Constructor

		#region Properties

		/// <summary>
		/// Returns the pane that will be moved into the new <see cref="TabGroupPane"/> with the panes being dragged.
		/// </summary>
		public ContentPane Pane
		{
			get { return this._pane; }
		}

		#endregion //Properties

		#region Base class overrides

		internal override int GetDragHashCode()
		{
			return this._pane.GetHashCode();
		}

		internal override bool IsActionAllowed(DragManager dragManager)
		{
			if (false == dragManager.IsDropLocationAllowed(XamDockManager.GetPaneLocation(this._pane)))
				return false;

			// AS 5/15/08 BR32634
			// AllowDockingInTabGroup only applies to dockable areas.
			//
			if (DockManagerUtilities.IsDockable(this._pane.PaneLocation))
			{
				// make sure the pane and the panes being dragged support being put into a tab group
				foreach (ContentPane pane in dragManager.PanesBeingDragged)
				{
					if (pane.AllowDockingInTabGroup == false)
						return false;
				}

				if (this._pane.AllowDockingInTabGroup == false)
					return false;
			}

			return true;
		}

		internal override bool IsSameResult(PaneDragAction action)
		{
			NewTabGroupAction dragAction = action as NewTabGroupAction;

			return null != dragAction &&
				dragAction._pane == this._pane;
		}

		internal override void PerformAction(DragManager dragManager, bool isPreview)
		{
			if (isPreview)
			{
				DropPreviewTabLocation location = XamDockManager.GetPaneLocation(this._pane) == PaneLocation.Document
					? DropPreviewTabLocation.Top
					: DropPreviewTabLocation.Bottom;

				dragManager.ShowDropPreview(this._pane, location, Size.Empty, this._pane.PaneLocation);
			}
			else
			{
				IContentPaneContainer container = this._pane.PlacementInfo.CurrentContainer;

				SplitPane split = container as SplitPane;
				Debug.Assert(null != split);

				// AS 9/11/09 TFS21330
				//if (null != split)
				if (null == split)
					return;

				// use a helper class to prevent the toolwindow from reshowing until all the panes have been added
				using (new PaneToolWindow.SuspendReshowHelper(split))
				{
					// create a new tab group to contain the panes being dragged
					TabGroupPane group = DockManagerUtilities.CreateTabGroup(dragManager.DockManager);
					IContentPaneContainer newContainer = group;

					// store the previous index of the pane being removed so we can put the new group there
					int oldIndex = split.Panes.IndexOf(this._pane);

                    // AS 10/15/08 TFS8068
                    using (GroupTempValueReplacement replacements = DockManagerUtilities.CreateMoveReplacement(dragManager.PanesBeingDragged))
                    {
						// AS 9/11/09 TFS21330
						//// AS 10/15/08 TFS8068
						//replacements.Add(DockManagerUtilities.CreateMoveReplacement(this._pane));
						DockManagerUtilities.AddMoveReplacement(replacements, _pane);

						// AS 4/28/11 TFS73532
						// Remove the panes being dragged first so they don't get an extra traversal
						// when we move the _pane into the tab group which would happen for any panes 
						// that are descendants of that only to then have their tree traversed again 
						// when they themselves are moved out and put in the tab group.
						//
						foreach (ContentPane pane in dragManager.PanesBeingDragged)
						{
							IContentPaneContainer oldPaneContainer = pane.PlacementInfo.CurrentContainer;
							oldPaneContainer.RemoveContentPane(pane, DockManagerUtilities.NeedsPlaceholder(oldPaneContainer.PaneLocation, newContainer.PaneLocation));
						}

                        // remove the pane on which the panes are being dropped
                        container.RemoveContentPane(this._pane, false);

                        // AS 5/21/08
                        // get the current containers so we can remove them if needed after the move
                        IList<IContentPaneContainer> oldContainers = dragManager.GetDragPaneContainers();

						// AS 4/28/11 TFS73532
						// Handle the remove and add separately and add them after the newcontainer is in the tree.
						//
						//// move the panes being dragged into the group first
						//foreach (ContentPane pane in dragManager.PanesBeingDragged)
						//    DockManagerUtilities.MovePane(pane, newContainer, null, newContainer.PaneLocation);
						//
						//// then add back the pane being dropped upon
						//newContainer.InsertContentPane(null, this._pane);
						//
						//// lastly put that new tab group into the split where the dropped on pane was
						//split.Panes.Insert(oldIndex, group);

						// first put the new group into the split so we build top-down
						split.Panes.Insert(oldIndex, group);

						// now we can put the panes being dragged back in since the group is in the tree
						foreach (ContentPane pane in dragManager.PanesBeingDragged)
						{
							newContainer.InsertContentPane(null, pane);
						}

						// then add back the pane being dropped upon
						newContainer.InsertContentPane(null, this._pane);

                        // AS 5/21/08
                        DockManagerUtilities.RemoveContainersIfNeeded(oldContainers);
                    }
				}
			}
		}



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


		#endregion //Base class overrides
	} 
	#endregion //NewTabGroupAction

	#region NewSplitPaneAction
	/// <summary>
	/// Drag action used when a new <see cref="SplitPane"/> will be created to contain a pane within the <see cref="XamDockManager"/> and the panes being dragged.
	/// </summary>
	/// <seealso cref="SplitPane"/>
	public class NewSplitPaneAction : PaneDragAction
	{
		#region Member Variables

		private FrameworkElement _pane;
		private Dock _side;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="NewSplitPaneAction"/>
		/// </summary>
		/// <param name="pane">The <see cref="ContentPane"/> or <see cref="TabGroupPane"/> which will be moved into the new <see cref="SplitPane"/> as a sibling to the panes being dragged.</param>
		/// <param name="side">The side on which the new panes will be positioned relative to the <paramref name="pane"/></param>
		public NewSplitPaneAction(FrameworkElement pane, Dock side)
		{
			DockManagerUtilities.ThrowIfNull(pane, "pane");
			DockManagerUtilities.ThrowIfInvalidEnum(side, "side");

			if (pane is TabGroupPane == false && pane is ContentPane == false)
				throw new ArgumentException(XamDockManager.GetString("LE_InvalidNewSplitPaneElement"), "pane");

			this._pane = pane;
			this._side = side;
		}

		#endregion //Constructor

		#region Properties

		/// <summary>
		/// Returns the pane that will be moved into the new <see cref="SplitPane"/> with the panes being dragged.
		/// </summary>
		public FrameworkElement Pane
		{
			get { return this._pane; }
		}

		/// <summary>
		/// Returns the side on which the panes being dragged will be positioned relative to the <see cref="Pane"/> within the new <see cref="SplitPane"/>
		/// </summary>
		public Dock Side
		{
			get { return this._side; }
		}

		#endregion //Properties

		#region Base class overrides

		internal override int GetDragHashCode()
		{
			return this._pane.GetHashCode();
		}

		internal override bool IsActionAllowed(DragManager dragManager)
		{
			return dragManager.IsDropLocationAllowed(XamDockManager.GetPaneLocation(this._pane));
		}

		internal override bool IsSameResult(PaneDragAction action)
		{
			NewSplitPaneAction dragAction = action as NewSplitPaneAction;

			return null != dragAction &&
				dragAction._pane == this._pane &&
				dragAction._side == this._side;
		}

		internal override void PerformAction(DragManager dragManager, bool isPreview)
		{
			PaneLocation newLocation = XamDockManager.GetPaneLocation(this._pane);

			if (isPreview)
			{
				Rect dropRect = SplitPane.PredictDropNewSplit(this.Side, this._pane);
				Dock dropSide = this._side == Dock.Left || this._side == Dock.Right ? Dock.Left : Dock.Top;
				double offset = dropSide == Dock.Left ? dropRect.X : dropRect.Y;
				dragManager.ShowDropPreview(this._pane, dropSide, dropRect.Size, offset, newLocation);
			}
			else
			{
				// see what kind of split we will need
				Orientation splitterOrientation = this.Side == Dock.Left || this.Side == Dock.Right ? Orientation.Vertical : Orientation.Horizontal;

				if (PaneLocation.Document == newLocation)
				{
					#region New Document Split
					// flatten the panes into a single new tab group
					TabGroupPane siblingGroup = this._pane as TabGroupPane ?? DockManagerUtilities.GetParentPane(this._pane) as TabGroupPane;

					Debug.Assert(null != siblingGroup);

					SplitPane containingSplit = DockManagerUtilities.GetParentPane(siblingGroup) as SplitPane;

					if (null != containingSplit)
					{
						TabGroupPane newTabGroup = DockManagerUtilities.CreateTabGroup(dragManager.DockManager);

                        // AS 10/15/08 TFS8068
						// AS 4/28/11 TFS73532
						//using (GroupTempValueReplacement replacements = new GroupTempValueReplacement())
                        using (GroupTempValueReplacement replacements = DockManagerUtilities.CreateMoveReplacement(dragManager.PanesBeingDragged))
                        {
							// AS 9/11/09 TFS21330
							//// AS 10/15/08 TFS8068
							//replacements.Add(DockManagerUtilities.CreateMoveReplacement(newTabGroup, containingSplit));
							DockManagerUtilities.AddMoveReplacement(replacements, newTabGroup, containingSplit);

                            // AS 5/21/08
                            // get the current containers so we can remove them if needed after the move
                            IList<IContentPaneContainer> oldContainers = dragManager.GetDragPaneContainers();

							// AS 4/28/11 TFS73532
							// Remove the panes being dragged first so they don't get an extra traversal
							// when we move the _pane into the tab group which would happen for any panes 
							// that are descendants of that only to then have their tree traversed again 
							// when they themselves are moved out and put in the tab group.
							//
							//// move all the panes into the new tab group
							//foreach (ContentPane pane in dragManager.PanesBeingDragged)
							//    DockManagerUtilities.MovePane(pane, newTabGroup, null, newLocation);
							foreach (ContentPane pane in dragManager.PanesBeingDragged)
							{
								IContentPaneContainer oldPaneContainer = pane.PlacementInfo.CurrentContainer;
								oldPaneContainer.RemoveContentPane(pane, DockManagerUtilities.NeedsPlaceholder(oldPaneContainer.PaneLocation, newLocation));
							}

                            // we're going to need to know where the sibling group is to know where to position
                            // the new group
                            int siblingIndex = containingSplit.Panes.IndexOf(siblingGroup);

                            Debug.Assert(containingSplit.SplitterOrientation != splitterOrientation);

							SplitPane newSplit = DockManagerUtilities.CreateSplitPane(dragManager.DockManager);
                            newSplit.SplitterOrientation = splitterOrientation;

							// AS 9/11/09 TFS21330
							//// AS 10/15/08 TFS8068
							//replacements.Add(DockManagerUtilities.CreateMoveReplacement(siblingGroup));
							DockManagerUtilities.AddMoveReplacement(replacements, siblingGroup);

                            // we need to move the sibling into the new split pane
                            containingSplit.Panes.Remove(siblingGroup);

							// AS 4/28/11 TFS73532
							// Moved up from below.
							//
							// put the new split back where the group used to be
                            containingSplit.Panes.Insert(siblingIndex, newSplit);

                            newSplit.Panes.Add(siblingGroup);
                            SplitPane.SetRelativeSize(newSplit, SplitPane.GetRelativeSize(siblingGroup));

                            // if the side is left or top then put the new tab group before the sibling group
                            int newTabGroupIndex = this.Side == Dock.Left || this.Side == Dock.Top ? 0 : 1;
                            newSplit.Panes.Insert(newTabGroupIndex, newTabGroup);

							// AS 4/28/11 TFS73532
							// Now move the panes back in.
							//
							//// lastly put the new split back where the group used to be
							//containingSplit.Panes.Insert(siblingIndex, newSplit);
							// move all the panes into the new tab group
							IContentPaneContainer newContainer = newTabGroup;
							foreach (ContentPane pane in dragManager.PanesBeingDragged)
								newContainer.InsertContentPane(null, pane);

                            // AS 5/21/08
                            DockManagerUtilities.RemoveContainersIfNeeded(oldContainers);
                        }
					} 
					#endregion //New Document Split
				}
				else
				{
					#region Non-Document Split
					// for non-document types then we want to keep the state of the panes being dragged
					SplitPane containingSplit = DockManagerUtilities.GetParentPane(this._pane) as SplitPane;

					Debug.Assert(null != containingSplit);

					// AS 9/11/09 TFS21330
					//if (null != containingSplit)
					if (null == containingSplit)
						return;

					// use a helper class to prevent the toolwindow from reshowing until all the panes have been added
					using (new PaneToolWindow.SuspendReshowHelper(_pane))
					{
                        // AS 10/15/08 TFS8068
                        using (GroupTempValueReplacement replacements = new GroupTempValueReplacement())
                        {
							IList<IContentPaneContainer> oldContainers = dragManager.GetDragPaneContainers(); // AS 3/17/11 TFS67321

							// AS 9/11/09 TFS21330
							//// AS 10/15/08 TFS8068
							//replacements.Add(DockManagerUtilities.CreateMoveReplacement(this._pane));
							DockManagerUtilities.AddMoveReplacement(replacements, _pane);

                            int siblingIndex = containingSplit.Panes.IndexOf(this._pane);
							SplitPane newSplit = DockManagerUtilities.CreateSplitPane(dragManager.DockManager);
                            newSplit.SplitterOrientation = splitterOrientation;
                            SplitPane.SetRelativeSize(newSplit, SplitPane.GetRelativeSize(this._pane));

							// AS 9/11/09 TFS21330
							//// AS 10/15/08 TFS8068
							//replacements.Add(DockManagerUtilities.CreateMoveReplacement(newSplit, containingSplit));
							DockManagerUtilities.AddMoveReplacement(replacements, newSplit, containingSplit);

                            containingSplit.Panes.RemoveAt(siblingIndex);
							// AS 4/28/11 TFS73532
							// Moved down so we do this after the newSplit is in the tree.
							//
							//newSplit.Panes.Add(this._pane);

                            // move the new split into the spot that contained the pane we split
                            containingSplit.Panes.Insert(siblingIndex, newSplit);

							// AS 4/28/11 TFS73532
							// Moved from above.
							//
							newSplit.Panes.Add(this._pane);

                            // reset the relative size so this pane and the one being moved in split the space
                            this._pane.ClearValue(SplitPane.RelativeSizeProperty);

                            // move the panes being dragged into the split pane
                            int newIndex = this.Side == Dock.Left || this.Side == Dock.Top ? 0 : 1;
                            FrameworkElement movedElement = dragManager.MoveToSplitPane(newSplit, newIndex);

                            // we don't want to carry the old relative size when creating a new split - both
                            // elements should take equal percentages
                            if (null != movedElement)
                                movedElement.ClearValue(SplitPane.RelativeSizeProperty);

							DockManagerUtilities.RemoveContainersIfNeeded(oldContainers); // AS 3/17/11 TFS67321
                        }
					} 
					#endregion //Non-Document Split
				}
			}
		}



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


		#endregion //Base class overrides
	} 
	#endregion //NewSplitPaneAction

	#region NewRootPaneAction
	/// <summary>
	/// Drag action used when creating a new <see cref="SplitPane"/> that will be docked directly to one of the edges of the <see cref="XamDockManager"/> 
	/// </summary>
	public class NewRootPaneAction : PaneDragAction
	{
		#region Member Variables

		private Dock _side;
		private RootSplitPaneLocation _location;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="NewRootPaneAction"/>
		/// </summary>
		/// <param name="side">The side on which the new <see cref="SplitPane"/> will be docked</param>
		/// <param name="location">The location where the new <see cref="SplitPane"/> will be created</param>
		public NewRootPaneAction(Dock side, RootSplitPaneLocation location)
		{
			DockManagerUtilities.ThrowIfInvalidEnum(side, "side");

			this._side = side;
			this._location = location;
		}

		#endregion //Constructor

		#region Properties

		/// <summary>
		/// Returns the side of the <see cref="XamDockManager"/> or <see cref="DocumentContentHost"/> on which the new <see cref="SplitPane"/> will be positioned.
		/// </summary>
		public Dock Side
		{
			get { return this._side; }
		}

		/// <summary>
		/// Returns the location where the new <see cref="SplitPane"/> will be created.
		/// </summary>
		public RootSplitPaneLocation Location
		{
			get { return this._location; }
		}

		#endregion //Properties

		#region Base class overrides

		internal override int GetDragHashCode()
		{
			return this._side.GetHashCode();
		}

		internal override bool IsActionAllowed(DragManager dragManager)
		{
			if (this._location == RootSplitPaneLocation.DocumentContentHost)
				return dragManager.IsDropLocationAllowed(AllowedDropLocations.Document);

			AllowedDropLocations location = 0;

			switch (this._side)
			{
				case Dock.Right:
					location = AllowedDropLocations.Right;
					break;
				case Dock.Left:
					location = AllowedDropLocations.Left;
					break;
				case Dock.Top:
					location = AllowedDropLocations.Top;
					break;
				case Dock.Bottom:
					location = AllowedDropLocations.Bottom;
					break;
			}

			return dragManager.IsDropLocationAllowed(location);
		}

		internal override bool IsSameResult(PaneDragAction action)
		{
			NewRootPaneAction dragAction = action as NewRootPaneAction;

			return null != dragAction &&
				dragAction._location == this._location &&
				dragAction._side == this._side;
		}

		internal override void PerformAction(DragManager dragManager, bool isPreview)
		{
			// see what kind of split we will need
			Orientation splitterOrientation = this.Side == Dock.Left || this.Side == Dock.Right ? Orientation.Vertical : Orientation.Horizontal;

			if (this._location == RootSplitPaneLocation.DocumentContentHost)
			{
				#region Document
				DocumentContentHost host = dragManager.DockManager.DocumentContentHost;

				if (isPreview)
				{
					#region Preview

					Rect dropRect;

					if (splitterOrientation != host.RootSplitterOrientation)
						dropRect = SplitPane.PredictDropNewSplit(this.Side, host.Panel);
					else
					{
						// now we can move the panes being dragged before/after the panes
						Size relativeSize = (Size)SplitPane.RelativeSizeProperty.DefaultMetadata.DefaultValue;
						int newIndex = this.Side == Dock.Left || this.Side == Dock.Top ? 0 : host.Panes.Count;
						dropRect = host.Panel.SplitPane.PredictDrop(relativeSize, newIndex);
					}

					Dock dockSide = this._side == Dock.Left || this._side == Dock.Right ? Dock.Left : Dock.Top;
					double offset = dockSide == Dock.Left ? dropRect.X : dropRect.Y;
					dragManager.ShowDropPreview(host.Panel, dockSide, dropRect.Size, offset, PaneLocation.Document);

					#endregion //Preview
				}
				else
				{
					#region Drop

					// AS 4/28/11 TFS73532
					// Reorganized this block so it follows more of a top-down approach. Since we will 
					// be manipulating the panes before moving the root splits we need to create the move 
					// replacement and get the old containers up front.
					// 
					// AS 10/15/08 TFS8068
					using (DockManagerUtilities.CreateMoveReplacement(dragManager.PanesBeingDragged))
					{
						// AS 5/21/08
						// get the current containers so we can remove them if needed after the move
						IList<IContentPaneContainer> oldContainers = dragManager.GetDragPaneContainers();

						// AS 4/28/11 TFS73532
						// Remove the panes at the start instead of using the MovePane method. This is important to avoid 
						// the tree traversal for these panes if they are within the documentcontenthost if the root split 
						// orientation is being changed.
						//
						foreach (ContentPane pane in dragManager.PanesBeingDragged)
						{
							IContentPaneContainer oldContainer = pane.PlacementInfo.CurrentContainer;
							Debug.Assert(oldContainer != null, "No old container for :" + pane.ToString());
							oldContainer.RemoveContentPane(pane, DockManagerUtilities.NeedsPlaceholder(pane.PaneLocation, PaneLocation.Document));
						}

						// if the root split is not in the same orientation that we want to create
						// then we need to move the current root panes down into a new group
						if (splitterOrientation != host.RootSplitterOrientation)
						{
							// create a new split for all of the current root panes
							SplitPane newRootPanesSplit = DockManagerUtilities.CreateSplitPane(dragManager.DockManager);
							newRootPanesSplit.SplitterOrientation = host.RootSplitterOrientation;

							SplitPane[] rootPanes = new SplitPane[host.Panes.Count];
							host.Panes.CopyTo(rootPanes, 0);

							// AS 10/15/08 TFS8068
							using (DockManagerUtilities.CreateMoveReplacement(rootPanes))
							{
								// AS 4/28/11 TFS73532
								// Moved up from below because we need the split in the tree before moving the 
								// splits into it.
								//
								// change the orientation of the root splits
								host.RootSplitterOrientation = splitterOrientation;

								// move the new split into the root split
								host.Panes.Add(newRootPanesSplit);

								// move the panes into the new root split
								foreach (SplitPane pane in rootPanes)
								{
									host.Panes.Remove(pane);
									newRootPanesSplit.Panes.Add(pane);
								}

								// AS 4/28/11 TFS73532
								//// change the orientation of the root splits
								//host.RootSplitterOrientation = splitterOrientation;
								//
								//// move the new split into the root split
								//host.Panes.Add(newRootPanesSplit);
							}
						}

						// create a tab group to contain all the panes being dragged
						SplitPane newSplit = DockManagerUtilities.CreateSplitPane(dragManager.DockManager);
						TabGroupPane newTabGroup = DockManagerUtilities.CreateTabGroup(dragManager.DockManager);

						// AS 4/28/11 TFS73532
						// Instead of removing and readding together, we're going to remove them up above 
						// and then re-add them below after the tab group and new split are in the tree.
						// In this way WPF should only traverse the elements during the remove and add 
						// as opposed to doing it on the remove of the ancestor and readd of the ancestor 
						// if the root splitter orientation is changing.
						//
						//foreach (ContentPane pane in dragManager.PanesBeingDragged)
						//    DockManagerUtilities.MovePane(pane, newTabGroup, null, PaneLocation.Document);

						// now we can move the panes being dragged before/after the panes
						int newIndex = this.Side == Dock.Left || this.Side == Dock.Top ? 0 : host.Panes.Count;
						newSplit.Panes.Add(newTabGroup);
						host.Panes.Insert(newIndex, newSplit);

						// AS 4/28/11 TFS73532
						IContentPaneContainer newContainer = newTabGroup;
						foreach (ContentPane pane in dragManager.PanesBeingDragged)
						{
							newContainer.InsertContentPane(null, pane);
						}

						// AS 5/21/08
						DockManagerUtilities.RemoveContainersIfNeeded(oldContainers);
					}
					#endregion //Drop
				} 
				#endregion //Document
			}
			else
			{
				DockManagerPanel panel = dragManager.DockManager.DockPanel;

				bool isOuter = this.Location == RootSplitPaneLocation.OuterDockManagerEdge;
				Size maxSize = isOuter ? new Size(panel.ActualWidth, panel.ActualHeight) : panel.LastContentRect.Size;
				double maxThickness = this._side == Dock.Left || this._side == Dock.Right ? maxSize.Width / 2 : maxSize.Height / 2;
				Size paneSize = dragManager.CurrentDragPaneSize;
				double thickness = Math.Min(maxThickness, this._side == Dock.Left || this._side == Dock.Right ? paneSize.Width : paneSize.Height);

				// AS 5/2/08 BR32290
				// VS seems to have a minimum extent for new root split panes of 60 pixels.
				//
				const double minThickness = 60d;
				thickness = Math.Max(minThickness, thickness);

				#region Inner|Outer DockManagerEdge
				if (isPreview)
				{
					#region Preview

					Size size;

					if (this._side == Dock.Left || this._side == Dock.Right)
						size = new Size(thickness, double.NaN);
					else
						size = new Size(double.NaN, thickness);

                    PaneLocation paneLocation = DockManagerUtilities.GetDockedLocation(this.Side);

					if (this._location == RootSplitPaneLocation.OuterDockManagerEdge)
					{
						dragManager.ShowDropPreview(panel, this.Side, size, 0d, paneLocation);
					}
					else
					{
						#region InnerDockManagerEdge

						double offsetX = 0;
						double offsetY = 0;

						HorizontalAlignment horzAlign = HorizontalAlignment.Stretch;
						VerticalAlignment vertAlign = VerticalAlignment.Stretch;

						// AS 10/5/09 NA 2010.1 - LayoutMode
						// In fill mode the innermost dock pane will be the fill pane.
						//
						if (dragManager.DockManager.LayoutMode == DockedPaneLayoutMode.FillContainer)
							size = panel.LastContentRect.Size;
						else
						{
							Rect contentRect = panel.LastContentRect;

							switch (this._side)
							{
								case Dock.Left:
									horzAlign = HorizontalAlignment.Left;
									offsetX = contentRect.X;
									break;
								case Dock.Right:
									horzAlign = HorizontalAlignment.Right;
									offsetX -= (panel.ActualWidth - contentRect.Right);
									break;
								case Dock.Top:
									vertAlign = VerticalAlignment.Top;
									offsetY = contentRect.Y;
									break;
								case Dock.Bottom:
									vertAlign = VerticalAlignment.Bottom;
									offsetY -= (panel.ActualHeight - contentRect.Bottom);
									break;
							}

							if (this._side == Dock.Left || this._side == Dock.Right)
							{
								vertAlign = VerticalAlignment.Top;
								offsetY = contentRect.Y;
								size.Height = contentRect.Height;
							}
							else
							{
								horzAlign = HorizontalAlignment.Left;
								offsetX = contentRect.X;
								size.Width = contentRect.Width;
							}
						}

                        dragManager.ShowDropPreview(panel, DropPreviewTabLocation.None, size, horzAlign, vertAlign, offsetX, offsetY, paneLocation);

						#endregion //InnerDockManagerEdge
					}

					#endregion //Preview
				}
				else
				{
					// this will be a new split in the xdm either in the middle or outside
					dragManager.MoveToNewRootSplitPane(this.Side, isOuter, thickness);
				} 
				#endregion //Inner|Outer DockManagerEdge
			}
		}



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


		#endregion //Base class overrides
	} 
	#endregion //NewRootPaneAction

	#region FloatPaneAction
	/// <summary>
	/// Drag action that results in the panes being dragged being displayed within a new <see cref="PaneToolWindow"/>
	/// </summary>
	public class FloatPaneAction : PaneDragAction
	{
		#region Member Variables

		private Size _size;
		private Point _location;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="FloatPaneAction"/>
		/// </summary>
		/// <param name="location">The location of the new floating pane</param>
		/// <param name="size">The size of the new floating pane</param>
		public FloatPaneAction(Point location, Size size)
		{
			this._location = location;
			this._size = size;
		}

		#endregion //Constructor

		#region Properties

		/// <summary>
		/// Returns the location at which the new floating window will be positioned.
		/// </summary>
		public Point Location
		{
			get { return this._location; }
			// AS 9/29/09 NA 2010.1 - PaneDragAction
			[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_MiscDockManagerFeatures_10_1)]
			set { _location = value; }
		}

		/// <summary>
		/// Returns the size of the new floating window.
		/// </summary>
		public Size Size
		{
			get { return this._size; }
			// AS 9/29/09 NA 2010.1 - PaneDragAction
			[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_MiscDockManagerFeatures_10_1)]
			set { this._size = value; }
		}

		#endregion //Properties

		#region Base class overrides

		internal override int GetDragHashCode()
		{
			return 100;
		}

		internal override bool IsActionAllowed(DragManager dragManager)
		{
			return dragManager.IsDropLocationAllowed(AllowedDropLocations.Floating);
		}

		internal override bool IsSameResult(PaneDragAction action)
		{
			return action is FloatPaneAction;
		}

		internal override void PerformAction(DragManager dragManager, bool isPreview)
		{
            // AS 3/13/09 FloatingWindowDragMode
            if (isPreview && dragManager.DeferFloatingWindowPositioning)
            {
                // Note: This size will not include the non-client area because we do not 
                // know what the non-client size will be. For that we would need the PaneToolWindow 
                // for the pane being floated but since positioning is deferred we will not 
                // have the toolwindow until the drop has occurred.
                //
                Size size = this.Size;
                dragManager.CreateFloatingPreviewWindow(this.Location, size);
                return;
            }

			if (dragManager.WindowBeingDragged == null)
			{
				dragManager.CreateWindowBeingDragged(this.Location, this.Size);
			}
		}



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


		#endregion //Base class overrides
	} 
	#endregion //FloatPaneAction

	#region AddToDocumentHostAction
	/// <summary>
	/// Drag action used when adding the panes being dragged into the <see cref="DocumentContentHost"/> of the <see cref="XamDockManager"/>
	/// </summary>
	public class AddToDocumentHostAction : PaneDragAction
	{
		#region Member Variables

		private TabGroupPane _group;
		private int _itemIndex;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="AddToDocumentHostAction"/>
		/// </summary>
		/// <param name="group">The group to which the panes being dragged will be added or null if a new group must be created</param>
		/// <param name="itemIndex">The index within the <see cref="TabGroupPane"/> at which the panes will be inserted</param>
		public AddToDocumentHostAction(TabGroupPane group, int itemIndex)
		{
			this._group = group;
			this._itemIndex = itemIndex;
		}

		#endregion //Constructor

		#region Properties

		/// <summary>
		/// Returns the group within the <see cref="DocumentContentHost"/> to which the panes will be added or null if a new group will be created
		/// </summary>
		public TabGroupPane Group
		{
			get { return this._group; }
		}

		/// <summary>
		/// Returns the index within the items collection of the <see cref="Group"/> at which the panes being dragged will be inserted
		/// </summary>
		public int ItemIndex
		{
			get { return this._itemIndex; }
		} 

		#endregion //Properties

		#region Base class overrides

		internal override int GetDragHashCode()
		{
			return this._group != null ? this._group.GetHashCode() : 101;
		}

		internal override bool IsActionAllowed(DragManager dragManager)
		{
			return dragManager.IsDropLocationAllowed(AllowedDropLocations.Document);
		}

		internal override bool IsSameResult(PaneDragAction action)
		{
			AddToDocumentHostAction dragAction = action as AddToDocumentHostAction;

			return null != dragAction &&
				dragAction._group == this._group;
		}

		internal override void PerformAction(DragManager dragManager, bool isPreview)
		{
			if (isPreview)
			{
				dragManager.ShowDropPreview(dragManager.DockManager.DocumentContentHost, DropPreviewTabLocation.None, Size.Empty, PaneLocation.Document);
			}
			else
			{
				foreach (ContentPane pane in dragManager.PanesBeingDragged)
					pane.ChangeToDocument();
			}
		}



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


		#endregion //Base class overrides
	}
	#endregion //AddToDocumentHostAction

	#region MoveInGroupAction
	/// <summary>
	/// Drag action used when repositioning panes within their containing group
	/// </summary>
	public class MoveInGroupAction : AddToGroupActionBase
	{
		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="MoveInGroupAction"/>
		/// </summary>
		/// <param name="group">The group containing the panes being dragged</param>
		/// <param name="index">The index in the containing collection where the pane being dragged will be positioned.</param>
		public MoveInGroupAction(FrameworkElement group, int index) : base(group, index)
		{
		}

		#endregion //Constructor

		#region Base class overrides

		#region PerformAction
		internal override void PerformAction(DragManager dragManager, bool isPreview)
		{
			// AS 12/9/09 TFS25268
			// If the pane is already in the collection at the specified index then 
			// we don't need to do anything. This is important since doing something 
			// could result in a selection change.
			//
			if (!isPreview && dragManager.PanesBeingDragged.Count == 1)
			{
				ContentPane cp = dragManager.PanesBeingDragged[0];
				IPaneContainer ipc = this.Group as IPaneContainer;

				if (null != ipc && ipc.Panes.IndexOf(cp) == this.Index)
				{
					return;
				}
			}

			base.PerformAction(dragManager, isPreview);
		} 
		#endregion //PerformAction



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		#endregion //Base class overrides
	} 
	#endregion //MoveInGroupAction

	// AS 5/28/08 RaisePaneDragOverForInvalidLocations
	#region InvalidDropLocation
	internal class InvalidDropLocation : PaneDragAction
	{
		#region Member Variables

		internal static readonly InvalidDropLocation Instance = new InvalidDropLocation();

		#endregion //Member Variables

		#region Constructor
		private InvalidDropLocation()
		{
		}
		#endregion //Constructor

		#region Base class overrides
		internal override int GetDragHashCode()
		{
			return 1;
		}

		internal override bool IsSameResult(PaneDragAction action)
		{
			return action == this;
		}

		internal override void PerformAction(DragManager dragManager, bool isPreview)
		{
			return;
		}

		internal override bool IsActionAllowed(DragManager dragManager)
		{
			return false;
		}
		#endregion //Base class overrides
	} 
	#endregion //InvalidDropLocation

	#region DragActionComparer
	internal class DragActionComparer : IEqualityComparer<PaneDragAction>
	{
		public bool Equals(PaneDragAction x, PaneDragAction y)
		{
			if (x == null || y == null)
				return x == y;

			if (x.GetType() != y.GetType())
				return false;

			return x.IsSameResult(y);
		}

		public int GetHashCode(PaneDragAction obj)
		{
			return obj.GetDragHashCode();
		}
	} 
	#endregion //DragActionComparer
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