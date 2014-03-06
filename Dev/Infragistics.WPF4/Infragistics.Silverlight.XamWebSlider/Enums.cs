namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// An Enum that describes the type of the frequency for DateTimeSLiderTickMarks. 
    /// </summary>
    public enum FrequencyType
    {
        /// <summary>
        /// Value of the frequency will be in seconds
        /// </summary>
        Seconds = 0,

        /// <summary>
        /// Value of the frequency will be in minutes
        /// </summary>
        Minutes = 1,

        /// <summary>
        /// Value of the frequency will be in hours
        /// </summary>
        Hours = 2,

        /// <summary>
        /// Value of the frequency will be in days
        /// </summary>
        Days = 3,

        /// <summary>
        /// Value of the frequency will be in months
        /// </summary>
        Months = 4,

        /// <summary>
        /// Value of the frequency will be in years
        /// </summary>
        Years = 5
    }

    /// <summary>
    /// An Enum that describes the behavior after click on the slider track. 
    /// </summary>
    public enum SliderTrackClickAction
    {
        /// <summary>
        /// There is no action after click on the slider track
        /// </summary>
        None = 0,

        /// <summary>
        /// The value of the ActiveThumb will be changed with the LargeChange value, 
        /// depending of the side where is click relative to the thumb position
        /// </summary>
        LargeChange = 1,

        /// <summary>
        /// ActiveThumb will be moved to the clicked position. 
        /// It value will be changed, relative to clicked position 
        /// and slider size
        /// </summary>
        MoveToPoint = 2
    }

    /// <summary>
    /// An Enum that describes the interaction between sliders 
    /// </summary>
    public enum SliderThumbInteractionMode
    {
        /// <summary>
        /// When the locked thumb is not active and active thumb interacts with 
        /// the locked thumb it is not possible to pass through this thumb
        /// </summary>
        Lock,

        /// <summary>
        /// When the thumb in push state is not active and active thumb interacts 
        /// with it the position of the thumb in push mode will be changed to position 
        /// of the active thumb
        /// </summary>
        Push,

        /// <summary>
        /// When the thumb in free state is not active and active thumb interacts 
        /// with it there is no special interaction between both thumbs.
        /// </summary>
        Free
    }

    /// <summary>
    /// An Enum that describes the type of the XamSliderBaseCommandBase command
    /// </summary>
    public enum XamSliderBaseCommand
    {
        /// <summary>
        /// Enum value for SmallDecreaseCommand
        /// </summary>
        SmallDecrease = 0,

        /// <summary>
        /// Enum value for SmallIncreaseCommand
        /// </summary>
        SmallIncrease = 1,

        /// <summary>
        /// Enum value for LargeDecreaseCommand
        /// </summary>
        LargeDecrease = 2,

        /// <summary>
        /// Enum value for LargeIncreaseCommand
        /// </summary>
        LargeIncrease = 3
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