using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Markup;
using System.Diagnostics;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Shapes;

using Infragistics.Windows.Selection;
using Infragistics.Windows.Licensing;
using Infragistics.Windows.Controls.Events;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Commands;
using Infragistics.Windows.Virtualization;
using System.Xml;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.Tiles
{
    /// <summary>
    /// Abstract base class that contains settings that are common to both Normal mode and Maximized mode in a <see cref="TilesPanel"/> or <see cref="XamTilesControl"/>.
    /// </summary>
    /// <seealso cref="NormalModeSettings"/>
    /// <seealso cref="MaximizedModeSettings"/>
    /// <seealso cref="XamTilesControl.NormalModeSettings"/>
    /// <seealso cref="XamTilesControl.MaximizedModeSettings"/>
    [CloneBehavior(CloneBehavior.CloneObject)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
    public abstract class ModeSettingsBase : DependencyObjectNotifier
    {
        #region Private members

        private const int DEFAULT_REPOSITION_ANIMATION_DURATION = 500;
        private const int DEFAULT_RESIZE_ANIMATION_DURATION = 500;

        #endregion //Private members	

        #region Constructors

        static ModeSettingsBase()
        {

            DoubleAnimation repositionAnimation = new DoubleAnimation(0, 1.0, TimeSpan.FromMilliseconds(DEFAULT_REPOSITION_ANIMATION_DURATION), FillBehavior.Stop);
            repositionAnimation.BeginTime = new Nullable<TimeSpan>();
            repositionAnimation.AutoReverse = false;
            repositionAnimation.AccelerationRatio = .6;
            repositionAnimation.DecelerationRatio = .4;
            repositionAnimation.FillBehavior = FillBehavior.Stop;
            repositionAnimation.Freeze();
            s_RepositionAnimation = DependencyProperty.Register("RepositionAnimation",
                    typeof(DoubleAnimationBase), typeof(ModeSettingsBase), new FrameworkPropertyMetadata(repositionAnimation));

            DoubleAnimation resizeAnimation = new DoubleAnimation(0, 1.0, TimeSpan.FromMilliseconds(DEFAULT_RESIZE_ANIMATION_DURATION), FillBehavior.Stop);
            resizeAnimation.BeginTime = new Nullable<TimeSpan>();
            resizeAnimation.AutoReverse = false;
            resizeAnimation.AccelerationRatio = .6;
            resizeAnimation.DecelerationRatio = .4;
            resizeAnimation.FillBehavior = FillBehavior.Stop;
            resizeAnimation.Freeze();
            s_ResizeAnimation = DependencyProperty.Register("ResizeAnimation",
                    typeof(DoubleAnimationBase), typeof(ModeSettingsBase), new FrameworkPropertyMetadata(resizeAnimation));

        }

        #endregion //Constructors	
    
        #region Base Class Overrides

			#region OnPropertyChanged

		/// <summary>
		/// Called when a property changes.
		/// </summary>
		/// <param name="e">A DependencyPropertyChangedEventArgs instance that contains information about the property that changed.</param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			this.RaisePropertyChangedEvent(e.Property.Name);
		}

			#endregion //OnPropertyChanged

            #region ToString

        /// <summary>
        /// Returns a string representation of the non-default settings
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Utilities.StringFromNonDefaultProperties(this);
        }

            #endregion //ToString

		#endregion //Base Class Overrides	

        #region Properties

            #region Public Properties

                #region AllowTileDragging

        /// <summary>
        /// Identifies the <see cref="AllowTileDragging"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AllowTileDraggingProperty = DependencyProperty.Register("AllowTileDragging",
            typeof(AllowTileDragging), typeof(ModeSettingsBase), new FrameworkPropertyMetadata(AllowTileDragging.Swap), new ValidateValueCallback(ValidateAllowTileDragging));

        private static bool ValidateAllowTileDragging(object value)
        {
            return Enum.IsDefined(typeof(AllowTileDragging), value);
        }

        /// <summary>
        /// Gets/sets whether the user is allowed to drag tiles to reposition them.
        /// </summary>
        /// <value>The AllowTileDragging enum. The default is 'Swap'</value>
        /// <seealso cref="AllowTileDraggingProperty"/>
        //[Description("Gets/sets whether the user is allowed to drag tiles to reposition them.")]
        //[Category("TilesControl Properties")]
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

                #region HorizontalTileAreaAlignment

        /// <summary>
        /// Identifies the <see cref="HorizontalTileAreaAlignment"/> dependency property
        /// </summary>
        public static readonly DependencyProperty HorizontalTileAreaAlignmentProperty = DependencyProperty.Register("HorizontalTileAreaAlignment",
            typeof(HorizontalAlignment), typeof(ModeSettingsBase), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentLeftBox), new ValidateValueCallback(ValidateHorizontalTileAreaAlignment));

        private static bool ValidateHorizontalTileAreaAlignment(object value)
        {
            return Enum.IsDefined(typeof(HorizontalAlignment), value);
        }

        /// <summary>
        /// Determines the horizontal alignment of the complete block of visible tiles within the control.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> For <see cref="MaximizedModeSettings"/>, this property applies to the minimized tile area only.</para>
        /// </remarks>
        /// <seealso cref="HorizontalTileAreaAlignmentProperty"/>
        //[Description("Determines the horizontal alignment of the complete block of visible tiles within the control.")]
        //[Category("TilesControl Properties")]
        public HorizontalAlignment HorizontalTileAreaAlignment
        {
            get
            {
                return (HorizontalAlignment)this.GetValue(ModeSettingsBase.HorizontalTileAreaAlignmentProperty);
            }
            set
            {
                this.SetValue(ModeSettingsBase.HorizontalTileAreaAlignmentProperty, value);
            }
        }

                #endregion //HorizontalTileAreaAlignment

                #region RepositionAnimation

        private static DependencyProperty s_RepositionAnimation;

        /// <summary>
        /// Identifies the <see cref="RepositionAnimation"/> dependency property
        /// </summary>
        public static DependencyProperty RepositionAnimationProperty { get { return s_RepositionAnimation; } }


        /// <summary>
        /// Determines how a <see cref="Tile"/> animates from one location to another.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property will be ignored if <see cref="ShouldAnimate"/> is set to 'False'.</para>
        /// </remarks>
        /// <seealso cref="ResizeAnimation"/>
        /// <seealso cref="ShouldAnimate"/>
        /// <seealso cref="RepositionAnimationProperty"/>
        //[Description("Determines how a Tile animates from one location to another")]
        //[Category("TilesControl Properties")]
        public DoubleAnimationBase RepositionAnimation
        {
            get
            {
                return (DoubleAnimationBase)this.GetValue(ModeSettingsBase.RepositionAnimationProperty);
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
        /// Determines how a <see cref="Tile"/> animates from one size to another.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property will be ignored if <see cref="ShouldAnimate"/> is set to 'False'.</para>
        /// </remarks>
        /// <seealso cref="RepositionAnimation"/>
        /// <seealso cref="ShouldAnimate"/>
        /// <seealso cref="ResizeAnimationProperty"/>
        //[Description("Determines how a Tile animates from one size to another")]
        //[Category("TilesControl Properties")]
        public DoubleAnimationBase ResizeAnimation
        {
            get
            {
                return (DoubleAnimationBase)this.GetValue(ModeSettingsBase.ResizeAnimationProperty);
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
        public static readonly DependencyProperty ShouldAnimateProperty = DependencyProperty.Register("ShouldAnimate",
            typeof(bool), typeof(ModeSettingsBase), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

        /// <summary>
        /// Gets/sets whether tiles will animate to their new position and size
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property defaults to 'True' but when set to 'False' will cause the <see cref="RepositionAnimation"/> and <see cref="ResizeAnimation"/> properties to be ignored.</para>
        /// </remarks>
        /// <seealso cref="ResizeAnimation"/>
        /// <seealso cref="RepositionAnimation"/>
        /// <seealso cref="ShouldAnimateProperty"/>
        //[Description("Gets/sets whether tiles will animate to their new position and size")]
        //[Category("TilesControl Properties")]
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

                #region VerticalTileAreaAlignment

        /// <summary>
        /// Identifies the <see cref="VerticalTileAreaAlignment"/> dependency property
        /// </summary>
        public static readonly DependencyProperty VerticalTileAreaAlignmentProperty = DependencyProperty.Register("VerticalTileAreaAlignment",
            typeof(VerticalAlignment), typeof(ModeSettingsBase), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentTopBox), new ValidateValueCallback(ValidateVerticalTileAreaAlignment));

        private static bool ValidateVerticalTileAreaAlignment(object value)
        {
            return Enum.IsDefined(typeof(VerticalAlignment), value);
        }

        /// <summary>
        /// Determines the vertical alignment of the complete block of visible tiles within the control.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> For <see cref="MaximizedModeSettings"/>, this property applies to the minimized tile area only.</para>
        /// </remarks>
        /// <seealso cref="VerticalTileAreaAlignmentProperty"/>
        //[Description("Determines the vertical alignment of the complete block of visible tiles within the control.")]
        //[Category("TilesControl Properties")]
        public VerticalAlignment VerticalTileAreaAlignment
        {
            get
            {
                return (VerticalAlignment)this.GetValue(ModeSettingsBase.VerticalTileAreaAlignmentProperty);
            }
            set
            {
                this.SetValue(ModeSettingsBase.VerticalTileAreaAlignmentProperty, value);
            }
        }

                #endregion //VerticalTileAreaAlignment

            #endregion //Public Properties

        #endregion //Properties

        #region Methods

            #region Internal Methods
         
                #region ValidateSpacing

        internal static bool ValidateSpacing(object objVal)
        {
            double val = (double)objVal;

            if (double.IsNaN(val))
                return true;

            return (!double.IsInfinity(val)) && val >= 0d;
        }

                #endregion // ValidateSpacing

                #region ValidateMaxRowColumn

        internal static bool ValidateMaxRowColumn(object objVal)
        {
            int val = (int)objVal;
            return val >= 0;
        }

                #endregion // ValidateMaxRowColumn

                #region ValidateMinRowColumn

        internal static bool ValidateMinRowColumn(object objVal)
        {
            int val = (int)objVal;
            return val > 0;
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