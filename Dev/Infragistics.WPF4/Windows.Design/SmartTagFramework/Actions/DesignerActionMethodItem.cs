using System;
using System.Reflection;
using System.Windows.Input;
using Infragistics.Windows.Design.SmartTagFramework;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Interaction;
using Microsoft.Windows.Design.Model;

namespace Infragistics.Windows.Design.SmartTagFramework
{
    /// <summary>
    /// Represents a panel item that is associated with a method in a class. 
    /// </summary>
    public class DesignerActionMethodItem : DesignerActionItem
    {
        #region Member Variables

        private DesignerActionList _actionList;
        private MethodInfo _methodInfo;
        private ModelItem _modelItem;
        private EditingContext _context;
        private bool _includeAsDesignerVerb;
        private MenuGroup _contextMenuGroup;
        private RoutedCommand _command = SmartTagFrameworkCommands.ExecuteDesignerActionMethodItemCommand;

		private object [] _parameters;

        #endregion //Member Variables

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the DesignerActionMethodItem class. 
        /// </summary>
        /// <param name="actionList">An ActionList object which contains the method.</param>
        /// <param name="methodName">Method name</param>
        /// <param name="displayName">The panel text for this item</param>       
        public DesignerActionMethodItem(DesignerActionList actionList, string methodName, string displayName)
            : base(displayName)
        {
            this._actionList = actionList;
            this._methodInfo = Utils.MethodHelper(actionList.GetType(), methodName);
            this._includeAsDesignerVerb = false;
        }

        /// <summary>
        /// Initializes a new instance of the DesignerActionMethodItem class. 
        /// </summary>
        /// <param name="actionList">An ActionList object which contains the method.</param>
        /// <param name="methodName">Method name</param>
        /// <param name="displayName">The panel text for this item</param>
        /// <param name="description">Property's description. If a description is specified, an icon pops up.</param>      
        public DesignerActionMethodItem(DesignerActionList actionList, string methodName, string displayName, string description)
            : base(displayName, description)
        {
            this._actionList = actionList;
            this._methodInfo = Utils.MethodHelper(actionList.GetType(), methodName);
            this._includeAsDesignerVerb = false;
        }

        /// <summary>
        /// Initializes a new instance of the DesignerActionMethodItem class. 
        /// </summary>
        /// <param name="actionList">An ActionList object which contains the method.</param>
        /// <param name="methodName">Method name</param>
        /// <param name="displayName">The panel text for this item</param>
        /// <param name="description">Property's description. If a description is specified, an icon pops up.</param>
        /// <param name="designerActionItemGroup">An object that defines the groupings of panel entries.</param>
        /// <param name="orderNumber">Specifies the order in a group</param>       
        public DesignerActionMethodItem(DesignerActionList actionList, string methodName, string displayName, string description, DesignerActionItemGroup designerActionItemGroup, int orderNumber)
            : base(displayName, description, designerActionItemGroup, orderNumber)
        {
            this._methodInfo = Utils.MethodHelper(actionList.GetType(), methodName);
            this._actionList = actionList;
            this._includeAsDesignerVerb = false;
        }

		/// <summary>
		/// Initializes a new instance of the DesignerActionMethodItem class. 
		/// </summary>
		/// <param name="actionList">An ActionList object which contains the method.</param>
		/// <param name="methodName">Method name</param>
		/// <param name="displayName">The panel text for this item</param>
		/// <param name="description">Property's description. If a description is specified, an icon pops up.</param>
		/// <param name="designerActionItemGroup">An object that defines the groupings of panel entries.</param>
		/// <param name="orderNumber">Specifies the order in a group</param>       
		/// <param name="parameters">Parameters that should be passed to the method when it is invoked</param>       
		public DesignerActionMethodItem(DesignerActionList actionList, string methodName, string displayName, string description, DesignerActionItemGroup designerActionItemGroup, int orderNumber, object[] parameters)
			: base(displayName, description, designerActionItemGroup, orderNumber)
		{
			this._methodInfo = Utils.MethodHelper(actionList.GetType(), methodName);
			this._actionList = actionList;
			this._includeAsDesignerVerb = false;
			this._parameters = parameters;
		}

