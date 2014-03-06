using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;
using System.Windows;
using System.Windows.Media;

namespace Infragistics.Windows.Helpers
{
    
    
    
    
    /// <summary>
    /// Custom <see cref="INameScope"/> that acts as the proxy for the namescope of an ancestor element.
    /// </summary>
    /// <remarks>
    /// <p class="body">The NameScopeProxy is useful when a class needs to use the same namescope of another 
    /// element when the elements are not part of the same tree.</p>
    /// </remarks>
    public abstract class NameScopeProxy : INameScope
    {
        #region Constructor
        /// <summary>
        /// Initializes a new <see cref="NameScopeProxy"/>
        /// </summary>
        protected NameScopeProxy()
        {
        }
        #endregion // Constructor

        #region Properties
        /// <summary>
        /// Returns the element at which the search for the namescope should begin.
        /// </summary>
        protected abstract DependencyObject StartingElement
        {
            get;
        }
        #endregion //Properties

        #region Methods

        #region GetNameScope
        private INameScope GetNameScope()
        {
            DependencyObject element = this.StartingElement;

            while (null != element)
            {
                INameScope nameScope = NameScope.GetNameScope(element);

                if (null != nameScope)
                    return nameScope;

                // AS 6/6/08 BR33512
                // Prefer the logical tree or else we could get the namescope for a template.
                //
                //element = Utilities.GetParent(element);
                element = LogicalTreeHelper.GetParent(element) ?? VisualTreeHelper.GetParent(element);
            }

            return null;
        }
        #endregion // GetNameScope

        #endregion //Methods

        #region INameScope Members

        object System.Windows.Markup.INameScope.FindName(string name)
        {
            INameScope scope = GetNameScope();

            return null != scope ? scope.FindName(name) : null;
        }

        void System.Windows.Markup.INameScope.RegisterName(string name, object scopedElement)
        {
            INameScope scope = GetNameScope();

            if (null != scope)
                scope.RegisterName(name, scopedElement);
        }

        void System.Windows.Markup.INameScope.UnregisterName(string name)
        {
            INameScope scope = GetNameScope();

            if (null != scope)
                scope.UnregisterName(name);
        }

        #endregion //INameScope Members
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