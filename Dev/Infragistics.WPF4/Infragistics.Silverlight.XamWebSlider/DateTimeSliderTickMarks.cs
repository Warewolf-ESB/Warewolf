using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    ///  class that provides SliderTickMarks from DateType type. 
    /// </summary>
    public class DateTimeSliderTickMarks : SliderTickMarks<DateTime>
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeSliderTickMarks"/> class.
        /// </summary>
        public DateTimeSliderTickMarks()
        {
            this.TickMarksFrequency = 1;
        }
        #endregion // Constructor

        #region FrequencyType

        /// <summary>
        /// Identifies the <see cref="FrequencyType"/> dependency property. 
        /// </summary>
        //Bug 33709 fix - added default value for FrequencyType = FrequencyType.Days - Mihail Mateev - 06/08/2010
        public static readonly DependencyProperty FrequencyTypeProperty = DependencyProperty.Register("FrequencyType", typeof(FrequencyType), typeof(DateTimeSliderTickMarks), new PropertyMetadata(FrequencyType.Days, new PropertyChangedCallback(FrequencyTypeChanged)));

        /// <summary>
        /// Gets or sets the type of the 
        /// <see cref="FrequencyType"/>
        /// from <see cref="Infragistics.Controls.Editors.FrequencyType"/> 
        /// enum type.
        /// </summary>
        /// <value>The type of the frequency.</value>
        public FrequencyType FrequencyType
        {
            get { return (FrequencyType)this.GetValue(FrequencyTypeProperty); }
            set { this.SetValue(FrequencyTypeProperty, value); }
        }

        private static void FrequencyTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            DateTimeSliderTickMarks tickmarks = obj as DateTimeSliderTickMarks;
            if (tickmarks != null)
            {
                tickmarks.OnChangeSliderTickMarks();
            }
        }

        #endregion // FrequencyType

        #region Overrides

        /// <summary>
        /// Gets the frequency value in double.
        /// </summary>
        /// <returns>value in double</returns>
        protected override double GetFrequencyValue()
        {
            if (this.Owner != null)
            {
                if (this.TickMarksFrequency == 0)
                {
                    this.TickMarksFrequency = 1;
                }

                return this.TickMarksFrequency;
            }

            return base.GetFrequencyValue();
        }

        /// <summary>
        /// Generates the values of the  tick marks, 
        /// based on values of the slider that is owner of the 
        /// <see cref="SliderTickMarks&lt;T&gt;"/> and values 
        /// of the Frequency, NumberOfTickMarks and UseFrequency
        /// properties
        /// </summary>
        /// <returns>ObservableCollection with values of the tick marks</returns>
        protected override ObservableCollection<DateTime> GenerateTickMarksValues()
        {
            ObservableCollection<DateTime> values = new ObservableCollection<DateTime>();
            if (this.Owner != null)
            {
                if (this.TickMarksValues.Count != 0)
                {
                    return this.TickMarksValues;
                }
                
                DateTime minValue = this.Owner.MinValue;
                DateTime maxValue = this.Owner.MaxValue;
                FrequencyType frequencyType = this.FrequencyType;
                int frequency = (int)this.GetFrequencyValue();
                XamDateTimeSlider dateTimeSlider = this.Owner as XamDateTimeSlider;
                XamDateTimeRangeSlider dateTimeRangeSlider = this.Owner as XamDateTimeRangeSlider;

                if (this.UseFrequency)
                {
                    if (frequency <= 0)
                    {
                        if (dateTimeRangeSlider != null)
                        {
                            frequency = (int)this.TickMarksFrequency;
                        }

                        if (dateTimeSlider != null)
                        {
                            frequency = (int)dateTimeSlider.SmallChange;
                        }
                    }

                    if (frequency <= 0)
                        frequency = 1;

                    switch (frequencyType)
                    {
                        case (FrequencyType.Days):
                            {
                                long count = (long)(maxValue - minValue).TotalDays;
                                int step = GetStepSize(count);
                                frequency = frequency > step ? frequency : step;

                                for (DateTime dt = minValue.AddDays(frequency); dt < maxValue; dt = dt.AddDays(frequency))
                                {
                                    values.Add(dt);
                                }
                                break;
                            }
                        case (FrequencyType.Hours):
                            {
                                long count = (long)(maxValue - minValue).TotalHours;
                                int step = GetStepSize(count);
                                frequency = frequency > step ? frequency : step;

                                for (DateTime dt = minValue.AddHours(frequency); dt < maxValue; dt = dt.AddHours(frequency))
                                {
                                    values.Add(dt);
                                }
                                break;
                            }
                        case (FrequencyType.Minutes):
                            {
                                long count = (long)(maxValue - minValue).TotalMinutes;
                                int step = GetStepSize(count);
                                frequency = frequency > step ? frequency : step;

                                for (DateTime dt = minValue.AddMinutes(frequency); dt < maxValue; dt = dt.AddMinutes(frequency))
                                {
                                    values.Add(dt);
                                }
                                break;
                            }
                        case (FrequencyType.Months):
                            {
                                for (DateTime dt = minValue.AddMonths(frequency); dt < maxValue; dt = dt.AddMonths(frequency))
                                {
                                    values.Add(dt);
                                }
                                break;
                            }
                        case (FrequencyType.Seconds):
                            {
                                long count = (long)(maxValue - minValue).TotalSeconds;
                                int step = GetStepSize(count);
                                frequency = frequency > step ? frequency : step;

                                for (DateTime dt = minValue.AddSeconds(frequency); dt < maxValue; dt = dt.AddSeconds(frequency))
                                {
                                    values.Add(dt);
                                }
                                break;
                            }
                        case (FrequencyType.Years):
                            {
                                for (DateTime dt = minValue.AddYears(frequency); dt < maxValue; dt = dt.AddYears(frequency))
                                {
                                    values.Add(dt);
                                }
                                break;
                            }
                    }
                }
                else
                {
                    



                    TimeSpan ts = maxValue - minValue;
                    long ticksPerRange = ts.Ticks / (this.NumberOfTickMarks + 1);

                    DateTime workingDate = minValue;
                    for (int i = 0; i < this.NumberOfTickMarks && workingDate < maxValue; i++)
                    {
                        workingDate = workingDate.Add(new TimeSpan(ticksPerRange));
                        values.Add(workingDate);
                    }
                }

                if (this.IncludeSliderEnds)
                {
                    values.Insert(0, minValue);
                    values.Add(maxValue);
                }
            }

            return values;
        }

        #region TickMarksValues

        /// <summary>
        /// Gets the collection of Tick Mark Values of Type DateTime that are used to set the slider.
        /// </summary>
        /// <value>The tick marks.</value>

        [Browsable(false)]

        public override ObservableCollection<DateTime> TickMarksValues
        {
            get
            {
                return base.TickMarksValues;
            }
        }

        #endregion //TickMarksValues

        #endregion Overrides

        #region Private

        /// <summary>
        /// Takes a look at the available visual space and determines the max number of tick marks to display, and returns the number of tick marks to skip between displayed tick marks
        /// </summary>
        /// <param name="countTickMarks">The number of Tick Marks we calculated</param>
        /// <returns>The number of tick marks to skip between displayed tick marks</returns>
        private int GetStepSize(long countTickMarks)
        {
            double trackSize = 0;

            if ((this.Owner.Orientation == Orientation.Horizontal) && (this.Owner.HorizontalTrack != null))
                trackSize = this.Owner.HorizontalTrack.ActualWidth;
            else if (this.Owner.VerticalTrack != null)
                trackSize = this.Owner.VerticalTrack.ActualHeight;
            
            if (trackSize > countTickMarks)
                return 1;

            return (int)(countTickMarks / trackSize);
        }

        #endregion
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