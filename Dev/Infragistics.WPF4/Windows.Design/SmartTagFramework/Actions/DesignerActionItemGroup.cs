
namespace Infragistics.Windows.Design.SmartTagFramework
{    
    /// <summary>
    /// Provides a class which is used for DesignerActionItems grouping.
    /// </summary>
    public class DesignerActionItemGroup
    {
        #region Member Variables

        private string _name;
        private int _orderNumber;
        private bool _isExpanded;

        #endregion //Member Variables	
    
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the DesignerActionItemGroup class
        /// </summary>        
        public DesignerActionItemGroup()
        {
            this._name = string.Empty;
            this._orderNumber = -1;
            _isExpanded = false;
            this.IsExpandable = true;
            this.IsHeaderVisible = true;
        }
        
        /// <summary>
        /// Initializes a new instance of the DesignerActionItemGroup class
        /// </summary>
        /// <param name="name">Name of the group</param>
        /// <param name="orderNumber">Order number of the group</param>
        public DesignerActionItemGroup(string name, int orderNumber)
        {
            this._name = name;
            this._orderNumber = orderNumber;
            _isExpanded = false;
            this.IsExpandable = true;
            this.IsHeaderVisible = true;
        }

        #endregion //Constructors	

        #region Properties

        #region Public Properties

        #region IsExpanded

        /// <summary>
        /// Specifies if the group item is exapnded or collapsed. If IsExpandalbe property is - False, IsExpanded property is always - True.
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                if (!this.IsHeaderVisible)
                {
                    return true;
                }

                return !this.IsExpandable ? true : _isExpanded;
            }

            set
            {
                _isExpanded = value;
            }
        }

        #endregion //IsExpanded

        #region IsExpandable

        /// <summary>
        ///  Specifies if the group item can be exapnded and collapsed.
        /// </summary>
        public bool IsExpandable
        {
            get;
            set;
        }

        #endregion //IsExpandable

        #region IsHeaderVisible

        /// <summary>
        ///  Specifies if the group item has a header.
        /// </summary>
        public bool IsHeaderVisible
        {
            get;
            set;
        }

        #endregion //IsHeaderVisible

        #region Name

        /// <summary>
        /// Gets, sets the group name
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

        #region OrderNumber

        /// <summary>
        /// Gets, sets the value used for group order in a smart tag pannel
        /// </summary>
        public int OrderNumber
        {
            get
            {
                return _orderNumber;
            }

            set
            {
                _orderNumber = value;
            }
        }

        #endregion //OrderNumber

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