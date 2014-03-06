using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using Infragistics.Windows.Helpers;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.ComponentModel;
using Infragistics.Windows.Controls;
using System.Windows.Input;
using Infragistics.Collections;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// Element used to host the root docked <see cref="SplitPane"/> instances in a <see cref="XamDockManager"/>
	/// </summary>
	[ContentProperty("Child")]
	//[ToolboxItem(false)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class DockManagerPanel : FrameworkElement,
		ISplitElementCollectionOwner
        // AS 3/30/09 TFS16355 - WinForms Interop
        , IHwndHostInfoOwner
	{
		#region Member Variables

		private UIElement _child;
		private ObservableSplitElementCollection<SplitPane> _panes;
		private UnpinnedTabFlyout _flyoutPanel;
		private Rect _lastContentRect;

		// AS 5/13/08
		// An unpinned pane can remain active even when the flyout is closed. In order to accomplish
		// this, the pane was remain "visible" or else the wpf framework will shift focus elsewhere.
		// To accomplish this we will put all unpinned panes into a custom element - the UnpinnedPaneContainer.
		// This element has a 0 opacity so it will not be rendered but we need to keep its Visibility
		// set to Visible or else the IsVisible of it and all its descendants will be true. Since it
		// is visible, we'll also set its HitTestVisible to false so it cannot receive mouse events.
		// Since unpinned elements are not in the visual tree, we need to be careful to remove them
		// from the container when a pane is pinned or when the template of the dm has changed.
		//
		private UnpinnedPaneContainer _unpinnedContainer;

		// AS 5/16/08 BR32579
		private UIElement _elementBeingMeasured;

        // AS 3/30/09 TFS16355 - WinForms Interop
        private ToolWindow _flyoutWindow;
        internal static readonly object FlyoutToolWindowId = new object();

        #endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="DockManagerPanel"/>
		/// </summary>
		public DockManagerPanel()
		{
			this._panes = new ObservableSplitElementCollection<SplitPane>(this, PaneSplitterMode.SinglePane);
			this._panes.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnPanesChanged);

            // AS 3/30/09 TFS16355 - WinForms Interop
            HwndHostInfo.SetHwndHost(this, new HwndHostInfo(this));
		}

		static DockManagerPanel()
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
			Rect finalRect = new Rect(finalSize);

			
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

			List<PaneSizeInfo> sizeInfos = new List<PaneSizeInfo>();

			// AS 10/5/09 NA 2010.1 - LayoutMode
			// The fill element may be the last pane.
			//
			UIElement fillElement = this.FillElement;

			Size extentUsed = new Size();
			Size reducableSize = new Size();

			#region Step 1 - Gather the SplitPane size information

			// build the list of information about the pane size
			for (int i = 0, count = this._panes.Count; i < count; i++)
			{
				SplitPane pane = this._panes[i];

				// AS 10/5/09 NA 2010.1 - LayoutMode
				// Ignore the fill pane since we will position it separately below.
				//
				if (pane == fillElement)
					continue;

				// AS 10/5/09 NA 2010.1 - LayoutMode
				// The pane and splitter visibilities are disconnected. One may be visible
				// without the other being visible.
				//
				PaneSizeInfo sizeInfo = new PaneSizeInfo(pane);

				// AS 10/5/09 NA 2010.1 - LayoutMode
				//if (pane.Visibility != Visibility.Collapsed)
				if (sizeInfo.IsPaneVisible || sizeInfo.IsSplitterVisible)
				{
					// AS 10/5/09 NA 2010.1 - LayoutMode
					//PaneSizeInfo sizeInfo = new PaneSizeInfo(pane);
					sizeInfos.Add(sizeInfo);

					bool isLeftRight = sizeInfo.IsLeftRight;

					// AS 10/5/09 NA 2010.1 - LayoutMode
					// Added if block since the splitter/pane visibilities are separated.
					//
					if (sizeInfo.IsPaneVisible)
					{
						Size desiredSize = pane.DesiredSize;

						// explicit extent
						// AS 9/24/09 TFS22599
						//sizeInfo.ExplicitExtent = isLeftRight ? pane.Width : pane.Height;
						sizeInfo.ExplicitExtent = DockManagerUtilities.GetExtent(pane, false, isLeftRight);

						// preferred extent - store the explicit extent if we have one
						if (double.IsNaN(sizeInfo.ExplicitExtent))
							sizeInfo.PreferredExtent = isLeftRight ? desiredSize.Width : desiredSize.Height;
						else
						{
							sizeInfo.PreferredExtent = sizeInfo.ExplicitExtent;

							if (isLeftRight)
								reducableSize.Width += sizeInfo.ExplicitExtent - sizeInfo.ExplicitMinExtent;
							else
								reducableSize.Height += sizeInfo.ExplicitExtent - sizeInfo.ExplicitMinExtent;
						}

						// minimum extent
						sizeInfo.MinExtent = GetMinimumExtent(pane, isLeftRight);
					}

					// splitter
					PaneSplitter splitter = sizeInfo.Splitter;
					// AS 10/5/09 NA 2010.1 - LayoutMode
					// The splitter could be collapsed.
					//
					//if (null != splitter)
					if (sizeInfo.IsSplitterVisible)
					{
						sizeInfo.SplitterExtent = isLeftRight ? splitter.DesiredSize.Width : splitter.DesiredSize.Height;
					}

					if (isLeftRight)
					{
						extentUsed.Width += sizeInfo.PreferredExtent + sizeInfo.SplitterExtent;
					}
					else
					{
						extentUsed.Height += sizeInfo.PreferredExtent + sizeInfo.SplitterExtent;
					}
				}
			} 
			#endregion //Step 1 - Gather the SplitPane size information

			#region Step 2 - Resize Elements If There Is Not Enough Room
			Size minContentSize = this.CalculateMinimumContentSize();

			// reduce widths of those docked left/right and the heights of those docked top/bottom
			double widthToReduce = Math.Min(reducableSize.Width, (minContentSize.Width + extentUsed.Width) - finalSize.Width);
			double heightToReduce = Math.Min(reducableSize.Height, (minContentSize.Height + extentUsed.Height) - finalSize.Height);
			bool reduceTowardsExplicitMin = false;

			// AS 5/5/10 TFS29190
			//while (widthToReduce > 0 || heightToReduce > 0)
			while (DockManagerUtilities.IsGreaterThan(widthToReduce, 0) || DockManagerUtilities.IsGreaterThan(heightToReduce, 0))
			{
				// AS 6/12/12 TFS111874
				// A minimize WinForm Form will layout the child controls with a 0,0 size when it is minimized. That ultimately 
				// gets passed down the by the ElementHost to the child elements it contains. When a root docked split pane has 
				// an explicit extent (e.g. the splitter bar was dragged or the pane was moved) then the panel will reduce the 
				// extent of that splitpane (partially to mimic VS 2008 upon which this was modeled but also because in WPF if 
				// an element has an explicit width/height it will be arranged at that regardless of what the ancestor arranges 
				// it with and it will just clip the contents which is not what we want). We want to continue that behavior but 
				// not in this situation. The simplest (and safest) way to work around this winforms behavior is to simply not 
				// resize the element's extent when the DockManagerPanel's content won't be visible anyway - i.e. when it is 
				// being arranged with a width and/or height of 0.
				//
				bool shouldReduceExplicitSize = CoreUtilities.GreaterThan(finalSize.Width, 0) && CoreUtilities.GreaterThan(finalSize.Height, 0);

				double totalReduction = 0;

				// reduce the size of the panes from innermost to outermost
				for (int i = sizeInfos.Count - 1; i >= 0; i--)
				{
					PaneSizeInfo sizeInfo = sizeInfos[i];

					// AS 10/5/09 NA 2010.1 - LayoutMode
					if (!sizeInfo.IsPaneVisible)
						continue;

					if (false == double.IsNaN(sizeInfo.ExplicitExtent))
					{
						bool isLeftRight = sizeInfo.IsLeftRight;
						double amtToReduce = isLeftRight ? widthToReduce : heightToReduce;

						// AS 5/5/10 TFS29190
						//if (amtToReduce > 0)
						if (DockManagerUtilities.IsGreaterThan(amtToReduce,0))
						{
							double reduction = 0;

							if (reduceTowardsExplicitMin == false)
								reduction = sizeInfo.ExplicitExtent - sizeInfo.MinExtent;
							else if (sizeInfo.MinExtent > sizeInfo.ExplicitMinExtent)
							{
								// incrementally reduce it
								reduction = Math.Min(1, sizeInfo.MinExtent - sizeInfo.ExplicitMinExtent);
								sizeInfo.MinExtent -= reduction;
							}
							else
							{
								// don't reduce below the explicit min
								continue;
							}

							// make sure we don't remove more than we have to
							reduction = Math.Min(amtToReduce, reduction);

							if (reduction > 0)
							{
								totalReduction += reduction;

								sizeInfo.PreferredExtent -= reduction;

								Size availableSize = sizeInfo.Pane.DesiredSize;

								if (isLeftRight)
								{
									// AS 6/12/12 TFS111874
									if (shouldReduceExplicitSize)
										sizeInfo.Pane.Width -= reduction;

									widthToReduce -= reduction;
									availableSize.Width = sizeInfo.PreferredExtent;
								}
								else
								{
									// AS 6/12/12 TFS111874
									if (shouldReduceExplicitSize)
										sizeInfo.Pane.Height -= reduction;

									heightToReduce -= reduction;
                                    // AS 11/5/08 TFS10203
									//availableSize.Width = sizeInfo.PreferredExtent;
                                    availableSize.Height = sizeInfo.PreferredExtent;
								}

								MeasureElementImpl(sizeInfo.Pane, availableSize);
							}
						}
					}
				}

				Debug.Assert(reduceTowardsExplicitMin == false || totalReduction > 0);

				if (reduceTowardsExplicitMin && totalReduction <= 0)
					break;

				reduceTowardsExplicitMin = true;

                // AS 11/5/08 TFS10192
                // If we reduce towards the absolute min then we could end up
                // leaving no portion of the split pane available so we will 
                // skip this step.
                //
                break;
            }

			#endregion //Step 2 - Resize Elements If There Is Not Enough Room

			#region Step 3 - Arrange The Elements
			for (int i = 0, count = sizeInfos.Count; i < count; i++)
			{
				PaneSizeInfo sizeInfo = sizeInfos[i];
				SplitPane pane = sizeInfo.Pane;

				PaneLocation location = XamDockManager.GetPaneLocation(pane);

				// AS 10/5/09 NA 2010.1 - LayoutMode
				// Added if block since a pane may not be visible but the splitter could be.
				//
				if (sizeInfo.IsPaneVisible)
				{
					Size paneSize = pane.DesiredSize;

					if (sizeInfo.IsLeftRight)
						paneSize.Width = sizeInfo.PreferredExtent;
					else
						paneSize.Height = sizeInfo.PreferredExtent;

					ArrangeDocked(pane, location, paneSize, ref finalRect);
				}

				// AS 10/5/09 NA 2010.1 - LayoutMode
				if (sizeInfo.IsSplitterVisible)
				{
					PaneSplitter splitter = sizeInfo.Splitter;

					Debug.Assert(null != splitter);

					if (null != splitter)
						ArrangeDocked(splitter, location, splitter.DesiredSize, ref finalRect);
				}
			} 
			#endregion //Step 3 - Arrange The Elements

			this._lastContentRect = finalRect;

			// AS 10/5/09 NA 2010.1 - LayoutMode
			// Use the fillElement local rather than assume we're dealing with the _child
			//
			if (null != fillElement)
			{
				fillElement.Arrange(finalRect);
			}

			// flyout handling
			if (null != this._flyoutPanel)
				this.ArrangeFlyout(finalSize);

			// unpinned container
			if (null != this._unpinnedContainer)
				this._unpinnedContainer.Arrange(new Rect(finalSize));

            // AS 11/5/08 TFS10192
            // If we had needed extra space then add that to the size we return
            // so the children will be clipped if needed.
            //
            if (widthToReduce > 0)
                finalSize.Width += widthToReduce;

            if (heightToReduce > 0)
                finalSize.Height += heightToReduce;

			return finalSize;
		}

		#endregion //ArrangeOverride

		#region GetVisualChild
		/// <summary>
		/// Returns the visual child at the specified index.
		/// </summary>
		/// <param name="index">Integer position of the child to return.</param>
		/// <returns>The child element at the specified position.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is greater than the <see cref="VisualChildrenCount"/></exception>
		protected override Visual GetVisualChild(int index)
		{
			int paneCount = this._panes.VisualChildrenCount;

			if (index < paneCount)
				return this._panes.GetVisualChild(index);

			index -= paneCount;

			if (null != this._child)
			{
				if (index == 0)
					return this._child;

				index--;
			}

            // AS 3/30/09 TFS16355 - WinForms Interop
            //if (null != this._flyoutPanel)
            if (null != this._flyoutPanel && this.FlyoutIsChildElement)
            {
				if (index == 0)
					return this._flyoutPanel;

				index--;
			}

			if (null != this._unpinnedContainer)
			{
				if (index == 0)
					return this._unpinnedContainer;

				index--;
			}

			return base.GetVisualChild(index);
		}
		#endregion //GetVisualChild

		#region LogicalChildren
		/// <summary>
		/// Returns an enumerator of the logical children
		/// </summary>
		protected override System.Collections.IEnumerator LogicalChildren
		{
			get
			{
				// we don't have to include the panes since they are logical children 
				// of the xdm but we do have to include the splitters so they get theme
				// change notifications
				return new MultiSourceEnumerator(this._panes.GetSplitterEnumerator(),
                    // AS 3/30/09 TFS16355 - WinForms Interop
                    //new SingleItemEnumerator(this._flyoutPanel),
                    new SingleItemEnumerator(this.FlyoutIsChildElement ? _flyoutPanel : null),
                    new SingleItemEnumerator(this._unpinnedContainer));
			}
		}
		#endregion //LogicalChildren

		#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			DockSize dockSize = new DockSize();
			Size originalAvailable = availableSize;

			
