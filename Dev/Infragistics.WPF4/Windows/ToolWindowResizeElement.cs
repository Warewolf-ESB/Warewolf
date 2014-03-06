using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Infragistics.Windows.Helpers;
using System.Diagnostics;
using System.Windows.Media;

namespace Infragistics.Windows.Controls
{
	

	// AS 3/19/08 NA 2008 Vol 1 - XamDockManager
	/// <summary>
	/// Custom element for defining a resize element for the <see cref="ToolWindow"/>
	/// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class ToolWindowResizeElement : Border
	{
		#region Constructor
		static ToolWindowResizeElement()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolWindowResizeElement), new FrameworkPropertyMetadata(typeof(ToolWindowResizeElement)));

			// default to transparent so we get hittest notifications
			Border.BorderBrushProperty.OverrideMetadata(typeof(ToolWindowResizeElement), new FrameworkPropertyMetadata(Brushes.Transparent));
		}

		/// <summary>
		/// Initializes a new <see cref="ToolWindowResizeElement"/>
		/// </summary>
		public ToolWindowResizeElement()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region OnInitialized
		/// <summary>
		/// Invoked when the control is initialized.
		/// </summary>
		/// <param name="e">Provides data for the event</param>
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			ToolWindow windowContent = (this.TemplatedParent ?? Utilities.GetAncestorFromType(this, typeof(ToolWindow), true)) as ToolWindow;

			if (null != windowContent)
			{
				this.SetBinding(WindowBorderThicknessProperty, Utilities.CreateBindingObject(Control.BorderThicknessProperty, BindingMode.OneWay, windowContent));
				this.SetBinding(WindowCornerRadiusProperty, Utilities.CreateBindingObject(Border.CornerRadiusProperty, BindingMode.OneWay, windowContent));
			}
			else
			{
				BindingOperations.ClearBinding(this, WindowBorderThicknessProperty);
				BindingOperations.ClearBinding(this, WindowCornerRadiusProperty);
			}

			this.InitializeAlignments();
			this.InitializeBorderThickness();
			this.InitializeCornerRadius();
			this.InitializeCursor();
			this.InitializeMargins();
		}
		#endregion //OnInitialized

		#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			Size baseSize = base.MeasureOverride(availableSize);

			// AS 8/22/11 TFS84720
			// I'm not sure why I was doubling the Border/Radius but that is wrong. Removed the "2 *" from below.
			//
			switch (this.BorderLocation)
			{
				case ToolWindowResizeElementLocation.BottomLeft:
					// make sure we're at least as wide as the radius/borderthickness
					baseSize.Width = Math.Max(baseSize.Width, Math.Max(this.BorderThickness.Left, this.CornerRadius.BottomLeft));
					baseSize.Height = Math.Max(baseSize.Height, Math.Max(this.BorderThickness.Bottom, this.CornerRadius.BottomLeft));
					break;
				case ToolWindowResizeElementLocation.BottomRight:
					// make sure we're at least as wide as the radius/borderthickness
					baseSize.Width = Math.Max(baseSize.Width, Math.Max(this.BorderThickness.Right, this.CornerRadius.BottomRight));
					baseSize.Height = Math.Max(baseSize.Height, Math.Max(this.BorderThickness.Bottom, this.CornerRadius.BottomRight));
					break;
				case ToolWindowResizeElementLocation.TopLeft:
					// make sure we're at least as wide as the radius/borderthickness
					baseSize.Width = Math.Max(baseSize.Width, Math.Max(this.BorderThickness.Left, this.CornerRadius.TopLeft));
					baseSize.Height = Math.Max(baseSize.Height, Math.Max(this.BorderThickness.Top, this.CornerRadius.TopLeft));
					break;
				case ToolWindowResizeElementLocation.TopRight:
					// make sure we're at least as wide as the radius/borderthickness
					baseSize.Width = Math.Max(baseSize.Width, Math.Max(this.BorderThickness.Right, this.CornerRadius.TopRight));
					baseSize.Height = Math.Max(baseSize.Height, Math.Max(this.BorderThickness.Top, this.CornerRadius.TopRight));
					break;
			}

			return baseSize;
		}
		#endregion //MeasureOverride

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region BorderLocation

		/// <summary>
		/// Identifies the <see cref="BorderLocation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty BorderLocationProperty = DependencyProperty.Register("BorderLocation",
			typeof(ToolWindowResizeElementLocation), typeof(ToolWindowResizeElement), new FrameworkPropertyMetadata(ToolWindowResizeElementLocation.Top, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnBorderLocationChanged)));

		private static void OnBorderLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ToolWindowResizeElement border = (ToolWindowResizeElement)d;

			border.InitializeBorderThickness();
			border.InitializeCornerRadius();
			border.InitializeAlignments();
			border.InitializeMargins();
			border.InitializeCursor();
		}

		/// <summary>
		/// Returns or sets the location for the resize border.
		/// </summary>
		/// <seealso cref="BorderLocationProperty"/>
		//[Description("Returns or sets the location for the resize border.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public ToolWindowResizeElementLocation BorderLocation
		{
			get
			{
				return (ToolWindowResizeElementLocation)this.GetValue(ToolWindowResizeElement.BorderLocationProperty);
			}
			set
			{
				this.SetValue(ToolWindowResizeElement.BorderLocationProperty, value);
			}
		}

		#endregion //BorderLocation

		#endregion //Public Properties

		#region Private Properties

		#region WindowBorderThickness

		/// <summary>
		/// Identifies the <see cref="WindowBorderThickness"/> dependency property
		/// </summary>
		private static readonly DependencyProperty WindowBorderThicknessProperty = DependencyProperty.Register("WindowBorderThickness",
			typeof(Thickness), typeof(ToolWindowResizeElement), new FrameworkPropertyMetadata(new Thickness(), new PropertyChangedCallback(OnWindowBorderThicknessChanged)));

		private static void OnWindowBorderThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ToolWindowResizeElement border = ((ToolWindowResizeElement)d);

			border.InitializeBorderThickness();
			border.InitializeMargins();
		}

		private Thickness WindowBorderThickness
		{
			get
			{
				return (Thickness)this.GetValue(ToolWindowResizeElement.WindowBorderThicknessProperty);
			}
		}

		#endregion //WindowBorderThickness

		#region WindowCornerRadius

		/// <summary>
		/// Identifies the <see cref="WindowCornerRadius"/> dependency property
		/// </summary>
		private static readonly DependencyProperty WindowCornerRadiusProperty = DependencyProperty.Register("WindowCornerRadius",
			typeof(CornerRadius), typeof(ToolWindowResizeElement), new FrameworkPropertyMetadata(new CornerRadius(), new PropertyChangedCallback(OnWindowCornerRadiusChanged)));

		private static void OnWindowCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ToolWindowResizeElement border = ((ToolWindowResizeElement)d);

			border.InitializeCornerRadius();
			border.InitializeMargins();
		}

		private CornerRadius WindowCornerRadius
		{
			get
			{
				return (CornerRadius)this.GetValue(ToolWindowResizeElement.WindowCornerRadiusProperty);
			}
		}

		#endregion //WindowBorderCornerRadius

		#endregion //Private Properties

		#endregion //Properties

		#region Methods

		#region Private Methods

		#region InitializeAlignments

		private void InitializeAlignments()
		{
			this.InitializeHorizontalAlignment();
			this.InitializeVerticalAlignment();
		}

		#endregion //InitializeAlignments

		#region InitializeCursor
		private void InitializeCursor()
		{
			Cursor cursor = null;

			switch (this.BorderLocation)
			{
				case ToolWindowResizeElementLocation.Left:
				case ToolWindowResizeElementLocation.Right:
					cursor = Cursors.SizeWE;
					break;
				case ToolWindowResizeElementLocation.Bottom:
				case ToolWindowResizeElementLocation.Top:
					cursor = Cursors.SizeNS;
					break;
				case ToolWindowResizeElementLocation.TopLeft:
				case ToolWindowResizeElementLocation.BottomRight:
					cursor = Cursors.SizeNWSE;
					break;
				case ToolWindowResizeElementLocation.BottomLeft:
				case ToolWindowResizeElementLocation.TopRight:
					cursor = Cursors.SizeNESW;
					break;
				default:
					Debug.Fail("Invalid border location!");
					break;
			}

			this.SetValue(CursorProperty, cursor);
			this.SetValue(ForceCursorProperty, KnownBoxes.TrueBox);
		}
		#endregion //InitializeCursor

		#region InitializeHorizontalAlignment

		private void InitializeHorizontalAlignment()
		{
			object alignment;

			switch (this.BorderLocation)
			{
				case ToolWindowResizeElementLocation.Left:
				case ToolWindowResizeElementLocation.TopLeft:
				case ToolWindowResizeElementLocation.BottomLeft:
					alignment = KnownBoxes.HorizontalAlignmentLeftBox;
					break;
				case ToolWindowResizeElementLocation.Right:
				case ToolWindowResizeElementLocation.TopRight:
				case ToolWindowResizeElementLocation.BottomRight:
					alignment = KnownBoxes.HorizontalAlignmentRightBox;
					break;
				default:
				case ToolWindowResizeElementLocation.Bottom:
				case ToolWindowResizeElementLocation.Top:
					alignment = KnownBoxes.HorizontalAlignmentStretchBox;
					break;
			}

			this.SetValue(HorizontalAlignmentProperty, alignment);
		}

		#endregion //InitializeHorizontalAlignment

		#region InitializeVerticalAlignment

		private void InitializeVerticalAlignment()
		{
			object alignment;

			switch (this.BorderLocation)
			{
				default:
				case ToolWindowResizeElementLocation.Left:
				case ToolWindowResizeElementLocation.Right:
					alignment = KnownBoxes.VerticalAlignmentStretchBox;
					break;

				case ToolWindowResizeElementLocation.TopLeft:
				case ToolWindowResizeElementLocation.TopRight:
				case ToolWindowResizeElementLocation.Top:
					alignment = KnownBoxes.VerticalAlignmentTopBox;
					break;

				case ToolWindowResizeElementLocation.BottomLeft:
				case ToolWindowResizeElementLocation.BottomRight:
				case ToolWindowResizeElementLocation.Bottom:
					alignment = KnownBoxes.VerticalAlignmentBottomBox;
					break;
			}

			this.SetValue(VerticalAlignmentProperty, alignment);
		}

		#endregion //InitializeVerticalAlignment

		#region InitializeMargins

		private void InitializeMargins()
		{
			Thickness margin;
			Thickness borderThickness = this.WindowBorderThickness;
			CornerRadius radius = this.WindowCornerRadius;

			// AS 8/22/11 TFS84720
			// I'm not sure why I was doubling the Border/Radius but that is wrong. Removed the "2 *" from below.
			//
			switch (this.BorderLocation)
			{
				default:
					margin = new Thickness();
					break;
				// AS 6/24/11 FloatingWindowCaptionSource
				// Not sure why I was doubling the border thickness on each side.
				//
				//case ToolWindowResizeElementLocation.Left:
				//    margin = new Thickness(0, 2 * Math.Max(borderThickness.Top, radius.TopLeft), 0, 2 * Math.Max(borderThickness.Bottom, radius.BottomLeft));
				//    break;
				//case ToolWindowResizeElementLocation.Right:
				//    margin = new Thickness(0, 2 * Math.Max(borderThickness.Top, radius.TopRight), 0, 2 * Math.Max(borderThickness.Bottom, radius.BottomRight));
				//    break;
				//case ToolWindowResizeElementLocation.Bottom:
				//    margin = new Thickness(2 * Math.Max(borderThickness.Left, radius.BottomLeft), 0, 2 * Math.Max(borderThickness.Right, radius.BottomRight), 0);
				//    break;
				//case ToolWindowResizeElementLocation.Top:
				//    margin = new Thickness(2 * Math.Max(borderThickness.Left, radius.TopLeft), 0, 2 * Math.Max(borderThickness.Right, radius.TopRight), 0);
				//    break;
				case ToolWindowResizeElementLocation.Left:
					margin = new Thickness(0, Math.Max(borderThickness.Top, radius.TopLeft), 0, Math.Max(borderThickness.Bottom, radius.BottomLeft));
					break;
				case ToolWindowResizeElementLocation.Right:
					margin = new Thickness(0, Math.Max(borderThickness.Top, radius.TopRight), 0, Math.Max(borderThickness.Bottom, radius.BottomRight));
					break;
				case ToolWindowResizeElementLocation.Bottom:
					margin = new Thickness(Math.Max(borderThickness.Left, radius.BottomLeft), 0, Math.Max(borderThickness.Right, radius.BottomRight), 0);
					break;
				case ToolWindowResizeElementLocation.Top:
					margin = new Thickness(Math.Max(borderThickness.Left, radius.TopLeft), 0, Math.Max(borderThickness.Right, radius.TopRight), 0);
					break;
			}

			this.Margin = margin;
		}

		#endregion //InitializeMargins

		#region InitializeBorderThickness
		private void InitializeBorderThickness()
		{
			Thickness thickness = this.WindowBorderThickness;

			switch (this.BorderLocation)
			{
				default:
				case ToolWindowResizeElementLocation.Left:
					thickness = new Thickness(thickness.Left, 0, 0, 0);
					break;
				case ToolWindowResizeElementLocation.Right:
					thickness = new Thickness(0, 0, thickness.Right, 0);
					break;
				case ToolWindowResizeElementLocation.Bottom:
					thickness = new Thickness(0, 0, 0, thickness.Bottom);
					break;
				case ToolWindowResizeElementLocation.Top:
					thickness = new Thickness(0, thickness.Top, 0, 0);
					break;

				case ToolWindowResizeElementLocation.TopLeft:
					thickness = new Thickness(thickness.Left, thickness.Top, 0, 0);
					break;
				case ToolWindowResizeElementLocation.TopRight:
					thickness = new Thickness(0, thickness.Top, thickness.Right, 0);
					break;
				case ToolWindowResizeElementLocation.BottomLeft:
					thickness = new Thickness(thickness.Left, 0, 0, thickness.Bottom);
					break;
				case ToolWindowResizeElementLocation.BottomRight:
					thickness = new Thickness(0, 0, thickness.Right, thickness.Bottom);
					break;
			}

			this.BorderThickness = thickness;
		}
		#endregion //InitializeBorderThickness

		#region InitializeCornerRadius
		private void InitializeCornerRadius()
		{
			CornerRadius radius = this.WindowCornerRadius;

			switch (this.BorderLocation)
			{
				// no corner for the sides
				default:
				case ToolWindowResizeElementLocation.Left:
				case ToolWindowResizeElementLocation.Right:
				case ToolWindowResizeElementLocation.Bottom:
				case ToolWindowResizeElementLocation.Top:
					radius = new CornerRadius();
					break;
				case ToolWindowResizeElementLocation.TopLeft:
					radius = new CornerRadius(radius.TopLeft, 0, 0, 0);
					break;
				case ToolWindowResizeElementLocation.TopRight:
					radius = new CornerRadius(0, radius.TopRight, 0, 0);
					break;
				case ToolWindowResizeElementLocation.BottomLeft:
					radius = new CornerRadius(0, 0, 0, radius.BottomLeft);
					break;
				case ToolWindowResizeElementLocation.BottomRight:
					radius = new CornerRadius(0, 0, radius.BottomRight, 0);
					break;
			}

			this.CornerRadius = radius;
		}
		#endregion //InitializeCornerRadius

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