using System.ComponentModel;

namespace Infragistics.Windows.Design.SmartTagFramework
{
    /// <summary>
    /// Provides the base class for types that represent a panel item on a smart tag panel. 
    /// </summary>
    public abstract class DesignerActionItem : INotifyPropertyChanged
    {
        #region Member Variables

        private DesignerActionItemGroup _designerActionItemGroup;
        private string _description;
        private string _displayName;
        private int _orderNumber = 0;
        private bool _isEnabled = true;

        #endregion //Member Variables

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the DesignerActionItem class.
        /// </summary>
        internal DesignerActionItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the DesignerActionItem class.
        /// </summary>
        /// <param name="displayName">The panel text for this item.</param>        
        public DesignerActionItem(string displayName)
        {
            this._displayName = displayName;
            this._designerActionItemGroup = new DesignerActionItemGroup(); //not in a group
        }

        /// <summary>
        /// Initializes a new instance of the DesignerActionItem class.
        /// </summary>
        /// <param name="displayName">The panel text for this item.</param>
        /// <param name="description">Supplemental text for this item, potentially used in ToolTips.</param>
        public DesignerActionItem(string displayName, string description)
        {
            this._displayName = displayName;
            this._description = description;
        }

        /// <summary>
        /// Initializes a new instance of the DesignerActionItem class.
        /// </summary>
        /// <param name="displayName">The panel text for this item.</param>        
        /// <param name="description">Supplemental text for this item, potentially used in ToolTips.</param>
        /// <param name="designerActionItemGroup">An object that defines the groupings of panel entries.</param>
        public DesignerActionItem(string displayName, string description, DesignerActionItemGroup designerActionItemGroup)
        {
            this._displayName = displayName;
            this._description = description;
            this._designerActionItemGroup = designerActionItemGroup;
        }

        /// <summary>
        /// Initializes a new instance of the DesignerActionItem class.
        /// </summary>
        /// <param name="displayName">The panel text for this item.</param>        
        /// <param name="description">Supplemental text for this item, potentially used in ToolTips.</param>
        /// <param name="designerActionItemGroup">An object that defines the groupings of panel entries.</param>
        /// <param name="orderNumber">Specifies the order in a group</param>
        public DesignerActionItem(string displayName, string description, DesignerActionItemGroup designerActionItemGroup, int orderNumber)
        {
            this._displayName = displayName;
            this._description = description;
            this._designerActionItemGroup = designerActionItemGroup;
            this._orderNumber = orderNumber;
        }

        #endregion //Constructor

        #region Properties

        #region Public Properties

        #region DesignerActionItemGroup

        /// <summary>
        /// Gets,sets the group object for an item.
        /// </summary>
        public DesignerActionItemGroup DesignerActionItemGroup
        {
            get
            {
                return this._designerActionItemGroup;
            }

            set
            {
                this._designerActionItemGroup = value;
            }
        }

        #endregion //DesignerActionItemGroup

        #region Description

        /// <summary>
        /// Gets the supplemental text for the item.
        /// </summary>
        public string Description
        {
            get
            {
				return this.GetDescriptionOverride();
			}
        }

        #endregion //Description

		#region DisplayName

		/// <summary>
        /// Gets, sets the text for this item.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return this._displayName;
            }

            set
            {
                this._displayName = value;
                NotifyPropertyChanged("DisplayName");
            }
        }

        #endregion //DisplayName

        #region HasDescription

        /// <summary>
        /// Gets the description for this item.
        /// </summary>
        public bool HasDescription
        {
            get
            {
                return !string.IsNullOrEmpty(this.Description);
            }
        }

        #endregion //HasDescription

        #region OrderNumber

        /// <summary>
        /// Specifies the order in a group
        /// </summary>
        public int OrderNumber
        {
            get
            {
                return this._orderNumber;
            }

            set
            {
                _orderNumber = value;
            }
        }

        #endregion //OrderNumber

        #region IsEnabled

        /// <summary>
        ///Gets or sets a value indicating whether this element is enabled
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return this._isEnabled;
            }

            set
            {
                this._isEnabled = value;
                NotifyPropertyChanged("IsEnabled");
            }
        }

        #endregion //IsEnabled

        #endregion //Public Properties

        #endregion //Properties

		#region Methods

			#region GetDescriptionOverride

		/// <summary>
		/// Allows derived classes to dynamically provide a description for the DesignerActionItem
		/// </summary>
		/// <returns></returns>
		protected virtual string GetDescriptionOverride()
		{
			return this._description;
		}

			#endregion //GetDescriptionOverride

		#endregion // Methods

		#region Events

		#region Notify Property Changed Members

		/// <summary>
		/// Defines an event that is raised when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Called when a property value changes.
		/// </summary>
		/// <param name="property"></param>
        protected void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion //Notify Property Changed Members

        #endregion //Events
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