#region Infragistics Source Cleanup (Region)























#endregion // Infragistics Source Cleanup (Region)

			// We want to make sure that the content area has a certain minimum amount
			// of space so we'll take that out of the size we use to measure the panes
			// and add it back in when measuring the content.
			//
			Size minContentSize = this.CalculateMinimumContentSize();
			Size requiredReservedDockedExtents = minContentSize;
			// AS 5/28/08 BR33169
			// We're already accounting for the size in the required so the optional should be empty.
			//
			//Size optionalReservedDockedExtents = minContentSize;
			Size optionalReservedDockedExtents = new Size();

			// AS 10/5/09 NA 2010.1 - LayoutMode
			// The inner element may be the fill pane.
			//
			UIElement fillElement = this.FillElement;

			// AS 5/16/08
			List<PaneSizeInfo> sizeInfos = new List<PaneSizeInfo>();

			#region Step 1 - Build pane list and basic info
			for (int i = 0, count = this._panes.Count; i < count; i++)
			{
				SplitPane pane = this._panes[i];

				// AS 10/5/09 NA 2010.1 - LayoutMode
				// The fill pane will be processed separately.
				//
				if (pane == fillElement)
					continue;

				// AS 10/5/09 NA 2010.1 - LayoutMode
				//if (pane.Visibility != Visibility.Collapsed)
				//{
				//    PaneSizeInfo sizeInfo = new PaneSizeInfo(pane);
				//    sizeInfos.Add(sizeInfo);
				//    PaneSplitter splitter = sizeInfo.Splitter;
				PaneSizeInfo sizeInfo = new PaneSizeInfo(pane);

				if (sizeInfo.IsPaneVisible || sizeInfo.IsSplitterVisible)
				{
					sizeInfos.Add(sizeInfo);

					// AS 10/5/09 NA 2010.1 - LayoutMode
					// Added if block since the pane/splitter visibilities are disconnected.
					//
					if (sizeInfo.IsPaneVisible)
					{
						sizeInfo.MinExtent = GetMinimumExtent(pane, sizeInfo.IsLeftRight);

						// cache how much the minimums use
						if (sizeInfo.IsLeftRight)
						{
							requiredReservedDockedExtents.Width += sizeInfo.ExplicitMinExtent;
							optionalReservedDockedExtents.Width += sizeInfo.MinExtent - sizeInfo.ExplicitMinExtent;
						}
						else
						{
							requiredReservedDockedExtents.Height += sizeInfo.ExplicitMinExtent;
							optionalReservedDockedExtents.Height += sizeInfo.MinExtent - sizeInfo.ExplicitMinExtent;
						}
					}

					// measure the splitter so we can get its thickness and remove
					// that from the available size as well
					// AS 10/5/09 NA 2010.1 - LayoutMode
					// The splitter may be collapsed.
					//
					//if (null != splitter)
					if (sizeInfo.IsSplitterVisible)
					{
						PaneSplitter splitter = sizeInfo.Splitter;
						splitter.Measure(availableSize);
						Size splitterSize = splitter.DesiredSize;

						if (sizeInfo.IsLeftRight)
						{
							sizeInfo.SplitterExtent = splitterSize.Width;
							requiredReservedDockedExtents.Width += sizeInfo.SplitterExtent;
						}
						else
						{
							sizeInfo.SplitterExtent = splitterSize.Height;
							requiredReservedDockedExtents.Height += sizeInfo.SplitterExtent;
						}
					}
				}
			} 
			#endregion //Step 1 - Build pane list and basic info

			#region Step 2 - Reduce minimums toward the hard minimums

			// obtain the total amount of minimum space we would like to allocate
			Size reservedDockedExtents = new Size(optionalReservedDockedExtents.Width + requiredReservedDockedExtents.Width,
				optionalReservedDockedExtents.Height + requiredReservedDockedExtents.Height);

			// find out how much over the minimum (if any) we are but don't go below
			// the required minimum
			Size optionalMinimumToRemove = new Size(
				Math.Max(reservedDockedExtents.Width - Math.Max(requiredReservedDockedExtents.Width, availableSize.Width), 0)
				,
				Math.Max(reservedDockedExtents.Height - Math.Max(requiredReservedDockedExtents.Height, availableSize.Height), 0)
				);

			// if the amount available is less than the optional required then reduce the optional minimums
			while (optionalMinimumToRemove.Width > 0 || optionalMinimumToRemove.Height > 0)
			{
				// AS 5/28/08 BR33169
				Size sizeBefore = optionalMinimumToRemove;

				for (int i = sizeInfos.Count - 1; i >= 0; i--)
				{
					// now that we have the required minimums, if the optional minimum
					// makes it too wide then start reducing the optional minimum
					PaneSizeInfo sizeInfo = sizeInfos[i];

					// AS 10/5/09 NA 2010.1 - LayoutMode
					if (!sizeInfo.IsPaneVisible)
						continue;

					if (sizeInfo.IsLeftRight && optionalMinimumToRemove.Width > 0 && sizeInfo.MinExtent > sizeInfo.ExplicitMinExtent)
					{
						double amtToRemove = Math.Min(1, optionalMinimumToRemove.Width);
						sizeInfo.MinExtent -= amtToRemove;
                        // AS 7/9/08 
						//optionalMinimumToRemove.Width -= amtToRemove;
                        DockManagerUtilities.SubtractWidth(ref optionalMinimumToRemove, amtToRemove);
					}
					else if (false == sizeInfo.IsLeftRight && optionalMinimumToRemove.Height > 0 && sizeInfo.MinExtent > sizeInfo.ExplicitMinExtent)
					{
						double amtToRemove = Math.Min(1, optionalMinimumToRemove.Height);
						sizeInfo.MinExtent -= amtToRemove;
                        // AS 7/9/08 
                        //optionalMinimumToRemove.Height -= amtToRemove;
                        DockManagerUtilities.SubtractHeight(ref optionalMinimumToRemove, amtToRemove);
                    }
				}

				// AS 5/28/08 BR33169
				// This shouldn't happen but just in case, exit the loop.
				//
				Debug.Assert(false == sizeBefore.Equals(optionalMinimumToRemove));
				if (sizeBefore.Equals(optionalMinimumToRemove))
					break;
			}
			#endregion //Step 2 - Reduce minimums toward the hard minimums

			#region Step 3 - Measure
			for (int i = 0, count = sizeInfos.Count; i < count; i++)
			{
				PaneSizeInfo sizeInfo = sizeInfos[i];
				SplitPane pane = sizeInfo.Pane;
				PaneSplitter splitter = sizeInfo.Splitter;
				bool isLeftRight = sizeInfo.IsLeftRight;

				// we need to pull out the reserved portion of the extents
				// so we leave space for the inner panes and their splitters
				double amtRemoved;
				if (isLeftRight)
				{
                    // AS 7/9/08 
                    //reservedDockedExtents.Width -= sizeInfo.MinExtent;
                    DockManagerUtilities.SubtractWidth(ref reservedDockedExtents, sizeInfo.MinExtent);
					amtRemoved = Math.Max(Math.Min(availableSize.Width - sizeInfo.MinExtent, reservedDockedExtents.Width), 0);

                    // AS 7/9/08 
                    //availableSize.Width -= amtRemoved;
                    DockManagerUtilities.SubtractWidth(ref availableSize, amtRemoved);
				}
				else
				{
                    // AS 7/9/08 
                    //reservedDockedExtents.Height -= sizeInfo.MinExtent;
                    DockManagerUtilities.SubtractHeight(ref reservedDockedExtents, sizeInfo.MinExtent);
					amtRemoved = Math.Max(Math.Min(availableSize.Height - sizeInfo.MinExtent, reservedDockedExtents.Height), 0);

                    // AS 7/9/08 
                    //availableSize.Height -= amtRemoved;
                    DockManagerUtilities.SubtractHeight(ref availableSize, amtRemoved);
				}

				// AS 10/5/09 NA 2010.1 - LayoutMode
				if (sizeInfo.IsPaneVisible)
				{
					pane.Measure(availableSize);

					// start with the pane's desired size as the size to use
					Size dockedSize = pane.DesiredSize;
					dockSize.AddDocked(isLeftRight, dockedSize);
					AdjustAvailableSize(isLeftRight, dockedSize, ref availableSize);
				}

				Debug.Assert(null != splitter);

				// AS 10/5/09 NA 2010.1 - LayoutMode
				//if (null != splitter)
				if (sizeInfo.IsSplitterVisible)
				{
					// reduce the amount we will put back by the splitter extent
					double amtReturned = Math.Min(amtRemoved, sizeInfo.SplitterExtent);
					amtRemoved -= amtReturned;

					// now add back the size for the splitter
					if (isLeftRight)
					{
                        // AS 7/9/08 
                        //reservedDockedExtents.Width -= sizeInfo.SplitterExtent;
                        DockManagerUtilities.SubtractWidth(ref reservedDockedExtents, sizeInfo.SplitterExtent);
						availableSize.Width += amtReturned;
					}
					else
					{
                        // AS 7/9/08 
                        //reservedDockedExtents.Height -= sizeInfo.SplitterExtent;
                        DockManagerUtilities.SubtractHeight(ref reservedDockedExtents, sizeInfo.SplitterExtent);
						availableSize.Height += amtReturned;
					}

					splitter.Measure(availableSize);
					dockSize.AddDocked(isLeftRight, splitter.DesiredSize);
					AdjustAvailableSize(isLeftRight, splitter.DesiredSize, ref availableSize);
				}

				// put the amount for the inner panes back in. we need to do this because
				// the next pane could be sized in the opposite orientation
				if (isLeftRight)
					availableSize.Width += amtRemoved;
				else
					availableSize.Height += amtRemoved;
			} 
			#endregion //Step 3 - Measure

			// AS 10/5/09 NA 2010.1 - LayoutMode
			// The fill element may be a splitpane now so use the local we 
			// initialized above.
			//
			//if (null != this._child)
			if (null != fillElement)
			{
				fillElement.Measure(availableSize);
				Size childSize = fillElement.DesiredSize;
				dockSize.Width += childSize.Width;
				dockSize.Height += childSize.Height;
			}

            // AS 3/30/09 TFS16355 - WinForms Interop
            //if (null != this._flyoutPanel)
            if (null != this._flyoutPanel && this.FlyoutIsChildElement)
            {
				this._flyoutPanel.Measure(originalAvailable);

				// note: the flyout shouldn't dictate the size for the dockmanager so
				// we won't evaluate its desired size but we do have to measure it
			}

			// we don't want the unpinned container to measure/arrange its elements based on the current size
			// of the flyout since that will likely not be the size of the flyout when the element is measured/
			// arranged.
			if (null != this._unpinnedContainer)
				this._unpinnedContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

			return dockSize.ToSize();
		}
		#endregion //MeasureOverride

		#region OnChildDesiredSizeChanged
		/// <summary>
		/// Invoked when the desired size of a child has been changed.
		/// </summary>
		/// <param name="child">The child whose size is being changed.</param>
		protected override void OnChildDesiredSizeChanged(UIElement child)
		{
			// AS 5/16/08 BR32579
			if (this._elementBeingMeasured == child)
				return;

			// do not worry about dirtying the arrange if the unpinned container changes
			// since its just a container so panes can retain the focus after the flyout
			// is closed
			if (child is UnpinnedPaneContainer)
				return;

			base.OnChildDesiredSizeChanged(child);
		}
		#endregion //OnChildDesiredSizeChanged

		#region VisualChildrenCount
		/// <summary>
		/// Returns the number of visual children for the element.
		/// </summary>
		protected override int VisualChildrenCount
		{
			get
			{
				int count = this._child != null ? 1 : 0;

				// include the split panes and splitter bars
				count += this._panes.VisualChildrenCount;

                // AS 3/30/09 TFS16355 - WinForms Interop
                //if (this._flyoutPanel != null)
                if (this._flyoutPanel != null && this.FlyoutIsChildElement)
                    count++;

				if (null != this._unpinnedContainer)
					count++;

				return count;
			}
		}
		#endregion //VisualChildrenCount

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region Child
		/// <summary>
		/// Returns/sets the content element.
		/// </summary>
		public UIElement Child
		{
			get { return this._child; }
			set
			{
				if (value != this._child)
				{
					if (null != this._child)
						this.RemoveVisualChild(this._child);

					this._child = value;

					if (null != this._child)
						this.AddVisualChild(this._child);
				}
			}
		}
		#endregion //Child

		#endregion //Public Properties 

		#region Internal Properties

		#region AvailableContentSize
		/// <summary>
		/// The amount of space that can be removed from the content area.
		/// </summary>
		internal Size AvailableContentSize
		{
			get
			{
				Size size = this._lastContentRect.Size;

				
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

				Size minContentSize = this.CalculateMinimumContentSize();

                // AS 7/9/08 
                //size.Width -= minContentSize.Width;
				//size.Height -= minContentSize.Height;
                DockManagerUtilities.SubtractWidth(ref size, minContentSize.Width);
                DockManagerUtilities.SubtractHeight(ref size, minContentSize.Height);

				return size;
			}
		}
		#endregion //AvailableContentSize

		#region FlyoutPanel
		/// <summary>
		/// The panel used to contain/display the currently selected unpinned <see cref="ContentPane"/>
		/// </summary>
		internal UnpinnedTabFlyout FlyoutPanel
		{
			get
			{
                
#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

                if (_flyoutPanel == null)
                    this.VerifyFlyoutPanelHost(null);

				return this._flyoutPanel;
			}
		}
		#endregion //FlyoutPanel

		#region HasFlyoutPanel
		/// <summary>
		/// Returns a boolean indicating if the flyout panel has been created.
		/// </summary>
		internal bool HasFlyoutPanel
		{
			get { return this._flyoutPanel != null; }
		} 
		#endregion //HasFlyoutPanel

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region HasHwndHost
        internal bool HasHwndHost
        {
            get
            {
                HwndHostInfo hhi = HwndHostInfo.GetHwndHost(this);
                return null != hhi && hhi.HasHwndHost;
            }
        }
        #endregion //HasHwndHost

		#region LastContentRect
		internal Rect LastContentRect
		{
			get { return this._lastContentRect; }
		} 
		#endregion //LastContentRect

		#region Panes
		internal ObservableCollectionExtended<SplitPane> Panes
		{
			get
			{
				return this._panes;
			}
		}
		#endregion //Panes

		#endregion //Internal Properties

		#region Private Properties

		// AS 10/5/09 NA 2010.1 - LayoutMode
		#region FillElement
		private UIElement FillElement
		{
			get
			{
				XamDockManager dm = XamDockManager.GetDockManager(this);

				if (dm == null)
					return _child;

				switch (dm.LayoutMode)
				{
					default:
					case DockedPaneLayoutMode.Standard:
						Debug.Assert(dm.LayoutMode == DockedPaneLayoutMode.Standard);
						return this._child;
					case DockedPaneLayoutMode.FillContainer:
						return dm.FillPane;
				}
			}
		}
		#endregion //FillElement

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region FlyoutIsChildElement
        private bool FlyoutIsChildElement
        {
            get
            {
                return _flyoutWindow == null;
            }
        } 
        #endregion //FlyoutIsChildElement

		#region UnpinnedContainer
		private UnpinnedPaneContainer UnpinnedContainer
		{
			get
			{
				if (null == this._unpinnedContainer)
				{
					this._unpinnedContainer = new UnpinnedPaneContainer();
					this.AddVisualChild(this._unpinnedContainer);
					this.AddLogicalChild(this._unpinnedContainer);
				}

				return this._unpinnedContainer;
			}
		}
		#endregion //UnpinnedContainer

		#endregion //Private Properties

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region AddUnpinnedPane
		/// <summary>
		/// Adds an element into the visual tree of the unpinned container which holds all the unpinned panes.
		/// </summary>
		/// <param name="pane">The unpinned pane to add.</param>
		internal void AddUnpinnedPane(ContentPane pane)
		{
			if (pane.IsPinned)
				return;

			Debug.Assert(null != pane && pane.IsPinned == false);
			Debug.Assert(VisualTreeHelper.GetParent(pane) != this.UnpinnedContainer);

			// AS 7/16/09 TFS18515
			// The pane may be a child of the flyout panel.
			//
			//if (VisualTreeHelper.GetParent(pane) != this.UnpinnedContainer)
			//	this.UnpinnedContainer.AddPane(pane);
			DependencyObject visualParent = VisualTreeHelper.GetParent(pane);

			if (visualParent == null)
			{
				this.UnpinnedContainer.AddPane(pane);
			}
			else
			{
				Debug.Assert(visualParent == this.UnpinnedContainer || (null != _flyoutPanel && pane == _flyoutPanel.Pane));
			}
		}
		#endregion //AddUnpinnedPane

		// AS 5/16/08 BR32579
		// AS 5/17/08 BR32019 - Changed to Internal
		#region GetMinimumExtent
		internal static double GetMinimumExtent(SplitPane split, bool width)
		{
			const double Extra = 10;
			double min;

			if (width)
			{
				double defaultMin = Extra + SystemParameters.VerticalScrollBarWidth;

				if (false == double.IsNaN(split.Width))
					defaultMin = Math.Min(defaultMin, split.Width);

				min = Math.Max(split.MinWidth, defaultMin);
			}
			else
			{
				double defaultMin = Extra + SystemParameters.HorizontalScrollBarHeight;

				if (false == double.IsNaN(split.Height))
					defaultMin = Math.Min(defaultMin, split.Height);

				min = Math.Max(split.MinHeight, defaultMin);
			}

			return min;
		}
		#endregion //GetMinimumExtent

		#region RemoveAllUnpinnedPanes
		/// <summary>
		/// Removes all referenced unpinned content panes.
		/// </summary>
		internal void RemoveAllUnpinnedPanes()
		{
			// make sure the flyout is closed
			if (this.HasFlyoutPanel)
				this._flyoutPanel.HideFlyout(null, false, false, true, false);

			if (null != this._unpinnedContainer)
				this._unpinnedContainer.RemoveAllPanes();
		}
		#endregion //RemoveAllUnpinnedPanes

		#region RemoveUnpinnedPane
		/// <summary>
		/// Removes a pane from the unpinned container and flyout.
		/// </summary>
		/// <param name="pane">The pane to remove</param>
		internal void RemoveUnpinnedPane(ContentPane pane)
		{
			if (this.HasFlyoutPanel)
				this._flyoutPanel.HideFlyout(pane, false, false, true, false);

			this.UnpinnedContainer.RemovePane(pane);
		}
		#endregion //RemoveUnpinnedPane

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region VerifyFlyoutPanelHost
        /// <summary>
        /// Helper routine to verify the parent container of the UnpinnedTabFlyout.
        /// </summary>
        /// <param name="useToolWindow">An explicit true/false indicating whether to parent it to a toolwindow or within the DockManagerPanel or Null to use the XDM's ShowFlyoutInToolWindow.</param>
        internal void VerifyFlyoutPanelHost(bool? useToolWindow)
        {
            // AS 3/30/09 TFS16355 - WinForms Interop [Start]
            XamDockManager dm = XamDockManager.GetDockManager(this);
            bool isUsingToolWindow = _flyoutWindow != null;
            bool initializeOwner = _flyoutPanel == null;

            bool shouldUseToolWindow = useToolWindow == null
                ? dm != null && dm.ShowFlyoutInToolWindow
                : useToolWindow.Value;

            // if we need the toolwindow but haven't created it then get it ready now
            if (shouldUseToolWindow && _flyoutWindow == null)
            {
                _flyoutWindow = new FlyoutToolWindow();
                _flyoutWindow.UseOSNonClientArea = false;
                _flyoutWindow.ResizeMode = ResizeMode.NoResize;
                _flyoutWindow.SetValue(XamDockManager.DockManagerPropertyKey, dm);
                _flyoutWindow.Template = Infragistics.Windows.DockManager.Dragging.DragManager.GetIndicatorToolWindowTemplate(dm);
                _flyoutWindow.VerticalAlignmentMode = ToolWindowAlignmentMode.UseAlignment;
                _flyoutWindow.HorizontalAlignmentMode = ToolWindowAlignmentMode.UseAlignment;

				// AS 7/14/09 TFS18424
				_flyoutWindow.SetBinding(FrameworkElement.FlowDirectionProperty, Utilities.CreateBindingObject(FrameworkElement.FlowDirectionProperty, System.Windows.Data.BindingMode.OneWay, this));
			}

            // if the state has changed then remove it from the old parent
            if (shouldUseToolWindow != isUsingToolWindow && _flyoutPanel != null)
            {
                initializeOwner = true;
                _flyoutPanel.HideFlyout(null, false, false, true, false);

                if (isUsingToolWindow)
                {
                    _flyoutWindow.Close();
                    _flyoutWindow.Content = null;
                    _flyoutWindow = null;
                }
                else
                {
                    this.RemoveLogicalChild(_flyoutPanel);
                    this.RemoveVisualChild(_flyoutPanel);
                }
            }
            // AS 3/30/09 TFS16355 - WinForms Interop [End]

            if (this._flyoutPanel == null)
            {
                // make sure the unpinned container is created and initialized since we'll expect it
                // to contain the unpinned pane elements
                FrameworkElement container = this.UnpinnedContainer;
                container = null;

                this._flyoutPanel = new UnpinnedTabFlyout();
                this._flyoutPanel.SetValue(FrameworkElement.VisibilityProperty, KnownBoxes.VisibilityCollapsedBox);
                this._flyoutPanel.SetValue(XamDockManager.PaneLocationPropertyKey, DockManagerKnownBoxes.PaneLocationUnpinnedBox);

				// AS 6/15/12 TFS114790
				// We may need to reduce the extent of the flyout based on the available size. This primarily applies to 
				// when there is an HwndHost and we use a popup but could also apply when we had an extent stored for a 
				// pane but the size is now too big for the available space.
				//
				this._flyoutPanel.SetBinding(UnpinnedTabFlyout.MaxFlyoutWidthProperty, new Binding { Path = new PropertyPath(FrameworkElement.ActualWidthProperty), Source = this });
				this._flyoutPanel.SetBinding(UnpinnedTabFlyout.MaxFlyoutHeightProperty, new Binding { Path = new PropertyPath(FrameworkElement.ActualHeightProperty), Source = this });

                // AS 3/30/09 TFS16355 - WinForms Interop
                // The visual/logical parent will depend on whether its being hosted
                // in a toolwindow or not.
                //
                //this.AddVisualChild(this._flyoutPanel);
                //this.AddLogicalChild(this._flyoutPanel);
            }

            // AS 3/30/09 TFS16355 - WinForms Interop
            if (initializeOwner)
            {
                if (shouldUseToolWindow == false)
                {
                    this.AddVisualChild(this._flyoutPanel);
                    this.AddLogicalChild(this._flyoutPanel);
                }
                else
                {
                    _flyoutWindow.Content = _flyoutPanel;
                    _flyoutWindow.Tag = FlyoutToolWindowId;
                    _flyoutPanel.InitializeToolWindowAlignment();
                }
            }
        }
        #endregion //VerifyFlyoutPanelHost

        #endregion //Internal Methods

		#region Private Methods

		#region AdjustAvailableSize
		private static void AdjustAvailableSize(bool isLeftRight, Size dockedSize, ref Size availableSize)
		{
			if (isLeftRight)
				availableSize.Width = Math.Max(availableSize.Width - dockedSize.Width, 0);
			else
				availableSize.Height = Math.Max(availableSize.Height - dockedSize.Height, 0);
		}
		#endregion //AdjustAvailableSize

		#region ArrangeDocked
		// AS 5/16/08 BR32579
		//private void ArrangeDocked(UIElement element, PaneLocation location, ref Rect finalRect)
		private void ArrangeDocked(UIElement element, PaneLocation location, Size paneSize, ref Rect finalRect)
		{
			// AS 5/16/08 BR32579
			// Let this get passed in since we may need to resize the element.
			//
			//Size paneSize = element.DesiredSize;
			Rect paneRect = new Rect(finalRect.X, finalRect.Y, paneSize.Width, paneSize.Height);

			switch (location)
			{
				case PaneLocation.DockedBottom:
					paneRect.Y = Math.Max(finalRect.Bottom - paneRect.Height, 0d);
					paneRect.Width = finalRect.Width;
					finalRect.Height = Math.Max(finalRect.Height - paneRect.Height, 0d);
					break;
				case PaneLocation.DockedLeft:
					paneRect.Height = finalRect.Height;
					finalRect.X += paneRect.Width;
					finalRect.Width = Math.Max(finalRect.Width - paneRect.Width, 0d);
					break;
				case PaneLocation.DockedRight:
					paneRect.Height = finalRect.Height;
					paneRect.X = Math.Max(finalRect.Right - paneRect.Width, 0d);
					finalRect.Width = Math.Max(finalRect.Width - paneRect.Width, 0d);
					break;
				case PaneLocation.DockedTop:
					paneRect.Width = finalRect.Width;
					finalRect.Y += paneRect.Height;
					finalRect.Height = Math.Max(finalRect.Height - paneRect.Height, 0d);
					break;
				default:
					Debug.Fail("Unexpected state!");
					paneRect = new Rect();
					break;
			}

			element.Arrange(paneRect);
		}
		#endregion //ArrangeDocked

		#region ArrangeFlyout
		private void ArrangeFlyout(Size finalSize)
		{
			XamDockManager dockManager = XamDockManager.GetDockManager(this);

			Debug.Assert(null == this._flyoutPanel || null != dockManager);

            // AS 3/30/09 TFS16355 - WinForms Interop
            if (!this.FlyoutIsChildElement)
                return;

            // flyout handling
			if (null != this._flyoutPanel && dockManager != null)
			{
				Dock side = this._flyoutPanel.Side;
				
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

				#region Calculate the relative rect
				Rect relativeRect;

				if (true)
				{
					relativeRect = new Rect(finalSize);

					// calculate the position
					switch (side)
					{
						default:
						case Dock.Left:
							relativeRect.Width = 0;
							break;

						case Dock.Right:
							relativeRect.X = relativeRect.Width;
							relativeRect.Width = 0;
							break;

						case Dock.Top:
							relativeRect.Height = 0;
							break;

						case Dock.Bottom:
							relativeRect.Y = relativeRect.Height;
							relativeRect.Height = 0;
							break;
					}
				}
				#endregion //Calculate the relative rect

				Size flyoutSize = this._flyoutPanel.DesiredSize;
				Rect flyoutRect = new Rect(flyoutSize);

				// calculate the position based on the relative rect of the unpinned area
				switch (side)
				{
					default:
					case Dock.Left:
						flyoutRect.Location = relativeRect.TopRight;
						flyoutRect.Height = relativeRect.Height;
						break;

					case Dock.Right:
						flyoutRect.Location = relativeRect.TopLeft;
						flyoutRect.X -= flyoutRect.Width;
						flyoutRect.Height = relativeRect.Height;
						break;

					case Dock.Top:
						flyoutRect.Location = relativeRect.BottomLeft;
						flyoutRect.Width = relativeRect.Width;
						break;

					case Dock.Bottom:
						flyoutRect.Location = relativeRect.TopLeft;
						flyoutRect.Y -= flyoutRect.Height;
						flyoutRect.Width = relativeRect.Width;
						break;
				}

				// make sure its in view?
				//flyoutRect.Intersect(new Rect(finalSize));

				this._flyoutPanel.Arrange(flyoutRect);
			}
		}

		#endregion //ArrangeFlyout

		// AS 5/16/08 BR32579
		// Created helper method from the routine within the AvailableContentSize property.
		//
		#region CalculateMinimumContentSize
		private Size CalculateMinimumContentSize()
		{
			// AS 5/16/08 BR32579
			// Do not remove the space for the child if the dm doesn't have content.
			// This will allow the user to use all the space when there is no content
			// but still allow us to enforce that a minimum space be left if there is
			// content within the dm similar to how VS leaves space for its mdi area
			// even when there are no documents.
			//
			//if (this._child != null)
			Size minSize = new Size();
			XamDockManager dm = XamDockManager.GetDockManager(this);

			// AS 10/5/09 NA 2010.1 - LayoutMode
			// Use the fill pane if we have one otherwise use the child.
			//
			//if (this._child != null && dm != null && dm.HasContent)
			UIElement fillElement = this.FillElement;

			// to maintain the previous behavior we will ignore the child
			// if we don't have content in the minimum content size
			// AS 10/25/10
			//if (fillElement == _child && !dm.HasContent)
			if (fillElement == _child && (dm == null || !dm.HasContent))
				return minSize;

			if (null != fillElement)
			{
				double minWidth = 0;
				double minHeight = 0;

				FrameworkElement frameworkChild = fillElement as FrameworkElement;

				if (null != frameworkChild)
				{
					minWidth = frameworkChild.MinWidth;
					minHeight = frameworkChild.MinHeight;
				}

				const double ExtraSpace = 5;
				minSize.Width = Math.Max(minWidth, SystemParameters.VerticalScrollBarWidth + ExtraSpace);
				minSize.Height = Math.Max(minHeight, SystemParameters.HorizontalScrollBarHeight + ExtraSpace);
			}

			return minSize;
		}
		#endregion //CalculateMinimumContentSize

		// AS 5/16/08 BR32579
		#region MeasureElementImpl
		private void MeasureElementImpl(UIElement element, Size availableSize)
		{
			Debug.Assert(this._elementBeingMeasured == null);

			UIElement oldElementBeingMeasured = this._elementBeingMeasured;
			this._elementBeingMeasured = element;

			try
			{
				element.Measure(availableSize);
			}
			finally
			{
				this._elementBeingMeasured = oldElementBeingMeasured;
			}
		} 
		#endregion //MeasureElementImpl

		#region OnPanesChanged
		private void OnPanesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			this.InvalidateMeasure();
		} 
		#endregion //OnPanesChanged

		#endregion //Private Methods

		#endregion //Methods

		#region ISplitElementCollectionOwner Members

		void ISplitElementCollectionOwner.OnElementAdding(FrameworkElement newElement)
		{
		}

		void ISplitElementCollectionOwner.OnElementAdded(FrameworkElement newElement)
		{
			this.AddVisualChild(newElement);
		}

		void ISplitElementCollectionOwner.OnElementRemoved(FrameworkElement oldElement)
		{
			this.RemoveVisualChild(oldElement);
		}

		PaneSplitter ISplitElementCollectionOwner.CreateSplitter()
		{
			return new DockedPaneSplitter();
		}

		void ISplitElementCollectionOwner.OnSplitterAdded(PaneSplitter splitter)
		{
			splitter.SetBinding(PaneSplitter.OrientationProperty, Utilities.CreateBindingObject(XamDockManager.PaneLocationProperty, System.Windows.Data.BindingMode.OneWay, splitter.ElementBeforeSplitter, DockedLocationToSplitterOrientationConverter.Instance));

			this.AddVisualChild(splitter);
			this.AddLogicalChild(splitter);
		}

		void ISplitElementCollectionOwner.OnSplitterRemoved(PaneSplitter splitter)
		{
			BindingOperations.ClearBinding(splitter, PaneSplitter.OrientationProperty);

			this.RemoveLogicalChild(splitter);
			this.RemoveVisualChild(splitter);
		}

		#endregion //ISplitElementCollectionOwner

        // AS 3/30/09 TFS16355 - WinForms Interop
        // The XamDockManager may not be using a DocumentContentHost but within the 
        // Content that is used, there could be one or more HwndHosts.
        //
        #region IHwndHostInfoOwner Members

        void IHwndHostInfoOwner.OnHasHostsChanged()
        {
            XamDockManager dm = XamDockManager.GetDockManager(this);

            if (null != dm)
                dm.DirtyHasHwndHosts();
        }

        #endregion //IHwndHostInfoOwner Members

		#region DockSize struct
		private struct DockSize
		{
			#region Member Variables

			internal double Width;
			internal double TopBottomWidth;
			internal double Height;
			internal double LeftRightHeight;

			#endregion //Member Variables

			#region Methods
			public void AddDocked(bool isLeftRight, Size size)
			{
				if (isLeftRight)
				{
					Width += size.Width;
					LeftRightHeight = Math.Max(LeftRightHeight, Height + size.Height);
				}
				else
				{
					Height += size.Height;
					TopBottomWidth = Math.Max(TopBottomWidth, Width + size.Width);
				}
			}

			public Size ToSize()
			{
				return new Size(Math.Max(Width, TopBottomWidth), Math.Max(Height, LeftRightHeight));
			}
			#endregion //Methods
		} 
		#endregion //DockSize struct

		// AS 5/16/08 BR32579
		#region PaneSizeInfo
		private class PaneSizeInfo
		{
			#region Member Variables

			internal readonly SplitPane Pane;
			internal readonly PaneSplitter Splitter;
			internal readonly bool IsLeftRight;

			internal double PreferredExtent;
			internal double MinExtent;
			internal double SplitterExtent;
			internal double ExplicitExtent;
			internal double ExplicitMinExtent;

			// AS 10/5/09 NA 2010.1 - LayoutMode
			internal bool IsPaneVisible;
			internal bool IsSplitterVisible;

			#endregion //Member Variables

			#region Constructor
			internal PaneSizeInfo(SplitPane pane)
			{
				this.Pane = pane;
				this.Splitter = PaneSplitter.GetSplitter(pane);
				this.IsLeftRight = pane.IsDockedLeftRight;

				// AS 10/5/09 NA 2010.1 - LayoutMode
				// The pane and splitter visibilities are disconnected.
				//
				//this.ExplicitMinExtent = this.IsLeftRight ? pane.MinWidth : pane.MinHeight;
				this.IsPaneVisible = pane.Visibility != Visibility.Collapsed;
				this.IsSplitterVisible = this.Splitter != null && this.Splitter.Visibility != Visibility.Collapsed;

				if (this.IsPaneVisible) 
					this.ExplicitMinExtent = this.IsLeftRight ? pane.MinWidth : pane.MinHeight;
			} 
			#endregion //Constructor
		} 
		#endregion //PaneSizeInfo

		#region UnpinnedPaneContainer class
        // AS 10/13/08 TFS6032
        //private class UnpinnedPaneContainer : FrameworkElement
		internal class UnpinnedPaneContainer : FrameworkElement
		{
			#region Member Variables

			private List<FrameworkElement> _panes;

			#endregion //Member Variables

			#region Constructor

			static UnpinnedPaneContainer()
			{
				// we want the element to be hidden but not collapsed
                // AS 4/16/09 TFS16355
                // I found this in a sample that was using HwndHost. When the flyout pane
                // contained the active control and it wasn't hidden but just moved into
                // the UnpinnedPaneContainer, even though we defaulted the opacity to 0,
                // the ContentPane was still rendered on screen. Oddly, setting the value
                // locally to 0 in the ctor does address this.
                //
				//UnpinnedPaneContainer.OpacityProperty.OverrideMetadata(typeof(UnpinnedPaneContainer), new FrameworkPropertyMetadata(0d));
				UnpinnedPaneContainer.IsHitTestVisibleProperty.OverrideMetadata(typeof(UnpinnedPaneContainer), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

				// AS 5/28/08 BR33402
				// Since the unpinned panes are now in the visual tree even when not in
				// the flyout, the events are routing up the visual tree and therefore not
				// getting to the unpinned tab area so we need to coerce the visibility of
				// the unpinned tab area if a pane is hidden/shown and also we need to help
				// show the flyout if we get a request for a pane that is not in the flyout.
				//
				EventManager.RegisterClassHandler(typeof(UnpinnedPaneContainer), FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(OnRequestBringIntoView));
				EventManager.RegisterClassHandler(typeof(UnpinnedPaneContainer), DockManagerUtilities.VisibilityChangedEvent, new RoutedEventHandler(OnChildVisibilityChanged));
			}

			internal UnpinnedPaneContainer()
			{
				this._panes = new List<FrameworkElement>();

                // AS 4/16/09 TFS16355
                // See the static ctor for more. Just in case I also set the 
                // clip to ensure nothing is rendered.
                //
                this.Opacity = 0;
                this.Clip = Geometry.Empty;
			}
			#endregion //Constructor

            #region Properties

            // AS 10/13/08 TFS6032
            #region IsHiddenUnpinned

            /// <summary>
            /// IsHiddenUnpinned Attached Dependency Property
            /// </summary>
            public static readonly DependencyProperty IsHiddenUnpinnedProperty =
                DependencyProperty.RegisterAttached("IsHiddenUnpinned", typeof(bool), typeof(UnpinnedPaneContainer),
                    new FrameworkPropertyMetadata(KnownBoxes.FalseBox,
                        FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior | // AS 11/4/08 TFS9618
                        FrameworkPropertyMetadataOptions.Inherits,
                        new PropertyChangedCallback(OnIsHiddenUnpinnedChanged)));

            /// <summary>
            /// Gets the IsHiddenUnpinned property.  This dependency property 
            /// indicates ....
            /// </summary>
            public static bool GetIsHiddenUnpinned(DependencyObject d)
            {
                return (bool)d.GetValue(IsHiddenUnpinnedProperty);
            }

            private static void OnIsHiddenUnpinnedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                ContentPane pane = d as ContentPane;

                // there's a bug in the hwndhost that will cause a flicker. it seems that they
                // use ShowWindowAsync to show/hide the associated window. the problem is that if
                // you are hiding the pane (like what happens when we unpin a pane with an hwnd host)
                // and you then change the parent chain - the reparenting of the pane causes the
                // the hwndhost to show the pane (at least in the WindowsFormsHost because of how 
                // the WinForms control responds to the setwindowpos). to avoid this we will 
                // try to synchronously hide the hwnd hosts when we are being put into the unpinned
                // pane container (the host for unpinned panes that are not in the flyout)
                //
                if (null != pane)
                {
                    pane.VerifyContentVisibility();
                    pane.HideAllHwndHosts();
                }
            }

            #endregion //IsHiddenUnpinned

            #endregion //Properties

            #region Methods

            #region AddPane
            internal void AddPane(FrameworkElement pane)
			{
				this._panes.Add(pane);
				this.AddVisualChild(pane);

                // AS 10/13/08 TFS6032
                pane.SetValue(IsHiddenUnpinnedProperty, KnownBoxes.TrueBox);
			} 
			#endregion //AddPane

			// AS 5/28/08 BR33402
			#region OnChildVisibilityChanged
			private static void OnChildVisibilityChanged(object sender, RoutedEventArgs e)
			{
				ContentPane cp = e.OriginalSource as ContentPane;
				Debug.Assert(null != cp);

				if (null != cp)
				{
					UnpinnedTabArea tabArea = LogicalTreeHelper.GetParent(cp) as UnpinnedTabArea;
					Debug.Assert(null != tabArea);

					if (null != tabArea)
					{
						tabArea.CoerceValue(UIElement.VisibilityProperty);
						e.Handled = true;
					}
				}
			}
			#endregion //OnChildVisibilityChanged

			// AS 5/28/08 BR33402
			#region OnRequestBringIntoView
			private static void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
			{
				UnpinnedPaneContainer tabCtrl = sender as UnpinnedPaneContainer;

				tabCtrl.OnRequestBringIntoView(e);
			}

			private void OnRequestBringIntoView(RequestBringIntoViewEventArgs e)
			{
				DependencyObject item = e.TargetObject;

				// find the root tab item
				while (item != null && item is ContentPane == false)
				{
                    
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

                    item = Utilities.GetParent(item, true);
				}

				ContentPane cp = item as ContentPane;

				if (null != cp && cp.Visibility == Visibility.Visible)
				{
					XamDockManager dockManager = XamDockManager.GetDockManager(cp);

					// AS 7/15/09 TFS18426
					// The flyout was not meant to be shown when the pane was activated via
					// the gotfocus of the dockmanager.
					//
					//if (null != dockManager)
					// AS 7/1/10 TFS34388
					// Do not show the flyout for a nested pane.
					//
					//if (null != dockManager && !dockManager.IsShowFlyoutSuspended)
					if (null != dockManager && dockManager == XamDockManager.GetDockManager(this) && !dockManager.IsShowFlyoutSuspended)
						dockManager.ShowFlyout(cp, false, false);
				}
			}
			#endregion //OnRequestBringIntoView

			#region RemoveAllPanes
			internal void RemoveAllPanes()
			{
				FrameworkElement[] elements = this._panes.ToArray();

				for (int i = elements.Length - 1; i >= 0; i--)
				{
					this._panes.RemoveAt(i);
					this.RemoveVisualChild(elements[i]);

                    // AS 10/13/08 TFS6032
                    elements[i].ClearValue(IsHiddenUnpinnedProperty);
                }
			}
			#endregion //RemoveAllPanes

			#region RemovePane
			internal void RemovePane(FrameworkElement pane)
			{
				this._panes.Remove(pane);
				this.RemoveVisualChild(pane);

                // AS 10/13/08 TFS6032
                pane.ClearValue(IsHiddenUnpinnedProperty);
            } 
			#endregion //RemovePane

			#endregion //Methods

			#region Base class overrides
			protected override Size MeasureOverride(Size availableSize)
			{
				return base.MeasureOverride(availableSize);
			}

			protected override Size ArrangeOverride(Size finalSize)
			{
				// we really don't want to arrange the children since the container
				// isn't going to show them and we don't want to incur the overhead
				// of arranging them at a size they are not likely to be at when they
				// are displayed.
				return base.ArrangeOverride(finalSize);
			}

			protected override int VisualChildrenCount
			{
				get
				{
					return this._panes.Count;
				}
			}

			protected override Visual GetVisualChild(int index)
			{
				return this._panes[index];
			}
			#endregion //Base class overrides
		}
		#endregion //UnpinnedPaneContainer class

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region FlyoutToolWindow
        private class FlyoutToolWindow : ToolWindow
        {
            #region Base class overrides

			// AS 8/4/11 TFS83465/TFS83469
			#region KeepOnScreen
			internal override bool KeepOnScreen
			{
				get
				{
					return false;
				}
			}
			#endregion //KeepOnScreen

            #region OnActivated
            /// <summary>
            /// Invoked when the window is activated.
            /// </summary>
            /// <param name="e">Provides data for the event</param>
            protected override void OnActivated(EventArgs e)
            {
                base.OnActivated(e);

                XamDockManager dockManager = XamDockManager.GetDockManager(this);

                if (null != dockManager)
                    dockManager.ActivePaneManager.OnToolWindowActivationChanged(this);
            }
            #endregion //OnActivated

            #region OnDeactivated
            /// <summary>
            /// Invoked when the window is activated.
            /// </summary>
            /// <param name="e">Provides data for the event</param>
            protected override void OnDeactivated(EventArgs e)
            {
                base.OnDeactivated(e);

                XamDockManager dockManager = XamDockManager.GetDockManager(this);

                if (null != dockManager)
                    dockManager.ActivePaneManager.OnToolWindowActivationChanged(this);
            }
            #endregion //OnDeactivated

            #region OnKeyDown

            /// <summary>
            /// Called when a key is pressed
            /// </summary>
            protected override void OnKeyDown(KeyEventArgs e)
            {
                base.OnKeyDown(e);

                if (e.Handled == true)
                    return;

                XamDockManager dockManager = XamDockManager.GetDockManager(this);

                if (null != dockManager)
                    dockManager.ProcessKeyDown(e);
            }
            #endregion //OnKeyDown

            #region OnKeyUp

            /// <summary>
            /// Called when a key is pressed
            /// </summary>
            protected override void OnKeyUp(KeyEventArgs e)
            {
                base.OnKeyUp(e);

                if (e.Handled == true)
                    return;

                XamDockManager dockManager = XamDockManager.GetDockManager(this);

                if (null != dockManager)
                    dockManager.ProcessKeyUp(e);
            }
            #endregion //OnKeyUp

			// AS 8/24/09 TFS19861
			#region SupportsAllowsTransparency
			protected override bool SupportsAllowsTransparency
			{
				get
				{
					return !DockManagerUtilities.GetPreventAllowsTransparency(this);
				}
			} 
			#endregion //SupportsAllowsTransparency

            #endregion //Base class overrides
        } 
        #endregion //FlyoutToolWindow
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