using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Infragistics.Controls.Editors.Primitives;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// A control that provides XamRangeSlider from DateTime type.
    /// </summary>

    
    


    public class XamDateTimeRangeSlider : XamRangeSlider<DateTime>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="XamDateTimeRangeSlider"/> class.
        /// </summary>
        public XamDateTimeRangeSlider()
        {

            Infragistics.Windows.Utilities.ValidateLicense(typeof(XamDateTimeRangeSlider), this);


            this.DefaultStyleKey = typeof(XamDateTimeRangeSlider);
            this.MinValue = new DateTime(1, 1, 1);
            this.MaxValue = new DateTime(1, 12, 1);
        }

        #endregion Constructors

        #region Overrides

        #region DoubleToValue

        /// <summary>
        /// Doubles to value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Value from generic type</returns>
        protected override DateTime DoubleToValue(double value)
        {
            if (value > 10)
            {
                DateTime dateTimeValue = new DateTime((long)value);
                return dateTimeValue;
            }

            return DateTime.MinValue;
        }

        #endregion DoubleToValue

        #region GetDelta

        /// <summary>
        /// Gets the delta, used to coerse the min max values.
        /// </summary>
        /// <returns>delta as double</returns>
        protected override double GetDelta()
        {
            return this.GetSmallChangeValue();
        }

        #endregion GetDelta

        #region GetLargeChangeValue

        /// <summary>
        /// Gets the LargeChange value in double.
        /// </summary>
        /// <returns>value in double</returns>
        protected internal override double GetLargeChangeValue()
        {
            return GetLargeChangeValue(this.ToDouble(new DateTime(1, 1, 1)), true);
        }

        /// <summary>
        /// Gets the LargeChange value in double, based off of a given starting point (but not including it, used for DateTime).
        /// </summary>
        /// <param name="baseValue">The starting point to add the large value to.</param>
        /// <param name="isIncreasing">Indicated whether the change is positive or negative.</param>
        /// <returns>value in double based of the starting point (but not including it, used for DateTime)</returns>
        protected internal override double GetLargeChangeValue(double baseValue, bool isIncreasing)
        {
            DateTime dateTime = this.ToValue(baseValue);
            var change = isIncreasing ? this.LargeChange : this.LargeChange * -1;

            return this.ToDouble(XamDateTimeSlider.CreateChangeValue(dateTime, isIncreasing, change, this.LargeChangeType)) - baseValue;
        }

        #endregion GetLargeChangeValue

        #region GetParameter

        /// <summary>
        /// Gets the parameter - control that
        /// can execute the command.
        /// </summary>
        /// <param name="source">The command source.</param>
        /// <returns>The object that can execute the command</returns>
        protected override object GetParameter(CommandSource source)
        {
            if (source.Command is XamSliderBaseCommandBase)
            {
                return this;
            }

            return null;
        }

        #endregion GetParameter

        #region GetSmallChangeValue

        /// <summary>
        /// Gets the SmallChange value in double.
        /// </summary>
        /// <returns>value in double</returns>
        protected internal override double GetSmallChangeValue()
        {
            DateTime dateTime = new DateTime(1, 1, 1);

            return this.ToDouble(XamDateTimeSlider.CreateChangeValue(dateTime, true, this.SmallChange, this.SmallChangeType));
        }

        #endregion GetSmallChangeValue

        #region LargeChangeTypeProperty

        /// <summary>
        /// Identifies the <see cref="LargeChangeType"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LargeChangeTypeProperty = DependencyProperty.Register("LargeChangeType", typeof(FrequencyType), typeof(XamDateTimeRangeSlider), null);

        /// <summary>
        /// Gets or sets the type of the
        /// <see cref="LargeChangeType"/>
        /// from <see cref="Infragistics.Controls.Editors.FrequencyType"/>
        /// enum type.
        /// </summary>
        /// <value>The type of the LargeChange.</value>
        public FrequencyType LargeChangeType
        {
            get { return (FrequencyType)this.GetValue(LargeChangeTypeProperty); }
            set { this.SetValue(LargeChangeTypeProperty, value); }
        }

        #endregion LargeChangeTypeProperty

        #region MaxValue

        /// <summary>
        /// Gets or sets the maximum allowable value for this slider's range.
        /// </summary>
        /// <value>The max value.</value>
        [TypeConverter(typeof(DateTimeConverter))]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override DateTime MaxValue
        {
            get
            {
                return base.MaxValue;
            }

            set
            {
                base.MaxValue = value;
            }
        }

        #endregion MaxValue

        #region MinValue

        /// <summary>
        /// Gets or sets the minimum allowable value for this slider's range.
        /// </summary>
        /// <value>The min value.</value>
        [TypeConverter(typeof(DateTimeConverter))]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override DateTime MinValue
        {
            get
            {
                return base.MinValue;
            }

            set
            {
                base.MinValue = value;
            }
        }

        #endregion MinValue

        #region SmallChangeTypeProperty


        /// <summary>
        /// Identifies the <see cref="SmallChangeType"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SmallChangeTypeProperty = DependencyProperty.Register("SmallChangeType", typeof(FrequencyType), typeof(XamDateTimeRangeSlider), null);

        /// <summary>
        /// Gets or sets the type of the
        /// <see cref="SmallChangeType"/>
        /// from <see cref="Infragistics.Controls.Editors.FrequencyType"/>
        /// enum type.
        /// </summary>
        /// <value>The type of the LargeChange.</value>
        public FrequencyType SmallChangeType
        {
            get { return (FrequencyType)this.GetValue(SmallChangeTypeProperty); }
            set { this.SetValue(SmallChangeTypeProperty, value); }
        }






        #endregion SmallChangeTypeProperty

        #region SupportsCommand

        /// <summary>
        /// Supportses the command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>the command as base command class</returns>
        protected override bool SupportsCommand(ICommand command)
        {
            return command is XamSliderBaseCommandBase;
        }

        #endregion SupportsCommand

        #region ValueToDouble

        /// <summary>
        /// Converts value from specific generic type to double.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>double value</returns>
        protected override double ValueToDouble(DateTime value)
        {
            double doubleValue = value.Ticks;
            return doubleValue;
        }

        #endregion ValueToDouble

        #region ProcessChanges

        /// <summary>
        /// Change the value of the active thumb with specified LargeChange or SmallChange value.
        /// </summary>
        /// <param name="isIncrease">if set to <c>true</c> value is increased.</param>
        /// <param name="isLargeChange">if set to <c>true</c> LargeChange is used.</param>
        /// <param name="forceMoveOneTick">if set to <c>true</c> and Snap to Tick is True, the thumb will move to the next tick mark regardless of change size.</param>
        protected internal override void ProcessChanges(bool isIncrease, bool isLargeChange, bool forceMoveOneTick)
        {
            if (this.IsDirectionReversed)
                isIncrease = !isIncrease;

            XamSliderThumb<DateTime> activeThumb = this.ActiveThumb;

            if (activeThumb == null)
                return;

            FrequencyType changeType = isLargeChange ? this.LargeChangeType : this.SmallChangeType;
            int changeValue = isLargeChange ? (int)this.LargeChange : (int)this.SmallChange;
            if (!isIncrease)
                changeValue *= -1;

            double thumbValue = ToDouble(XamDateTimeSlider.CreateChangeValue(activeThumb.Value, isIncrease, changeValue, changeType));

            if (activeThumb.IsSnapToTickEnabled && forceMoveOneTick)
            {
                thumbValue = NextTickMarkValue(thumbValue, isIncrease);
            }

            activeThumb.Value = activeThumb.PreviewCoerceValue(ToValue(thumbValue));
        }

        #endregion ProcessChanges

        #region TickMarks

        /// <summary>
        /// Gets the collection of tick marks.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override ObservableCollection<SliderTickMarks<DateTime>> TickMarks
        {
            get
            {
                return base.TickMarks;
            }
        }

        #endregion TickMarks

        #endregion Overrides
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