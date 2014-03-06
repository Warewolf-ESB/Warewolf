using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace Infragistics.Controls.Interactions.Primitives
{
	/// <summary>
	/// Contains a list of auto-complete items based on user input in a formula editor.
	/// </summary>
	public class AutoCompleteList : ListBox
	{
		#region Member Variables

		private DispatcherTimer _autoScrollTimer;
		private Panel _itemsHost;
		private ScrollViewer _scrollViewer;







		#endregion  // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="AutoCompleteList"/> instance.
		/// </summary>
		public AutoCompleteList()
		{
		}

		#endregion  // Constructor

		#region Base Class Overrides

		#region GetContainerForItemOverride

		/// <summary>
		/// Creates a containers used to display an item.
		/// </summary>
		/// <returns>An <see cref="AutoCompleteListItem"/> instance.</returns>
		protected override System.Windows.DependencyObject GetContainerForItemOverride()
		{
			return new AutoCompleteListItem();
		}

		#endregion  // GetContainerForItemOverride

		#region MeasureOverride

		/// <summary>
		/// Called to re-measure the <see cref="AutoCompleteList"/>
		/// </summary>
		/// <param name="constraint">The constraint of the size.</param>
		/// <returns>The new size of the AutoCompleteList.</returns>
		protected override Size MeasureOverride(Size constraint)
		{
			Size measuredSize = base.MeasureOverride(constraint);

			int maxItemsToDisplay = this.MaxItemsToDisplay;
			if (maxItemsToDisplay > 0)
			{
				AutoCompleteListItem item = this.GetAnyVisibleAutoCompleteListItem();

				if (item != null)
				{
					double height = item.DesiredSize.Height;
					double maxHeight = maxItemsToDisplay * height;

					if (maxHeight < measuredSize.Height)
					{
						constraint.Height = maxHeight;
						return base.MeasureOverride(constraint);
					}
				}
			}

			return measuredSize;
		}

		#endregion  // MeasureOverride

		#region OnApplyTemplate

		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_itemsHost = null;
			_scrollViewer = null;

			// MD 5/9/12 - TFS98850
			// Due to the timing in SL, OnApplyTemplate could get called after we have already initialized the contents and 
			// selected index, so don't overwrite the SelectedIndex if it is already valid.
			//if (this.Items.Count > 0)
			if (this.Items.Count > 0 && this.SelectedIndex < 0)
				this.SelectedIndex = 0;
		}

		#endregion  // OnApplyTemplate


		#region OnIsMouseCapturedChanged

		/// <summary>
		/// Occurs when mouse capture is lost by the <see cref="AutoCompleteList"/>.
		/// </summary>
		/// <param name="e">The event data.</param>
		protected override void OnIsMouseCapturedChanged(DependencyPropertyChangedEventArgs e)
		{
			this.OnIsMouseCapturedChanged();

			// We can't call the base OnIsMouseCapturedChanged in WPF because it will start the auto-scroll timer, which 
			// will focus the new item in view.
			//base.OnIsMouseCapturedChanged(e);
		}

		#endregion  // OnIsMouseCapturedChanged


#region Infragistics Source Cleanup (Region)









































#endregion // Infragistics Source Cleanup (Region)


		#region OnMouseMove

		/// <summary>
		/// Occurs when the mouse moves over the <see cref="AutoCompleteList"/> or when it has capture.
		/// </summary>
		/// <param name="e">The event data.</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (this.HasMouseCapture && this.IsLeftButtonPressed)
			{
				// Select the current item under the mouse.


#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

				Point position = e.GetPosition(this);
				DependencyObject obj = this.InputHitTest(position) as DependencyObject;
				while (obj != null && (obj is AutoCompleteList) == false)
				{
					AutoCompleteListItem li = obj as AutoCompleteListItem;
					if (li != null)
					{
						li.IsSelected = true;
						break;
					}

					obj = VisualTreeHelper.GetParent(obj);
				}


				this.DoAutoScroll();
			}

			// We can't call the base OnMouseMove in WPF because it will auto-scroll and focus the new item in view.



		}

		#endregion  // OnMouseMove

		#endregion  // Base Class Overrides

		#region Methods

		#region Internal Methods

		#region BringItemIntoView

		internal void BringItemIntoView(object item, int elementIndex = -1)
		{
			if (elementIndex == -1)
				elementIndex = this.Items.IndexOf(item);

			if (elementIndex == -1)
				return;

			ScrollViewer scrollViewer = this.ScrollViewer;
			if (scrollViewer == null)
				return;

			Panel itemsHost = this.ItemsHost;
			if (itemsHost == null)
				return;

			while (this.MakeVisible(elementIndex))
			{
				double horizontalOffset = scrollViewer.HorizontalOffset;
				double verticalOffset = scrollViewer.VerticalOffset;

				itemsHost.UpdateLayout();

				if (AutoCompleteList.AreValuesClose(horizontalOffset, scrollViewer.HorizontalOffset) &&
					AutoCompleteList.AreValuesClose(verticalOffset, scrollViewer.VerticalOffset))
				{
					break;
				}
			}
		}

		#endregion  // BringItemIntoView

		#region NavigateByLine

		internal void NavigateByLine(bool navigateDown)
		{
			FrameworkElement itemHost = this.ItemsHost;
			if (itemHost == null)
				return;

			object startingItem = this.SelectedItem;

			if (startingItem == null)
			{
				// Navigate to the first item if there are any.
				this.NavigateToItem(this.GetItem(0), 0);
				return;
			}

			if (this.IsItemInView(startingItem) == false)
			{
				this.MakeVisible(this.Items.IndexOf(startingItem));
				itemHost.UpdateLayout();
			}



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

			FrameworkElement startingItemContainer = this.ItemContainerGenerator.ContainerFromItem(startingItem) as FrameworkElement;

			if (startingItemContainer == null ||
				AutoCompleteList.IsAncestorOf(itemHost, startingItemContainer) == false)
			{
				startingItemContainer = this.ScrollViewer;
			}

			FrameworkElement nextItem = startingItemContainer.PredictFocus(
				navigateDown ? FocusNavigationDirection.Down : FocusNavigationDirection.Up) as FrameworkElement;

			if (nextItem != null && AutoCompleteList.IsAncestorOf(itemHost, nextItem))
			{
				object associatedItem = this.GetAssociatedItem(nextItem);

				if (associatedItem != DependencyProperty.UnsetValue)
					this.NavigateToItem(associatedItem);
			}


		}

		#endregion  // NavigateByLine

		#region NavigateByPage

		internal void NavigateByPage(bool navigateDown)
		{
			object startingItem = this.SelectedItem;

			if (startingItem == null)
			{
				this.NavigateToFirstItemOnCurrentPage(startingItem, navigateDown);
				return;
			}

			if (this.IsItemInView(startingItem) == false)
				this.BringItemIntoView(startingItem);

			int firstItemIndex;
			object firstItem = this.GetFirstItemOnCurrentPage(startingItem, navigateDown, out firstItemIndex);

			if (startingItem.Equals(firstItem) == false)
			{
				if (firstItem != DependencyProperty.UnsetValue && firstItem != null)
					this.NavigateToItem(firstItem, firstItemIndex);

				return;
			}

			ScrollViewer scrollViewer = this.ScrollViewer;
			if (scrollViewer == null)
				return;



#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

			if (navigateDown)
				scrollViewer.PageDown();
			else
				scrollViewer.PageUp();


			if (this.ItemsHost != null)
			{
				this.ItemsHost.UpdateLayout();
				this.NavigateToFirstItemOnCurrentPage(startingItem, navigateDown);
			}
		}

		#endregion  // NavigateByPage

		#region NotifyListItemClicked

		internal bool NotifyListItemClicked(AutoCompleteListItem item, MouseButton mouseButton)
		{
			if (mouseButton == MouseButton.Left)
			{





				this.CaptureMouse();
			}

			switch (this.SelectionMode)
			{
				case SelectionMode.Single:
					item.IsSelected = true;
					break;

				default:
					return false;
			}

			return true;
		}

		#endregion  // NotifyListItemClicked

		#endregion  // Internal Methods

		#region Private Methods

		#region AreValuesClose

		private static bool AreValuesClose(double value1, double value2)
		{
			if (value1 == value2)
				return true;

			const double DBL_EPSILON = 2.2204460492503131E-16;

			double threshold = (Math.Abs(value1) + Math.Abs(value2) + 10.0) * DBL_EPSILON;
			double difference = value1 - value2;

			return -threshold < difference && threshold > difference;
		}

		#endregion  // AreValuesClose

		#region DoAutoScroll

		private void DoAutoScroll()
		{
			FrameworkElement relativeTo = (FrameworkElement)this.ScrollViewer ?? this.ItemsHost;

			if (relativeTo == null)
				return;







			Point position = Mouse.GetPosition(relativeTo);


			if (position.Y < 0)
				this.NavigateByLine(false);
			else if (position.Y >= relativeTo.RenderSize.Height)
				this.NavigateByLine(true);
		}

		#endregion  // DoAutoScroll

		#region GetAncestorOfType

		private static T GetAncestorOfType<T>(DependencyObject item) where T : DependencyObject
		{
			while (item != null)
			{
				item = VisualTreeHelper.GetParent(item);

				T ancestor = item as T;
				if (ancestor != null)
					return ancestor;
			}

			return null;
		}

		#endregion  // GetAncestorOfType

		#region GetAnyVisibleAutoCompleteListItem

		private AutoCompleteListItem GetAnyVisibleAutoCompleteListItem()
		{
			int index = this.SelectedIndex;

			AutoCompleteListItem item = null;

			if (index >= 0)
				item = this.ItemContainerGenerator.ContainerFromIndex(index) as AutoCompleteListItem;

			for (int i = 0; i < this.Items.Count && item == null; i++)
				item = this.ItemContainerGenerator.ContainerFromIndex(i) as AutoCompleteListItem;

			return item;
		}

		#endregion  // GetAnyVisibleAutoCompleteListItem

		#region GetAssociatedItem

		private object GetAssociatedItem(FrameworkElement element)
		{
			object item = DependencyProperty.UnsetValue;

			while (item == DependencyProperty.UnsetValue && element != null)
			{
				item = this.ItemContainerGenerator.ItemFromContainer(element);
				element = VisualTreeHelper.GetParent(element) as FrameworkElement;
			}

			return item;
		}

		#endregion  // GetAssociatedItem

		#region GetFirstItemOnCurrentPage

		private object GetFirstItemOnCurrentPage(object startingItem, bool navigateDown, out int foundIndex)
		{
			ScrollViewer scrollViewer = this.ScrollViewer;

			if (scrollViewer == null)
			{
				foundIndex = -1;
				return null;
			}

			if (navigateDown)
				foundIndex = (int)(scrollViewer.VerticalOffset + scrollViewer.ViewportHeight - 1.0);
			else
				foundIndex = (int)scrollViewer.VerticalOffset;

			object item = this.GetItem(foundIndex);
			if (item == null)
				foundIndex = -1;

			return item;
		}

		#endregion  // GetFirstItemOnCurrentPage

		#region GetItem

		private object GetItem(int startIndex)
		{
			if (0 <= startIndex && startIndex < this.Items.Count)
				return this.Items[startIndex];

			return null;
		}

		#endregion  // GetItem

		#region IsAncestorOf

		private static bool IsAncestorOf(



			Visual

			ancestor, DependencyObject child)
		{


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

			return ancestor.IsAncestorOf(child);

		}

		#endregion  // IsAncestorOf

		#region IsItemOnInView

		private bool IsItemInView(object item)
		{
			FrameworkElement element = this.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;

			if (element == null)
				return false;

			FrameworkElement scrollHost = (FrameworkElement)this.ScrollViewer ?? this.ItemsHost;

			if (scrollHost == null || AutoCompleteList.IsAncestorOf(scrollHost, element) == false)
				return false;

			GeneralTransform transform =



				element.TransformToAncestor(scrollHost);


			Rect elementRectInScrollHost = transform.TransformBounds(new Rect(new Point(), element.RenderSize));

			if (CoreUtilities.LessThanOrClose(0, elementRectInScrollHost.Top) &&
				CoreUtilities.LessThanOrClose(elementRectInScrollHost.Bottom, scrollHost.RenderSize.Height))
			{
				return true;
			}

			return false;
		}

		#endregion  // IsItemOnInView

		#region MakeVisible

		private bool MakeVisible(int index)
		{
			if (index == -1)
				return false;

			ScrollViewer scrollViewer = this.ScrollViewer;
			if (scrollViewer == null)
				return false;

			double verticalOffset = scrollViewer.VerticalOffset;
			double newVerticalOffset = verticalOffset;

			int nextIndex = index + 1;
			if (nextIndex > (verticalOffset + scrollViewer.ViewportHeight))
				newVerticalOffset = Math.Max(0d, nextIndex - scrollViewer.ViewportHeight);

			if (index < verticalOffset)
				newVerticalOffset = index;

			if (AutoCompleteList.AreValuesClose(verticalOffset, newVerticalOffset) == false)
			{
				scrollViewer.ScrollToVerticalOffset(newVerticalOffset);
				return true;
			}

			return false;
		}

		#endregion  // MakeVisible

		#region NavigateToFirstItemOnCurrentPage

		private void NavigateToFirstItemOnCurrentPage(object startingItem, bool navigateDown)
		{
			int foundIndex;
			object item = this.GetFirstItemOnCurrentPage(startingItem, navigateDown, out foundIndex);

			if (item != DependencyProperty.UnsetValue)
				this.SelectedItem = item;
		}

		#endregion  // NavigateToFirstItemOnCurrentPage

		#region NavigateToItem

		private void NavigateToItem(object item)
		{
			this.NavigateToItem(item, -1);
		}

		private void NavigateToItem(object item, int elementIndex)
		{
			if (item == DependencyProperty.UnsetValue || item == null)
				return;

			this.BringItemIntoView(item, elementIndex);
			this.SelectedItem = item;
		}

		#endregion  // NavigateToItem

		#region OnAutoScrollTimerTick

		private void OnAutoScrollTimerTick(object sender, EventArgs e)
		{
			if (this.IsLeftButtonPressed && this.HasMouseCapture)
				this.DoAutoScroll();
		}

		#endregion  // OnAutoScrollTimerTick

		#region OnIsMouseCapturedChanged

		private void OnIsMouseCapturedChanged()
		{
			if (this.HasMouseCapture)
			{
				if (_autoScrollTimer == null)
				{
					_autoScrollTimer = new DispatcherTimer(

						DispatcherPriority.SystemIdle

);
					_autoScrollTimer.Interval = TimeSpan.FromMilliseconds(400);
					_autoScrollTimer.Tick += new EventHandler(this.OnAutoScrollTimerTick);
					_autoScrollTimer.Start();
				}
			}
			else if (_autoScrollTimer != null)
			{
				_autoScrollTimer.Stop();
				_autoScrollTimer = null;
			}
		}

		#endregion  // OnIsMouseCapturedChanged

		#endregion  // Private Methods

		#endregion  // Methods

		#region Properties

		#region Public Properties

		#region MaxItemsToDisplay

		/// <summary>
		/// Identifies the <see cref="MaxItemsToDisplay"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxItemsToDisplayProperty = DependencyPropertyUtilities.Register("MaxItemsToDisplay",
			typeof(int), typeof(AutoCompleteList),
			DependencyPropertyUtilities.CreateMetadata(9, new PropertyChangedCallback(OnMaxItemsToDisplayChanged))
			);

		private static void OnMaxItemsToDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			AutoCompleteList instance = (AutoCompleteList)d;
			instance.InvalidateMeasure();
		}

		/// <summary>
		/// Gets or sets the maximum number of items that should display in the auto-complete list.
		/// </summary>
		/// <seealso cref="MaxItemsToDisplayProperty"/>
		public int MaxItemsToDisplay
		{
			get
			{
				return (int)this.GetValue(AutoCompleteList.MaxItemsToDisplayProperty);
			}
			set
			{
				this.SetValue(AutoCompleteList.MaxItemsToDisplayProperty, value);
			}
		}

		#endregion //MaxItemsToDisplay

		#endregion  // Public Properties

		#region Private Properties

		#region HasMouseCapture

		private bool HasMouseCapture
		{
			get
			{



				return this.IsMouseCaptured;

			}
		}

		#endregion  // HasMouseCapture

		#region IsLeftButtonPressed

		private bool IsLeftButtonPressed
		{
			get
			{



				return Mouse.LeftButton == MouseButtonState.Pressed;

			}
		}

		#endregion  // IsLeftButtonPressed

		#region ItemsHost

		private Panel ItemsHost
		{
			get
			{
				if (_itemsHost == null)
					_itemsHost = AutoCompleteList.GetAncestorOfType<Panel>(this.GetAnyVisibleAutoCompleteListItem());

				return _itemsHost;
			}
		}

		#endregion  // ItemsHost

		#region ScrollViewer

		private ScrollViewer ScrollViewer
		{
			get
			{
				if (_scrollViewer == null)
					_scrollViewer = AutoCompleteList.GetAncestorOfType<ScrollViewer>(this.GetAnyVisibleAutoCompleteListItem());

				return _scrollViewer;
			}
		}

		#endregion  // ScrollViewer

		#endregion  // Private Properties

		#endregion  // Properties
	}

	#region AutoCompleteListItem class

	/// <summary>
	/// Represents an item in the auto-complete list.
	/// </summary>
	public class AutoCompleteListItem : ListBoxItem
	{
		#region Base Class Overrides

		#region OnMouseLeftButtonDown

		/// <summary>
		/// Occurs when the left mouse button is pressed down.
		/// </summary>
		/// <param name="e">The event data.</param>
		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			if (e.Handled == false)
				e.Handled = this.HandleMouseButtonDown(MouseButton.Left);

			base.OnMouseLeftButtonDown(e);
		}

		#endregion  // OnMouseLeftButtonDown

		#region OnMouseRightButtonDown

		/// <summary>
		/// Occurs when the right mouse button is pressed down.
		/// </summary>
		/// <param name="e">The event data.</param>
		protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			if (e.Handled == false)
				e.Handled = this.HandleMouseButtonDown(MouseButton.Right);

			base.OnMouseRightButtonDown(e);
		}

		#endregion  // OnMouseRightButtonDown

		#endregion  // Base Class Overrides

		#region Methods

		#region HandleMouseButtonDown

		private bool HandleMouseButtonDown(MouseButton mouseButton)
		{
			AutoCompleteList parentListBox = ItemsControl.ItemsControlFromItemContainer(this) as AutoCompleteList;

			if (parentListBox != null)
				return parentListBox.NotifyListItemClicked(this, mouseButton);

			return false;
		}

		#endregion  // HandleMouseButtonDown

		#endregion  // Methods
	}

	#endregion  // AutoCompleteListItem class

	internal enum MouseButton
	{
		Left,
		Right
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