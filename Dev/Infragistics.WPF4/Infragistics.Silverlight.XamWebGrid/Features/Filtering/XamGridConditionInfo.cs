using System;
using System.ComponentModel;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// A class that is used by the <see cref="Infragistics.Controls.Grids.Primitives.CompoundFilterDialogControl"/> to show the data on that form.
    /// </summary>
    /// <remarks>Not intended for general use.</remarks>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class XamGridConditionInfo : INotifyPropertyChanged
    {
        #region Members

        XamGridConditionInfoGroup _group;
        FilterOperand _filterOperand;
        object _filterValue;
        string _errorMessage;
        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="XamGridConditionInfo"/> class.
        /// </summary>
        public XamGridConditionInfo(XamGridConditionInfoGroup parentGroup)
        {
            this.Group = parentGroup;
        }

        #endregion // Constructor

        #region Properties

        #region Group
        /// <summary>
        /// Gets / sets the <see cref="XamGridConditionInfoGroup"/> that owns this <see cref="XamGridConditionInfo"/> object.
        /// </summary>
        public XamGridConditionInfoGroup Group
        {
            get
            {
                return this._group;
            }
            set
            {
                if (this._group != null)
                {
                    this._group.InfoObjects.Remove(this);
                }

                this._group = value;

                if (this._group != null)
                {
                    this._group.InfoObjects.Add(this);
                }

                this.OnPropertyChanged("Group");
            }
        }

        #endregion // Group

        #region FilterOperand

        /// <summary>
        /// Gets / sets the <see cref="FilterOperand"/> that will be used in the filter.
        /// </summary>
        public FilterOperand FilterOperand
        {
            get
            {
                return this._filterOperand;
            }
            set
            {
                if (this._filterOperand != value)
                {
                    this._filterOperand = value;
                    if (this._filterOperand != null)
                    {
                        if (!this._filterOperand.RequiresFilteringInput)
                        {
                            this.FilterValue = null;
                        }
                    }
                    this.OnPropertyChanged("FilterOperand");
                }
            }
        }

        #endregion // FilterOperand

        #region FilterValue

        /// <summary>
        /// Gets / sets the value that the filter will be applied to.
        /// </summary>
        public object FilterValue
        {
            get
            {
                return this._filterValue;
            }
            set
            {
                if (_filterValue != value)
                {
                    _filterValue = value;
                    this.OnPropertyChanged("FilterValue");
                }
            }
        }

        #endregion // FilterValue

        #region SortOrder

        public int SortOrder
        {
            get
            {
                return this.Group.Level; // my index
            }
        }

        #endregion // SortOrder

        #region ErrorMessage
        public string ErrorMessage
        {
            get { return this._errorMessage; }
            set
            {
                if (this._errorMessage != value)
                {
                    this._errorMessage = value;
                    this.OnPropertyChanged("ErrorMessage");
                }
            }
        }
        #endregion // ErrorMessage

        #region HasError
        public bool HasError
        {
            get
            {
                return !string.IsNullOrEmpty(this._errorMessage);
            }
        }
        #endregion // HasError

        public string InfoObjectName
        {
            get
            {
                if (this.Group != null)
                    return this.Group.LevelName + "." + this.Group.InfoObjects.IndexOf(this);

                return "NoGroup";
            }
        }

        #endregion // Properties

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Event raised when a property on this object changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="name"></param>
        protected virtual void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion
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