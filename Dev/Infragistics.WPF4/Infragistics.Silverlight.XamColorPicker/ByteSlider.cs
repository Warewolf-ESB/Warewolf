using System;
using System.ComponentModel;
using System.Windows;

namespace Infragistics.Controls.Editors.Primitives
{
    /// <summary>
    /// A derived <see cref="Infragistics.Controls.Editors.XamSliderBase"/> for use on <see cref="Byte"/> data.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ByteSlider : XamSimpleSliderBase<Byte>
    {
        #region Constructor


        /// <summary>
        /// Static constructor for the <see cref="ByteSlider"/> class.
        /// </summary>
        static ByteSlider()
		{
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ByteSlider), new FrameworkPropertyMetadata(typeof(ByteSlider)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteSlider"/> class.
        /// </summary>
        public ByteSlider()
        {



            this.Thumb = new ByteSliderThumb();
        }

        #endregion // Constructor

        #region Overrides

        #region DoubleToValue
        /// <summary>
        ///  Converts value double type to specific genetic type.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override byte DoubleToValue(double value)
        {
            if (value < 0)
                value = 0;
            if (value > 255)
                value = 255;
            return Convert.ToByte(value);
        }
        #endregion // DoubleToValue

        #region ValueToDouble

        /// <summary>
        /// Converts value from specific genetic type to double.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override double ValueToDouble(byte value)
        {

            return Convert.ToDouble(value);
        }

        #endregion // ValueToDouble

        #region MaxValue

        /// <summary>
        /// Gets or sets the value of the MaxValue property.
        /// </summary>
        /// <value>The max value.</value>

        public override Byte MaxValue
        {
            get
            {
                return 255;
            }

            set
            {
                base.MaxValue = value;
            }
        }
        #endregion //MaxValue

        #region MinValue

        /// <summary>
        /// Gets or sets the min value.
        /// </summary>
        /// <value>The min value.</value>
        public override Byte MinValue
        {
            get
            {
                return 0;
            }

            set
            {
                base.MinValue = value;
            }
        }

        #endregion //MinValue

        #region GetLargeChangeValue
        /// <summary>
        /// Gets the LargeChange value in double.
        /// </summary>
        /// <returns></returns>
        protected override double GetLargeChangeValue()
        {
            return 5;
        }
        #endregion // GetLargeChangeValue

        #region GetSmallChangeValue
        /// <summary>
        /// Gets the SmallChange value in double.
        /// </summary>
        /// <returns></returns>
        protected override double GetSmallChangeValue()
        {
            return 1;
        }
        #endregion // GetSmallChangeValue

        #endregion // Overrides
    }

    /// <summary>
    /// A control that provides XamSliderThumb from double type. 
    /// </summary>
    public class ByteSliderThumb : XamSliderThumb<Byte>
    {
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