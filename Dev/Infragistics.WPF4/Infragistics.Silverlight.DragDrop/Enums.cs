namespace Infragistics.DragDrop
{
    #region Public Enums

    #region OperationType
    /// <summary>
    /// Describes the operation that can be performed against the dragged object.
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// Initiated drop operation cannot be completed.
        /// </summary>
        DropNotAllowed,

        /// <summary>
        /// The dragged element can be copied.
        /// </summary>
        Copy,

        /// <summary>
        /// The dragged element can be moved.
        /// </summary>
        Move
    }

    #endregion // OperationType

    #region FindDropTargetMode

    /// <summary>
    /// Describes the way <see cref="DragSource"/> instance finds the appropriate <see cref="DropTarget"/> instance.
    /// </summary>
    public enum FindDropTargetMode
    {
        /// <summary>
        /// In order to be found the <see cref="DropTarget"/> instance just needs to have the same channel as the <see cref="DragSource"/> instance.
        /// </summary>
        TopMostMatchedChannelTarget,

        /// <summary>
        /// In order to be found the <see cref="DropTarget"/> instance has to be attached to top most drop target element.
        /// If there is no match found between <see cref="DropTarget.DropChannels"/> of the top most drop target and <see cref="DragSource.DragChannels"/> of
        /// the drag source then is assumed that there is no valid drop target.
        /// </summary>
        TopMostTargetOnly                   
    }

    #endregion // FindDropTargetMode

    #endregion // Public Enums
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