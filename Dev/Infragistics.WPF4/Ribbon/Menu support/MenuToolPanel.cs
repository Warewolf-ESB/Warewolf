using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Ribbon;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using Infragistics.Windows.Controls;
using System.Windows.Threading;
using Infragistics.Shared;
using System.ComponentModel;

namespace Infragistics.Windows.Ribbon.Internal
{
	/// <summary>
	/// A Panel derived element used to layout tools in a menu.  
	/// </summary>
	/// <remarks>
	/// <p class="note"><b>Note: </b>This is for internal use only by the <see cref="XamRibbon"/>.</p>
	/// </remarks>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class MenuToolPanel : StackPanel
	{
		#region Member Variables

		private ToolMenuItem _parentMenuItem;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="MenuToolPanel"/>
		/// </summary>
		public MenuToolPanel()
		{
		}

		static MenuToolPanel()
		{
			StackPanel.OrientationProperty.OverrideMetadata(typeof(MenuToolPanel), new FrameworkPropertyMetadata(KnownBoxes.OrientationVerticalBox, null, new CoerceValueCallback(CoerceOrientation)));
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

			UIElementCollection elements = this.InternalChildren;

			ToolMenuItem galleryToolMenuItem = null;

			int galleryToolIndex = -1;
			int count = elements.Count;
			Rect galleryToolRect = rect;

			#region Arrange Pre-GalleryTool items

			for (int i = 0; i < count; i++)
			{
				UIElement child = elements[i];

				if (child == null || child.Visibility == Visibility.Collapsed)
					continue;

				ToolMenuItem tmi = child as ToolMenuItem;

				if (tmi != null)
				{
					// check if the tool is a gallery tool and delay the measure of
					// these tools until after because their height may be limited by the 
					// overall space avaliable
					GalleryTool gTool = tmi.RibbonTool as GalleryTool;

					if (gTool != null)
					{
						galleryToolMenuItem = tmi;
						galleryToolIndex = i;
						galleryToolRect = rect;
						break;
					}
				}

				rect.Height = child.DesiredSize.Height;

				child.Arrange(rect);

				rect.Y += rect.Height;
			}

			#endregion //Arrange pre-GalleryTool items	

			if (galleryToolIndex >= 0)
			{
				#region Arrange Post-GalleryTool items

				rect = new Rect(finalSize);

				rect.Y = finalSize.Height;

				//loop over the items trailing the gallery tool backwards 
				for (int i = count - 1; i > galleryToolIndex; i--)
				{
					UIElement child = elements[i];

					if (child == null || child.Visibility == Visibility.Collapsed)
						continue;

					rect.Height = child.DesiredSize.Height;
					rect.Y -= rect.Height;

					child.Arrange(rect);
				}

				#endregion //Arrange pre-GalleryTool items	

				#region Arrange GalleryTool

				if (rect.Y > galleryToolRect.Y)
				{
					galleryToolRect.Height = rect.Y - galleryToolRect.Y;
					
					galleryToolMenuItem.Arrange(galleryToolRect);
				}

				#endregion //Arrange GalleryTool	
			}
    
			return finalSize;
		}

			#endregion //ArrangeOverride

			#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			Debug.WriteLine("************ MenuToolPanel.MeasureOverride ***************** size:" + availableSize.ToString());
			#region Setup

			UIElementCollection elements = this.InternalChildren;
			ToolMenuItem galleryToolMenuItem = null;
			GalleryTool galleryTool = null;

			double largestWidth = 0;
			double heightUsed = 0;

			Size constrainedWidthSize = new Size(availableSize.Width, double.PositiveInfinity);

			#endregion //Setup

			#region Measure the non-gallery tools

			// first measure all tools but gallery tools
			for (int i = 0, count = elements.Count; i < count; i++)
			{
				UIElement child = elements[i];

				if (child == null || child.Visibility == Visibility.Collapsed)
					continue;

				ToolMenuItem tmi = child as ToolMenuItem;

				if (tmi != null)
				{
					// check if the tool is a gallery tool and delay the measure of
					// these tools until after because their height may be limited by the 
					// overall space avaliable
					GalleryTool gtool = tmi.RibbonTool as GalleryTool;

					if (gtool != null)
					{
						galleryTool				= gtool;
						galleryToolMenuItem		= tmi;
						continue;
					}
				}

				// measure the child
				child.Measure(constrainedWidthSize);

				// get its desired size
				Size childSize = child.DesiredSize;

				// keep track of the widest width
				largestWidth = Math.Max( childSize.Width, largestWidth );

				// calculate the total height used so far 
				heightUsed += childSize.Height;
			}

			#endregion //Measure the non-gallery tools

			#region Measure the gallery tool

			if (galleryToolMenuItem != null)
			{
				Popup popup = Utilities.GetAncestorFromType(this, typeof(Popup), true) as Popup;

				double availableHeight = availableSize.Height;

				// If the available height is ininfite the limit it to what can fir on the screen
				if (double.IsPositiveInfinity(availableHeight))
				{
					availableHeight = SafeSystemInformation.VirtualScreenHeight;

					// allow for some chrome up the visual tree chain
					availableHeight -= 20;

					if (popup != null && popup.Placement == PlacementMode.RelativePoint)
					{
						FrameworkElement target = popup.PlacementTarget as FrameworkElement;

						if (target != null)
						{
                            // JJD 11/06/07 - Call PointToScreenSafe so we don't get an exception throw
                            // in XBAP semi-trust applications
                            //double spaceAbove = target.PointToScreen(new Point()).Y;
							double spaceAbove = Utilities.PointToScreenSafe( target, new Point()).Y;
							double spaceBelow = availableHeight - (spaceAbove + popup.VerticalOffset);
							availableHeight = Math.Max(spaceAbove - 20, spaceBelow);

							Debug.WriteLine("constrained height:" + availableHeight.ToString());
						}
					}
				}

				// subtract what the other items used so far
				availableHeight = Math.Max(availableHeight - heightUsed, 1);
				
				Debug.WriteLine("height for gallery:" + availableHeight.ToString());
				
				Size childSize;

				bool isInfiniteWidth = double.IsPositiveInfinity(availableSize.Width );

				if ( isInfiniteWidth )
				{
					Size tempSize = new Size(availableSize.Width, availableHeight);
					if ( galleryToolMenuItem != null )
					{
						galleryToolMenuItem.Measure(tempSize);

						// get its desired size
						childSize = galleryToolMenuItem.DesiredSize;

						// keep track of the widest width
						largestWidth = Math.Max( childSize.Width, largestWidth );

					}

					availableSize.Width = largestWidth;

					// constrain the width if we have a popup
					if (popup != null)
					{
						availableSize.Width = Math.Max(availableSize.Width, popup.MinWidth);
					}
				}

				bool constrainPopupToMinimumSize = false;

				double minHeight = 10;

				if (galleryTool != null)
				{
					PopupResizerDecorator.Constraints constraints = PopupResizerDecorator.GetResizeConstraints(galleryTool);
					if (constraints != null && !double.IsNaN(constraints.MinimumHeight))
						minHeight = Math.Max( minHeight, constraints.MinimumHeight);

					if (this._parentMenuItem == null)
						this._parentMenuItem = galleryToolMenuItem.ParentMenuItem;

					if (this._parentMenuItem != null)
						constrainPopupToMinimumSize = this._parentMenuItem.IsPopupConstrainedVertically;
				}

				double heightForTool;

				if (constrainPopupToMinimumSize)
					heightForTool = minHeight;
				else
					heightForTool = Math.Max( minHeight, availableHeight);

				// measure the gallery tool
				galleryToolMenuItem.Measure(new Size (availableSize.Width, heightForTool));

				// get its desired size
				childSize = galleryToolMenuItem.DesiredSize;

				// keep track of the widest width
				largestWidth = Math.Max( childSize.Width, largestWidth );

				// calculate the total height used so far 
				heightUsed += childSize.Height;

			}

			#endregion //Measure the gallery tool	

			// if the heightUsed exceeds the available height then
			// set the IsPopupConstrainedVertically flag to true so we
			// turn on scrolling
			if (!double.IsPositiveInfinity(availableSize.Height) &&
				this._parentMenuItem != null &&
				this._parentMenuItem.IsPopupConstrainedVertically == false)
			{
				if (heightUsed > availableSize.Height)
					this.Dispatcher.BeginInvoke(DispatcherPriority.Render, new XamRibbon.MethodInvoker(this.SetIsPopupConstrained));
			}
 
			// constrain the largest width by the passed in available size
			if (!double.IsPositiveInfinity(availableSize.Width))
				largestWidth = Math.Min(largestWidth, availableSize.Width);
   
			return new Size(largestWidth, heightUsed);
		}

