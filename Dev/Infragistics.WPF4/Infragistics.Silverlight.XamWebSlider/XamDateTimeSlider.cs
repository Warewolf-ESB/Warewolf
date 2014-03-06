using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Infragistics.Controls.Editors.Primitives;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// A control that provides simple slider from DateTime type.
    /// </summary>

    
    


    public class XamDateTimeSlider : XamSimpleSliderBase<DateTime>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="XamDateTimeSlider"/> class.
        /// </summary>
        public XamDateTimeSlider()
        {

                Infragistics.Windows.Utilities.ValidateLicense(typeof(XamDateTimeSlider), this);


            this.DefaultStyleKey = typeof(XamDateTimeSlider);
            this.MinValue = new DateTime(1, 1, 1);
            this.MaxValue = new DateTime(1, 12, 1);
            this.Value = this.MinValue;

            this.Thumb = new XamSliderDateTimeThumb();
        }

        #endregion Constructors

        #region Overrides

        #region DoubleToValue

        /// <summary>
        /// Doubles to value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Value from DateTime type</returns>
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

            return this.ToDouble(CreateChangeValue(dateTime, isIncreasing, change, this.LargeChangeType)) - baseValue;
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

            return this.ToDouble(CreateChangeValue(dateTime, true, this.SmallChange, this.SmallChangeType)) - ToDouble(dateTime);
        }

        #endregion GetSmallChangeValue

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

        #region Value

        /// <summary>
        /// Gets or sets the value.
        /// It is the current value of the ActiveThumb , respectively
        /// Thumb of the simple slider
        /// </summary>
        /// <value>The value.</value>
        [TypeConverter(typeof(DateTimeConverter))]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override DateTime Value
        {
            get
            {
                return base.Value;
            }

            set
            {
                base.Value = value;
            }
        }

        #endregion Value

        #region ValueToDouble

        /// <summary>
        /// Converts value from specific DateTime type to double.
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

            FrequencyType changeType = isLargeChange ? this.LargeChangeType : this.SmallChangeType;
            int changeValue = isLargeChange ? (int)this.LargeChange : (int)this.SmallChange;
            if (!isIncrease)
                changeValue *= -1;

            XamSliderThumb<DateTime> thumb = this.ActiveThumb;
            double thumbValue = ToDouble(CreateChangeValue(thumb.Value, isIncrease, changeValue, changeType));
            
            if (thumb.IsSnapToTickEnabled && forceMoveOneTick)
            {
                thumbValue = NextTickMarkValue(thumbValue, isIncrease);
            }

            thumb.Value = thumb.PreviewCoerceValue(ToValue(thumbValue));
        }

        #endregion ProcessChanges

        #region OnThumbChanged

        /// <summary>
        /// Called when the value of Thumb property is changed.
        /// </summary>
        /// <param name="thumb">The thumb.</param>
        protected override void OnThumbChanged(XamSliderThumb<DateTime> thumb)
        {
            foreach (XamSliderThumb<DateTime> t in this.Thumbs)
            {
                t.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(Thumb_PropertyChanged);
            }

            base.OnThumbChanged(thumb);

            foreach (XamSliderThumb<DateTime> t in this.Thumbs)
            {
                t.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Thumb_PropertyChanged);
            }
        }

        #endregion OnThumbChanged

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

        #region EventHandlers

        private void Thumb_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsActive")
            {
                XamSliderThumb<DateTime> t = (XamSliderThumb<DateTime>)sender;
                if (!t.IsActive)
                    t.IsActive = true;
            }
        }

        #endregion EventHandlers

        #region Properties

        #region Public Properties

        #region LargeChangeTypeProperty

        /// <summary>
        /// Identifies the <see cref="LargeChangeType"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LargeChangeTypeProperty = DependencyProperty.Register("LargeChangeType", typeof(FrequencyType), typeof(XamDateTimeSlider), null);

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

        #region SmallChangeTypeProperty



        /// <summary>
        /// Identifies the <see cref="SmallChangeType"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SmallChangeTypeProperty = DependencyProperty.Register("SmallChangeType", typeof(FrequencyType), typeof(XamDateTimeSlider), null);

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



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


        #endregion SmallChangeTypeProperty

        #endregion Public Properties

        #endregion //Properties

        #region Methods

        #region Private

        #region CreateChangeValue

        /// <summary>
        /// Takes in a given set of parameters and coerces out the appropriate date time.  This function is sensitive to gracefully touching DateTime Min and Max
        /// </summary>
        /// <param name="dateTime">The starting DateTime</param>
        /// <param name="isIncreasing">Whether or not the value is increasing</param>
        /// <param name="change">The amount of the change</param>
        /// <param name="frequencyType">The change type ie: Days, Hours, ...</param>
        /// <returns>The new DateTime adjusted to the value</returns>
        internal static DateTime CreateChangeValue(DateTime dateTime, bool isIncreasing, double change, FrequencyType frequencyType)
        {
            long changeTicks = 0;
            long pendingTicks = 0;
            int landingYear = 0;

            switch (frequencyType)
            {
                case FrequencyType.Seconds:
                    changeTicks = TimeSpan.TicksPerSecond * (long)change;
                    pendingTicks = dateTime.Ticks + changeTicks;

                    if (pendingTicks < DateTime.MinValue.Ticks || pendingTicks > DateTime.MaxValue.Ticks)
                        dateTime = isIncreasing ? DateTime.MaxValue : DateTime.MinValue;
                    else
                        dateTime = dateTime.AddSeconds(change);
                    break;
                case FrequencyType.Minutes:
                    changeTicks = TimeSpan.TicksPerMinute * (long)change;
                    pendingTicks = dateTime.Ticks + changeTicks;

                    if (pendingTicks < DateTime.MinValue.Ticks || pendingTicks > DateTime.MaxValue.Ticks)
                        dateTime = isIncreasing ? DateTime.MaxValue : DateTime.MinValue;
                    else
                        dateTime = dateTime.AddMinutes(change);
                    break;
                case FrequencyType.Hours:
                    changeTicks = TimeSpan.TicksPerHour * (long)change;
                    pendingTicks = dateTime.Ticks + changeTicks;

                    if (pendingTicks < DateTime.MinValue.Ticks || pendingTicks > DateTime.MaxValue.Ticks)
                        dateTime = isIncreasing ? DateTime.MaxValue : DateTime.MinValue;
                    else
                        dateTime = dateTime.AddHours(change);
                    break;
                case FrequencyType.Days:
                    changeTicks = TimeSpan.TicksPerDay * (long)change;
                    pendingTicks = dateTime.Ticks + changeTicks;

                    if (pendingTicks < DateTime.MinValue.Ticks || pendingTicks > DateTime.MaxValue.Ticks)
                        dateTime = isIncreasing ? DateTime.MaxValue : DateTime.MinValue;
                    else
                        dateTime = dateTime.AddDays((int)change);
                    break;
                case FrequencyType.Months:
                    int years = (int)change / 12;
                    int months = (int)change - (years * 12);
                    
                    landingYear = dateTime.Year + (int)years;

                    //Since Min Value is in Jan, and Max Value is in Dec, we just need to validate the year.
                    if ((dateTime.Month + months) > 12)
                        landingYear++;

                    if (landingYear < DateTime.MinValue.Year || landingYear > DateTime.MaxValue.Year)
                    {
                        dateTime = isIncreasing ? DateTime.MaxValue : DateTime.MinValue;
                        break;
                    }

                    dateTime = dateTime.AddMonths((int)change);
                    break;
                case FrequencyType.Years:
                    landingYear = dateTime.Year + (int)change;

                    if (landingYear < DateTime.MinValue.Year || landingYear > DateTime.MaxValue.Year)
                        dateTime = isIncreasing ? DateTime.MaxValue : DateTime.MinValue;
                    else
                        dateTime = dateTime.AddYears((int)change);
                    break;
            }

            return dateTime;
        }

        #endregion //CreateChangeValue

        #endregion //Private

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