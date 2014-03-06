using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Data;
using Infragistics.Windows.Helpers;
using System.Windows.Media;
using System.ComponentModel;
using Infragistics.Shared;

namespace Infragistics.Windows.Controls
{
	// AS 3/19/08 NA 2008 Vol 1 - XamDockManager
	/// <summary>
	/// Custom adorner for managing the positioning of <see cref="ToolWindow"/> elements within an adorner layer.
	/// </summary>
	internal class ToolWindowAdorner : Adorner
	{
		#region Member Variables

		private ToolWindowAdornerPanel _panel;
		private ToolWindowAdornerPanel _topMostPanel;

		#endregion //Member Variables

		#region Constructor
		internal ToolWindowAdorner(UIElement adornedElement)
			: base(adornedElement)
		{
			this._panel = new ToolWindowAdornerPanel();
			this.AddVisualChild(this._panel);
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
			Rect rect = new Rect(finalSize);

			this._panel.Arrange(rect);

			if (null != this._topMostPanel)
				this._topMostPanel.Arrange(rect);

			return finalSize;
		}
		#endregion //ArrangeOverride

		#region GetVisualChild
		protected override System.Windows.Media.Visual GetVisualChild(int index)
		{
			if (index == 0)
				return this._panel;

			if (index == 1 && this._topMostPanel != null)
				return this._topMostPanel;

			throw new ArgumentOutOfRangeException();
		}
		#endregion //GetVisualChild

		#region VisualChildrenCount
		protected override int VisualChildrenCount
		{
			get
			{
				int count = 1;

				// be sure to include the topmost panel
				if (null != this._topMostPanel)
					count++;

				return count;
			}
		}
		#endregion //VisualChildrenCount

		#endregion //Base class overrides

		#region Properties

		#region TopMostPanel
		private ToolWindowAdornerPanel TopMostPanel
		{
			get
			{
				if (null == this._topMostPanel)
				{
					this._topMostPanel = new ToolWindowAdornerPanel();
					this.AddVisualChild(this._topMostPanel);

					// we need to invalidate our measure so the panel will be measured
					this.InvalidateMeasure();
				}

				return this._topMostPanel;
			}
		}
		#endregion //TopMostPanel

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region AddWindow

		/// <summary>
		/// Helper method for adding a <see cref="ToolWindow"/> to the adorner layer
		/// </summary>
		/// <param name="window">The ToolWindow to display within the adorner layer</param>
		/// <param name="owner">The owning element for the content</param>
		/// <returns>Returns an object that can be used to show/cancel the window.</returns>
		// AS 4/28/11 TFS73532
		// Added return value.
		//
		internal ToolWindow.IDeferShow AddWindow(ToolWindow window, FrameworkElement owner)
		{
			Debug.Assert(Utilities.IsDescendantOf(this.AdornedElement, owner));

			
			// of the elements for that adorner. when unloaded remove the adorner items

			ToolWindowAdornerItem item = new ToolWindowAdornerItem();
			item.Child = window;

			if (item.Topmost == false)
				this._panel.Children.Add(item);
			else
				this.TopMostPanel.Children.Add(item);

			// hook activated so we can change its zorder
			window.Activated += new EventHandler(OnToolWindowActivated);

			// AS 4/28/11 TFS73532
			return new DeferShowHelper(item);
		}
		#endregion //AddWindow

		#region BringToFront
		internal void BringToFront(ToolWindow toolWindow)
		{
			ToolWindowAdornerItem item = VisualTreeHelper.GetParent(toolWindow) as ToolWindowAdornerItem;
			ToolWindowAdornerPanel panel = item != null ? this.GetPanel(item) : null;

			if (null != panel)
				panel.BringToFront(item);
		}
		#endregion //BringToFront

		#region GetToolWindows
		internal void GetToolWindows(FrameworkElement owner, List<ToolWindow> windows, ref int insertAtIndex )
		{
			if (null != this._topMostPanel)
				this._topMostPanel.GetToolWindows(owner, windows, ref insertAtIndex );

			this._panel.GetToolWindows(owner, windows, ref insertAtIndex );
		}
		#endregion //GetToolWindows

