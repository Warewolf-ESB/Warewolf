using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Diagnostics;
//using System.Windows.Events;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Data;
using System.Windows.Markup;

namespace Infragistics.Windows.Helpers
{
    /// <summary>
    /// Abstract base class used to supply styles dynamically that can be triggered lazily off the Style 'get'
    /// </summary>
    public abstract class StyleSelectorHelperBase
    {
		private DependencyObject _target;

		/// <summary>
		/// Initializes a new instance of the <see cref="StyleSelectorHelperBase"/> class
		/// </summary>
		/// <param name="target">The target FrameworkElement or FrameworkContentElement</param>
		/// <exception cref="NotSupportedException">Will be raised if the target is not a FrameworkElement or a FrameworkContentElement.</exception>
		protected StyleSelectorHelperBase(DependencyObject target)
		{
			this._target = target;

			if (!(target is FrameworkContentElement) &&
				!(target is FrameworkElement))
				throw new NotSupportedException(SR.GetString("LE_NotSupportedException_3"));
		}

        /// <summary>
        /// The style to be used as the source of a binding (read-only)
        /// </summary>
        public abstract Style Style {get; }

        /// <summary>
        /// Notifies listeners that the style has changed
        /// </summary>
        public void InvalidateStyle()
        {
			Style style = this.Style;

			FrameworkContentElement fce = this._target as FrameworkContentElement;
			FrameworkElement fe = this._target as FrameworkElement;

			if (style == null)
			{
				if (fe != null)
					fe.ClearValue(FrameworkElement.StyleProperty);
				else
					fce.ClearValue(FrameworkContentElement.StyleProperty);
			}
			else
			{
				if (fe != null)
					fe.Style = style;
				else
					fce.Style = style;
			}
        }
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