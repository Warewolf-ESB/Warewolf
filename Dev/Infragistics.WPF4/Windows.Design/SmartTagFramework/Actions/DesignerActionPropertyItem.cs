using System;
using Microsoft.Windows.Design;
using System.ComponentModel;

namespace Infragistics.Windows.Design.SmartTagFramework
{
    /// <summary>
    /// Represents a panel item that is associated with a property in a class. This class cannot be inherited. 
    /// </summary>
    public sealed class DesignerActionPropertyItem : DesignerActionItem
    {
        #region Member Variables

        private bool			_isReadOnly = false;
        private string			_name;
        private Type			_propertyType;
		private Type			_owningType;
		private object			_value;
        private EditingContext	_context;
		private string			_descriptionOverride;

        #endregion //Member Variables

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the DesignerActionPropertyItem class. 
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="displayName">The panel text for this item</param>       
		/// <param name="owningType">The Type of the instance that contains this property</param>       
		public DesignerActionPropertyItem(string propertyName, string displayName, Type owningType)
            : base(displayName)
        {
            this._name						= propertyName;
			this._owningType				= owningType;
            this.DesignerActionItemGroup	= new DesignerActionItemGroup();
        }

        /// <summary>
        /// Initializes a new instance of the DesignerActionPropertyItem class. 
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="displayName">The panel text for this item</param>
		/// <param name="owningType">The Type of the instance that contains this property</param>       
		/// <param name="designerActionItemGroup">An object that defines the groupings of panel entries.</param>             
		public DesignerActionPropertyItem(string propertyName, string displayName, Type owningType, DesignerActionItemGroup designerActionItemGroup)
			: base(displayName, string.Empty, designerActionItemGroup)
        {
            this._name				= propertyName;
			this._owningType		= owningType;
		}

        /// <summary>
        /// Initializes a new instance of the DesignerActionPropertyItem class. 
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="displayName">The panel text for this item</param>
		/// <param name="owningType">The Type of the instance that contains this property</param>       
		/// <param name="designerActionItemGroup">An object that defines the groupings of panel entries.</param>
        /// <param name="orderNumber">Specifies the order in a group</param>        
		public DesignerActionPropertyItem(string propertyName, string displayName, Type owningType, DesignerActionItemGroup designerActionItemGroup, int orderNumber)
			: base(displayName, string.Empty, designerActionItemGroup, orderNumber)
        {
            this._name				= propertyName;
			this._owningType		= owningType;
		}

        /// <summary>
        /// Initializes a new instance of the DesignerActionPropertyItem class. 
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="displayName">The panel text for this item</param>
		/// <param name="owningType">The Type of the instance that contains this property</param>       
		/// <param name="designerActionItemGroup">An object that defines the groupings of panel entries.</param>
        /// <param name="orderNumber">Specifies the order in a group</param>
        /// <param name="isReadOnly">If it is true, the value's editor will be disabled.</param>        
		public DesignerActionPropertyItem(string propertyName, string displayName, Type owningType, DesignerActionItemGroup designerActionItemGroup, int orderNumber, bool isReadOnly)
			: base(displayName, string.Empty, designerActionItemGroup, orderNumber)
        {
            this._name				= propertyName;
			this._owningType		= owningType;
			this._isReadOnly		= isReadOnly;
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

        #region IsReadOnly

        /// <summary>
        ///Gets or sets a value indicating whether this element can be changed
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return _isReadOnly;
            }

            set
            {
                _isReadOnly = value;
            }
        }

        #endregion //IsReadOnly

        #region IsWriteable

        /// <summary>
        /// /// <summary>
        ///Gets a value indicating whether this element can be changed
        /// </summary>
        /// </summary>
        public bool IsWriteable
        {
            get
            {
                return !IsReadOnly;
            }
        }

        #endregion //IsWriteable

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
            get
            {
                return _propertyType;
            }

            set
            {
                _propertyType = value;
            }
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

		#region Internal Properties

			#region OwningType

		internal Type OwningType
		{
			get { return this._owningType; }
		}

			#endregion //OwningType

		#endregion //Internal Properties

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