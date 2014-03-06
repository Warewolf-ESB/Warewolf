using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using Infragistics.Shared;
using System.ComponentModel;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// A custom <see cref="Panel"/> used to position the <see cref="ItemsControl.Items"/> of an <see cref="ApplicationMenu"/>
	/// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class ApplicationMenuItemsPanel : Panel, IScrollInfo
	{
		#region Member Variables

		private ApplicationMenuPresenter _menu;
		private bool _generateChildrenCalled = false;
		private List<UIElement> _generatedChildren;
		private ItemContainerGenerator _itemContainerGenerator;
		private UIElementCollection _privateChildren;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes an instance of the <see cref="ApplicationMenuItemsPanel"/> class.
		/// </summary>
		public ApplicationMenuItemsPanel()
		{
		}

		#endregion //Constructor

		#region Base Class Overrides

		#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			// JM BR28203 11-06-07 
			if (this._privateChildren == null)
				return finalSize;

			this.ScrollDataInfo.VerifyScrollData(finalSize, this.ScrollDataInfo._extent);

			Rect finalRect = new Rect(finalSize);

			finalRect.Location = new Point(-this.ScrollDataInfo._offset.X, -this.ScrollDataInfo._offset.Y);

			for (int i = 0; i < this._privateChildren.Count; i++)
			{
				UIElement element = this._privateChildren[i];

				if (null != element && element.Visibility != Visibility.Collapsed)
				{
					Size desiredSize = element.DesiredSize;
					finalRect.Height = desiredSize.Height;
					finalRect.Width = Math.Max(desiredSize.Width, finalSize.Width);

					element.Arrange(finalRect);

					finalRect.Y += finalRect.Height;
				}
			}

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
			if (index < 0 || index >= this.VisualChildrenCount)
				throw new ArgumentOutOfRangeException("index");

			if (this._privateChildren == null)
				return base.GetVisualChild(index);

			return this._privateChildren[index];
		}

		#endregion //GetVisualChild

		#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			Size desiredSize = new Size();

			if (this._menu != null)
			{
				ApplicationMenuRecentItemsPanel recentItemsPanel = this._menu.RecentItemsPanel;

				if (recentItemsPanel == null)
					throw new InvalidOperationException(XamRibbon.GetString("LE_ApplicationMenuRecentItemsPanelMissing"));

				this.GenerateChildren();

				Size measureSize = availableSize;

				if (true) // isscrolling)
					measureSize.Height = double.PositiveInfinity;

				// always get the full desired width
				measureSize.Width = double.PositiveInfinity;

				for (int i = 0; i < this._privateChildren.Count; i++)
				{
					UIElement child = this._privateChildren[i];

					if (null != child && child.Visibility != Visibility.Collapsed)
					{
						child.Measure(measureSize);

						Size childSize = child.DesiredSize;

						if (childSize.Width > desiredSize.Width)
							desiredSize.Width = childSize.Width;

						desiredSize.Height += childSize.Height;
					}
				}
			}

			// verify the scroll extent
			this.ScrollDataInfo.VerifyScrollData(availableSize, desiredSize);

			// if we need more room than we are offered, just indicate that we
			// need the space returned since we will be scrolling the rest
			if (availableSize.Width < desiredSize.Width)
				desiredSize.Width = availableSize.Width;
			if (availableSize.Height < desiredSize.Height)
				desiredSize.Height = availableSize.Height;

			return desiredSize;
		}

		#endregion //MeasureOverride

		#region OnIsItemsHostChanged
		/// <summary>
		/// Invoked when the panel becomes or is no longer the items host of an ItemsControl.
		/// </summary>
		/// <param name="oldIsItemsHost">Old state</param>
		/// <param name="newIsItemsHost">New state</param>
		protected override void OnIsItemsHostChanged(bool oldIsItemsHost, bool newIsItemsHost)
		{
			// AS 9/21/09 TFS21981
			if (newIsItemsHost == false)
			{
				this.Release();
			}

			System.Diagnostics.Debug.Assert(!newIsItemsHost || !_generateChildrenCalled);

			base.OnIsItemsHostChanged(oldIsItemsHost, newIsItemsHost);
		}
		#endregion //OnIsItemsHostChanged

		#region VisualChildrenCount

		/// <summary>
		/// Returns the number of visual children for the element.
		/// </summary>
		protected override int VisualChildrenCount
		{
			get
			{
				if (this._privateChildren == null)
					return base.VisualChildrenCount;

				return this._privateChildren.Count;
			}
		}

		#endregion //VisualChildrenCount

		#endregion //Base Class Overrides

		#region Properties

		#region Internal Properties

		#region GeneratedChildren

		internal List<UIElement> GeneratedChildren
		{
			get
			{
				if (this._generatedChildren == null)
					this._generatedChildren = new List<UIElement>();

				return this._generatedChildren;
			}
		}

		#endregion //GeneratedChildren

		#region Menu

		internal ApplicationMenuPresenter Menu
		{
			get { return this._menu; }
			set 
			{
				// AS 9/21/09 TFS21981
				//this._menu = value; 
				if (value != _menu)
				{
					this.Release();
					this._menu = value;
				}
			}
		}

		#endregion //Menu

		#endregion //Internal Properties

		#region Private Properties

		#region ScrollDataInfo

		private ScrollData _scrollDataInfo;

		private ScrollData ScrollDataInfo
		{
			get
			{
				if (this._scrollDataInfo == null)
					this._scrollDataInfo = new ScrollData();

				return this._scrollDataInfo;
			}
		}
		#endregion //ScrollDataInfo

		#endregion //Private Properties

		#endregion //Properties

		#region Methods

		#region Private Methods

		#region GenerateChildren

		private void GenerateChildren()
		{
			if (this._generateChildrenCalled)
				return;
		
			// AS 1/5/12 TFS96761
			//if (base.IsItemsHost && (this.Menu != null))
			if (base.IsItemsHost && this.Menu != null && this.Menu.RibbonTool != null)
			{
				if (this._itemContainerGenerator == null)
				{
					this._itemContainerGenerator = ((IItemContainerGenerator)this.Menu.ItemContainerGenerator).GetItemContainerGeneratorForPanel(this);
					this._itemContainerGenerator.ItemsChanged += new ItemsChangedEventHandler(this.OnItemsChanged);
				}

				IItemContainerGenerator generator = this._itemContainerGenerator;
				generator.RemoveAll();
				if (this._privateChildren == null)
					this._privateChildren = this.CreateUIElementCollection(null);
				else
					this._privateChildren.Clear();


				this.GeneratedChildren.Clear();
				ApplicationMenuRecentItemsPanel recentItemsPanel = this.Menu.RecentItemsPanel;
				if (recentItemsPanel != null)
					recentItemsPanel.Children.Clear();


				
#region Infragistics Source Cleanup (Region)

































#endregion // Infragistics Source Cleanup (Region)

				this._generateChildrenCalled = true;
				int index = 0;
				ApplicationMenu menu = this.Menu.RibbonTool as ApplicationMenu;
				int maxIndex = menu.Items.Count;

				using (IDisposable disposable = generator.StartAt(new GeneratorPosition(-1, 0), GeneratorDirection.Forward))
				{
					UIElement generatedElement;
					while ((generatedElement = generator.GenerateNext() as UIElement) != null)
					{
						this.GeneratedChildren.Add(generatedElement);

						if (index < maxIndex || recentItemsPanel == null)
							this._privateChildren.Add(generatedElement);
						else
							recentItemsPanel.Children.Add(generatedElement);

						index++;

						generator.PrepareItemContainer(generatedElement);
					}
				}
			}
			else
				this._privateChildren = base.InternalChildren;
		}

		#endregion //GenerateChildren

		#region OnItemsChanged

		private void OnItemsChanged(object sender, ItemsChangedEventArgs args)
		{
			switch (args.Action)
			{
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
				case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
				case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
				case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
					this._generateChildrenCalled = false;
					this.GenerateChildren();

					break;
			}

			base.InvalidateMeasure();
		}

		#endregion //OnItemsChanged

		// AS 9/21/09 TFS21981
		#region Release
		private void Release()
		{
			if (null != this._itemContainerGenerator)
				this._itemContainerGenerator.ItemsChanged -= new ItemsChangedEventHandler(this.OnItemsChanged);

			if (null != _privateChildren)
			{
				_privateChildren.Clear();
			}

			if (null != _generatedChildren)
			{
				_generatedChildren.Clear();
			}

			if (null != _menu)
			{
				ApplicationMenuRecentItemsPanel recentItemsPanel = _menu.RecentItemsPanel;

				if (recentItemsPanel != null)
					recentItemsPanel.Children.Clear();
			}
		}
		#endregion //Release

		#endregion //Private Methods

		#endregion //Methods

		#region ScrollData class
		internal class ScrollData
		{
			#region Member Variables

			internal ScrollViewer _scrollOwner = null;
			internal Size _extent = new Size();
			internal Size _viewport = new Size();
			internal Vector _offset = new Vector();
			internal bool _canHorizontallyScroll = false;
			internal bool _canVerticallyScroll = false;

			#endregion //Member Variables

			#region Methods

			#region Reset

			internal void Reset()
			{
				this._offset = new Vector();
				this._extent = new Size();
				this._viewport = new Size();
			}

			#endregion //Reset

			#region VerifyScrollData
			internal void VerifyScrollData(Size viewPort, Size extent)
			{
				// if we have endless space use the space we need
				if (double.IsInfinity(viewPort.Width))
					viewPort.Width = extent.Width;
				if (double.IsInfinity(viewPort.Height))
					viewPort.Height = extent.Height;

				bool isDifferent = false == RibbonGroupPanel.AreClose(this._viewport.Width, viewPort.Width) ||
					false == RibbonGroupPanel.AreClose(this._viewport.Height, viewPort.Height) ||
					false == RibbonGroupPanel.AreClose(this._extent.Width, extent.Width) ||
					false == RibbonGroupPanel.AreClose(this._extent.Width, extent.Width);

				this._viewport = viewPort;
				this._extent = extent;

				isDifferent |= this.VerifyOffset();

				// dirty the scroll viewer if something has changed
				if (null != this._scrollOwner && isDifferent)
				{
					this._scrollOwner.InvalidateScrollInfo();
				}
			}
			#endregion //VerifyScrollData

			#region VerifyOffset
			private bool VerifyOffset()
			{
				double offsetX = Math.Max(Math.Min(this._offset.X, this._extent.Width - this._viewport.Width), 0);
				double offsetY = Math.Max(Math.Min(this._offset.Y, this._extent.Height - this._viewport.Height), 0);
				Vector oldOffset = this._offset;
				this._offset = new Vector(offsetX, offsetY);

				// return true if the offset has changed
				return false == RibbonGroupPanel.AreClose(this._offset.X, oldOffset.X) ||
					false == RibbonGroupPanel.AreClose(this._offset.Y, oldOffset.Y);
			}
			#endregion //VerifyOffset

			#endregion //Methods
		}
		#endregion //ScrollData class

		#region IScrollInfo Members

		const double LineOffset = 16;

		private void AdjustVerticalOffset(double adjustment)
		{
			((IScrollInfo)this).SetVerticalOffset(adjustment + ((IScrollInfo)this).VerticalOffset);
		}

		private void AdjustHorizontalOffset(double adjustment)
		{
			((IScrollInfo)this).SetHorizontalOffset(adjustment + ((IScrollInfo)this).HorizontalOffset);
		}

		bool IScrollInfo.CanHorizontallyScroll
		{
			get
			{
				return this.ScrollDataInfo._canHorizontallyScroll;
			}
			set
			{
				this.ScrollDataInfo._canHorizontallyScroll = value;
			}
		}

		bool IScrollInfo.CanVerticallyScroll
		{
			get
			{
				return this.ScrollDataInfo._canVerticallyScroll;
			}
			set
			{
				this.ScrollDataInfo._canVerticallyScroll = value;
			}
		}

		double IScrollInfo.ExtentHeight
		{
			get { return this.ScrollDataInfo._extent.Height; }
		}

		double IScrollInfo.ExtentWidth
		{
			get { return this.ScrollDataInfo._extent.Width; }
		}

		double IScrollInfo.HorizontalOffset
		{
			get { return this.ScrollDataInfo._offset.X; }
		}

		void IScrollInfo.LineDown()
		{
			this.AdjustVerticalOffset(LineOffset);
		}

		void IScrollInfo.LineLeft()
		{
			this.AdjustHorizontalOffset(-LineOffset);
		}

		void IScrollInfo.LineRight()
		{
			this.AdjustHorizontalOffset(LineOffset);
		}

		void IScrollInfo.LineUp()
		{
			this.AdjustVerticalOffset(-LineOffset);
		}

		Rect IScrollInfo.MakeVisible(System.Windows.Media.Visual visual, Rect rectangle)
		{
			if (rectangle.IsEmpty || visual == null || this.IsAncestorOf(visual) == false)
				return Rect.Empty;

			Rect visualRect = visual.TransformToAncestor(this).TransformBounds(rectangle);

			Rect availableRect = new Rect(this.RenderSize);
			Rect intersection = Rect.Intersect(visualRect, availableRect);

			if (intersection.Width != visualRect.Width)
			{
				double offsetX = 0;

				// try to get the right side in view
				if (visualRect.Right > availableRect.Right)
					offsetX = visualRect.Right - availableRect.Right;

				// make sure that the left side is in view
				if (visualRect.Left - offsetX - availableRect.Left < 0)
					offsetX += visualRect.Left - offsetX - availableRect.Left;

				visualRect.X -= offsetX;

				offsetX += ((IScrollInfo)this).HorizontalOffset;

				((IScrollInfo)this).SetHorizontalOffset(offsetX);
			}

			if (intersection.Height != visualRect.Height)
			{
				double offsetY = 0;

				// try to get the right side in view
				if (visualRect.Bottom > availableRect.Bottom)
					offsetY = visualRect.Bottom - availableRect.Bottom;

				// make sure that the left side is in view
				if (visualRect.Top - offsetY - availableRect.Top < 0)
					offsetY += visualRect.Top - offsetY - availableRect.Top;

				visualRect.Y -= offsetY;

				offsetY += ((IScrollInfo)this).VerticalOffset;

				((IScrollInfo)this).SetVerticalOffset(offsetY);
			}

			return visualRect;
		}

		void IScrollInfo.MouseWheelDown()
		{
			this.AdjustVerticalOffset(SystemParameters.WheelScrollLines * LineOffset);
		}

		void IScrollInfo.MouseWheelLeft()
		{
			this.AdjustHorizontalOffset(-SystemParameters.WheelScrollLines * LineOffset);
		}

		void IScrollInfo.MouseWheelRight()
		{
			this.AdjustHorizontalOffset(SystemParameters.WheelScrollLines * LineOffset);
		}

		void IScrollInfo.MouseWheelUp()
		{
			this.AdjustVerticalOffset(-SystemParameters.WheelScrollLines * LineOffset);
		}

		void IScrollInfo.PageDown()
		{
			this.AdjustVerticalOffset(-((IScrollInfo)this).ViewportHeight);
		}

		void IScrollInfo.PageLeft()
		{
			this.AdjustHorizontalOffset(-((IScrollInfo)this).ViewportWidth);
		}

		void IScrollInfo.PageRight()
		{
			this.AdjustHorizontalOffset(((IScrollInfo)this).ViewportWidth);
		}

		void IScrollInfo.PageUp()
		{
			this.AdjustVerticalOffset(((IScrollInfo)this).ViewportHeight);
		}

		ScrollViewer IScrollInfo.ScrollOwner
		{
			get
			{
				return this.ScrollDataInfo._scrollOwner;
			}
			set
			{
				this.ScrollDataInfo._scrollOwner = value;
			}
		}

		void IScrollInfo.SetHorizontalOffset(double offset)
		{
			RibbonGroupPanel.EnsureIsANumber(offset);
			offset = Math.Max(offset, 0);

			if (false == RibbonGroupPanel.AreClose(offset, this.ScrollDataInfo._offset.X))
			{
				this.ScrollDataInfo._offset.X = offset;
				this.InvalidateArrange();
			}
		}

		void IScrollInfo.SetVerticalOffset(double offset)
		{
			RibbonGroupPanel.EnsureIsANumber(offset);
			offset = Math.Max(offset, 0);

			if (RibbonGroupPanel.AreClose(offset, this.ScrollDataInfo._offset.Y) == false)
			{
				this.ScrollDataInfo._offset.Y = offset;
				this.InvalidateArrange();
			}
		}

		double IScrollInfo.VerticalOffset
		{
			get { return this.ScrollDataInfo._offset.Y; }
		}

		double IScrollInfo.ViewportHeight
		{
			get { return this.ScrollDataInfo._viewport.Height; }
		}

		double IScrollInfo.ViewportWidth
		{
			get { return this.ScrollDataInfo._viewport.Width; }
		}

		#endregion // IScrollInfo
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