		#region RemoveWindow
		/// <summary>
		/// Removes a content window.
		/// </summary>
		/// <param name="window">The window to be removed.</param>
		internal void RemoveWindow(ToolWindow window)
		{
			ToolWindowAdornerPanel panel = this.GetPanel(window);

			Debug.Assert(panel != null);

			if (null != panel)
				this.RemoveWindow(window, panel);
		}

		private bool RemoveWindow(ToolWindow window, ToolWindowAdornerPanel panel)
		{
			for (int i = 0, count = panel.Children.Count - 1; i < count; i++)
			{
				ToolWindowAdornerItem item = panel.Children[i] as ToolWindowAdornerItem;

				if (null != item && item.Child == window)
				{
					// unhook activated
					window.Activated -= new EventHandler(OnToolWindowActivated);
					item.Child = null;
					panel.Children.RemoveAt(i);
					return true;
				}
			}

			return false;
		}
		#endregion //RemoveWindow

		#endregion //Internal Methods

		#region Private Methods

		#region GetPanel
		private ToolWindowAdornerPanel GetPanel(ToolWindow window)
		{
			ToolWindowAdornerItem item = VisualTreeHelper.GetParent(window) as ToolWindowAdornerItem;

			Debug.Assert(null != item);

			return item == null ? null : GetPanel(item);
		}

		private ToolWindowAdornerPanel GetPanel(ToolWindowAdornerItem item)
		{
			ToolWindowAdornerPanel panel = VisualTreeHelper.GetParent(item) as ToolWindowAdornerPanel;

			Debug.Assert(null != panel || VisualTreeHelper.GetParent(item) == null);

			return panel;
		} 
		#endregion //GetPanel

		#region OnToolWindowActivated
		private void OnToolWindowActivated(object sender, EventArgs e)
		{
			Debug.Assert(sender is ToolWindow);

			ToolWindowAdornerItem item = VisualTreeHelper.GetParent((DependencyObject)sender) as ToolWindowAdornerItem;

			if (null != item)
			{
				ToolWindowAdornerPanel panel = this.GetPanel(item);

				if (null != panel)
					panel.BringToFront(item);
			}
		}
		#endregion //OnToolWindowActivated

		#region TransferToOtherPanel
		private void TransferToOtherPanel(ToolWindowAdornerItem child, ToolWindowAdornerPanel currentPanel)
		{
			ToolWindowAdornerPanel newPanel = currentPanel == this._panel ? this.TopMostPanel : this._panel;

			currentPanel.Children.Remove(child);
			newPanel.Children.Add(child);
		}
		#endregion //TransferToOtherPanel

		#endregion //Private Methods

		#endregion //Methods

		#region ToolWindowAdornerPanel
		private class ToolWindowAdornerPanel : Panel
		{
			#region Member Variables

			private int _topZIndex = 0;
			private Dictionary<FrameworkElement, ToolWindowOwnerInfo> _owners;

			#endregion //Member Variables