			#endregion //MeasureOverride

		#region OnVisualChildrenChanged
		/// <summary>
		/// Invoked when a child is added/removed.
		/// </summary>
		/// <param name="visualAdded">The child that was added</param>
		/// <param name="visualRemoved">The child that was removed</param>
		protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
		{
			base.OnVisualChildrenChanged(visualAdded, visualRemoved);

			// AS 2/24/12 TFS102461
			// for some reason the measure is not being invalidated when children are added/removed. we only need 
			// to do this when the measure is valid. if we do it when it's in our own measure then that seems to 
			// introduce some issues
			if (this.IsMeasureValid)
				((UIElement)System.Windows.Media.VisualTreeHelper.GetParent(this)).InvalidateMeasure();
		}
		#endregion //OnVisualChildrenChanged

		#endregion //Base class overrides

		#region Methods

		#region Private Methods

		#region CoerceOrientation

		private static object CoerceOrientation(DependencyObject target, object value)
		{
			if (value is Orientation &&
				(Orientation)value != Orientation.Vertical)
				throw new NotSupportedException(XamRibbon.GetString("LE_InvalidMenuToolPanelOrientation"));

			return Orientation.Vertical;
		}

				#endregion //CoerceOrientation

				#region SetIsPopupConstrained

		private void SetIsPopupConstrained()
		{
			if (this._parentMenuItem != null)
				this._parentMenuItem.IsPopupConstrainedVertically = true;
		}

