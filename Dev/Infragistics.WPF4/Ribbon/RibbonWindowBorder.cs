using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Infragistics.Shared;
using System.ComponentModel;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Represents the window border of a <see cref="XamRibbonWindow"/>
	/// </summary>
	/// <remarks>
    /// <para class="note"><b>Note:</b> The border can only be used inside the template of a <see cref="RibbonWindowContentHost"/>.</para>
	/// </remarks>
    /// <seealso cref="RibbonWindowContentHost"/>
	/// <seealso cref="XamRibbonWindow"/>
	/// <seealso cref="XamRibbon"/>
    /// <exception cref="NotSupportedException">If this element is not used inside the template of a RibbonWindowContentHost.</exception>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class RibbonWindowBorder : Border
	{
		#region Private Members

        // JJD 11/30/07 - BR27696
        // Added RibbonWindowContentHost
        private RibbonWindowContentHost _contentHost;
		// AS 6/3/08 BR32772
		//private XamRibbonWindow _window;
		private IRibbonWindow _window;

        
        
        

        // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
        private bool? _isScenicTheme = null;

        private const int ScenicRoundedCornerRadius = 7;

		#endregion //Private Members
		    
		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="RibbonWindowBorder"/>
		/// </summary>
		public RibbonWindowBorder()
		{
		}        

		#endregion //Constructor

		#region Base class overrides

			#region OnRender

		/// <summary>
		/// Called when the element needs to be rendered
		/// </summary>
        /// <param name="dc">The <see cref="System.Windows.Media.DrawingContext"/> that defines the object to be drawn.</param>
		protected override void OnRender(DrawingContext dc)
		{
            // JJD 05/17/10 - NA 2010 volume 2 - Scenic Ribbon support
            // Bail out if BorderZThickness is less than 1 all around
            // Bail out if the thickness is, for example, (0,0,0,0), or otherwise not the typical border that we're expecting.
            //if (this.BorderThickness != ExpectedThickness)
            //    return;

            Thickness borderThickness = this.BorderThickness;

            if (borderThickness.Top         < 1 &&
                borderThickness.Bottom      < 1 &&
                borderThickness.Left        < 1 &&
                borderThickness.Right       < 1)
                return;

			if ( this._contentHost == null )
			{
                // JJD 11/30/07 - BR27696
                // now the template is the content host
                //this._window = this.TemplatedParent as XamRibbonWindow;
                this._contentHost = this.TemplatedParent as RibbonWindowContentHost;

				if ( this._contentHost == null )
                    throw new NotSupportedException(XamRibbon.GetString("LE_InvalidRibbonWindowBorderParent"));

                // JJD 11/30/07 - BR27696
                // get the window (Note: this may return null)
				// AS 6/3/08 BR32772
				//this._window = Utilities.GetAncestorFromType(this._contentHost, typeof(XamRibbonWindow), true) as XamRibbonWindow;
                this._window = Utilities.GetAncestorFromType(this._contentHost, typeof(IRibbonWindow), true) as IRibbonWindow;

                // JJD 11/30/07 - BR27696
                // check for the case where we aren't in a XamRibbonWindow (e.g. in the designer)
                if (this._window != null)
                {
                    this._window.Activated += new EventHandler(OnWindowActivationChanged);
                    this._window.Deactivated += new EventHandler(OnWindowActivationChanged);


                }

                
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)

			}

            // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
            bool isScenicTheme = false;

            if (this._window != null)
                isScenicTheme = this._window.IsScenicThemeInternal;

            // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
            // Initialize the brush properties based on whether a scenic theme is being used
            if (this._isScenicTheme.HasValue == false ||
                 this._isScenicTheme.Value != isScenicTheme)
            {
                this._isScenicTheme = isScenicTheme;

                if (isScenicTheme)
                {
				    this.SetResourceReference(Inner1BrushProperty, RibbonBrushKeys.ScenicWindowBorderInner1BrushKey);
				    this.SetResourceReference(Inner1ShadowBrushProperty, RibbonBrushKeys.ScenicWindowBorderInner1ShadowBrushKey);
				    this.SetResourceReference(Inner2BrushProperty, RibbonBrushKeys.ScenicWindowBorderInner2BrushKey);
				    this.SetResourceReference(Inner3BrushProperty, RibbonBrushKeys.ScenicWindowBorderInner3BrushKey);
				    this.SetResourceReference(Inner1BrushActiveProperty, RibbonBrushKeys.ScenicActiveWindowBorderInner1BrushKey);
				    this.SetResourceReference(Inner1ShadowBrushActiveProperty, RibbonBrushKeys.ScenicActiveWindowBorderInner1ShadowBrushKey);
				    this.SetResourceReference(Inner2BrushActiveProperty, RibbonBrushKeys.ScenicActiveWindowBorderInner2BrushKey);
				    this.SetResourceReference(Inner3BrushActiveProperty, RibbonBrushKeys.ScenicActiveWindowBorderInner3BrushKey);
                }
                else
                {
				    // set the dynamic resource bindings for when the window is inactive
                    // JJD 12/10/07 - BR28958
                    // Moved outer border drawiing logic to XamRibbonWindow
                    //this.SetResourceReference(OuterBrushProperty, RibbonBrushKeys.WindowBorderOuterBrushKey);
				    this.SetResourceReference(Inner1BrushProperty, RibbonBrushKeys.WindowBorderInner1BrushKey);
				    this.SetResourceReference(Inner2BrushProperty, RibbonBrushKeys.WindowBorderInner2BrushKey);
				    this.SetResourceReference(Inner3BrushProperty, RibbonBrushKeys.WindowBorderInner3BrushKey);

				    // set the dynamic resource bindings for when the window is active
                    // JJD 12/10/07 - BR28958
                    // Moved outer border drawiing logic to XamRibbonWindow
				    //this.SetResourceReference(OuterBrushActiveProperty, RibbonBrushKeys.ActiveWindowBorderOuterBrushKey);
				    this.SetResourceReference(Inner1BrushActiveProperty, RibbonBrushKeys.ActiveWindowBorderInner1BrushKey);
				    this.SetResourceReference(Inner2BrushActiveProperty, RibbonBrushKeys.ActiveWindowBorderInner2BrushKey);
				    this.SetResourceReference(Inner3BrushActiveProperty, RibbonBrushKeys.ActiveWindowBorderInner3BrushKey);
                }

            }
 
            // JJD 12/10/07 - BR28958
            // Reworked logic to use brushes instead of pens which had a problem with anti-aliasing effects
            // Also moved drawing of outer 1 pixel border to XamRibbonWindow
            #region Old code

            //Brush background = this.Background;

            //Brush outerBrush;
            //Brush inner1Brush;
            //Brush inner2Brush;
            //Brush inner3Brush;

            //// JJD 11/30/07 - BR27696
            //// check for the case where we aren't in a XamRibbonWindow (e.g. in the designer)
            ////if (this._contentHost.IsActive)
            //if ( this._window != null && this._window.IsActive)
            //{
            //    outerBrush = this.OuterBrushActive;
            //    inner1Brush = this.Inner1BrushActive;
            //    inner2Brush = this.Inner2BrushActive;
            //    inner3Brush = this.Inner3BrushActive;
            //}
            //else
            //{
            //    outerBrush = this.OuterBrush;
            //    inner1Brush = this.Inner1Brush;
            //    inner2Brush = this.Inner2Brush;
            //    inner3Brush = this.Inner3Brush;
            //}

            //Pen outerPen		= new Pen(outerBrush, 1);
            //Pen inner1Pen		= new Pen(inner1Brush, 1);
            //Pen inner2Pen		= new Pen(inner2Brush, 1);
            //Pen inner3Pen		= new Pen(inner3Brush, 1);

            //// If the status bar is visible, we need to draw our rounded rectangle, but if it's hidden the bottom
            //// borders are not rounded.
            //Size size = this.RenderSize;
            //if (this._contentHost.StatusBarVisibility != Visibility.Visible)
            //{
            //    dc.DrawRectangle(null, outerPen, new Rect(size));
            //    dc.DrawRectangle(null, inner1Pen, new Rect(new Point(1, 2), new Point(size.Width - 1, size.Height - 2)));
            //    dc.DrawRectangle(null, inner2Pen, new Rect(new Point(2, 2), new Point(size.Width - 2, size.Height - 2)));
            //    dc.DrawRectangle(null, inner3Pen, new Rect(new Point(3, 3), new Point(size.Width - 3, size.Height - 3)));
            //}
            //else
            //{
            //    dc.DrawRoundedRectangle(null, outerPen, new Rect(size), XamRibbonWindow.WINDOW_CORNER_RADIUS, XamRibbonWindow.WINDOW_CORNER_RADIUS);

            //    // We don't want to draw the borders into the status bar because the rounded corners will cause portions of these borders to be visible
            //    double borderHeight = size.Height;
            //    if (this._contentHost.StatusBar != null)
            //        borderHeight -= this._contentHost.StatusBar.ActualHeight;

            //    // First inner borders
            //    dc.DrawLine(inner1Pen, new Point(1, 2), new Point(1, borderHeight));
            //    dc.DrawLine(inner1Pen, new Point(size.Width - 1, 2), new Point(size.Width - 1, borderHeight));

            //    // Second inner borders
            //    dc.DrawLine(inner2Pen, new Point(2, 2), new Point(2, borderHeight));
            //    dc.DrawLine(inner2Pen, new Point(size.Width - 2, 2), new Point(size.Width - 2, borderHeight));

            //    // Third inner borders
            //    dc.DrawLine(inner3Pen, new Point(3, 1), new Point(3, borderHeight));
            //    dc.DrawLine(inner3Pen, new Point(size.Width - 3, 1), new Point(size.Width - 3, borderHeight));
            //}

            #endregion //Old code	
    
            Brush inner1Brush;
            Brush inner1ShadowBrush;
            Brush inner2Brush;
            Brush inner3Brush;

            // JJD 11/30/07 - BR27696
            // check for the case where we aren't in a XamRibbonWindow (e.g. in the designer)
            //if (this._contentHost.IsActive)
            if (this._window != null && this._window.IsActive)
            {
                inner1Brush = this.Inner1BrushActive;
                inner1ShadowBrush = this.Inner1ShadowBrushActive;
                inner2Brush = this.Inner2BrushActive;
                inner3Brush = this.Inner3BrushActive;
            }
            else
            {
                inner1Brush = this.Inner1Brush;
                inner1ShadowBrush = this.Inner1ShadowBrush;
                inner2Brush = this.Inner2Brush;
                inner3Brush = this.Inner3Brush;
            }

            bool wasClientAreaClipped = false;

            Rect rect = new Rect(this.RenderSize);
            Rect originalRect = rect;

            // adjust for outer borders
            rect.Inflate(-1, -1);

            // clip out the client area (within the borders
            //if (rect.Width > 3 && rect.Height > 3)
            if (rect.Width >= borderThickness.Left && rect.Height >= borderThickness.Top)
            {
                Geometry clip = new RectangleGeometry(rect);

                Rect insideRect = new Rect(rect.Size);

                // JJD 05/17/10 - NA 2010 volume 2 - Scenic Ribbon support
                // Use actual border thickness to clip 
                //insideRect.Inflate(-4, -4);
                insideRect.X += borderThickness.Left;
                insideRect.Y += borderThickness.Top;
                insideRect.Width = Math.Max(insideRect.Width - (borderThickness.Left + borderThickness.Right), 1);
                insideRect.Height = Math.Max(insideRect.Height - (borderThickness.Top + borderThickness.Bottom), 1);


                clip = new CombinedGeometry( GeometryCombineMode.Exclude, clip, new RectangleGeometry(insideRect));

                // JJD 4/29/10 - Optimization
                // Freeze the clip geometry so the framework doesn't need to listen for changes
                clip.Freeze();

                dc.PushClip(clip);

                wasClientAreaClipped = true;
            }

            dc.DrawRectangle(inner1Brush, null, rect);

            // JJD 5/17/10 - NA 2010 Volumne 2 - Scenic Riboon
            // For scenic themes draw the inner shadow
            if (isScenicTheme && inner1ShadowBrush != null && inner1ShadowBrush != inner1Brush)
            {
                Geometry clip = new RectangleGeometry(rect);
                Geometry clipOffset = new RectangleGeometry(Rect.Offset(rect, -1.0, -1.0));

                clip = new CombinedGeometry(GeometryCombineMode.Exclude, clip, clipOffset);

                clip.Freeze();

                dc.PushClip(clip);

                dc.DrawRectangle(inner1ShadowBrush, null, rect);

                dc.Pop();
            }

            rect.Inflate(-1, -1);
            dc.DrawRectangle(inner2Brush, null, rect);

            rect.Inflate(-1, -1);
            dc.DrawRectangle(inner3Brush, null, rect);

            if (wasClientAreaClipped)
                dc.Pop();

            if (isScenicTheme &&
                this._window != null &&
                this._contentHost != null &&
                this._contentHost.IsClassicOSTheme == false)
            {
                #region Draw rounded left/top borders

                Brush outerBorderBrush = this._window.OuterBorderBrush;
                rect = originalRect;
                
                // bump the width and height so we don't draw any corner but the top/left
                rect.Width = rect.Width + 100;
                rect.Height = rect.Height + 100;

                Pen outerPen = new Pen(outerBorderBrush, 1);
                outerPen.Freeze();

                dc.DrawRoundedRectangle(null, outerPen, rect, ScenicRoundedCornerRadius, ScenicRoundedCornerRadius);

                rect.Inflate(-1, -1);

                Pen innerPen = new Pen(inner1Brush, 1);
                innerPen.Freeze();

                dc.DrawRoundedRectangle(null, innerPen, rect, ScenicRoundedCornerRadius, ScenicRoundedCornerRadius);

                #endregion //Draw rounded left/top borders	
                
                #region Draw rounded right/top border

                rect = originalRect;

                if (rect.Width > ScenicRoundedCornerRadius && rect.Height >= borderThickness.Top)
                {
                    // bump the height so we don't draw the bottom/right corner
                    rect.Height = rect.Height + 100;

                    Geometry clip = new RectangleGeometry(rect);
                    Geometry clipOffset = new RectangleGeometry(Rect.Offset(rect, -4, 0));

                    clip = new CombinedGeometry(GeometryCombineMode.Exclude, clip, clipOffset);

                    dc.PushClip(clip);
                    Brush outerShadowBrush = this._window.OuterShadowBrush;

                    outerPen = new Pen(outerShadowBrush, 1);
                    outerPen.Freeze();

                    dc.DrawRoundedRectangle(null, outerPen, rect, ScenicRoundedCornerRadius, ScenicRoundedCornerRadius);

                    rect.Inflate(-1, -1);

                    innerPen = new Pen(inner1ShadowBrush, 1);
                    innerPen.Freeze();

                    dc.DrawRoundedRectangle(null, innerPen, rect, ScenicRoundedCornerRadius, ScenicRoundedCornerRadius);

                    dc.Pop();
                }

                #endregion //Draw rounded right/top border	
    
            }
		}

			#endregion //OnRender

		#endregion //Base class overrides

		#region Properties

			#region Internal Properties

				#region Inner1Brush

		internal static readonly DependencyProperty Inner1BrushProperty = DependencyProperty.Register("Inner1Brush",
			typeof(Brush), typeof(RibbonWindowBorder), new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

		internal Brush Inner1Brush
		{
			get
			{
				return (Brush)this.GetValue(RibbonWindowBorder.Inner1BrushProperty);
			}
			set
			{
				this.SetValue(RibbonWindowBorder.Inner1BrushProperty, value);
			}
		}

				#endregion //Inner1Brush

				#region Inner1BrushActive

		internal static readonly DependencyProperty Inner1BrushActiveProperty = DependencyProperty.Register("Inner1BrushActive",
			typeof(Brush), typeof(RibbonWindowBorder), new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

		internal Brush Inner1BrushActive
		{
			get
			{
				return (Brush)this.GetValue(RibbonWindowBorder.Inner1BrushActiveProperty);
			}
			set
			{
				this.SetValue(RibbonWindowBorder.Inner1BrushActiveProperty, value);
			}
		}

				#endregion //Inner1BrushActive

                // JJD 5/17/10 - NA 2010 Volumne 2 - Scenic Riboon
				#region Inner1ShadowBrush

		internal static readonly DependencyProperty Inner1ShadowBrushProperty = DependencyProperty.Register("Inner1ShadowBrush",
			typeof(Brush), typeof(RibbonWindowBorder), new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

		internal Brush Inner1ShadowBrush
		{
			get
			{
				return (Brush)this.GetValue(RibbonWindowBorder.Inner1ShadowBrushProperty);
			}
			set
			{
				this.SetValue(RibbonWindowBorder.Inner1ShadowBrushProperty, value);
			}
		}

				#endregion //Inner1ShadowBrush

                // JJD 5/17/10 - NA 2010 Volumne 2 - Scenic Riboon
				#region Inner1ShadowBrushActive

		internal static readonly DependencyProperty Inner1ShadowBrushActiveProperty = DependencyProperty.Register("Inner1ShadowBrushActive",
			typeof(Brush), typeof(RibbonWindowBorder), new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

		internal Brush Inner1ShadowBrushActive
		{
			get
			{
				return (Brush)this.GetValue(RibbonWindowBorder.Inner1ShadowBrushActiveProperty);
			}
			set
			{
				this.SetValue(RibbonWindowBorder.Inner1ShadowBrushActiveProperty, value);
			}
		}

				#endregion //Inner1ShadowBrushActive

				#region Inner2Brush

		internal static readonly DependencyProperty Inner2BrushProperty = DependencyProperty.Register("Inner2Brush",
			typeof(Brush), typeof(RibbonWindowBorder), new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

		internal Brush Inner2Brush
		{
			get
			{
				return (Brush)this.GetValue(RibbonWindowBorder.Inner2BrushProperty);
			}
			set
			{
				this.SetValue(RibbonWindowBorder.Inner2BrushProperty, value);
			}
		}

				#endregion //Inner2Brush

				#region Inner2BrushActive

		internal static readonly DependencyProperty Inner2BrushActiveProperty = DependencyProperty.Register("Inner2BrushActive",
			typeof(Brush), typeof(RibbonWindowBorder), new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

		internal Brush Inner2BrushActive
		{
			get
			{
				return (Brush)this.GetValue(RibbonWindowBorder.Inner2BrushActiveProperty);
			}
			set
			{
				this.SetValue(RibbonWindowBorder.Inner2BrushActiveProperty, value);
			}
		}

				#endregion //Inner2BrushActive

				#region Inner3Brush

		internal static readonly DependencyProperty Inner3BrushProperty = DependencyProperty.Register("Inner3Brush",
			typeof(Brush), typeof(RibbonWindowBorder), new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

		internal Brush Inner3Brush
		{
			get
			{
				return (Brush)this.GetValue(RibbonWindowBorder.Inner3BrushProperty);
			}
			set
			{
				this.SetValue(RibbonWindowBorder.Inner3BrushProperty, value);
			}
		}

				#endregion //Inner3Brush

				#region Inner3BrushActive

		internal static readonly DependencyProperty Inner3BrushActiveProperty = DependencyProperty.Register("Inner3BrushActive",
			typeof(Brush), typeof(RibbonWindowBorder), new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

		internal Brush Inner3BrushActive
		{
			get
			{
				return (Brush)this.GetValue(RibbonWindowBorder.Inner3BrushActiveProperty);
			}
			set
			{
				this.SetValue(RibbonWindowBorder.Inner3BrushActiveProperty, value);
			}
		}

				#endregion //Inner3BrushActive

			#endregion //Internal Properties	
    
		#endregion //Properties

		#region Methods

			#region Private Methods

				#region OnWindowActivationChanged

		private void OnWindowActivationChanged(object sender, EventArgs e)
		{
			Window w = sender as Window;

			// AS 1/13/10 TFS25545
			// There is no need to invalidate the visual and force an arrange
			// when the window is minimized.
			//
			if (w.WindowState == WindowState.Minimized)
				return;

			this.InvalidateVisual();

            // JJD 12/10/07 - BR28958
            // Since the XamRibbonWindow is now drawing the outer border we 
            // need to invalidate its Visual
            if (this._window != null)
                this._window.InvalidateVisual();
		}

				#endregion //OnWindowActivationChanged

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