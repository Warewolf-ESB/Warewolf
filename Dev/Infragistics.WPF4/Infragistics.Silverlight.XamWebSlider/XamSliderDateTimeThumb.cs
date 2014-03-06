using System;
using System.ComponentModel;


namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// A control that provides XamSliderThumb from DateTime type. 
    /// </summary>
    public class XamSliderDateTimeThumb : XamSliderThumb<DateTime>
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="XamSliderDateTimeThumb"/> class.
        /// </summary>
        public XamSliderDateTimeThumb()
        {
            this.DefaultStyleKey = typeof(XamSliderDateTimeThumb);
        }

        #endregion // Constructor

        #region Overrides

        #region GetDelta
        /// <summary>
        /// Gets the delta, used to coerse the thumb interaction.
        /// </summary>
        /// <returns>delta as double</returns>
        protected override double GetDelta()
        {
            return 1000;
        }
        #endregion //GetDelta

        #region Value

        /// <summary>
        /// Gets or sets the DateTime value of
        /// the Value property.
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
        #endregion //Value

        #region SnapRangeInRangeToTickMarks

        internal override void SnapRangeInRangeToTickMarks(XamSliderThumb<DateTime> thumb)
        {
            double thisValue = this.ResolveValue();
            double thumbValue = thumb.ResolveValue();
            double delta = thisValue - thumbValue;
            thisValue = this.CoerceSnapToTickMarks(thisValue);

            if ((thumb.IsSnapToTickEnabled && thumb.InitialValue.Day == 1)
                && (this.IsSnapToTickEnabled && this.InitialValue.Day == 1)
                && this.InitialValue.Month != thumb.InitialValue.Month)
            {
                DateTime initialDate = this.InitialValue;
                DateTime currDate = this.Owner.ToValue(thisValue);

                var days = 0;

                if (initialDate > currDate)
                {
                    while (initialDate.Month > currDate.Month)
                    {
                        int offset = DateTime.DaysInMonth(initialDate.Year, initialDate.Month) - DateTime.DaysInMonth(initialDate.Year, initialDate.Month - 1);
                        //System.Diagnostics.Debug.WriteLine("Month: {0} - Month: {1} = {2}", initialDate.Month , initialDate.Month - 1, offset);
                        days += offset;
                        initialDate = initialDate.AddMonths(-1);
                    }
                    delta += new TimeSpan(days, 0, 0, 0).Ticks;
                }
                else
                {
                    while (initialDate.Month < currDate.Month)
                    {
                        int offset = DateTime.DaysInMonth(initialDate.Year, initialDate.Month - 1) - DateTime.DaysInMonth(initialDate.Year, initialDate.Month);
                        //System.Diagnostics.Debug.WriteLine("Month: {0} - Month: {1} = {2}", initialDate.Month - 1, initialDate.Month, offset);
                        days += offset;
                        initialDate = initialDate.AddMonths(1);
                    }
                    delta -= new TimeSpan(days, 0, 0, 0).Ticks;
                }
                //System.Diagnostics.Debug.WriteLine("Days: {0}", days);
            }

            thumbValue = thisValue - delta;
            this.Value = this.Owner.ToValue(thisValue);
            //System.Diagnostics.Debug.WriteLine("Thumb Before: {0}", this.Owner.ToValue(thumbValue));
            thumb.Value = this.Owner.ToValue(thumbValue);
            //System.Diagnostics.Debug.WriteLine("Thumb After: {0}", thumb.Value);
        }

        #endregion // SnapRangeInRangeToTickMarks

        #endregion //Overrides
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