				#endregion //SetIsPopupConstrained	
    
			#endregion //Private Methods

		#endregion //Methods	
            
		#region SafeSystemInformation static class

		internal static class SafeSystemInformation
		{
			#region Private Members

			private static bool hasSecurityExceptionBeenThrown = false;

			#endregion //Private Members

			#region ConvertToLogicalPixels

			private static double ConvertToLogicalPixels(int pixelValue)
			{
				// AS 3/24/10 TFS27164
				//int pixelScreenWidth = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width;
				//
				//double logicalPixelScreenWidth = SystemParameters.PrimaryScreenWidth;
				//
				//return ((double)pixelValue * logicalPixelScreenWidth) / (double)pixelScreenWidth;
				return Utilities.ConvertToLogicalPixels(pixelValue);
			}

			#endregion //ConvertToLogicalPixels

			#region VirtualScreenHeight

			internal static double VirtualScreenHeight
			{
				get
				{
					if (hasSecurityExceptionBeenThrown)
						return VirtualScreenHeightFallback;

					try
					{
						return SystemParameters.VirtualScreenHeight;
					}
					catch (SecurityException)
					{
						hasSecurityExceptionBeenThrown = true;
						return VirtualScreenHeightFallback;
					}
				}
			}

			private static double VirtualScreenHeightFallback
			{
				[MethodImpl(MethodImplOptions.NoInlining)]
				get
				{
					return ConvertToLogicalPixels(System.Windows.Forms.SystemInformation.VirtualScreen.Height);
				}
			}

			#endregion //VirtualScreenHeight

			#region VirtualScreenWidth

			internal static double VirtualScreenWidth
			{
				get
				{
					if (hasSecurityExceptionBeenThrown)
						return VirtualScreenWidthFallback;

					try
					{
						return SystemParameters.VirtualScreenWidth;
					}
					catch (SecurityException)
					{
						hasSecurityExceptionBeenThrown = true;
						return VirtualScreenWidthFallback;
					}
				}
			}

			private static double VirtualScreenWidthFallback
			{
				[MethodImpl(MethodImplOptions.NoInlining)]
				get
				{
					return ConvertToLogicalPixels(System.Windows.Forms.SystemInformation.VirtualScreen.Width);
				}
			}

			#endregion //VirtualScreenWidth
		}

		#endregion //SafeSystemInformation static class
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