			#region Constructor
			internal ToolWindowAdornerPanel()
			{
				this._owners = new Dictionary<FrameworkElement, ToolWindowOwnerInfo>();
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
				Rect screenRect = new Rect(finalSize);

				// deflate by a couple of pixels
				const double ScreenBorder = 10d;
				screenRect.Inflate(-ScreenBorder, -ScreenBorder);

				foreach (ToolWindowAdornerItem child in this.Children)
				{
					ToolWindow toolWindow = child.Child as ToolWindow;

					double left = child.Left;
					double top = child.Top;
					double width = child.Width;
					double height = child.Height;

					if (double.IsNaN(left))
						left = 0d;

					if (double.IsNaN(top))
						top = 0d;

					if (double.IsNaN(width))
						width = child.DesiredSize.Width;

					if (double.IsNaN(height))
						height = child.DesiredSize.Height;

					Rect rect = new Rect(left, top, width, height);

					// update for relative positioning if necessary
					if (toolWindow != null && toolWindow.Owner != null)
					{
						FrameworkElement relativeElement = toolWindow.Owner;

						if (relativeElement.FindCommonVisualAncestor(this) != null)
						{
							Rect ownerRect = relativeElement.TransformToVisual(this).TransformBounds(new Rect(0, 0, relativeElement.ActualWidth, relativeElement.ActualHeight));

							// AS 11/2/10 TFS49402/TFS49912/TFS51985
							child.PerformInitialPositioning(ref rect);

							toolWindow.AdjustRelativeRect(ownerRect, ref rect);
						}
					}

					if (rect.IntersectsWith(screenRect) == false
						&& (toolWindow == null || toolWindow.KeepOnScreen) )
					{
						// AS 1/5/10 TFS24684
						// Moved to a helper method so we can use similar logic 
						// to ensure the caption is in view.
						//
						//if (rect.X > screenRect.Right)
						//    rect.X = screenRect.Right - 1;
						//else if (rect.X < screenRect.Left)
						//    rect.X = (screenRect.Left + 1) - rect.Width;
						//
						//if (rect.Y > screenRect.Bottom)
						//    rect.Y = screenRect.Bottom - 1;
						//else if (rect.Y < screenRect.Top)
						//    rect.Y = (screenRect.Top + 1) - rect.Height;
						rect.Offset(CalculateAdjustment(screenRect, rect));
					}

					child.Arrange(rect);

					// AS 1/5/10 TFS24684
					// We need to make sure the caption is in view.
					//
					if (null != toolWindow && toolWindow.KeepOnScreen )
					{
						FrameworkElement captionElement = toolWindow.CaptionElement;

						if (null != captionElement && captionElement.IsVisible)
						{
							GeneralTransform gt = captionElement.TransformToAncestor(child);

							if (null != gt)
							{
								// find where the caption area is within the window and then 
								// adjust that based on where the window is positioned so we 
								// can find out what portion if any is clipped by the logical
								// screen
								Rect captionRect = gt.TransformBounds(new Rect(captionElement.RenderSize));

								captionRect.Offset(rect.X, rect.Y);

								// if the entire caption is out of view...
								if (!captionRect.IntersectsWith(screenRect))
								{
									rect.Offset(CalculateAdjustment(screenRect, captionRect));
									child.Arrange(rect);
								}
							}
						}
					}
				}

				return finalSize;
			}
			#endregion //ArrangeOverride

			#region MeasureOverride
			protected override Size MeasureOverride(Size constraint)
			{
				foreach (ToolWindowAdornerItem child in this.Children)
				{
					// AS 8/21/09 TFS20689
					// Since the LabelPresenter contains some shapes that are set to stretch
					// when we measure it with a given constraint they are simply returning 
					// that value as their desired size and so the height of the element 
					// is coming back as the height we are measuring. Since really these items 
					// represents a "window" their height/width shouldn't really be based on 
					// the containing "screen" (i.e. this panel's) constraint. If it turns out 
					// that we need it to matter then perhaps we should measure it with infinity 
					// and only if the desired extent (width and/or height) is beyond that of 
					// our constraint remeasure with the constrained desired size.
					//
					//double width = (double)child.GetValue(FrameworkElement.WidthProperty);
					//double height = (double)child.GetValue(FrameworkElement.HeightProperty);
					//
					//if (double.IsNaN(width))
					//    width = constraint.Width;
					//
					//if (double.IsNaN(height))
					//    height = constraint.Height;
					//
					//Size measureSize = new Size(width, height);
					Size measureSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

					child.Measure(measureSize);
				}

				return constraint;
			}
			#endregion //MeasureOverride

			#region OnVisualChildrenChanged
			protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
			{
				ToolWindowAdornerItem oldItem = visualRemoved as ToolWindowAdornerItem;
				ToolWindowAdornerItem newItem = visualAdded as ToolWindowAdornerItem;

				if (null != oldItem)
					this.OnItemRemoved(oldItem);

				if (null != newItem)
					this.OnItemAdded(newItem);

				base.OnVisualChildrenChanged(visualAdded, visualRemoved);
			} 
			#endregion //OnVisualChildrenChanged

			#endregion //Base class overrides

			#region Methods

			#region BringToFront
			internal void BringToFront(UIElement child)
			{
				Visual topMostElement = this.GetVisualChild(this.VisualChildrenCount - 1);

				if (child != topMostElement)
				{
					child.SetValue(Panel.ZIndexProperty, ++this._topZIndex);
				}
			}
			#endregion //BringToFront

