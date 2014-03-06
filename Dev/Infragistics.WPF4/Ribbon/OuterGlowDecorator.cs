using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Infragistics.Windows.Ribbon
{
	// AS 8/19/11 TFS83576
	// Since the OuterGlowBitmapEffect is deprecated in CLR4 and there was no equivalent provided, we have to provide 
	// a glow like effect ourselves.
	//
	/// <summary>
	/// Decorator used to provide an outer glow like effect around a given element.
	/// </summary>
	[DesignTimeVisible(false)]
	public class OuterGlowDecorator : Decorator
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="OuterGlowDecorator"/>
		/// </summary>
		public OuterGlowDecorator()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		protected override Size ArrangeOverride(Size arrangeSize)
		{
			this.InvalidateVisual();

			return base.ArrangeOverride(arrangeSize);
		}

		#region OnRender
		/// <summary>
		/// Invoked when the element should render.
		/// </summary>
		/// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
		protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
		{
			if (this.IsGlowVisible)
			{
				this.DrawGlow(drawingContext);
			}

			base.OnRender(drawingContext);
		}
		#endregion //OnRender

		#endregion //Base class overrides

		#region Properties

		#region GlowColor

		/// <summary>
		/// Identifies the <see cref="GlowColor"/> dependency property
		/// </summary>
		public static readonly DependencyProperty GlowColorProperty = DependencyProperty.Register("GlowColor",
			typeof(Color), typeof(OuterGlowDecorator), new FrameworkPropertyMetadata(Colors.White, new PropertyChangedCallback(OnGlowSettingsChanged)));

		/// <summary>
		/// Returns or sets the color of the glow.
		/// </summary>
		/// <seealso cref="GlowColorProperty"/>
		[Bindable(true)]
		public Color GlowColor
		{
			get
			{
				return (Color)this.GetValue(OuterGlowDecorator.GlowColorProperty);
			}
			set
			{
				this.SetValue(OuterGlowDecorator.GlowColorProperty, value);
			}
		}

		#endregion //GlowColor

		#region GlowOpacity

		/// <summary>
		/// Identifies the <see cref="GlowOpacity"/> dependency property
		/// </summary>
		public static readonly DependencyProperty GlowOpacityProperty = DependencyProperty.Register("GlowOpacity",
			typeof(double), typeof(OuterGlowDecorator), new FrameworkPropertyMetadata(0.6, new PropertyChangedCallback(OnGlowSettingsChanged)));

		/// <summary>
		/// Returns or sets the opacity used for the glow.
		/// </summary>
		/// <seealso cref="GlowOpacityProperty"/>
		[Description("Returns or sets the opacity used for the glow.")]
		[Category("Behavior")]
		[Bindable(true)]
		public double GlowOpacity
		{
			get
			{
				return (double)this.GetValue(OuterGlowDecorator.GlowOpacityProperty);
			}
			set
			{
				this.SetValue(OuterGlowDecorator.GlowOpacityProperty, value);
			}
		}

		#endregion //GlowOpacity

		#region IsGlowVisible

		/// <summary>
		/// Identifies the <see cref="IsGlowVisible"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsGlowVisibleProperty = DependencyProperty.Register("IsGlowVisible",
			typeof(bool), typeof(OuterGlowDecorator), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnGlowSettingsChanged)));

		/// <summary>
		/// Returns or sets a boolean indicating whether a glow is rendered around the child.
		/// </summary>
		/// <seealso cref="IsGlowVisibleProperty"/>
		[Bindable(true)]
		public bool IsGlowVisible
		{
			get
			{
				return (bool)this.GetValue(OuterGlowDecorator.IsGlowVisibleProperty);
			}
			set
			{
				this.SetValue(OuterGlowDecorator.IsGlowVisibleProperty, value);
			}
		}

		#endregion //IsGlowVisible

		#endregion //Properties

		#region Methods

		#region DrawGlow
		private void DrawGlow(DrawingContext drawingContext)
		{
			var child = this.Child;

			if (child == null)
				return;

			Rect childRect = VisualTreeHelper.GetDescendantBounds(child);

			if (childRect.IsEmpty)
				return;

			Size renderSize = childRect.Size;

			if (renderSize.Width < 1 || renderSize.Height < 1)
				return;

			// if the glow is provided by a bitmap effect then exit
			if (this.GetValue(BitmapEffectProperty) is OuterGlowBitmapEffect)
				return;

			Color glowColor = this.GlowColor;

			if (glowColor.A == 0)
				return;

			Rect renderRect = Rect.Inflate(childRect, 6, 6);
			renderSize = renderRect.Size;

			double opacity = this.GlowOpacity;
			opacity = Math.Max(opacity * .9, 0);

			var stops = new GradientStopCollection(2);
			stops.Add(new GradientStop { Offset = 0, Color = glowColor });
			stops.Add(new GradientStop { Offset = 1, Color = Colors.Transparent });

			LinearGradientBrush linearBrush = new LinearGradientBrush { Opacity = opacity, GradientStops = stops };
			RadialGradientBrush radialBrush = new RadialGradientBrush { Opacity = opacity, GradientStops = stops, RadiusX = 1, RadiusY = 1 };

			double edgeHeight = Math.Min(renderSize.Height / 2, 12);
			double edgeWidth = Math.Min(renderSize.Width / 2, 12);
			double middleWidth = Math.Max(renderRect.Width - (edgeWidth * 2), 0);
			double middleHeight = Math.Max(renderRect.Height - (edgeHeight * 2), 0);

			Rect cornerRect = new Rect(renderRect.X, renderRect.Y, edgeWidth, edgeHeight);

			GuidelineSet guidelines = new GuidelineSet();

			// horizontal guidelines
			guidelines.GuidelinesX.Add(renderRect.Left);
			guidelines.GuidelinesX.Add(renderRect.Left + edgeWidth);

			if (middleWidth > 0)
				guidelines.GuidelinesX.Add(renderRect.Right - edgeWidth);

			guidelines.GuidelinesX.Add(renderRect.Right);

			// vertical guidelines
			guidelines.GuidelinesY.Add(renderRect.Top);
			guidelines.GuidelinesY.Add(renderRect.Top + edgeHeight);

			if (middleHeight > 0)
				guidelines.GuidelinesY.Add(renderRect.Bottom - edgeHeight);

			guidelines.GuidelinesY.Add(renderRect.Bottom);
			drawingContext.PushGuidelineSet(guidelines);

			// upper left
			radialBrush.Center = radialBrush.GradientOrigin = new Point(1, 1);
			drawingContext.DrawRectangle(radialBrush, null, cornerRect);

			// upper middle
			if (middleWidth > 0)
			{
				linearBrush.StartPoint = new Point(0, 1);
				linearBrush.EndPoint = new Point(0, 0);
				drawingContext.DrawRectangle(linearBrush, null, new Rect(renderRect.X + edgeWidth, renderRect.Y, middleWidth, edgeHeight));
			}

			// upper right
			radialBrush = radialBrush.Clone();
			radialBrush.Center = radialBrush.GradientOrigin = new Point(0, 1);
			drawingContext.DrawRectangle(radialBrush, null, Rect.Offset(cornerRect, renderSize.Width - edgeWidth, 0));

			if (middleWidth > 0)
			{
				// middle left
				linearBrush = linearBrush.Clone();
				linearBrush.StartPoint = new Point(1, 0);
				linearBrush.EndPoint = new Point(0, 0);
				drawingContext.DrawRectangle(linearBrush, null, new Rect(renderRect.X, renderRect.Y + edgeHeight, edgeWidth, middleHeight));

				// middle
				if (middleHeight > 0)
				{
					drawingContext.DrawRectangle(new SolidColorBrush { Color = glowColor, Opacity = opacity }, null, new Rect(renderRect.X + edgeWidth, renderRect.Y + edgeHeight, middleWidth, middleHeight));
				}

				// middle right
				linearBrush = linearBrush.Clone();
				linearBrush.StartPoint = new Point(0, 0);
				linearBrush.EndPoint = new Point(1, 0);
				drawingContext.DrawRectangle(linearBrush, null, new Rect(renderRect.Right - edgeWidth, renderRect.Y + edgeHeight, edgeWidth, middleHeight));
			}


			// lower left
			radialBrush = radialBrush.Clone();
			radialBrush.Center = radialBrush.GradientOrigin = new Point(1, 0);
			drawingContext.DrawRectangle(radialBrush, null, Rect.Offset(cornerRect, 0, renderSize.Height - edgeHeight));

			// lower middle
			if (middleWidth > 0)
			{
				linearBrush = linearBrush.Clone();
				linearBrush.StartPoint = new Point(0, 0);
				linearBrush.EndPoint = new Point(0, 1);
				drawingContext.DrawRectangle(linearBrush, null, new Rect(renderRect.X + edgeWidth, renderRect.Bottom - edgeHeight, Math.Max(renderRect.Width - (edgeWidth * 2), 0), edgeHeight));
			}

			// lower right
			radialBrush = radialBrush.Clone();
			radialBrush.Center = radialBrush.GradientOrigin = new Point(0, 0);
			drawingContext.DrawRectangle(radialBrush, null, Rect.Offset(cornerRect, renderSize.Width - edgeWidth, renderSize.Height - edgeHeight));

			drawingContext.Pop();
		}
		#endregion //DrawGlow

		#region OnGlowSettingsChanged
		private static void OnGlowSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var instance = d as OuterGlowDecorator;
			instance.VerifyOuterGlowBitmapEffect();
		}
		#endregion //OnGlowSettingsChanged

		#region VerifyOuterGlowBitmapEffect
		private void VerifyOuterGlowBitmapEffect()
		{
			if (!this.IsGlowVisible)
			{
				this.ClearValue(BitmapEffectProperty);
			}
			else
			{
				if (System.Environment.Version.Major < 4 && !System.Windows.Interop.BrowserInteropHelper.IsBrowserHosted)
				{
					try
					{
						var effect = new OuterGlowBitmapEffect();
						effect.Opacity = this.GlowOpacity;
						effect.GlowColor = this.GlowColor;
						this.SetValue(BitmapEffectProperty, effect);
					}
					catch (System.Security.SecurityException)
					{
					}
				}
			}
		}
		#endregion //VerifyOuterGlowBitmapEffect

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