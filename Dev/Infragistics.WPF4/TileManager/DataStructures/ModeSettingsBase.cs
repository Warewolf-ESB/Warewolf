using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;


using Infragistics.Windows.Internal;


namespace Infragistics.Controls.Layouts
{
    /// <summary>
    /// Abstract base class that contains settings that are common to both Normal mode and Maximized mode in a <see cref="XamTileManager"/>.
    /// </summary>
    /// <seealso cref="NormalModeSettings"/>
    /// <seealso cref="MaximizedModeSettings"/>
    /// <seealso cref="XamTileManager.NormalModeSettings"/>
    /// <seealso cref="XamTileManager.MaximizedModeSettings"/>

    [CloneBehavior(CloneBehavior.CloneObject)]

    public abstract class ModeSettingsBase : DependencyObjectNotifier
    {
        #region Private members

        private const int DEFAULT_REPOSITION_ANIMATION_DURATION = 500;
        private const int DEFAULT_RESIZE_ANIMATION_DURATION = 500;

        #endregion //Private members	

        #region Constructors

        static ModeSettingsBase()
        {
			CubicEase easing = new CubicEase();
            easing.EasingMode = EasingMode.EaseInOut;




            DoubleAnimation repositionAnimation = new DoubleAnimation();
            repositionAnimation.From = 0;
			repositionAnimation.To = 1.0;
			repositionAnimation.Duration = TimeSpan.FromMilliseconds(DEFAULT_REPOSITION_ANIMATION_DURATION);
            repositionAnimation.AutoReverse = false;
			repositionAnimation.EasingFunction = easing;
            repositionAnimation.FillBehavior = FillBehavior.Stop;


            repositionAnimation.Freeze();

            s_RepositionAnimation = DependencyPropertyUtilities.Register("RepositionAnimation",
				   typeof(Timeline), typeof(ModeSettingsBase),
					repositionAnimation, new PropertyChangedCallback(OnAnimationPropertyChanged)
				   );

            DoubleAnimation resizeAnimation = new DoubleAnimation();
			resizeAnimation.From = 0;
			resizeAnimation.To = 1.0;
			resizeAnimation.Duration = TimeSpan.FromMilliseconds(DEFAULT_RESIZE_ANIMATION_DURATION);
            resizeAnimation.AutoReverse = false;
			resizeAnimation.EasingFunction = easing;
            resizeAnimation.FillBehavior = FillBehavior.Stop;


            resizeAnimation.Freeze();

			s_ResizeAnimation = DependencyPropertyUtilities.Register("ResizeAnimation",
				   typeof(Timeline), typeof(ModeSettingsBase),
				   resizeAnimation, new PropertyChangedCallback(OnAnimationPropertyChanged)
				   );

        }

        #endregion //Constructors	
    
        #region Base Class Overrides

            #region ToString

		///// <summary>
		///// Returns a string representation of the non-default settings
		///// </summary>
		///// <returns></returns>
		//public override string ToString()
		//{
		//    //return Utilities.StringFromNonDefaultProperties(this);
		//}

            #endregion //ToString

		#endregion //Base Class Overrides	

        #region Properties

            #region Public Properties

				#region AllowTileDragging

		/// <summary>
		/// Identifies the <see cref="AllowTileDragging"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowTileDraggingProperty = DependencyPropertyUtilities.Register("AllowTileDragging",
			typeof(AllowTileDragging), typeof(ModeSettingsBase),
			DependencyPropertyUtilities.CreateMetadata(AllowTileDragging.Swap, new PropertyChangedCallback(OnAllowTileDraggingChanged))
			);

		private static void OnAllowTileDraggingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ModeSettingsBase instance = d as ModeSettingsBase;

			CoreUtilities.ValidateEnum(typeof(AllowTileDragging), e.NewValue);

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Gets/sets whether the user is allowed to drag tiles to reposition them.
        /// </summary>
        /// <value>The AllowTileDragging enum. The default is 'Swap'</value>
        /// <seealso cref="AllowTileDraggingProperty"/>
        public AllowTileDragging AllowTileDragging
        {
            get
            {
                return (AllowTileDragging)this.GetValue(ModeSettingsBase.AllowTileDraggingProperty);
            }
            set
            {
                this.SetValue(ModeSettingsBase.AllowTileDraggingProperty, value);
            }
        }

                #endregion //AllowTileDragging

				#region RepositionAnimation

        private static DependencyProperty s_RepositionAnimation;