			// AS 1/5/10 TFS24684
			#region CalculateAdjustment
			private static Vector CalculateAdjustment(Rect screenRect, Rect rect)
			{
				Vector adjustment = new Vector();

				if (rect.Left >= screenRect.Right)
					adjustment.X = (screenRect.Right - 1) - rect.Left;
				else if (rect.Right < screenRect.Left)
					adjustment.X = (screenRect.Left + 1) - rect.Right;

				if (rect.Top >= screenRect.Bottom)
					adjustment.Y = (screenRect.Bottom - 1) - rect.Top;
				else if (rect.Bottom < screenRect.Top)
					adjustment.Y = (screenRect.Top + 1) - rect.Bottom;

				return adjustment;
			}
			#endregion //CalculateAdjustment

			#region GetToolWindows
			internal void GetToolWindows(FrameworkElement owner, List<ToolWindow> windows, ref int insertAtIndex )
			{
				for (int i = 0, count = VisualTreeHelper.GetChildrenCount(this); i < count; i++)
				{
					ToolWindowAdornerItem item = VisualTreeHelper.GetChild(this, i) as ToolWindowAdornerItem;
					ToolWindow toolWindow = item != null ? item.Child as ToolWindow : null;

					if (null != toolWindow && toolWindow.Owner == owner)
					{
						// AS 1/10/12 TFS90890
						//windows.Add(toolWindow);
						windows.Insert(insertAtIndex++, toolWindow);
					}
				}
			}
			#endregion //GetToolWindows

			#region OnItemAdded
			private void OnItemAdded(ToolWindowAdornerItem item)
			{
				ToolWindow window = item.Child as ToolWindow;
				Debug.Assert(null != window);

				if (null != window)
				{
					FrameworkElement owner = window.Owner;

					Debug.Assert(null != owner);

					ToolWindowOwnerInfo ownerInfo;

					if (false == this._owners.TryGetValue(owner, out ownerInfo))
					{
						ownerInfo = new ToolWindowOwnerInfo(owner, this);
						this._owners.Add(owner, ownerInfo);
					}

					ownerInfo.AddWindow(item);
				}
			}
			#endregion //OnItemAdded

			#region OnItemRemoved
			private void OnItemRemoved(ToolWindowAdornerItem item)
			{
				ToolWindow window = item.Child as ToolWindow;
				Debug.Assert(null != window);
				Debug.Assert(this._owners != null);

				if (null != window)
				{
					FrameworkElement owner = window.Owner;
					Debug.Assert(null != owner);

					if (null != owner)
					{
						ToolWindowOwnerInfo ownerInfo = this._owners[owner];
						ownerInfo.RemoveWindow(item);
					}
				}
			} 
			#endregion //OnItemRemoved

			#region OnRelativeStateChanged
			internal void OnRelativeStateChanged(ToolWindowAdornerItem item)
			{
				ToolWindow window = item.Child as ToolWindow;
				Debug.Assert(null != window);

				if (null != window)
				{
					FrameworkElement owner = window.Owner;

					Debug.Assert(null != owner);

					ToolWindowOwnerInfo ownerInfo = this._owners[owner];

					ownerInfo.VerifyRelationPositionCount();
				}
			} 
			#endregion //OnRelativeStateChanged

			#region OnTopmostStateChanged

			internal void OnTopmostStateChanged(ToolWindowAdornerItem child)
			{
				int index = this.Children.IndexOf(child);

				if (index >= 0)
				{
					ToolWindowAdorner adorner = VisualTreeHelper.GetParent(this) as ToolWindowAdorner;

					Debug.Assert(null != adorner);

					if (null != adorner)
						adorner.TransferToOtherPanel(child, this);
				}
			}
			#endregion //OnTopmostStateChanged

			#endregion //Methods
		}
		#endregion //ToolWindowAdornerPanel

		#region ToolWindowOwnerInfo
		private class ToolWindowOwnerInfo
		{
			#region Member Variables

