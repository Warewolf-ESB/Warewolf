using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Model;

namespace Infragistics.Windows.Design.DockManager
{
    /// <summary>
    /// Provides default values for various TabGroupPane properties.
    /// </summary>
    public class TabGroupPaneDefaultInitializer : DefaultInitializer
    {
        /// <summary>
        /// Initializes various defaults for the element wrapped by the specified Modelitem.
        /// </summary>
        /// <param name="item">The ModeItem that wraps the element whose defaults are to be set.</param>
        public override void InitializeDefaults(ModelItem item)
        {
            // Remove any TabItems added by the TabControl DefaultInitializer.
            item.Properties["Items"].Collection.Clear();

			// AS 5/11/09 TFS15478
			// JM 10-28-08 TFS9626, TFS9627 - In 3.5 SP1 the InitializeDefaults method on the base class (DefaultInitializer) is declared as virtual
			// so the following statement which calls the base works fine.  However, in 3.5 original (i.e. non-SP1) the InitializeDefaults method
			// is declared as abstract.  Since we are compiling our projects under SP1 the following line compiles correctly however when the following
			// line executes in an environment that only has 3.5 non-SP1 installed, a 'Bad IL Format' exception is generated.
			//base.InitializeDefaults(item);
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