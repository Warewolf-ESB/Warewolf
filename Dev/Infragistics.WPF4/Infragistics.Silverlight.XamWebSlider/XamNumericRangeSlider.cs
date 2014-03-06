using System.Windows.Input;
using Infragistics.Controls.Editors.Primitives;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// A control that provides XamRangeSlider from double type.
    /// </summary>

    
    

    public class XamNumericRangeSlider : XamRangeSlider<double>
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="XamNumericRangeSlider"/> class.
        /// </summary>
        public XamNumericRangeSlider()
        {

            Infragistics.Windows.Utilities.ValidateLicense(typeof(XamNumericRangeSlider), this);


            this.DefaultStyleKey = typeof(XamNumericRangeSlider);
        }
        #endregion //Constructors

        #region Overrides

        #region MaxValue

        /// <summary>
        /// Gets or sets the maximum allowable value for this slider's range.
        /// </summary>
        /// <value>The max value.</value>
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override double MaxValue
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
        #endregion //MaxValue

        #region MinValue

        /// <summary>
        /// Gets or sets the minimum allowable value for this slider's range.
        /// </summary>
        /// <value>The min value.</value>
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override double MinValue
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
        #endregion //MinValue

        #region DoubleToValue
        /// <summary>
        /// Doubles to value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Value from generic type</returns>
        protected override double DoubleToValue(double value)
        {
            return value;
        }
        #endregion //DoubleToValue

        #region ValueToDouble
        /// <summary>
        /// Converts value from specific generic type to double.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>double value</returns>
        protected override double ValueToDouble(double value)
        {
            return value;
        }
        #endregion //ValueToDouble

        #region  GetParameter
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
        #endregion // GetParameter

        #region TickMarks

        /// <summary>
        /// Gets the collection of tick marks.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override ObservableCollection<SliderTickMarks<double>> TickMarks
        {
            get
            {
                return base.TickMarks;
            }
        }

        #endregion

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
        #endregion // SupportsCommand 
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