			private ToolWindowAdornerPanel _panel;
			private FrameworkElement _owner;
			private List<ToolWindowAdornerItem> _windows;
			private Rect _lastRelativeBounds = Rect.Empty;
			private int _relativeItemCount;

			#endregion //Member Variables

			#region Constructor
			internal ToolWindowOwnerInfo(FrameworkElement owner, ToolWindowAdornerPanel panel)
			{
				this._owner = owner;
				this._panel = panel;
				this._windows = new List<ToolWindowAdornerItem>();
			}
			#endregion //Constructor

			#region Methods

			#region AddWindow
			internal void AddWindow(ToolWindowAdornerItem item)
			{
				Debug.Assert(false == this._windows.Contains(item));
				this._windows.Add(item);

				if (item.UsesRelativePosition)
				{
					this._relativeItemCount++;

					if (this._relativeItemCount == 1)
					{
						this._panel.LayoutUpdated += new EventHandler(OnPanelLayoutUpdated);
						this._lastRelativeBounds = this._owner.TransformToVisual(this._panel).TransformBounds(new Rect(0, 0, this._owner.ActualWidth, this._owner.ActualHeight));
					}
				}
			} 
			#endregion //AddWindow

			#region OnPanelLayoutUpdated
			private void OnPanelLayoutUpdated(object sender, EventArgs e)
			{
				if (null != this._owner.FindCommonVisualAncestor(this._panel))
				{
					Rect newBounds = this._owner.TransformToVisual(this._panel).TransformBounds(new Rect(0, 0, this._owner.ActualWidth, this._owner.ActualHeight));

					if (newBounds != this._lastRelativeBounds)
					{
						this._lastRelativeBounds = newBounds;
						this._panel.InvalidateArrange();
					}
				}
			} 
			#endregion //OnPanelLayoutUpdated

			#region RemoveWindow
			internal void RemoveWindow(ToolWindowAdornerItem item)
			{
				Debug.Assert(true == this._windows.Contains(item));
				this._windows.Remove(item);

				if (item.UsesRelativePosition)
				{
					this._relativeItemCount--;

					if (this._relativeItemCount == 0)
					{
						this._panel.LayoutUpdated -= new EventHandler(OnPanelLayoutUpdated);
						this._lastRelativeBounds = Rect.Empty;
					}
				}
			} 
			#endregion //RemoveWindow

			#region VerifyRelationPositionCount
			internal void VerifyRelationPositionCount()
			{
				int relativeItemCount = 0;

				for (int i = 0, count = this._windows.Count; i < count; i++)
				{
					if (this._windows[i].UsesRelativePosition)
						relativeItemCount++;
				}

				// we have a change...
				if (relativeItemCount != this._relativeItemCount)
				{
					if (this._relativeItemCount == 0)
					{
						this._panel.LayoutUpdated += new EventHandler(OnPanelLayoutUpdated);
						this._panel.InvalidateArrange();
					}
					else if (relativeItemCount == 0)
					{
						this._panel.LayoutUpdated -= new EventHandler(OnPanelLayoutUpdated);
						this._lastRelativeBounds = Rect.Empty;
						this._panel.InvalidateArrange();
					}

					this._relativeItemCount = relativeItemCount;
				}
			} 
			#endregion //VerifyRelationPositionCount

			#endregion //Methods
		} 
		#endregion //ToolWindowOwnerInfo

        #region ToolWindowAdornerItem (old)
        
#region Infragistics Source Cleanup (Region)





































































































































































































































































































































































































































































































































































































































































#endregion // Infragistics Source Cleanup (Region)

		#endregion //ToolWindowAdornerItem (old)

		#region ToolWindowAdornerItem (new)
		private class ToolWindowAdornerItem : ToolWindowContainer
			, IToolWindowHost // AS 9/11/09 TFS21330
		{
			#region Constructor
			static ToolWindowAdornerItem()
			{
				ToolWindowContainer.TopmostProperty.OverrideMetadata(typeof(ToolWindowAdornerItem), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnTopmostChanged)));
			}
			#endregion //Constructor

			#region Base class overrides