        /// <summary>
        /// Initializes a new instance of the DesignerActionMethodItem class. 
        /// </summary>
        /// <param name="actionList">An ActionList object which contains the method.</param>
        /// <param name="methodName">Method name</param>
        /// <param name="displayName">The panel text for this item</param>
        /// <param name="description">Property's description. If a description is specified, an icon pops up.</param>
        /// <param name="designerActionItemGroup">An object that defines the groupings of panel entries.</param>
        /// <param name="orderNumber">Specifies the order in a group</param>
		/// <param name="parameters">Parameters that should be passed to the method when it is invoked</param>       
		/// <param name="includeAsDesignerVerb">If it is true, the method will be added to the context menu of the control.</param>
        /// <param name="contextMenuGroup">A MenuGroup object that defines the groupings of method entries in the context menu.</param>
		public DesignerActionMethodItem(DesignerActionList actionList, string methodName, string displayName, string description, DesignerActionItemGroup designerActionItemGroup, int orderNumber, object[] parameters, bool includeAsDesignerVerb, MenuGroup contextMenuGroup)
            : base(displayName, description, designerActionItemGroup, orderNumber)
        {
            this._actionList = actionList;
            this._methodInfo = Utils.MethodHelper(actionList.GetType(), methodName);
            this._includeAsDesignerVerb = includeAsDesignerVerb;
			this._parameters = parameters;
			this._contextMenuGroup = contextMenuGroup;

            MenuAction menuAction = new MenuAction(this.DisplayName);
            menuAction.Name = this.MethodInfo.Name;
            contextMenuGroup.Items.Add(menuAction);
        }

        #endregion //Constructors

        #region Mehods

        #region Public Methods

        #region Invoke

        /// <summary>
        /// Invokes the method or constructor represented by the current instance.
        /// </summary>
        public virtual void Invoke()
        {
            if (null == _modelItem)
            {
                throw new Exception("ModelItem is not specified!");
            }

            object[] args = this._parameters != null ? new object[] { this._context, this._modelItem, this._actionList, this._parameters } :
													   new object[] { this._context, this._modelItem, this._actionList };
            this._methodInfo.Invoke(null, args);
        }

        #endregion //Invoke

        #endregion //Public Methods

        #region Internal Methods

        #region Invoke

        internal void Invoke(object sender, EventArgs args)
        {
            this.Invoke();
        }

        #endregion //Invoke

        #endregion //Internal Methods

        #endregion //Mehods

        #region Properties

        #region Public Properties

        #region ContextMenuGroup

        /// <summary>
        /// A MenuGroup object that defines the groupings of method entries in the context menu.
        /// </summary>
        public MenuGroup ContextMenuGroup
        {
            get
            {
                return this._contextMenuGroup;
            }

            set
            {
                this._contextMenuGroup = value;
            }
        }

        #endregion //ContextMenuGroup

        #region Context

        /// <summary>
        /// EditingContext of the control
        /// </summary>
        public EditingContext Context
        {
            get
            {
                return this._context;
            }

            set
            {
                this._context = value;
            }
        }

        #endregion //Context

        #region Command

        /// <summary>
        /// A RoutedCommand associated with the method
        /// </summary>
        public RoutedCommand Command
        {
            get
            {
                return this._command;
            }

            set
            {
                this._command = value;
            }
        }
        #endregion //Command

        #region IncludeAsDesignerVerb

        /// <summary>
        /// If it is true, the method will be added to the context menu of the control.
        /// </summary>
        public bool IncludeAsDesignerVerb
        {
            get
            {
                return this._includeAsDesignerVerb;
            }

            set
            {
                this._includeAsDesignerVerb = value;
            }
        }

        #endregion //IncludeAsDesignerVerb

        #region MethodInfo

        /// <summary>
        /// A MethodInfo associated with the method.
        /// </summary>
        public MethodInfo MethodInfo
        {
            get
            {
                return this._methodInfo;
            }

            set
            {
                this._methodInfo = value;
            }
        }

        #endregion //MethodInfo

        #region ModelItem

        /// <summary>
        /// A ModelItem of the control
        /// </summary>
        public ModelItem ModelItem
        {
            get
            {
                return this._modelItem;
            }

            set
            {
                this._modelItem = value;
            }
        }

        #endregion //ModelItem

        #endregion //Public Properties

        #endregion //Properties
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