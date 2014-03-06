using System;
using System.Windows;
using Microsoft.Windows.Design.Interaction;

namespace Infragistics.Windows.Design.SmartTagFramework
{
    /// <summary>
    /// A base class used for creating a context menu in design time
    /// </summary>
    public abstract class GenericContextMenuProvider : PrimarySelectionContextMenuProvider
    {
        #region Member Variables

        private DesignerActionList				_customDesingerActionList;

        #endregion //Member Variables

        #region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
        public GenericContextMenuProvider()
        {
            // Called right before this provider shows its tabs, opportunity to set states
            UpdateItemStatus += new EventHandler<MenuActionEventArgs>(CustomContextMenuProvider_UpdateItemStatus);
        }

        #endregion //Constructors

        #region Methods

        #region Public Methods

        #region GetDesignerActionListTypeFromControlType

        /// <summary>
        /// An abstract method wich is used for association between predefined DesignerActionList and an Infragistics control
        /// </summary>
        /// <param name="controlType">The control's type</param>
        /// <returns>Predefined DesignerActionList type</returns>
        public abstract Type GetDesignerActionListTypeFromControlType(Type controlType);

        #endregion //GetDesignerActionListTypeFromControlType

        #region GetCustomActionList

        /// <summary>
        /// Returns predefined DesignerActionList
        /// </summary>
        /// <param name="e">MenuActionEventArgs</param>
        /// <returns>Predefined DesignerActionList</returns>
        public DesignerActionList GetCustomActionList(MenuActionEventArgs e)
        {
            if (null == this._customDesingerActionList)
            {
                Type designerActionListType = this.GetDesignerActionListTypeFromControlType(e.Selection.PrimarySelection.ItemType);
                if (designerActionListType != null)
                {
                    object[] args = new object[] { e.Context, e.Selection.PrimarySelection, null, string.Empty };
                    _customDesingerActionList = (DesignerActionList)Activator.CreateInstance(designerActionListType, args);
                }
                else
                {
                    MessageBox.Show(string.Format(DesignResources.LST_SmartTagFramework_GenericContextMenuProvider_GetCustomActionListMSG, e.Selection.PrimarySelection.ItemType.Name));
                }
            }
            return _customDesingerActionList;
        }

        #endregion //GetCustomActionList

        #endregion //Public Methods

        #region Private Methods

        private void CustomContextMenuProvider_UpdateItemStatus(object sender, MenuActionEventArgs e)
        {
            this.Items.Clear();
            DesignerActionList designerActionList = GetCustomActionList(e);
            MenuGroup rootMenuGroup = designerActionList.RootContextMenuGroup;
            this.Items.Add(rootMenuGroup);

            RecursiveEventHandlerCreator(rootMenuGroup);
        }

        private void RecursiveEventHandlerCreator(MenuGroup menuGroup)
        {
            foreach (MenuBase menuBase in menuGroup.Items)
            {
                if (menuBase is MenuAction)
                {
                    MenuAction menuAction = menuBase as MenuAction;
                    menuAction.Execute += new EventHandler<MenuActionEventArgs>(GenericAction_Execute);
                }
                else
                {
                    RecursiveEventHandlerCreator(menuBase as MenuGroup);
                }
            }
        }

        private void GenericAction_Execute(object sender, MenuActionEventArgs e)
        {
            MenuAction clickedMenuAction = sender as MenuAction;

            DesignerActionList designerActionList = GetCustomActionList(e);
            foreach (DesignerActionMethodItem methodItem in designerActionList.MethodItems)
            {
                if (methodItem.MethodInfo.Name.Equals(clickedMenuAction.Name))
                {
                    methodItem.Invoke();
                    break;
                }
            }
        }

        #endregion //Private Methods

        #endregion //Methods
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