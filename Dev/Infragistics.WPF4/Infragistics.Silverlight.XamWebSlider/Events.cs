using System;
using Infragistics.Controls.Editors;
using System.Collections.Generic;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// A class listing the information needed during the <see cref="XamSliderBase{T}.ThumbValueChanged"/> event
    /// </summary>
    /// <typeparam name="T">Generic type</typeparam>
    public class ThumbValueChangedEventArgs<T> : CancellableEventArgs
    {
        /// <summary>
        /// Gets the old value of the thumb 
        /// of the thumb
        /// </summary>
        /// <value>The old value.</value>
        public T OldValue { get; internal set; }

        /// <summary>
        /// Gets the new value of the thumb.
        /// </summary>
        /// <value>The new value.</value>
        public T NewValue { get; internal set; }

        /// <summary>
        /// Gets the thumb which property is changed.
        /// </summary>
        /// <value>The thumb item.</value>
        public XamSliderThumb<T> Thumb { get; internal set; }
    }

    /// <summary>
    /// A class listing the information needed during the <see cref="XamSliderBase{T}.TrackClick"/> event
    /// </summary>
    /// <typeparam name="T">Generic type</typeparam>
    public class TrackClickEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the value where track is ckicked; 
        /// of the thumb
        /// </summary>
        /// <value>The value.</value>
        public T Value { get; internal set; }        
    }

    /// <summary>
    /// A class listing the information needed during the <see cref="XamRangeSlider{T}.TrackFillDragCompleted"/> event
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TrackFillChangedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// The Thumb with the lesser value on the TrackFill Edge
        /// </summary>
        public XamSliderThumb<T> LesserThumb {get; internal set;}

        /// <summary>
        /// The Thumb with the greater value on the TrackFill Edge
        /// </summary>
        public XamSliderThumb<T> GreaterThumb { get; internal set; }
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