        /// <summary>
        /// Identifies the <see cref="RepositionAnimation"/> dependency property
        /// </summary>
        public static DependencyProperty RepositionAnimationProperty { get { return s_RepositionAnimation; } }


	    private static void OnAnimationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	    {

			ModeSettingsBase instance = d as ModeSettingsBase;

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));

			if (e.NewValue == null)
				return;


			if (!(e.NewValue is DoubleAnimationBase))




				throw new ArgumentException(TileUtilities.GetString("LE_MustBeDoubleAnimation"));

	    }

        /// <summary>
        /// Determines how a <see cref="XamTile"/> animates from one location to another.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property will be ignored if <see cref="ShouldAnimate"/> is set to 'False'.</para>
        /// </remarks>
        /// <seealso cref="ResizeAnimation"/>
        /// <seealso cref="ShouldAnimate"/>
        /// <seealso cref="RepositionAnimationProperty"/>
        public Timeline RepositionAnimation
        {
            get
            {
                return (Timeline)this.GetValue(ModeSettingsBase.RepositionAnimationProperty);
            }
            set
            {
                this.SetValue(ModeSettingsBase.RepositionAnimationProperty, value);
            }
        }

                #endregion //RepositionAnimation

                #region ResizeAnimation
        
        private static DependencyProperty s_ResizeAnimation;

        /// <summary>
        /// Identifies the <see cref="ResizeAnimation"/> dependency property
        /// </summary>
        public static DependencyProperty ResizeAnimationProperty { get { return s_ResizeAnimation; } }

        /// <summary>
        /// Determines how a <see cref="XamTile"/> animates from one size to another.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property will be ignored if <see cref="ShouldAnimate"/> is set to 'False'.</para>
        /// </remarks>
        /// <seealso cref="RepositionAnimation"/>
        /// <seealso cref="ShouldAnimate"/>
        /// <seealso cref="ResizeAnimationProperty"/>
        public Timeline ResizeAnimation
        {
            get
            {
                return (Timeline)this.GetValue(ModeSettingsBase.ResizeAnimationProperty);
            }
            set
            {
                this.SetValue(ModeSettingsBase.ResizeAnimationProperty, value);
            }
        }

                #endregion //ResizeAnimation

				#region ShouldAnimate

		/// <summary>
		/// Identifies the <see cref="ShouldAnimate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShouldAnimateProperty = DependencyPropertyUtilities.Register("ShouldAnimate",
			typeof(bool), typeof(ModeSettingsBase),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnShouldAnimateChanged))
			);

		private static void OnShouldAnimateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ModeSettingsBase instance = d as ModeSettingsBase;

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Gets/sets whether tiles will animate to their new position and size
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property defaults to 'True' but when set to 'False' will cause the <see cref="RepositionAnimation"/> and <see cref="ResizeAnimation"/> properties to be ignored.</para>
        /// </remarks>
        /// <seealso cref="ResizeAnimation"/>
        /// <seealso cref="RepositionAnimation"/>
        /// <seealso cref="ShouldAnimateProperty"/>
        public bool ShouldAnimate
        {
            get
            {
                return (bool)this.GetValue(ModeSettingsBase.ShouldAnimateProperty);
            }
            set
            {
                this.SetValue(ModeSettingsBase.ShouldAnimateProperty, value);
            }
        }

                #endregion //ShouldAnimate

            #endregion //Public Properties

        #endregion //Properties

        #region Methods

            #region Internal Methods
            
                #region ValidateSpacing

        internal static void ValidateSpacing(object objVal)
        {
            double val = (double)objVal;

            if (double.IsNaN(val))
                return;

			if (double.IsInfinity(val) || val < 0d)
				throw new ArgumentOutOfRangeException(TileUtilities.GetString("LE_CantBeNegativeOrInfinity"));
        }

                #endregion // ValidateSpacing

                #region ValidateMaxRowColumn

        internal static void ValidateMaxRowColumn(object objVal)
        {
            int val = (int)objVal;
			if (val < 0)
				throw new ArgumentOutOfRangeException(TileUtilities.GetString("LE_CantBeNegative"));
        }

                #endregion // ValidateMaxRowColumn

                #region ValidateMinRowColumn

        internal static void ValidateMinRowColumn(object objVal)
        {
            int val = (int)objVal;
            if ( val < 1 )
				throw new ArgumentOutOfRangeException(TileUtilities.GetString("LE_CantBeNegativeOrZero"));
        }

                #endregion // ValidateMinRowColumn

            #endregion //Internal Methods	

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