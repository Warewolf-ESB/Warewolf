
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media.Animation;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// Animates a chart element using linear interpolation 
    /// over a specified Duration. 
    /// </summary>
    /// <remarks>
    /// There are two ways how animation could be used with XamChart. The first way is to use WPF animation which is supported 
    /// as Transformation animation, Brush animation, etc. The second way is to use Animation class which is part of 
    /// Infragistics.Windows.Chart namespace. This animation is used for some complex cases where WPF animation cannot be used. 
    /// This animation is implemented inside chart control.
    /// </remarks>
    public class Animation : ChartFrameworkContentElement
    {

        #region Fields

        // Private Fields
        private object _chartParent;

        #endregion Fields

        #region Internal Properties

        /// <summary>
        /// The parent object
        /// </summary>
        internal object ChartParent
        {
            get
            {
                return _chartParent;
            }
            set
            {
                _chartParent = value;
            }
        }

        #endregion Internal Properties

        #region Methods

        /// <summary>
        /// Initializes a new instance of the Animation class. 
        /// </summary>
        public Animation()
        {
        }

        /// <summary>
        /// Represents the callback that is invoked when the effective property value of a given dependency property changes.
        /// </summary>
        /// <param name="d">The DependencyObject on which the property is used.</param>
        /// <param name="e">Arguments that are issued by any event that tracks changes to the effective value of this property.</param>
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamChart control = XamChart.GetControl(d);
            if (control != null && e.NewValue != e.OldValue)
            {
                control.RefreshProperty();
            }
        }

        /// <summary>
        /// Creates Double animation from Animation
        /// </summary>
        /// <returns>Double Animation</returns>
        internal DoubleAnimation GetDoubleAnimation()
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.AccelerationRatio = this.AccelerationRatio;
            doubleAnimation.DecelerationRatio = this.DecelerationRatio;
            doubleAnimation.RepeatBehavior = this.RepeatBehavior;
            doubleAnimation.BeginTime = this.BeginTime;
            doubleAnimation.Duration = this.Duration;

            return doubleAnimation;
        }

        #endregion Methods

        #region Public Properties

        #region BeginTime

        /// <summary>
        /// Identifies the <see cref="BeginTime"/> dependency property
        /// </summary>
        public static readonly DependencyProperty BeginTimeProperty = DependencyProperty.Register("BeginTime",
            typeof(TimeSpan), typeof(Animation), new FrameworkPropertyMetadata(TimeSpan.Zero, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the time at which this Timeline should begin.
        /// </summary>
        /// <seealso cref="BeginTimeProperty"/>
        //[Description("Gets or sets the time at which this Timeline should begin.")]
        public TimeSpan BeginTime
        {
            get
            {
                return (TimeSpan)this.GetValue(Animation.BeginTimeProperty);
            }
            set
            {
                this.SetValue(Animation.BeginTimeProperty, value);
            }
        }

        #endregion BeginTime

        #region Duration

        /// <summary>
        /// Identifies the <see cref="Duration"/> dependency property
        /// </summary>
        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("Duration",
            typeof(Duration), typeof(Animation), new FrameworkPropertyMetadata(new Duration(new TimeSpan(0, 0, 2)), new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the length of time for which this timeline plays, not counting repetitions.
        /// </summary>
        /// <seealso cref="DurationProperty"/>
        //[Description("Gets or sets the length of time for which this timeline plays, not counting repetitions.")]
        public Duration Duration 
        {
            get
            {
                return (Duration)this.GetValue(Animation.DurationProperty);
            }
            set
            {
                this.SetValue(Animation.DurationProperty, value);
            }
        }

        #endregion Duration

        #region AccelerationRatio

        /// <summary>
        /// Identifies the <see cref="AccelerationRatio"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AccelerationRatioProperty = DependencyProperty.Register("AccelerationRatio",
            typeof(double), typeof(Animation), new FrameworkPropertyMetadata((double)0, new PropertyChangedCallback(OnPropertyChanged)), new ValidateValueCallback(AccelerationRatioValidateCallback));

        /// <summary>
        /// Gets or sets a value specifying the percentage of the timeline's Duration spent accelerating the passage of time from zero to its maximum rate.
        /// </summary>
        /// <seealso cref="DurationProperty"/>
        /// <remarks>
        /// A value between 0 and 1, inclusive, that specifies the percentage of the timeline's Duration spent accelerating the passage of time from zero to its maximum rate. If the timeline's DecelerationRatio property is also set, the sum of AccelerationRatio and DecelerationRatio must be less than or equal to 1. The default value is 0. 
        /// </remarks>
        //[Description("Gets or sets a value specifying the percentage of the timeline's Duration spent accelerating the passage of time from zero to its maximum rate.")]
        public double AccelerationRatio
        {
            get
            {
                return (double)this.GetValue(Animation.AccelerationRatioProperty);
            }
            set
            {
                this.SetValue(Animation.AccelerationRatioProperty, value);
            }
        }

        /// <summary>
        /// Represents a method used as a callback when registering a new dependency property or attached property. 
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <returns>true if the value was validated; false if the submitted value was invalid.</returns>
        private static bool AccelerationRatioValidateCallback(object value)
        {
            double speedRatio = (double)value;

            return (speedRatio >= 0 && speedRatio <= 1);
        }

        #endregion AccelerationRatio

        #region DecelerationRatio

        /// <summary>
        /// Identifies the <see cref="DecelerationRatio"/> dependency property
        /// </summary>
        public static readonly DependencyProperty DecelerationRatioProperty = DependencyProperty.Register("DecelerationRatio",
            typeof(double), typeof(Animation), new FrameworkPropertyMetadata((double)0, new PropertyChangedCallback(OnPropertyChanged)), new ValidateValueCallback(DecelerationRatioValidateCallback));

        /// <summary>
        /// Gets or sets a value specifying the percentage of the timeline's Duration spent decelerating the passage of time from its maximum rate to zero.
        /// </summary>
        /// <seealso cref="DurationProperty"/>
        /// <remarks>
        /// A value between 0 and 1, inclusive, that specifies the percentage of the timeline's Duration spent decelerating the passage of time from its maximum rate to zero. If the timeline's AccelerationRatio property is also set, the sum of DecelerationRatio and AccelerationRatio must be less than or equal to 1. The default value is 0. 
        /// </remarks>
        //[Description("Gets or sets a value specifying the percentage of the timeline's Duration spent decelerating the passage of time from its maximum rate to zero.")]
        public double DecelerationRatio
        {
            get
            {
                return (double)this.GetValue(Animation.DecelerationRatioProperty);
            }
            set
            {
                this.SetValue(Animation.DecelerationRatioProperty, value);
            }
        }

        /// <summary>
        /// Represents a method used as a callback when registering a new dependency property or attached property. 
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <returns>true if the value was validated; false if the submitted value was invalid.</returns>
        private static bool DecelerationRatioValidateCallback(object value)
        {
            double decelerationRatio = (double)value;

            return (decelerationRatio >= 0 && decelerationRatio <= 1);
        }

        #endregion DecelerationRatio

        #region RepeatBehavior

        /// <summary>
        /// Identifies the <see cref="RepeatBehavior"/> dependency property
        /// </summary>
        public static readonly DependencyProperty RepeatBehaviorProperty = DependencyProperty.Register("RepeatBehavior",
            typeof(RepeatBehavior), typeof(Animation), new FrameworkPropertyMetadata(new RepeatBehavior(1.0), new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the repeating behavior of this timeline, either as an iteration Count or a repeat Duration.
        /// </summary>
        /// <seealso cref="DurationProperty"/>
        //[Description("Gets or sets the repeating behavior of this timeline, either as an iteration Count or a repeat Duration.")]
        public RepeatBehavior RepeatBehavior
        {
            get
            {
                return (RepeatBehavior)this.GetValue(Animation.RepeatBehaviorProperty);
            }
            set
            {
                this.SetValue(Animation.RepeatBehaviorProperty, value);
            }
        }
               
        #endregion DecelerationRatio

        #region Sequential

        /// <summary>
        /// Identifies the <see cref="Sequential"/> dependency property
        /// </summary>
        public static readonly DependencyProperty SequentialProperty = DependencyProperty.Register("Sequential",
            typeof(bool), typeof(Animation), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a value indicating whether the sub elements are animated all together or sequentially. Used for series, gridlines and tickmarks.
        /// </summary>
        /// <seealso cref="DurationProperty"/>
        //[Description("Gets or sets a value indicating whether the sub elements are animated all together or sequentially. Used for series, gridlines and tickmarks.")]
        public bool Sequential
        {
            get
            {
                return (bool)this.GetValue(Animation.SequentialProperty);
            }
            set
            {
                this.SetValue(Animation.SequentialProperty, value);
            }
        }

        #endregion Sequential

        #endregion Public Properties
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