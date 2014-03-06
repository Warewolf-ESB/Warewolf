using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Windows.Internal
{
    // JJD 2/11/09 - TFS10860/TFS13609
    /// <summary>
    /// An attribute placed on a class or property to determine whether to clone the object when its
    /// owing object is cloned (deep clone) or to set the same instance on the corresponding property of the cloned owner.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CloneBehaviorAttribute : Attribute
    {
        private CloneBehavior _behavior;

        /// <summary>
        /// Creates a new instance of a <b>CloneBehaviorAttribute</b>
        /// </summary>
        /// <param name="behavior">The desired behavior</param>
        public CloneBehaviorAttribute(CloneBehavior behavior)
        {
            this._behavior = behavior;
        }

        /// <summary>
        /// Returns the desired behavior (read-only)
        /// </summary>
        public CloneBehavior Behavior { get { return this._behavior; } }
    }

    /// <summary>
    /// Determines whether or not t perform a deep clone on a sub object property
    /// </summary>
    public enum CloneBehavior
    {
        /// <summary>
        /// Share the same instance between the ordinal owner and irrts clone
        /// </summary>
        ShareInstance = 0,

        /// <summary>
        /// Clone the object so that the original owner and the cloned owner each have separate instances of the object.
        /// </summary>
        CloneObject = 1,
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