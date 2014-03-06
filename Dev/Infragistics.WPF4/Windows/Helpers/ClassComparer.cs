using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Windows.Helpers
{
    // AS 4/21/09 NA 2009.2 ClipboardSupport
    /// <summary>
    /// Base class for a comparer that compares class type objects and performs the necessary null checks before calling the actual comparison routine.
    /// </summary>
    /// <typeparam name="T">A reference type that will be compared</typeparam>
    public abstract class ClassComparer<T> : IComparer<T>
        where T : class
    {
        #region Constructor
        /// <summary>
        /// Initializes a new <see cref="ClassComparer&lt;T&gt;"/>
        /// </summary>
        protected ClassComparer()
        {
        }
        #endregion //Constructor

        #region Methods

        /// <summary>
        /// Performs the actual compare of the two non-null instances.
        /// </summary>
        /// <param name="x">The first object to compare</param>
        /// <param name="y">The second object to compare</param>
        /// <returns>0 if the objects are equal. -1 if the first is less than the second. Otherwise 1 to indicate the second is less than the first.</returns>
        protected abstract int CompareOverride(T x, T y);

        #endregion //Methods

        #region IComparer<T> Members

        /// <summary>
        /// Compares two objects and returns a value indicating if one is greater than, less than or equal to the other.
        /// </summary>
        /// <param name="x">The first object to compare</param>
        /// <param name="y">The second object to compare</param>
        /// <returns>0 if the objects are equal. -1 if the first is less than the second. Otherwise 1 to indicate the second is less than the first.</returns>
        public int Compare(T x, T y)
        {
            if (x == y)
                return 0;
            else if (x == null)
                return -1;
            else if (y == null)
                return 1;

            return this.CompareOverride(x, y);
        }

        #endregion //IComparer<T> Members
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