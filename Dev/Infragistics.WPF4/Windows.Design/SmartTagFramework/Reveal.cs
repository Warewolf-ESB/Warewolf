using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Diagnostics;

namespace Infragistics.Windows.Design.SmartTagFramework
{
    /// <summary>
    /// Modified arrangement in order the content could be stretched.
    /// </summary>
    public class Reveal : Decorator
	{
		#region Member Variables

		private int				_lastExpandedChangeWhileUnloadedTickCount;

		#endregion //Member Variables

		#region Constructors

		/// <summary>
        /// Initializes the <see cref="Reveal"/> class.
        /// </summary>
        static Reveal()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(Reveal), new FrameworkPropertyMetadata(true));
        }

        #endregion //Constructors

        #region Base Class Overrides

        /// <summary>
        /// Measures the child element of a <see cref="T:System.Windows.Controls.Decorator"/> to prepare for arranging it during the <see cref="M:System.Windows.Controls.Decorator.ArrangeOverride(System.Windows.Size)"/> pass.
        /// </summary>
        /// <param name="constraint">An upper limit <see cref="T:System.Windows.Size"/> that should not be exceeded.</param>
        /// <returns>
        /// The target <see cref="T:System.Windows.Size"/> of the element.
        /// </returns>
        protected override Size MeasureOverride(Size constraint)
        {
            UIElement child = Child;
            child.Measure(constraint);

            double percent	= this.GetPercentHelper();
            double width	= CalculateWidth(child.DesiredSize.Width, percent, HorizontalReveal);
            double height	= CalculateHeight(child.DesiredSize.Height, percent, VerticalReveal);
            return new Size(width, height);
        }

        /// <summary>
        /// Arranges the content of a <see cref="T:System.Windows.Controls.Decorator"/> element.
        /// </summary>
        /// <param name="arrangeSize">The <see cref="T:System.Windows.Size"/> this element uses to arrange its child content.</param>
        /// <returns>
        /// The <see cref="T:System.Windows.Size"/> that represents the arranged size of this <see cref="T:System.Windows.Controls.Decorator"/> element and its child.
        /// </returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            UIElement				child				= Child;
			double					percent				= this.GetPercentHelper();
            HorizontalRevealMode	horizontalReveal	= HorizontalReveal;
            VerticalRevealMode		verticalReveal		= VerticalReveal;

            double childWidth	= child.DesiredSize.Width;
            double childHeight	= child.DesiredSize.Height;

            if (this.UseDesiredSize)
            {
                double x = CalculateLeft(childWidth, percent, horizontalReveal);
                double y = CalculateTop(childHeight, percent, verticalReveal);
                child.Arrange(new Rect(x, y, childWidth, childHeight));
            }
            else
                child.Arrange(new Rect(0, 0, arrangeSize.Width, arrangeSize.Height));

            childWidth		= child.RenderSize.Width;
            childHeight		= child.RenderSize.Height;
            double width	= CalculateWidth(childWidth, percent, horizontalReveal);
            double height	= CalculateHeight(childHeight, percent, verticalReveal);

            return new Size(width, height);
        }

        #endregion //Base Class Overrides

        #region Public Properties

        /// <summary>
        /// Identifies the <see cref="UseDesiredSize"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty UseDesiredSizeProperty =
          DependencyProperty.Register("UseDesiredSize", typeof(bool), typeof(Reveal), new UIPropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value indicating whether arranged controls should use desired or decorator suggested size. This is a dependency property.
        /// </summary>
        /// <value><c>true</c> if the desired size should be used; otherwise, <c>false</c>.</value>
        public bool UseDesiredSize
        {
            get { return (bool)GetValue(UseDesiredSizeProperty); }
            set { SetValue(UseDesiredSizeProperty, value); }
        }

        /// <summary>
        ///     Whether the child is expanded or not.
        ///     Note that an animation may be in progress when the value changes.
        ///     This is not meant to be used with AnimationProgress and can overwrite any 
        ///     animation or values in that property.
        ///     This is a dependency property.
        /// </summary>
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="IsExpanded"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(Reveal), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsExpandedChanged)));

        private static void OnIsExpandedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
			Reveal reveal = sender as Reveal;
			if (reveal.IsLoaded == false)
				reveal._lastExpandedChangeWhileUnloadedTickCount = Environment.TickCount;
			else
				reveal._lastExpandedChangeWhileUnloadedTickCount = -1;
		}

        /// <summary>
        ///     The duration in milliseconds of the reveal animation.
        ///     Will apply to the next animation that occurs (not to currently running animations).
        ///     This is a dependency property.
        /// </summary>
        public double Duration
        {
            get { return (double)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Duration"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(double), typeof(Reveal), new UIPropertyMetadata(250.0));

        /// <summary>
        /// Gets or sets the horizontal reveal mode.
        /// </summary>
        /// <value>The horizontal reveal mode.</value>
        public HorizontalRevealMode HorizontalReveal
        {
            get { return (HorizontalRevealMode)GetValue(HorizontalRevealProperty); }
            set { SetValue(HorizontalRevealProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="HorizontalReveal"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalRevealProperty =
            DependencyProperty.Register("HorizontalReveal", typeof(HorizontalRevealMode), typeof(Reveal), new UIPropertyMetadata(HorizontalRevealMode.None));

        /// <summary>
        /// Gets or sets the vertical reveal mode.
        /// </summary>
        /// <value>The vertical reveal mode.</value>
        public VerticalRevealMode VerticalReveal
        {
            get { return (VerticalRevealMode)GetValue(VerticalRevealProperty); }
            set { SetValue(VerticalRevealProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="VerticalReveal"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalRevealProperty =
            DependencyProperty.Register("VerticalReveal", typeof(VerticalRevealMode), typeof(Reveal), new UIPropertyMetadata(VerticalRevealMode.FromTopToBottom));

        /// <summary>
        ///     Value between 0 and 1 (inclusive) to move the reveal along.
        ///     This is not meant to be used with IsExpanded.
        ///     This is a dependency property.
        /// </summary>
        public double AnimationProgress
        {
            get { return (double)GetValue(AnimationProgressProperty); }
            set { SetValue(AnimationProgressProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="AnimationProgress"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AnimationProgressProperty =
            DependencyProperty.Register("AnimationProgress", typeof(double), typeof(Reveal), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure, null, new CoerceValueCallback(OnCoerceAnimationProgress)));

        private static object OnCoerceAnimationProgress(DependencyObject d, object baseValue)
        {
            double num = (double)baseValue;
            if (num < 0.0)
                return 0.0;
            else if (num > 1.0)
                return 1.0;

            return baseValue;
        }

        #endregion //Public Properties
        
        #region Methods

        #region Private Methods

        private static double CalculateLeft(double width, double percent, HorizontalRevealMode reveal)
        {
            if (reveal == HorizontalRevealMode.FromRightToLeft)
                return (percent - 1.0) * width;
            else if (reveal == HorizontalRevealMode.FromCenterToEdge)
                return (percent - 1.0) * width * 0.5;
            else
                return 0.0;
        }

        private static double CalculateTop(double height, double percent, VerticalRevealMode reveal)
        {
            if (reveal == VerticalRevealMode.FromBottomToTop)
                return (percent - 1.0) * height;
            else if (reveal == VerticalRevealMode.FromCenterToEdge)
                return (percent - 1.0) * height * 0.5;
            else
                return 0.0;
        }

        private static double CalculateWidth(double originalWidth, double percent, HorizontalRevealMode reveal)
        {
            if (reveal == HorizontalRevealMode.None)
                return originalWidth;
            else
                return originalWidth * percent;
        }

        private static double CalculateHeight(double originalHeight, double percent, VerticalRevealMode reveal)
        {
            if (reveal == VerticalRevealMode.None)
                return originalHeight;
            else
                return originalHeight * percent;
        }

		private double GetPercentHelper()
		{
			// We don't want to animate expansion/collapsion if we are unloaded or the elapsed time since the last
			// expansion change that occurred when we were unloaded is less than the duration of the animation (using
			// 500 milliseconds).  Return 0.0 or 1.0 as the percent to in effect prevent animation.
			if (this.IsLoaded)
			{
				if (this._lastExpandedChangeWhileUnloadedTickCount == -1 ||
					(Environment.TickCount - this._lastExpandedChangeWhileUnloadedTickCount > 500))
					return this.AnimationProgress;
			}

			if (this.IsExpanded)
				return 1.0;
			else
				return 0.0;
		}

        #endregion //Private Methods

        #endregion //Methods
    }

    #region HorizontalRevealMode

    /// <summary>
    /// The horizontal reveal mode.
    /// </summary>
    public enum HorizontalRevealMode
    {
        /// <summary>
        ///     No horizontal reveal animation.
        /// </summary>
        None,

        /// <summary>
        ///     Reveal from the left to the right.
        /// </summary>
        FromLeftToRight,

        /// <summary>
        ///     Reveal from the right to the left.
        /// </summary>
        FromRightToLeft,

        /// <summary>
        ///     Reveal from the center to the bounding edge.
        /// </summary>
        FromCenterToEdge,
    }

    #endregion //HorizontalRevealMode

    #region VerticalRevealMode

    /// <summary>
    /// The vertical reveal mode.
    /// </summary>
    public enum VerticalRevealMode
    {
        /// <summary>
        ///     No vertical reveal animation.
        /// </summary>
        None,

        /// <summary>
        ///     Reveal from top to bottom.
        /// </summary>
        FromTopToBottom,

        /// <summary>
        ///     Reveal from bottom to top.
        /// </summary>
        FromBottomToTop,

        /// <summary>
        ///     Reveal from the center to the bounding edge.
        /// </summary>
        FromCenterToEdge,
    }

    #endregion //VerticalRevealMode
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