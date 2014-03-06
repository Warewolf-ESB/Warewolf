using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Specialized;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids.Primitives
{
    #region FilterValueProxy

    /// <summary>
    /// For internal use only. This is used for Unique values in the FilterMen.
    /// </summary>
    public class FilterValueProxy : INotifyPropertyChanged
    {
        #region Consturctor
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterValueProxyRowsFilter"/> class.
        /// </summary>
        public FilterValueProxy()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterValueProxyRowsFilter"/> class.
        /// </summary>
        /// <param name="value"></param>
        public FilterValueProxy(object value) : this(value, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterValueProxyRowsFilter"/> class.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="isChecked"></param>
        public FilterValueProxy(object value, bool isChecked)
        {
            this._value = value;
            this._isChecked = isChecked;
        }

        #endregion // Consturctor

        #region Members

        private object _value;
        private bool _isChecked;

        #endregion // Members

        #region Properties

        /// <summary>
        /// The Value that the FilterValueProxy represents.
        /// </summary>
        public object Value
        {
            get { return this._value; }
            set
            {
                if (this._value != value)
                {
                    this._value = value;
                    this.OnPropertyChanged("Value");
                }
            }
        }

        /// <summary>
        /// The string used for display purposes.
        /// </summary>
        public string ContentString
        {
            get
            {
                if (this.DataType == typeof(string))
                {
                    if (string.IsNullOrEmpty((string)this.Value))
                    {
                        return this.EmptyString;
                    }
                }
                if (this.Value == null)
                {
                    return this.NullString;
                }
                return string.IsNullOrEmpty(this.FormatString) ? this.Value.ToString() : string.Format(this.FormatString, this.Value);
            }
        }

        /// <summary>
        /// Whether or not the FilterValue is selected in the FilterSelectionControl.
        /// </summary>
        public bool IsChecked
        {
            get { return this._isChecked; }
            set
            {
                if (this._isChecked != value)
                {
                    this._isChecked = value;
                    this.OnPropertyChanged("IsChecked");
                }
            }
        }

        internal string FormatString
        {
            get;
            set;
        }

        internal Type DataType
        {
            get;
            set;
        }

        internal string EmptyString
        {
            get;
            set;
        }

        internal string NullString
        {
            get;
            set;
        }

        #endregion // Properties

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion



#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion // FilterValueProxy

    #region FilterValueProxyCollection

    /// <summary>
    ///  For Internal use only. Used for managing the Unique list of items for the FilterMenu.
    /// </summary>
    public class FilterValueProxyCollection : FilterItemCollection<FilterValueProxy>
    {
    }

    #endregion // FilterValueProxyCollection

    #region FilterItemCollection

    /// <summary>
    /// A base class for the collection which is used by the <see cref="FilterSelectionControl"/>.
    /// </summary>
    public abstract class FilterItemCollection : IEnumerable, INotifyCollectionChanged
    {
        #region IEnumerable Members
        /// <summary>
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion // IEnumerable Members

        #region Properties

        /// <summary>
        /// Gets the <see cref="System.Object"/> at the specified index.
        /// </summary>
        public abstract object this[int index]
        {
            get;
        }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerator GetEnumerator();

        /// <summary>
        /// Adds the specified item to the list.
        /// </summary>
        /// <param name="item"></param>
        public abstract void Add(object item);

        /// <summary>
        /// Adds the specified item to the list without raising collection changed events.
        /// </summary>
        /// <param name="item"></param>
        public abstract void AddSilently(object item);

        /// <summary>
        /// Clears the list
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Gets the number of items in the list.
        /// </summary>
        public abstract int Count
        {
            get;
        }

        /// <summary>
        /// Raises the CollectionChanged event.
        /// </summary>
        public virtual void RaiseCollectionChanged()
        {
            this.OnCollectionChanged();
        }

        /// <summary>
        /// Raised when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Raises the collection changed event.
        /// </summary>
        protected virtual void OnCollectionChanged()
        {
            if (this.CollectionChanged != null)
                this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        #endregion // Methods
    }

    /// <summary>
    ///  For Internal use only. Used for managing the Unique list of items for the FilterMenu.
    /// </summary>
    public class FilterItemCollection<T> : FilterItemCollection, IEnumerable<T>
    {
        #region Members
        List<T> _list = new List<T>();
        #endregion // Members

        #region Properties
        /// <summary>
        /// Gets the list of items
        /// </summary>
        protected List<T> List
        {
            get
            {
                return this._list;
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> at the specified index.
        /// </summary>
        public override object this[int index]
        {
            get { return this._list[index]; }
        }
        #endregion // Properties

        #region Methods
        /// <summary>
        /// Clears the list
        /// </summary>
        public override void Clear()
        {
            this._list.Clear();
            this.OnCollectionChanged();
        }

        /// <summary>
        /// Adds the specified item to the list.
        /// </summary>
        /// <param name="item"></param>
        public override void Add(object item)
        {
            this.AddSilently(item);
            this.OnCollectionChanged();
        }

        /// <summary>
        /// Gets the number of items in the list.
        /// </summary>
        public override int Count
        {
            get { return this._list.Count; }
        }

        /// <summary>
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        protected override IEnumerator GetEnumerator()
        {
            return this._list.GetEnumerator();
        }

        /// <summary>
        /// Adds the specified item to the list without raising collection changed events.
        /// </summary>
        /// <param name="item"></param>
        public override void AddSilently(object item)
        {
            this._list.Add((T)item);            
        }

        #endregion // Methods

        #region IEnumerable<T> Members

        /// <summary>
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this._list.GetEnumerator();
        }

        #endregion
    }

    #endregion // FilterItemCollection<T>
}

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// A <see cref="RowsFilter"/> object designed for filtering <see cref="FilterValueProxy"/> objects.
    /// </summary>
    public class FilterValueProxyRowsFilter : RowsFilter
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterValueProxyRowsFilter"/> class.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="typeInfo"></param>
        public FilterValueProxyRowsFilter(Type objectType, CachedTypedInfo typeInfo)
        {
            this.ObjectType = objectType;
            this.ObjectTypedInfo = typeInfo;
        }
        #endregion // Constructor

        #region Overrides
        /// <summary>
        /// Generates the current expression for this <see cref="RowsFilter"/>.
        /// </summary>
        protected override System.Linq.Expressions.Expression GetCurrentExpression()
        {
            FilterContext context = FilterContext.CreateGenericFilter(
                this.ObjectTypedInfo,
                typeof(FilterValueProxy),
                false,
                false);

            Expression exp = GetCurrentExpression(context);

            return exp;
        }
        #endregion // Overrides

        #region Properties

        #region FieldName

        /// <summary>
        /// Gets the field name of the property that is being filtered on.
        /// </summary>
        public override string FieldName
        {
            get
            {
                return "ContentString";
            }
        }

        #endregion // FieldName

        #region FieldType

        /// <summary>
        /// Gets the Type of the FieldName property.
        /// </summary>
        public override Type FieldType
        {
            get
            {
                return typeof(string);
            }
        }

        #endregion // FieldType

        #endregion // Properties
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