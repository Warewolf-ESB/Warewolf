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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections;
using System.Reflection;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// An ObservableCollection of <see cref="ComboColumn"/> objects.
    /// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_MultiColumnCombo, Version = FeatureInfo.Version_11_2)]

	public class ComboColumnCollection : ObservableCollection<ComboColumn>
	{
		#region Member Variables

		List<ComboColumn> _internalFixedAdornerColumns;
		ReadOnlyKeyedComboColumnCollection<ComboColumn> _fixedAdornerColumns;
        List<ComboColumn> _internalVisibleColumns;
        ReadOnlyKeyedComboColumnCollection<ComboColumn> _visibleColumns;
        Collection<ComboColumn> _starColumns;
        ReadOnlyKeyedComboColumnCollection<ComboColumn> _starColumnsReadOnly;

		RowSelectionCheckBoxColumn _rowSelectionCheckBoxColumn;
        FixedComboColumnsCollection _fixedColumnsCollectionLeft, _fixedColumnsCollectionRight;
        FillerComboColumn _fillerColumn;
        XamMultiColumnComboEditor _comboEditor;

		#endregion //Member Variables

		#region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ComboColumnCollection"/> class.
        /// </summary>
		/// <param name="comboEditor"></param>
		public ComboColumnCollection(XamMultiColumnComboEditor comboEditor)
		{
            this._comboEditor = comboEditor;
			this._internalFixedAdornerColumns	= new List<ComboColumn>();
			this._fixedAdornerColumns			= new ReadOnlyKeyedComboColumnCollection<ComboColumn>(this._internalFixedAdornerColumns);

            this._internalVisibleColumns = new List<ComboColumn>();
            this._visibleColumns = new ReadOnlyKeyedComboColumnCollection<ComboColumn>(this._internalVisibleColumns);

            this._fillerColumn = new FillerComboColumn();
            this._fillerColumn.ComboEditor = this._comboEditor;

            this.RowSelectionCheckBoxColumn.ComboEditor = this._comboEditor;

            this._starColumns = new Collection<ComboColumn>();
            this._starColumnsReadOnly = new ReadOnlyKeyedComboColumnCollection<ComboColumn>(this._starColumns);	
		}

		#endregion Constructor

		#region Properties

		#region Public

		#region FixedAdornerColumns

		/// <summary>
		/// A collection of <see cref="ComboColumn"/> objects that should be displayed first and fixed in the <see cref="XamMultiColumnComboEditor"/>
		/// Note: these columns are generally columns that don't represent fields in the underlying data source such as the <see cref="RowSelectionCheckBoxColumn"/>. 
		/// </summary>
		public ReadOnlyKeyedComboColumnCollection<ComboColumn> FixedAdornerColumns
		{
			get
			{
				return this._fixedAdornerColumns;
			}
		}

		#endregion //FixedAdornerColumns

        #region FixedColumnsLeft

        /// <summary>
        /// Gets a collection of <see cref="ComboColumn"/> objects that are pinned to the left side./>
        /// </summary>
        [Browsable(false)]
        internal FixedComboColumnsCollection FixedColumnsLeft
        {
            get
            {
                if (this._fixedColumnsCollectionLeft == null)
                    this._fixedColumnsCollectionLeft = new FixedComboColumnsCollection(ComboColumnFixedState.Left);
                return this._fixedColumnsCollectionLeft;
            }
        }

        #endregion // FixedColumnsLeft

        #region FixedColumnsRight

        /// <summary>
        /// Gets a collection of <see cref="ComboColumn"/> objects that are pinned to the right side./>
        /// </summary>
        [Browsable(false)]
        internal FixedComboColumnsCollection FixedColumnsRight
        {
            get
            {
                if (this._fixedColumnsCollectionRight == null)
                    this._fixedColumnsCollectionRight = new FixedComboColumnsCollection(ComboColumnFixedState.Right);
                return this._fixedColumnsCollectionRight;
            }
        }

        #endregion // FixedColumnsRight

		#region Indexer
		/// <summary>
        /// Gets the <see cref="ComboColumn"/> that has the specified key. 
        /// </summary>
        /// <param propertyName="key"></param>
        /// <returns>
        /// The column with the specified Key. 
        /// If more than one <see cref="ComboColumn"/> has the same key, the first Column is returned.
        /// </returns>
        public ComboColumn this[string key]
        {
            get
            {
                ComboColumn val = null;

                foreach (ComboColumn item in this.Items)
                {
                    if (item.Key == key)
                    {
                        val = item;
                        break;
                    }
                }

                return val;
            }

        }
        #endregion // Indexer

        #region FillerColumn

        /// <summary>
        /// Gets the column that is used to fill up empty space in a row. 
        /// </summary>
        public FillerComboColumn FillerColumn
        {
            get { return this._fillerColumn; }
        }

        #endregion // FillerColumn

		#endregion //Public 

        #region Protected

        #region VisibleColumns

        /// <summary>
        /// Gets a readonly list of <see cref="ComboColumn"/> objects that are not hidden.
        /// </summary>
        protected internal ReadOnlyCollection<ComboColumn> VisibleColumns
        {
            get { return this._visibleColumns; }
        }

        #endregion // VisibleColumns

        #region StarColumns

        /// <summary>
        /// Gets a readonly list of <see cref="ComboColumn"/> objects that have a width that are Star.
        /// </summary>
        protected internal ReadOnlyCollection<ComboColumn> StarColumns
        {
            get { return this._starColumnsReadOnly; }
        }

        #endregion // StarColumns

        #endregion // Protected

		#region Internal

		#region RowSelectionCheckBoxColumn

		internal RowSelectionCheckBoxColumn RowSelectionCheckBoxColumn
		{
			get
			{
				if (null == this._rowSelectionCheckBoxColumn)
					this._rowSelectionCheckBoxColumn = new RowSelectionCheckBoxColumn();

				return this._rowSelectionCheckBoxColumn;
			}
		}

		#endregion //RowSelectionCheckBoxColumn

        internal bool IsInitialized
        {
            get;
            set;
        }

		#endregion //Internal

		#endregion //Properties

		#region Methods

		#region Private

		#region RegisterFixedAdornerColumn

		/// <summary>
		/// Registers the specified <see cref="ComboColumn"/> as a fixed column.
		/// </summary>
		/// <param propertyName="column"></param>
		/// <param propertyName="first">Specifies whether or not the column should be inserted at the first index position.</param>
		protected virtual void RegisterFixedAdornerColumn(ComboColumn column, bool first)
		{
			if (!this._internalFixedAdornerColumns.Contains(column))
			{
				if (first)
					this._internalFixedAdornerColumns.Insert(0, column);
				else
					this._internalFixedAdornerColumns.Add(column);
			}
		}

		#endregion // RegisterFixedAdornerColumn

		#region UnregisterFixedAdornerColumn

		/// <summary>
		/// Unregisters the specified <see cref="ComboColumn"/> as a fixed column.
		/// </summary>
		/// <param propertyName="column"></param>
		protected virtual void UnregisterFixedAdornerColumn(ComboColumn column)
		{
			this._internalFixedAdornerColumns.Remove(column);
		}

		#endregion // UnregisterFixedAdornerColumn

        #region InvalidateCollection

        private void InvalidateCollection(IList list)
        {
            int count = list.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                if (!this.Items.Contains((ComboColumn)list[i]))
                    list.RemoveAt(i);
            }
        }

        #endregion // InvalidateCollection

		#endregion //Private

        #region Protected 

        #region InvalidateColumnsCollections

        /// <summary>
        /// Determines which columns are Fixed, Visible or Hidden. 
        /// </summary>
        /// <param propertyName="fullInvalidate"></param>
        protected internal virtual void InvalidateColumnsCollections(bool fullInvalidate)
        {
            this._internalVisibleColumns.Clear();
            this._starColumns.Clear();
            bool starColumnFound = false;

            foreach (ComboColumn col in this.Items)
            {
                bool isFixed = !(col.IsFixed == ComboColumnFixedState.NotFixed);

                if (!isFixed && col.Visibility == Visibility.Visible)
                    this._internalVisibleColumns.Add(col);

                if (col.WidthResolved.WidthType == ComboColumnWidthType.Star)
                {
                    starColumnFound = true;
                    this._starColumns.Add(col);
                }
            }

            if (!starColumnFound)
                this._internalVisibleColumns.Add(this._fillerColumn);

            if (fullInvalidate)
            {
                this.InvalidateCollection(this.FixedColumnsLeft);
                this.InvalidateCollection(this.FixedColumnsRight);
            }

            this._comboEditor.InvalidateScrollPanel(true);

        }

        #endregion // InvalidateColumnsCollections

        #endregion // Protected

        #region Internal

        internal void InternalRegisterFixedAdornerColumn(ComboColumn column, bool first)
		{
			this.RegisterFixedAdornerColumn(column, first);
		}

		internal void InternalUnregisterFixedAdornerColumn(ComboColumn column)
		{
			this.UnregisterFixedAdornerColumn(column);
		}

		#endregion // Internal

		#endregion //Methods

        #region EventHandlers

        /// <summary>
        /// Raised when the underlying collection changes.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                if (this.IsInitialized && this._comboEditor != null)
                {
                    DataManagerBase manager = this._comboEditor.DataManager;

                    // JM 10-3-11 TFS90123
                    if (manager == null)
                    {
                        base.OnCollectionChanged(e);
                        return;
                    }


                    PropertyDescriptorCollection pdcs = manager.CachedTypedInfo.PropertyDescriptors;                    


                    foreach (ComboColumn col in e.NewItems)
                    {
                        col.ComboEditor = this._comboEditor;

                        if (this._comboEditor.DataKeys != null)
                        {
                            XamMultiColumnComboEditor.ValidateColumn(col, this._comboEditor.DataKeys, manager

                            ,pdcs

);
                        }
                    }
                }
                else
                {
                    foreach (ComboColumn col in e.NewItems)
                    {
                        col.ComboEditor = this._comboEditor;
                    }
                }
            }
            base.OnCollectionChanged(e);

            this.InvalidateColumnsCollections(true);
        }

        #endregion // EventHandlers
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