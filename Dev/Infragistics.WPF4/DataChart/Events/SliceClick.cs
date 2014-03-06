using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Contains PieChart click event data.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SliceClickEventArgs : EventArgs
    {



        internal SliceClickEventArgs(Slice slice)

        {
            Slice = slice;




            if (slice == null) return;

            IsSelected = slice.IsSelected;
            IsExploded = slice.IsExploded;
        }

        /// <summary>
        /// Gets the current slice.
        /// </summary>
        [DontObfuscate]
        internal Slice Slice { get; set; }

        /// <summary>
        /// Gets or sets whether the slice is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                Slice.IsSelected = value;
            }
        }
        private bool _isSelected;

        /// <summary>
        /// Gets or sets whether the slice is exploded.
        /// </summary>
        public bool IsExploded
        {
            get { return _isExploded; }
            set
            {
                _isExploded = value;
                Slice.IsExploded = value;
            }
        }
        private bool _isExploded;

        /// <summary>
        /// Gets whether the current slice is part of the others slice.
        /// </summary>
        public bool IsOthersSlice { get { return Slice.IsOthersSlice; } }

        /// <summary>
        /// Gets the slice data context.
        /// </summary>
        /// <value>The data context.</value>
        public object DataContext
        {
            get
            {
                if (this.Slice != null)
                {
                    return this.Slice.DataContext;
                }

                return null;
            }
        }





    }

    /// <summary>
    /// Event handler for SliceClick event.
    /// </summary>
    /// <param name="sender">Chart control</param>
    /// <param name="e">Slice parameters</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public delegate void SliceClickEventHandler(object sender, SliceClickEventArgs e);

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