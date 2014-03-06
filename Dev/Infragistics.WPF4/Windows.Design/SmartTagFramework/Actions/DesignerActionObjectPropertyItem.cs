using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Model;
using System.Reflection;
using System.ComponentModel;

namespace Infragistics.Windows.Design.SmartTagFramework
{
	/// <summary>
	/// Represents a property of type object. 
	/// </summary>
	public class DesignerActionObjectPropertyItem : DesignerActionItem
	{
		#region Member Variables

		private string				_name;
		private DesignerActionList	_actionList;
		private Type				_actionListType;
		private Type				_propertyType;
		private object				_value;
		private EditingContext		_context;
		private Type				_owningType;
		private ModelItem			_parentModelItem;
		private ModelItem			_modelItem;
		private string				_id;
		private List<string>		_itemsToShow;
		private string				_descriptionOverride;
		private string				_alternateAdornerTitle;

		#endregion //Member Variables

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the DesignerActionObjectPropertyItem class. 
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="displayName">The panel text for this item</param>       
		/// <param name="actionListType">The ActionList Type that should be used to get the actions for this object</param>       
		/// <param name="parentModelItem">The MpdelItem of the instance that contains this object property</param>       
		/// <param name="owningType">The Type of the instance that contains this object property</param>       
		/// <param name="propertyType">The Type of this object property</param>       
		/// <param name="id">An id that uniquely identifies this item.  This would normally be the same as the proeprtyName, but can be different if there are 2 DesignerActionObjectPropertyItems on the same page for the same property.</param>       
		public DesignerActionObjectPropertyItem(string propertyName, string displayName, Type actionListType, ModelItem parentModelItem, Type owningType, Type propertyType, string id)
            : base(displayName)
        {
            this._name						= propertyName;
			this._actionListType			= actionListType;
			this._parentModelItem			= parentModelItem;
			this._owningType				= owningType;
			this._propertyType				= propertyType;
			this._id						= id;
            this.DesignerActionItemGroup	= new DesignerActionItemGroup();
        }

        /// <summary>
        /// Initializes a new instance of the DesignerActionObjectPropertyItem class. 
        /// </summary>
		/// <param name="propertyName">Property name</param>
		/// <param name="displayName">The panel text for this item</param>
		/// <param name="actionListType">The ActionList Type that should be used to get the actions for this object</param>       
		/// <param name="parentModelItem">The MpdelItem of the instance that contains this object property</param>       
		/// <param name="owningType">The Type of the instance that contains this object property</param>       
		/// <param name="propertyType">The Type of this object property</param>       
		/// <param name="id">An id that uniquely identifies this item.  This would normally be the same as the proeprtyName, but can be different if there are 2 DesignerActionObjectPropertyItems on the same page for the same property.</param>       
		/// <param name="designerActionItemGroup">An object that defines the groupings of panel entries.</param>             
		/// <param name="itemsToShow">A list of string identifiers that derived classes can look at to determine which items to include in the list.</param>
		public DesignerActionObjectPropertyItem(string propertyName, string displayName, Type actionListType, ModelItem parentModelItem, Type owningType, Type propertyType, string id, DesignerActionItemGroup designerActionItemGroup, List<string> itemsToShow)
			: base(displayName, string.Empty, designerActionItemGroup)
        {
            this._name				= propertyName;
			this._actionListType	= actionListType;
			this._parentModelItem	= parentModelItem;
			this._owningType		= owningType;
			this._propertyType		= propertyType;
			this._id				= id;
			this._itemsToShow		= itemsToShow;
		}

        /// <summary>
        /// Initializes a new instance of the DesignerActionObjectPropertyItem class. 
        /// </summary>
		/// <param name="propertyName">Property name</param>
		/// <param name="displayName">The panel text for this item</param>
		/// <param name="actionListType">The ActionList Type that should be used to get the actions for this object</param>       
		/// <param name="parentModelItem">The MpdelItem of the instance that contains this object property</param>       
		/// <param name="owningType">The Type of the instance that contains this object property</param>       
		/// <param name="propertyType">The Type of this object property</param>       
		/// <param name="id">An id that uniquely identifies this item.  This would normally be the same as the proeprtyName, but can be different if there are 2 DesignerActionObjectPropertyItems on the same page for the same property.</param>       
		/// <param name="designerActionItemGroup">An object that defines the groupings of panel entries.</param>             
		/// <param name="itemsToShow">A list of string identifiers that derived classes can look at to determine which items to include in the list.</param>
		/// <param name="orderNumber">Specifies the order in a group</param>        
		public DesignerActionObjectPropertyItem(string propertyName, string displayName, Type actionListType, ModelItem parentModelItem, Type owningType, Type propertyType, string id, DesignerActionItemGroup designerActionItemGroup, List<string> itemsToShow, int orderNumber)
			: base(displayName, string.Empty, designerActionItemGroup, orderNumber)
        {
            this._name				= propertyName;
			this._actionListType	= actionListType;
			this._parentModelItem	= parentModelItem;
			this._owningType		= owningType;
			this._propertyType		= propertyType;
			this._id				= id;
			this._itemsToShow		= itemsToShow;
		}

