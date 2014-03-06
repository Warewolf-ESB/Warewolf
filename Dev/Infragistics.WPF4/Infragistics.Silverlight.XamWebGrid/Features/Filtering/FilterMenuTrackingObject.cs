using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics.Controls.Grids.Primitives;
using System.Collections.Generic;
using System.Windows.Data;
using Infragistics.Controls.Menus;
using System.ComponentModel;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// An object that will be used aby the FilterMenu of the FilterSelectionControl to display filters.
    /// </summary>
    public class FilterMenuTrackingObject : INotifyPropertyChanged, IProvidePropertyPersistenceSettings
    {
        #region Members

        private List<FilterMenuTrackingObject> _children = new List<FilterMenuTrackingObject>();
        private List<FilterOperand> _filterOperands;
        private string _label;
        private bool _isChecked;

        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterMenuTrackingObject"/> class.
        /// </summary>
        public FilterMenuTrackingObject()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterMenuTrackingObject"/> class.
        /// </summary>
        public FilterMenuTrackingObject(FilterOperand fo)
        {
            this.FilterOperands.Add(fo);
        }

        #endregion // Constructor

        #region Properties

        #region FilterOperands
        /// <summary>
        /// Gets the List of <see cref="FilterOperand"/> objects which can be displayed.
        /// </summary>
        public List<FilterOperand> FilterOperands
        {
            get
            {
                if (this._filterOperands == null)
                {
                    this._filterOperands = new List<FilterOperand>();
                }
                return this._filterOperands;
            }
        }
        #endregion // FilterOperands

        #region IsSeparator

        /// <summary>
        /// Gets / sets if the <see cref="FilterMenuTrackingObject"/> is a visual separator.
        /// </summary>
        public bool IsSeparator { get; set; }

        #endregion // IsSeparator

        #region IsCheckable

        /// <summary>
        /// Gets if this <see cref="FilterMenuTrackingObject"/> is checkable, or if it is a parent menu option.
        /// </summary>
        public bool IsCheckable
        {
            get
            {
                return !this.IsSeparator && this.FilterOperands != null && this.FilterOperands.Count > 0;
            }
        }

        #endregion // IsCheckable

        #region Label

        /// <summary>
        /// Gets / sets the text that will be displayed on the menu.
        /// </summary>        
        public string Label
        {
            set
            {
                this._label = value;
            }
            get
            {
                if (!string.IsNullOrEmpty(this._label))
                {
                    return this._label;
                }
                if (this.FilterOperands.Count > 0)
                    return this.FilterOperands[0].DisplayName;

                return "";
            }
        }

        #endregion // Label

        #region IsChecked

        /// <summary>
        /// Gets / sets if the option is currently checked.
        /// </summary>
        public bool IsChecked
        {
            get
            {
                return this._isChecked;
            }
            set
            {
                if (value != this._isChecked)
                {
                    this._isChecked = value;
                    this.OnPropertyChanged("IsChecked");
                }
            }
        }

        #endregion // IsChecked

        #region Children
        /// <summary>
        /// Gets the List of <see cref="FilterMenuTrackingObject"/>s that will be child menu options.
        /// </summary>
        public List<FilterMenuTrackingObject> Children
        {
            get
            {
                if (this._children == null)
                {
                    this._children = new List<FilterMenuTrackingObject>();
                }
                return this._children;
            }
        }
        #endregion // Children

        #region FilterColumnSettings
        /// <summary>
        /// Gets the <see cref="FilterColumnSettings"/> object that is associated with this <see cref="FilterMenuTrackingObject"/>.
        /// </summary>
        public FilterColumnSettings FilterColumnSettings
        {
            get;
            protected internal set;
        }
        #endregion // FilterColumnSettings

        internal bool IsCustomOption { get; set; }

        #endregion // Properties

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Invoked when a property changes on the <see cref="TrackIsChecked"/> object.
        /// </summary>
        /// <param name="name">The name of the property that has changed.</param>
        protected virtual void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// Fired when a property changes on the <see cref="TrackIsChecked"/>.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IProvidePropertyPersistenceSettings Members

        #region PropertiesToIgnore

        List<string> _propertiesThatShouldntBePersisted;

        /// <summary>
        /// Gets a List of properties that shouldn't be saved when the PersistenceManager goes to save them.
        /// </summary>
        protected virtual List<string> PropertiesToIgnore
        {
            get
            {
                if (this._propertiesThatShouldntBePersisted == null)
                {
                    this._propertiesThatShouldntBePersisted = new List<string>()
					{
						"FilterColumnSettings"
					};
                }

                return this._propertiesThatShouldntBePersisted;
            }
        }

        List<string> IProvidePropertyPersistenceSettings.PropertiesToIgnore
        {
            get
            {
                return this.PropertiesToIgnore;
            }
        }

        #endregion // PropertiesToIgnore

        #region PriorityProperties

        /// <summary>
        /// Gets a List of properties that should be applied, before even trying to look at any other property on the object.
        /// </summary>
        protected virtual List<string> PriorityProperties
        {
            get
            {
                return null;
            }
        }
        List<string> IProvidePropertyPersistenceSettings.PriorityProperties
        {
            get { return this.PriorityProperties; }
        }

        #endregion // PriorityProperties

        #region FinishedLoadingPersistence

        /// <summary>
        /// Allows an object to perform an operation, after it's been loaded.
        /// </summary>
        protected virtual void FinishedLoadingPersistence()
        {
           
        }

        void IProvidePropertyPersistenceSettings.FinishedLoadingPersistence()
        {
            this.FinishedLoadingPersistence();
        }

        #endregion // FinishedLoadingPersistence

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