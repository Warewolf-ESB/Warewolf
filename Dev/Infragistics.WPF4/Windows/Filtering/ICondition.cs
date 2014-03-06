using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Infragistics.Windows.Controls
{
    #region ICondition Interface

    /// <summary>
    /// Provides common interface for testing a value to see if it matches a condition.
    /// </summary>
    /// <remarks>
    /// <para class="body">
    /// <b>ICondition</b> is an interface that various condition classes implement. It provides
    /// standard interface to check if a value matches a condition. Classes that implement this
    /// interface include <see cref="ComplementCondition"/>, <see cref="ConditionGroup"/> and
    /// <see cref="ComparisonCondition"/>.
    /// </para>
    /// <para class="body">
    /// Also see <see cref="SpecialFilterOperandBase"/> which is used along with <see cref="ComparisonCondition"/>
    /// to specify common useful conditions. Various SpecialOperandBase implementations are provided by
    /// the <see cref="SpecialFilterOperands"/> class via static properties (like <see cref="SpecialFilterOperands.Blanks"/>,
    /// <see cref="SpecialFilterOperands.NonBlanks"/> etc...).
    /// </para>
    /// </remarks>
    /// <seealso cref="ComparisonCondition"/>
    /// <seealso cref="ComplementCondition"/>
    /// <seealso cref="ConditionGroup"/>
    public interface ICondition : ICloneable
    {
        /// <summary>
        /// Returns true if the specified value matches the condition. False otherwise.
        /// </summary>
        /// <param name="value">Value to test.</param>
        /// <param name="context">Context information on where the value came from.</param>
        /// <returns>True if the value passes the condition, false otherwise.</returns>
        bool IsMatch( object value, ConditionEvaluationContext context );
    }

    #endregion // ICondition Interface
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