        /// <summary>
        /// Initializes a new instance of the DesignerActionObjectPropertyItem class. 
        /// </summary>
		/// <param name="propertyName">Property name</param>
		/// <param name="displayName">The panel text for this item</param>
		/// <param name="actionListType">The ActionList Type that should be used to get the actions for this object</param>       
		/// <param name="parentModelItem">The MpdelItem of the instance that contains this object property</param>       
		/// <param name="owningType">The Type of the instance that contains this object property</param>       
		/// <param name="propertyType">The Type of this object property</param>       
		/// <param name="id">An id that uniquely identifies this item.  This would normally be the same as the proeprtyName, but can be different if there are 2 DesignerActionObjectPropertyItems on the same page for the same property.</param>       
		/// <param name="designerActionItemGroup">An object that defines the groupings of panel entries.</param>             
		/// <param name="itemsToShow">A list of string identifiers that derived classes can look at to determine which items to include in the list.</param>
		/// <param name="orderNumber">Specifies the order in a group</param>        
		/// <param name="alternateAdornerTitle">Specifies an alternate Title for the GenericAdorner that hosts the DesignerActionList associated with this item, overriding the one specified in the DesignerActionList itself.</param>        
		public DesignerActionObjectPropertyItem(string propertyName, string displayName, Type actionListType, ModelItem parentModelItem, Type owningType, Type propertyType, string id, DesignerActionItemGroup designerActionItemGroup, List<string> itemsToShow, int orderNumber, string alternateAdornerTitle)
			: base(displayName, string.Empty, designerActionItemGroup, orderNumber)
        {
            this._name					= propertyName;
			this._actionListType		= actionListType;
			this._parentModelItem		= parentModelItem;
			this._owningType			= owningType;
			this._propertyType			= propertyType;
			this._id					= id;
			this._itemsToShow			= itemsToShow;
			this._alternateAdornerTitle	= alternateAdornerTitle;
		}

        #endregion //Constructors

		#region Base Class Overrides

			#region GetDescriptionOverride

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected override string GetDescriptionOverride()
		{
			if (string.IsNullOrEmpty(this._descriptionOverride))
				this._descriptionOverride = Utils.GetDescriptionAttributeForProperty(this._owningType, this.Name);

			return this._descriptionOverride;
		}

			#endregion //GetDescriptionOverride

		#endregion //Base Class Overrides 

		#region Properties

			#region Public Properties

				#region ActionList

		/// <summary>
		/// Property ActionList
		/// </summary>
		public DesignerActionList ActionList
		{
			get
			{
				if (this._actionList == null || this._actionList.GetType() != this.ActionListTypeResolved)
				{
					object[]	args	= new object[] { this._context, this.ModelItem, this._itemsToShow, this._alternateAdornerTitle };

					this._actionList	= (DesignerActionList)Activator.CreateInstance(this.ActionListTypeResolved, args);
				}

				return _actionList;
			}

			set
			{
				_actionList = value;
			}
		}

				#endregion //ActionList

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

				#region Id

		/// <summary>
		/// Property Id
		/// </summary>
		public string Id
		{
			get
			{
				return _id;
			}

			set
			{
				_id = value;
			}
		}

				#endregion //Id

				#region ModelItem

		/// <summary>
		/// Property ModelItem
		/// </summary>
		public ModelItem ModelItem
		{
			get
			{
				return _modelItem;
			}

			set
			{
				_modelItem = value;
			}
		}

				#endregion //ModelItem

				#region Name

		/// <summary>
		/// Property name
		/// </summary>
		public string Name
		{
			get
			{
				return _name;
			}

			set
			{
				_name = value;
			}
		}

				#endregion //Name

				#region PropertyType

		/// <summary>
		/// Property type
		/// </summary>
		public Type PropertyType
		{
			get	{ return this.PropertyTypeResolved; }
			set { _propertyType = value; }
		}

				#endregion //PropertyType

				#region Value

		/// <summary>
		/// Property value
		/// </summary>
		public object Value
		{
			get
			{
				return _value;
			}

			set
			{
				_value = value;
				NotifyPropertyChanged("Value");
			}
		}

				#endregion //Value

			#endregion //Public Properties

			#region Private Properties

				#region ActionListTypeResolved

		private Type ActionListTypeResolved
		{
			get 
			{
				Type overrideActionListType = this.GetActionListTypeOverride();

				return overrideActionListType != null ? overrideActionListType : this._actionListType;
			}
		}

				#endregion //ActionListTypeResolved

				#region PropertyTypeResolved

		private Type PropertyTypeResolved
		{
			get 
			{
				Type overridePropertyType = this.GetPropertyTypeOverride();

				return overridePropertyType != null ? overridePropertyType : this._propertyType;
			}
		}

				#endregion //PropertyTypeResolved

			#endregion //Private Properties

			#region Protected Properties

				#region ParentModelItem

		/// <summary>
		/// Returns the Modelitem of the parent.
		/// </summary>
		protected ModelItem ParentModelItem
		{
			get { return this._parentModelItem; }
		}

				#endregion //ParentModelItem

			#endregion //Protected Properies

		#endregion //Properties

		#region Methods

			#region GetActionListTypeOverride

		/// <summary>
		/// A virtual method that allows derived classes to dynamically supply an ActionList type at time of need.  By default, the 
		/// Type passed into the constructor of DesignerActionObjectPropertyItem is returned.
		/// </summary>
		/// <returns></returns>
		protected virtual Type GetActionListTypeOverride()
		{
			return this._actionListType;
		}

			#endregion //GetActionListTypeOverride

			#region GetPropertyTypeOverride

		/// <summary>
		/// A virtual method that allows derived classes to dynamically supply a Property type at time of need.  By default, the 
		/// Type passed into the constructor of DesignerActionObjectPropertyItem is returned.
		/// </summary>
		/// <returns></returns>
		protected virtual Type GetPropertyTypeOverride()
		{
			return this._propertyType;
		}

			#endregion //GetPropertyTypeOverride

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