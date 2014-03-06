using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;

namespace Infragistics.Windows.Helpers
{

    // JJD 12/13/07
    // Added class to support binding to a list of lists
    /// <summary>
    /// Wraps an object that implements the <see cref="IEnumerable"/> interface to expose both its name and its items.
    /// </summary>
    /// <remarks>
    /// <para class="body">This is used by controls like the XamDataGrid when its DataSource is set to a list of lists, e.g. a DataSet.</para>
    /// </remarks>
    public class EnumerableObjectWrapper : PropertyChangeNotifier
    {
        #region Private Members

        private string _name;
        private IEnumerable _items;

        #endregion //Private Members	
    
        #region Constructor

        /// <summary>
        /// Creates a new instance of an <see cref="EnumerableObjectWrapper"/>
        /// </summary>
        /// <param name="items">The underlying list of items.</param>
        /// <exception cref="ArgumentNullException">If items parameter is null.</exception>
        /// <exception cref="ArgumentException">If items parameter is a string.</exception>
        public EnumerableObjectWrapper(IEnumerable items) : this(items, null)
        {
        }

        /// <summary>
        /// Creates a new instance of an <see cref="EnumerableObjectWrapper"/>
        /// </summary>
        /// <param name="items">The underlying list of items.</param>
        /// <param name="name">The name of the object.</param>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if the 'name' parameter is null then the <see cref="Name"/> property will be initialized by either calling the <see cref="System.ComponentModel.ITypedList"/> interface's GetListName method. . Otherwise the ToString method is called.</para></remarks>
        /// <exception cref="ArgumentNullException">If items parameter is null.</exception>
        /// <exception cref="ArgumentException">If items parameter is a string.</exception>
        public EnumerableObjectWrapper(IEnumerable items, string name)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            if (items is string)
                throw new ArgumentException(SR.GetString("LE_ArgumentException_22"));

            this._items = items;

            if (name != null)
                this._name = name;
            else
            {
                // since a name wasn't supllied get it from the object itself 
                ITypedList tl = this._items as ITypedList;

                if (tl != null)
                    this._name = tl.GetListName(null);
                else
                    this._name = items.ToString();
            }
        }

        #endregion //Constructor	
    
        #region Properties

            #region Items

        /// <summary>
        /// Returns the underlying list of items (read-only).
        /// </summary>
        /// <value>Returns the list of items that was passed into the constructor.</value>
        public IEnumerable Items
        {
            get { return this._items; }
        }

            #endregion //Items	
    
            #region Name

        /// <summary>
        /// Gets/sets the name of the wrapped list of items.
        /// </summary>
        /// <value>The string representing the name of the list of items.</value>
        /// <remarks>
        /// <para class="note"><b>Note:</b> If <see cref="Items"/> implements the <see cref="System.ComponentModel.ITypedList"/> interface then the name is initialized by call <see cref="System.ComponentModel.ITypedList.GetListName"/> method. Otherwise the ToString method is called.</para>
        /// </remarks>
        public string Name
        {
            get { return _name; }
            set
            {
                if (value != this._name)
                {
                    this._name = value;
                    this.RaisePropertyChangedEvent("Name");
                }
            }
        }

            #endregion //Name	
    
        #endregion //Properties	
    
		#region Base class overrides
		/// <summary>
		/// Returns true if the passed in object is equal
		/// </summary>
		public override bool Equals(object obj)
		{
			EnumerableObjectWrapper wrapper = obj as EnumerableObjectWrapper;

			return null != wrapper &&
				wrapper._items == _items;
		}

		/// <summary>
		/// Caclulates a value used for hashing
		/// </summary>
		public override int GetHashCode()
		{
			return _items.GetHashCode();
		} 
		#endregion //Base class overrides
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