			#region GetRelativeScreenPoint
			protected override Point GetRelativeScreenPoint(MouseEventArgs e)
			{
				// constrain the point based on the root visual
				FrameworkElement rootElement = Utilities.GetAncestorFromType(this, typeof(AdornerLayer), true) as FrameworkElement;

				Point ancestorPoint = e.GetPosition(rootElement);

				if (ancestorPoint.X > rootElement.RenderSize.Width)
					ancestorPoint.X = rootElement.RenderSize.Width;
				else if (ancestorPoint.X < 0)
					ancestorPoint.X = 0;

				if (ancestorPoint.Y > rootElement.RenderSize.Height)
					ancestorPoint.Y = rootElement.RenderSize.Height;
				else if (ancestorPoint.Y < 0)
					ancestorPoint.Y = 0;

				// AS 1/6/10 TFS25834
				//Point newPoint = rootElement.TransformToDescendant(this).Transform(ancestorPoint);
				GeneralTransform gt = rootElement.TransformToDescendant(this);
				Debug.Assert(null != gt);
				if (gt == null)
					return ancestorPoint;

				Point newPoint = gt.Transform(ancestorPoint);

				newPoint = Utilities.PointToScreenSafe(this, newPoint);

				return newPoint;
			} 
			#endregion //GetRelativeScreenPoint

			#region Close
			public override void Close()
			{
				ToolWindowAdornerPanel panel = VisualTreeHelper.GetParent(this) as ToolWindowAdornerPanel;
				ToolWindow window = this.Child as ToolWindow;

				Debug.Assert(null != panel || VisualTreeHelper.GetParent(this) == null);
				Debug.Assert(null != window);

				if (panel != null)
				{
					if (null != window && window.OnClosingInternal(new CancelEventArgs()))
						return;

					panel.Children.Remove(this);
					this.Child = null;

					if (null != window)
						window.OnClosedInternal();
				}
			} 
			#endregion //Close

			#region RelativePositionStateChanged
			public override void RelativePositionStateChanged()
			{
				ToolWindowAdornerPanel panel = VisualTreeHelper.GetParent(this) as ToolWindowAdornerPanel;

				Debug.Assert(null != panel || VisualTreeHelper.GetParent(this) == null);

				if (null != panel)
					panel.OnRelativeStateChanged(this);
			} 
			#endregion //RelativePositionStateChanged

			// AS 5/14/08 BR32842
			#region BringToFront
			public override void BringToFront()
			{
				ToolWindowAdornerPanel panel = VisualTreeHelper.GetParent(this) as ToolWindowAdornerPanel;

				Debug.Assert(null != panel || VisualTreeHelper.GetParent(this) == null);

				if (null != panel)
					panel.BringToFront(this);
			} 
			#endregion //BringToFront

			// AS 9/11/09 TFS21330
			#region AllowsTransparency
			public override bool AllowsTransparency
			{
				get
				{
					return true;
				}
			}
			#endregion //AllowsTransparency

			// AS 9/11/09 TFS21330
			#region SupportsAllowTransparency
			public override bool SupportsAllowTransparency(bool allowsTransparency)
			{
				return allowsTransparency == true;
			}
			#endregion //SupportsAllowTransparency

			#endregion //Base class overrides

			#region Methods

			private static void OnTopmostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
			{
				ToolWindowAdornerItem adornerItem = d as ToolWindowAdornerItem;
				ToolWindowAdornerPanel panel = VisualTreeHelper.GetParent(adornerItem) as ToolWindowAdornerPanel;

				Debug.Assert(null != panel || VisualTreeHelper.GetParent(adornerItem) == null);
				Debug.Assert(null != adornerItem);

				if (null != panel && null != adornerItem)
					panel.OnTopmostStateChanged(adornerItem);
			}

			#endregion //Methods
		} 
		#endregion //ToolWindowAdornerItem (new)

		// AS 4/28/11 TFS73532
		#region DeferShowHelper class
		private class DeferShowHelper : ToolWindow.IDeferShow
		{
			private ToolWindowAdornerItem _window;

			internal DeferShowHelper(ToolWindowAdornerItem window)
			{
				_window = window;
			}

			public void Show(bool activate)
			{
				if (activate)
					_window.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
			}

			public void Cancel()
			{
				_window.Close();
			}
		}
		#endregion //DeferShowHelper class
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