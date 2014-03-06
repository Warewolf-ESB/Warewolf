using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Collections.Specialized;
using Infragistics.Windows.Helpers;
using System.Collections.ObjectModel;
using Infragistics.Collections;
using System.Windows.Controls;
using System.Windows.Media;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// Helper class for maintaining information about the unpinned areas on the edges of the <see cref="XamDockManager"/>
	/// </summary>
	internal class UnpinnedTabAreaInfo : IContentPaneContainer
	{
		#region Member Variables

		private XamDockManager _dockManager;
		private UnpinnedTabArea _tabArea;
		private ObservableCollectionExtended<ContentPane> _panes;
		private ReadOnlyObservableCollection<ContentPane> _readOnlyPanes;
		private bool _sortingPanes;

		#endregion //Member Variables

		#region Constructor
		internal UnpinnedTabAreaInfo(XamDockManager dockManager)
		{
			DockManagerUtilities.ThrowIfNull(dockManager, "dockManager");

			this._dockManager = dockManager;
			this._panes = new ObservableCollectionExtended<ContentPane>();
			this._panes.CollectionChanged += new NotifyCollectionChangedEventHandler(OnPanesChanged);
			this._readOnlyPanes = new ReadOnlyObservableCollection<ContentPane>(this._panes);
		}
		#endregion //Constructor

		#region Properties
		public ReadOnlyObservableCollection<ContentPane> Panes
		{
			get { return this._readOnlyPanes; }
		}

		public UnpinnedTabArea TabArea
		{
			get { return this._tabArea; }
			set
			{
				if (value != this._tabArea)
				{
					// AS 9/16/09 TFS22219
					// Added the using block since we will be changing the logical 
					// parent of the panes.
					//
					using (DockManagerUtilities.CreateMoveReplacement(_panes))
					{
						if (this._tabArea != null)
						{
							this._tabArea.ItemsSource = null;

							// add all unpinned panes as logical children of the area
							foreach (ContentPane pane in this._panes)
								this._tabArea.RemoveLogicalChildInternal(pane);

							this._tabArea.ClearValue(XamDockManager.PaneLocationPropertyKey);
						}
						else
						{
							// AS 9/16/09 TFS22219
							// If we didn't have a tab area then any added panes were made logical
							// children of an element in the xdm so they remain in the logical tree
							// and won't be unnecessarily removed.
							//
							foreach (ContentPane pane in _panes)
								_dockManager.UnpinnedPaneHolder.RemoveLogicalChildInternal(pane);
						}

						this._tabArea = value;

						if (this._tabArea != null)
						{
							this._tabArea.SetValue(XamDockManager.PaneLocationPropertyKey, DockManagerKnownBoxes.PaneLocationUnpinnedBox);

							// AS 4/28/11 TFS73532
							// Moved up so they're in the logical tree first.
							//
							foreach (ContentPane pane in this._panes)
								this._tabArea.AddLogicalChildInternal(pane);

							this._tabArea.ItemsSource = this.Panes;

							// AS 4/28/11 TFS73532
							// Normally objects are added to the logical tree before they have the change to be in the 
							// visual tree so just in case I think we should add these as logical children first so I'm 
							// moving this up.
							//
							//// add all unpinned panes as logical children of the area
							//foreach (ContentPane pane in this._panes)
							//    this._tabArea.AddLogicalChildInternal(pane);
						}
						else
						{
							// AS 9/16/09 TFS22219
							// If we don't have a tab area then we need to contain them in the 
							// dockmanager's logical tree anyway to ensure they don't get removed
							// accidentally.
							//
							foreach (ContentPane pane in _panes)
								_dockManager.UnpinnedPaneHolder.AddLogicalChildInternal(pane);
						}
					}
				}
			}
		}
		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region AddPane
		/// <summary>
		/// Adds a pane to the unpinned area
		/// </summary>
		/// <param name="pane">The pane to add</param>
		internal void AddPane(ContentPane pane)
		{
			// add it at the end of the group if there are other 
			// panes from the same group
			int newIndex = this._panes.Count - 1;

			for (; newIndex >= 0; newIndex--)
			{
				if (pane.PlacementInfo.IsInSameDockedContainer(this._panes[newIndex]))
					break;
			}

			if (newIndex < 0)
				newIndex = this._panes.Count;
			else // position after the item with the same group
				newIndex++;

			this._panes.Insert(newIndex, pane);
		}
		#endregion //AddPane

		#region RemovePane
		/// <summary>
		/// Removes a pane from the unpinned area
		/// </summary>
		/// <param name="pane">The pane to remove</param>
		internal void RemovePane(ContentPane pane)
		{
			this._panes.Remove(pane);
		}
		#endregion //RemovePane

		#region SortPanes
		/// <summary>
		/// Helper method to sort the panes in the list
		/// </summary>
		/// <param name="list">A list of sort order objects that must match the panes in the unpinned area</param>
		internal void SortPanes(List<SortOrderInfo> list)
		{
			Debug.Assert(this._sortingPanes == false);

			if (this._sortingPanes)
				return;

			// sort list
			list.Sort();

			Debug.Assert(list.Count == this._panes.Count);

			this._panes.BeginUpdate();

			this._sortingPanes = true;

			try
			{
				// there could have been panes in the unpinned area that were not
				// part of the loaded layout so the supplied list should be the panes
				// after the original panes.
				int start = Math.Max(0, this._panes.Count - list.Count);

				for (int i = start, count = list.Count; i < count; i++)
				{
					ContentPane sortedPane = list[i].Pane;
					int currentIndex = this._panes.IndexOf(sortedPane);

					Debug.Assert(currentIndex >= 0);

					if (currentIndex != i && currentIndex >= 0)
						this._panes.Move(currentIndex, i);
				}

			}
			finally
			{
				this._panes.EndUpdate();
				this._sortingPanes = false;
			}
		}
		#endregion //SortPanes

		#endregion //Internal Methods

		#region Private Methods

		#region DirtyMeasure
		private void DirtyMeasure()
		{
			// AS 10/12/10 TFS57232
			// The UnpinnedTabArea is a sibling to the DockManagerPanel (at least in our default
			// template) so that DockPanel will give whatever space is left to the DockManagerPanel 
			// after the UnpinnedTabArea gets measured and its desired size is carved out. However 
			// when you unpin a pane and we add the pane to the items collection of the itemscontrol 
			// (the unpinnedtabarea) that only dirties the itemspanel within. So the unpinned tab
			// area will get remeasured with the size it was last but since its measure is valid 
			// that's as far as it goes. So the dockmanagerpanel is given all the space. Once the 
			// wpf framework gets to the itemspanel, the itemcontainergenerator generates an element 
			// and the panel's desired size changes which eventually percolates up the ancestor 
			// chain and the dockmanager panel will get remeasured with less space as it should. 
			// This isn't wrong per se since that is how the wpf framework works but we can try 
			// to avoid the extra measure by invalidating the measure up the chain between the 
			// panel and the parent of the unpinned tab area so when the dockpanel measures the 
			// unpinned tab area, its descendants will be measured and the unpinnedtabarea can
			// consider its descendants more accurately before the dockpanel moves on to the 
			// adjacent elements.
			//
			if (_tabArea != null && _tabArea.IsMeasureValid)
			{
				var tabAreaParent = VisualTreeHelper.GetParent(_tabArea) as UIElement;

				if (null != tabAreaParent)
				{
					var itemsPresenter = Utilities.GetTemplateChild<ItemsPresenter>(_tabArea);

					if (itemsPresenter != null && VisualTreeHelper.GetChildrenCount(itemsPresenter) == 1)
					{
						DependencyObject descendant = VisualTreeHelper.GetChild(itemsPresenter, 0);

						while (descendant != null)
						{
							UIElement element = descendant as UIElement;

							if (element != null)
								element.InvalidateMeasure();

							if (descendant == tabAreaParent)
								break;

							descendant = VisualTreeHelper.GetParent(descendant);
						}
					}
				}
			}
		} 
		#endregion //DirtyMeasure

		#region OnPanesChanged
		private void OnPanesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// when panes are added/remove we need to add/remove it from the logical
			// children of the associated tab area
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				Debug.Assert(e.NewItems.Count == 1);

				if (e.NewItems.Count != 1)
					throw new InvalidOperationException();

				DockManagerUtilities.InitializeCurrentContainer(e.NewItems, this);

				ContentPane pane = e.NewItems[0] as ContentPane;

				if (this._tabArea != null)
					this._tabArea.AddLogicalChildInternal(pane);
				else
				{
					// AS 9/16/09 TFS22219
					_dockManager.UnpinnedPaneHolder.AddLogicalChildInternal(pane);
				}

				// we need to keep the panes in the visual tree and in such a way that they
				// will be able to remain the active/focused element so we need to put them
				// into a container in the dockmanagerpanel when they are unpinned.
				if (null != pane)
				{
					DockManagerPanel panel = this._dockManager.DockPanel;

					if (null != panel)
						panel.AddUnpinnedPane(pane);
				}

				// AS 10/12/10 TFS57232
				this.DirtyMeasure();
			}
			else if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				Debug.Assert(e.OldItems.Count == 1);

				// AS 9/16/09 TFS22219
				// Moved the count check and cast out of the _tabArea if block below.
				//
				if (e.OldItems.Count != 1)
					throw new InvalidOperationException();

				ContentPane pane = e.OldItems[0] as ContentPane;

				if (this._tabArea != null)
				{
					this._tabArea.RemoveLogicalChildInternal(pane);
					this._dockManager.HideFlyout(pane, false, false, true, false);

					DockManagerPanel panel = this._dockManager.DockPanel;

					// when the pane is no longer unpinned pull it out of the unpinned
					// container so it can be put into the visual tree elsewhere.
					if (null != panel)
						panel.RemoveUnpinnedPane(pane);
				}
				else
				{
					_dockManager.UnpinnedPaneHolder.RemoveLogicalChildInternal(pane);
				}

				DockManagerUtilities.InitializeCurrentContainer(e.OldItems, null);

				// AS 10/12/10 TFS57232
				this.DirtyMeasure();
			}
			else
			{
				if (this._sortingPanes && e.Action == NotifyCollectionChangedAction.Reset)
					return;

				Debug.Fail("Unexpected action!:" + e.Action.ToString());
			}
		}
		#endregion //OnPanesChanged

		#endregion //Private Methods

		#endregion //Methods

		#region IContentPaneContainer Members

		PaneLocation IContentPaneContainer.PaneLocation
		{
			get
			{
				return PaneLocation.Unpinned;
			}
		}

		FrameworkElement IContentPaneContainer.ContainerElement
		{
			get { return this._tabArea; }
		}

		void IContentPaneContainer.RemoveContentPane(ContentPane pane, bool replaceWithPlaceholder)
		{
			Debug.Assert(false == replaceWithPlaceholder);

			this.RemovePane(pane);
		}

		void IContentPaneContainer.InsertContentPane(int? newIndex, ContentPane pane)
		{
			Debug.Assert(this._panes.Contains(pane) == false);
			Debug.Assert(newIndex == null, "The item is always inserted based on where the group is");

			this.AddPane(pane);
		}

		IList<ContentPane> IContentPaneContainer.GetVisiblePanes()
		{
			return DockManagerUtilities.CreateVisiblePaneList(this._panes);
		}

		IList<ContentPane> IContentPaneContainer.GetAllPanesForPaneAction(ContentPane pane)
		{
			IList<ContentPane> panes = new List<ContentPane>();

			foreach (ContentPane child in this.Panes)
			{
				if (child.Visibility == Visibility.Visible && child.PlacementInfo.IsInSameDockedContainer(pane))
					panes.Add(child);
			}

			return panes;
		}
		#endregion //IContentPaneContainer

		#region SortOrderInfo
		internal class SortOrderInfo : IComparable<SortOrderInfo>
		{
			#region Member Variables

			internal ContentPane Pane;
			internal int Order;

			#endregion //Member Variables

			#region Constructor
			internal SortOrderInfo(ContentPane pane, int order)
			{
				this.Pane = pane;
				this.Order = order;
			} 
			#endregion //Constructor

			#region IComparable<SortOrderInfo> Members

			public int CompareTo(SortOrderInfo other)
			{
				Debug.Assert(other != null);

				if (other == null)
					return 1;

				if (this.Pane == other.Pane)
				{
					Debug.Assert(this.Order == other.Order);
					return 0;
				}

				return this.Order.CompareTo(other.Order);
			}

			#endregion
		} 
		#endregion //SortOrderInfo
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