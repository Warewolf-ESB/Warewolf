using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows;
using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media;
using Infragistics.Windows.Helpers;
using System.Diagnostics;
using System.Windows.Data;
using System.Collections;
using Infragistics.Collections;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Provides a UI for scrolling <see cref="GalleryItem"/>s in the gallery preview area.
	/// </summary>
	[ContentProperty("Child")]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class GalleryPreviewScroller : FrameworkElement
	{
		#region Member Variables

		private UIElement _child;
		private GalleryToolPreviewPresenter _containingGalleryToolPreviewPresenter = null;
		private TranslateTransform _childTransform;
		private double _verticalOffset;
		private int _animationCount;

		#endregion //Member Variables

		#region Commands

		/// <summary>
		/// Drops down the MenuTool that contains the associated GalleryTool.
		/// </summary>
		public static readonly RoutedCommand DropdownGalleryCommand;

		/// <summary>
		/// Scrolls the items in the gallery preview area up.
		/// </summary>
		public static readonly RoutedCommand ScrollUpCommand;

		/// <summary>
		/// Scrolls the items in the gallery preview area down.
		/// </summary>
		public static readonly RoutedCommand ScrollDownCommand;

		#endregion //Commands

		#region Constants

		private const double SCROLL_ANIMATION_DURATION_MILLISECONDS = 300;

		#endregion //Constants

		#region Constructor

		static GalleryPreviewScroller()
		{
			GalleryPreviewScroller.DropdownGalleryCommand = new RoutedCommand("DropdownGallery", typeof(GalleryPreviewScroller));
			GalleryPreviewScroller.ScrollUpCommand = new RoutedCommand("ScrollUp", typeof(GalleryPreviewScroller));
			GalleryPreviewScroller.ScrollDownCommand = new RoutedCommand("ScrollDown", typeof(GalleryPreviewScroller));

			CommandManager.RegisterClassCommandBinding(typeof(GalleryPreviewScroller), new CommandBinding(GalleryPreviewScroller.DropdownGalleryCommand, new ExecutedRoutedEventHandler(GalleryPreviewScroller.OnExecuteCommand), new CanExecuteRoutedEventHandler(GalleryPreviewScroller.OnCanExecuteCommand)));
			CommandManager.RegisterClassInputBinding(typeof(GalleryPreviewScroller), new InputBinding(GalleryPreviewScroller.DropdownGalleryCommand, new KeyGesture(Key.F4)));
			CommandManager.RegisterClassCommandBinding(typeof(GalleryPreviewScroller), new CommandBinding(GalleryPreviewScroller.ScrollUpCommand, new ExecutedRoutedEventHandler(GalleryPreviewScroller.OnExecuteCommand), new CanExecuteRoutedEventHandler(GalleryPreviewScroller.OnCanExecuteCommand)));
			CommandManager.RegisterClassCommandBinding(typeof(GalleryPreviewScroller), new CommandBinding(GalleryPreviewScroller.ScrollDownCommand, new ExecutedRoutedEventHandler(GalleryPreviewScroller.OnExecuteCommand), new CanExecuteRoutedEventHandler(GalleryPreviewScroller.OnCanExecuteCommand)));

			EventManager.RegisterClassHandler(typeof(GalleryPreviewScroller), FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(GalleryPreviewScroller.OnRequestBringIntoView));

			FrameworkElement.ClipToBoundsProperty.OverrideMetadata(typeof(GalleryPreviewScroller), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));
			FrameworkElement.HorizontalAlignmentProperty.OverrideMetadata(typeof(GalleryPreviewScroller), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentCenterBox));
			FrameworkElement.VerticalAlignmentProperty.OverrideMetadata(typeof(GalleryPreviewScroller), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentCenterBox));
		}

		/// <summary>
		/// Initializes a new <see cref="GalleryPreviewScroller"/>
		/// </summary>
		public GalleryPreviewScroller()
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
			if (null != this._child)
			{
				Size childSize = this._child.DesiredSize;
				Rect arrangeRect = new Rect(childSize);

				// AS 8/7/12 TFS117983
				var wrapPanel = _child as GalleryWrapPanel;

				if ( null != wrapPanel )
				{
					// see how many rows will fit completely based on the arrange size
					var maxHeight = wrapPanel.CalculateFullyVisibleRowAreaHeight( finalSize );

					if ( this.VerticalAlignment == System.Windows.VerticalAlignment.Stretch )
					{
						// center the wrap panel
						arrangeRect.Y = (finalSize.Height - maxHeight) / 2;

						// clip ourselves to fit the number of rows completely
						var clipRect = arrangeRect;
						clipRect.Height = maxHeight;
						var geo = new RectangleGeometry( clipRect );
						geo.Freeze();

						this.Clip = geo;
					}
					else
					{
						// we don't need the clipping
						this.ClearValue( ClipProperty );
					}
				}

				this._child.Arrange(arrangeRect);

				// make sure that the child is within view
				if (null != this._childTransform && this._animationCount == 0)
				{
					double offset = this.VerticalOffset;

					if (offset < 0)
						offset = 0;
					else if (offset > this.MaxVerticalOffset)
						offset = this.MaxVerticalOffset;

					if (this.VerticalOffset != offset)
						this.AnimateVerticalOffset(offset);
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
			if (this._child == null || index != 0)
				throw new ArgumentOutOfRangeException();

			return this._child;
		}
			#endregion //GetVisualChild

			#region LogicalChildren
		/// <summary>
		/// Returns an enumerator for iterating the logical children of this element.
		/// </summary>
		protected override System.Collections.IEnumerator LogicalChildren
		{
			get
			{
				return this._child == null
				    ? (IEnumerator)EmptyEnumerator.Instance
				    : (IEnumerator)new SingleItemEnumerator(this._child);
			}
		} 
			#endregion //LogicalChildren

			#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="constraint">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size constraint)
		{
			Size desiredSize = new Size();

			if (this._child != null)
			{
				// we scroll vertically but restrict our height so measure the child
				// with an infinite height
				Size sizeToMeasure = new Size(constraint.Width, double.PositiveInfinity);

				this._child.Measure(sizeToMeasure);

				desiredSize = this._child.DesiredSize;

				// limit our height to what would fully fit the panel's items
				double restrictToHeight = double.NaN;

				if (this._child is GalleryWrapPanel)
					restrictToHeight = ((GalleryWrapPanel)this._child).CalculateFullyVisibleRowAreaHeight(constraint);

				if (double.IsNaN(restrictToHeight) == false)
				{
					desiredSize.Height = restrictToHeight;

					// AS 8/7/12 TFS117983
					// If the height of a row is taller than the height of an item we want the same effect we had when 
					// we were centered whereby the top and bottom of the item would be clipped so return the constrained 
					// size so we can offset the arrange rect.
					//
					if (this.VerticalAlignment == VerticalAlignment.Stretch && CoreUtilities.GreaterThan(desiredSize.Height, constraint.Height))
						desiredSize.Height = constraint.Height;
				}
				else if (constraint.Height < desiredSize.Height)
					desiredSize.Height = constraint.Height;
			}

			return desiredSize;
		} 
			#endregion //MeasureOverride

			#region VisualChildrenCount
		/// <summary>
		/// Returns the number of visual children for the element.
		/// </summary>
		protected override int VisualChildrenCount
		{
			get
			{
				return this._child != null ? 1 : 0;
			}
		}
			#endregion //VisualChildrenCount

		#endregion //Base class overrides

		#region Properties

			#region Private Properties

				#region Child

		/// <summary>
		/// Returns the element to be scrolled by the <see cref="GalleryPreviewScroller"/>
		/// </summary>
		public UIElement Child
		{
			get { return this._child; }
			set
			{
				if (value != this._child)
				{
					UIElement oldChild = this._child;

					// if we had an old child...
					if (null != this._child)
					{
						this.RemoveVisualChild(this._child);
						this.RemoveLogicalChild(this._child);
						this._child.RenderTransform = Transform.Identity;
						this._childTransform = null;

						// AS 8/7/12 TFS117983
						this.ClearValue( ClipProperty );
					}

					this._child = value;

					// if we have a new child...
					if (null != this._child)
					{
						this.AddVisualChild(this._child);
						this.AddLogicalChild(this._child);
						this._child.RenderTransform = this._childTransform = new TranslateTransform();
					}

					this.InvalidateMeasure();
				}
			}
		}

				#endregion //Child

				#region ContainingGalleryToolPreviewPresenter

		private GalleryToolPreviewPresenter ContainingGalleryToolPreviewPresenter
		{
			get
			{
				if (this._containingGalleryToolPreviewPresenter == null)
					this._containingGalleryToolPreviewPresenter = (GalleryToolPreviewPresenter)Utilities.GetAncestorFromType(this, typeof(GalleryToolPreviewPresenter),  true);

				return this._containingGalleryToolPreviewPresenter;
			}
		}

				#endregion //ContainingGalleryToolPreviewPresenter

				#region MaxVerticalOffset

		private double MaxVerticalOffset
		{
			get
			{
				// AS 8/7/12 TFS117983
				// We want to scroll only as much as needed but keep the control filled with items.
				//
				var wrapPanel = _child as GalleryWrapPanel;
				if ( wrapPanel != null && this.VerticalAlignment == VerticalAlignment.Stretch )
				{
					var maxToFit = wrapPanel.CalculateFullyVisibleRowAreaHeight(this.RenderSize);
					return Math.Max( this.Child.RenderSize.Height - maxToFit, 0 );
				}

				if (this.Child != null)
					return this.Child.RenderSize.Height - this.ScrollAmount;

				return 0;
			}
		}

				#endregion //MaxVerticalOffset

				#region ScrollAmount

		private double ScrollAmount
		{
			get
			{
				// AS 8/7/12 TFS117983
				// If we have a wrap panel then scroll 1 item at a time.
				//
				if ( _child is GalleryWrapPanel && this.VerticalAlignment == VerticalAlignment.Stretch )
					return ((GalleryWrapPanel)_child).ItemHeight;

				return this.ActualHeight;
			}
		}

				#endregion //ScrollAmount

				#region VerticalOffset

		private double VerticalOffset
		{
			get
			{
				return this._childTransform != null ? -this._verticalOffset : 0;
			}
		}

				#endregion //VerticalOffset

			#endregion //Private Properties

		#endregion //Properties

		#region Methods

			#region Private Methods

				#region AnimateVerticalOffset

		private void AnimateVerticalOffset(double newVerticalOffset)
		{
			if (this._childTransform != null)
			{
				Debug.WriteLine(string.Format("Animating Vertical Offset: Old={0}, New={1}", this._verticalOffset, newVerticalOffset));
				this._verticalOffset = -newVerticalOffset;
				this._animationCount++;

				TimeSpan timeSpan = TimeSpan.FromMilliseconds(SCROLL_ANIMATION_DURATION_MILLISECONDS);
				DoubleAnimation animation = new DoubleAnimation(-newVerticalOffset, new Duration(timeSpan));
				animation.Completed += new EventHandler(OnTransformAnimationCompleted);
				this._childTransform.BeginAnimation(TranslateTransform.YProperty, animation, HandoffBehavior.Compose);
			}
		}

				#endregion //AnimateVerticalOffset

                // JJD 11/29/07 - BR28786 - added
                #region CloseMenu

        private void CloseMenu()
        {
            GalleryToolPreviewPresenter galleryToolPreviewPresenter = this.ContainingGalleryToolPreviewPresenter;
            if (galleryToolPreviewPresenter != null)
            {
                MenuToolBase menu = galleryToolPreviewPresenter.MenuTool;

                if (menu != null && menu.IsOpen)
                    menu.IsOpen = false;
            }
        }

                #endregion //CloseMenu	
    
				#region OnCanExecuteCommand

		private static void OnCanExecuteCommand(object target, CanExecuteRoutedEventArgs args)
		{
			GalleryPreviewScroller galleryPreviewScroller = target as GalleryPreviewScroller;
			if (galleryPreviewScroller != null)
			{
				if (args.Command == GalleryPreviewScroller.DropdownGalleryCommand)
				{
					GalleryToolPreviewPresenter galleryToolPreviewPresenter = galleryPreviewScroller.ContainingGalleryToolPreviewPresenter;
					if (galleryToolPreviewPresenter != null && galleryToolPreviewPresenter.MenuTool != null)
						args.CanExecute = galleryToolPreviewPresenter.MenuTool.IsOpen == false;
					else
						args.CanExecute = false;
				}
				else if (args.Command == GalleryPreviewScroller.ScrollUpCommand)
					args.CanExecute = galleryPreviewScroller.VerticalOffset != 0;
				else if (args.Command == GalleryPreviewScroller.ScrollDownCommand)
					args.CanExecute = galleryPreviewScroller.VerticalOffset < galleryPreviewScroller.MaxVerticalOffset;
				else
					return;

				args.Handled = true;
			}
		}

				#endregion //OnCanExecuteCommand

				#region OnExecuteCommand

		private static void OnExecuteCommand(object target, ExecutedRoutedEventArgs args)
		{
			GalleryPreviewScroller galleryPreviewScroller = target as GalleryPreviewScroller;
			if (galleryPreviewScroller != null)
			{
				if (args.Command == GalleryPreviewScroller.DropdownGalleryCommand)
				{
					GalleryToolPreviewPresenter galleryToolPreviewPresenter = galleryPreviewScroller.ContainingGalleryToolPreviewPresenter;
					if (galleryToolPreviewPresenter != null && galleryToolPreviewPresenter.MenuTool != null)
					{
						args.Handled = true;
						galleryToolPreviewPresenter.MenuTool.IsOpen = true;
					}
				}
				else if (args.Command == GalleryPreviewScroller.ScrollUpCommand)
				{
                    // JJD 11/29/07 - BR28786
                    // Make sure the menu is closed before doing the scroll
                    galleryPreviewScroller.CloseMenu();

					galleryPreviewScroller.AnimateVerticalOffset(Math.Max(galleryPreviewScroller.VerticalOffset - galleryPreviewScroller.ScrollAmount, 0));
					args.Handled = true;
				}
				else if (args.Command == GalleryPreviewScroller.ScrollDownCommand)
				{
                    // JJD 11/29/07 - BR28786
                    // Make sure the menu is closed before doing the scroll
                    galleryPreviewScroller.CloseMenu();

					galleryPreviewScroller.AnimateVerticalOffset(Math.Min(galleryPreviewScroller.MaxVerticalOffset, galleryPreviewScroller.VerticalOffset + galleryPreviewScroller.ScrollAmount));
					args.Handled = true;
				}
			}
		}

				#endregion //OnExecuteCommand

				#region OnRequestBringIntoView
		private static void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
		{
			GalleryPreviewScroller scroller = sender as GalleryPreviewScroller;
			Visual child = e.TargetObject as Visual;
			if (((child != null) && (child != scroller)) && child.IsDescendantOf(scroller))
			{
				Rect rect = e.TargetRect;

				if (rect.IsEmpty && child is UIElement)
					rect = new Rect(((UIElement)child).RenderSize);

				rect = child.TransformToAncestor(scroller._child).TransformBounds(rect);
				double verticalOffset = scroller.VerticalOffset;
				double pageHeight = scroller.ActualHeight;

				if (rect.Y < verticalOffset)
					verticalOffset -= Math.Ceiling((verticalOffset - rect.Y) / pageHeight) * pageHeight;
				else if (rect.Bottom >= verticalOffset + pageHeight)
					verticalOffset += Math.Ceiling((rect.Bottom - (verticalOffset + pageHeight)) / pageHeight) * pageHeight;

				// if its still not in view then don't do page scrolling
				if (rect.Bottom >= verticalOffset + pageHeight)
					verticalOffset += rect.Bottom - (verticalOffset + pageHeight);

				if (rect.Y < verticalOffset)
					verticalOffset -= (verticalOffset - rect.Y);

				verticalOffset = Math.Max(Math.Min(scroller.MaxVerticalOffset, verticalOffset), 0);

				if (verticalOffset != scroller.VerticalOffset)
				{
					scroller.AnimateVerticalOffset(verticalOffset);
					e.Handled = true;

					rect.Y += verticalOffset;
					rect = scroller._child.TransformToAncestor(scroller).TransformBounds(rect);
					scroller.BringIntoView(rect);

				}
			}
		}
				#endregion //OnRequestBringIntoView

				#region OnTransformAnimationCompleted
		void OnTransformAnimationCompleted(object sender, EventArgs e)
		{
			this._animationCount--;

			// this is actually a different object than the animation that we hooked the 
			// event of but it seems that they share the same eventhandlerstore instance
			// alternatively we could probably get the timeline from the clock and unhook
			// from that
			((AnimationClock)sender).Completed -= new EventHandler(OnTransformAnimationCompleted);

			// invalidate the element so things like the focus rect are properly redrawn
			this.InvalidateVisual();

			// request a requery since the scroll up/down may have changed state
			CommandManager.InvalidateRequerySuggested();
		}
				#endregion //OnTransformAnimationCompleted

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