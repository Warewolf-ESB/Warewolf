using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Data;
using Infragistics.Shared;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.DataPresenter
{
	// JJD 4/26/07
	// Optimization - replaced sparse array in CellsCollection
	#region Obsolete code

	//    #region CellSparseArray

	//#if DEBUG
	//    /// <summary>
	//    /// Sparse array class for storing cells in sorted order. This is also used to manager
	//    /// visible and scroll indeces.
	//    /// </summary>
	//#endif
	//    internal class CellSparseArray : SparseArray, ICreateItemCallback
	//    {
	//        #region Private Variables

	//        private CellCollection _ownerCollection = null;

	//        #endregion // Private Variables

	//        #region Constructor

	//#if DEBUG
	//        /// <summary>
	//        /// Constructor.
	//        /// </summary>
	//#endif
	//        internal CellSparseArray(CellCollection ownerCollection, int factor, float growthFactor)
	//            : base(false, factor, growthFactor)
	//        {
	//            if (null == ownerCollection)
	//                throw new ArgumentNullException("ownerCollection");

	//            this._ownerCollection = ownerCollection;
	//        }

	//        #endregion // Constructor

	//        #region DataPresenterBase

	//        internal DataPresenterBase DataPresenter { get { return this._ownerCollection.DataPresenter; } }

	//        #endregion //DataPresenterBase	

	//        #region OwnerCollection

	//        internal CellCollection OwnerCollection { get { return this._ownerCollection; } }

	//        #endregion //OwnerCollection	

	//        #region GetOwnerData

	//        protected override object GetOwnerData(object item)
	//        {
	//            return ((Cell)item)._sparseArrayOwnerData;
	//        }

	//        #endregion //GetOwnerData

	//        #region SetOwnerData

	//        protected override void SetOwnerData(object item, object ownerData)
	//        {
	//            ((Cell)item)._sparseArrayOwnerData = ownerData;
	//        }

	//        #endregion //SetOwnerData

	//        #region GetItem

	//        internal Cell GetItem(int index, bool create)
	//        {
	//            bool newCellCreated;
	//            return this.GetItem(index, create, out newCellCreated);
	//        }

	//        internal Cell GetItem(int index, bool create, out bool newCellCreated)
	//        {
	//            this._ownerCollection.VerifyVersion();

	//            newCellCreated = false;
	//            Cell cell = (Cell)this[index];

	//            if (null == cell && create)
	//            {
	//                cell = this.OwnerCollection.AllocateNewCell(index);
	//                newCellCreated = true;
	//                this[index] = cell;
	//            }

	//            return cell;
	//        }

	//        #endregion //GetItem

	//        #region ToArray

	//        internal Cell[] ToArray(bool create)
	//        {
	//            Cell[] array = (Cell[])base.ToArray(typeof(Cell));

	//            if (create)
	//            {
	//                for (int i = 0; i < array.Length; i++)
	//                {
	//                    if (null == array[i])
	//                    {
	//                        this[i] = array[i] = this.OwnerCollection.AllocateNewCell(i);
	//                    }
	//                }
	//            }

	//            return array;
	//        }

	//        #endregion //ToArray

	//        #region ICreateItemCallback.CreateItem

	//        public object CreateItem(SparseArray array, int relativeIndex)
	//        {
	//            bool newCellCreated;
	//            Cell cell = this.GetItem(relativeIndex, true, out newCellCreated);

	//            return cell;
	//        }

	//        #endregion // ICreateItemCallback.CreateItem
	//    }

	//    #endregion // CellSparseArray

	#endregion //Obsolete code	
    
    /// <summary>
    /// A collection of Cells exposed off a DataRecord
    /// </summary>
	/// <remarks>
	/// <p class="body">Refer to the <a href="xamData_Terms_Fields.html">Fields</a> topic in the Developer's Guide for an explanation of fields.</p>
	/// <p class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an overall explanation of how everything works together.</p>
	/// <p class="body">Refer to the <a href="xamData_Cells_CellValuePresenters_and_Cell_Virtualization.html">Cells, CellValuePresenters and Cell Virtualization</a> topic in the Developer's Guide for an explanation of cells.</p>
	/// <p class="body">Refer to the <a href="xamData_Assigning_a_FieldLayout.html">Assigning a FieldLayout</a> topic in the Developer's Guide for an example of adding an <see cref="UnboundField"/> to a <see cref="FieldLayout"/>.</p>
	/// </remarks>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
    /// <seealso cref="Cell"/>
    /// <seealso cref="DataRecord"/>
	/// <seealso cref="DataRecord.Cells"/>
	// SSP 12/16/08 - NAS9.1 Record Filtering
	// Removed sealed qualifier. FilterCellCollection derives from this.
	// 
	//sealed public class CellCollection : IList, IList<Cell>
	public class CellCollection : IList, IList<Cell>
    {
        #region Private Members

        private DataRecord _record;
		// JJD 4/26/07
		// Optimization - replaced sparse array in CellsCollection
		private Cell[] _cellArray;
        private int _fieldCollectionVersion;
		private int _cachedCount;
		private int _allocatedRecordCount;

        #endregion //Private Members	
    
        #region Constructor

        internal CellCollection(DataRecord record)
        {
            if (record == null)
                throw new ArgumentNullException("record");
            
            if (record.FieldLayout == null)
				throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_2" ) );

            this._record = record;
        }

        #endregion //Constructor	
    
        #region Properties

            #region Public Properties

                #region Count

        /// <summary>
        /// Returns the number of records in the collection (read-only)
        /// </summary>
        public int Count
        {
            get 
            {
                this.VerifyVersion();

				if ( this._cellArray != null )
					return this._cellArray.Length;

				return this._cachedCount;
            }
        }

                #endregion //Count	
    
                #region DataPresenterBase

        /// <summary>
        /// Returns the associated DataPresenterBase control
        /// </summary>
        public DataPresenterBase DataPresenter { get { return this._record.DataPresenter; } }

                #endregion //DataPresenterBase	

                #region Record

        /// <summary>
        /// Returns the <see cref="DataRecord"/> that owns this collection. (read-only)
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataRecord Record
        {
            get
            {
                return this._record;
            }
        }

                #endregion //Record

            #endregion //Public Properties

		// JJD 4/26/07
		// Optimization - replaced sparse array in CellsCollection
		#region Obsolete code

		//        #region SparseArray

		//internal CellSparseArray SparseArray
		//{
		//    get
		//    {
		//        if (this._sparseArray == null)
		//        {
		//            this._sparseArray = new CellSparseArray(this, Math.Max(4, Math.Min(40, this._record.FieldLayout.Fields.Count)), 0.1f);
		//            this._sparseArray.Expand(this._record.FieldLayout.Fields.Count);
		//            this._fieldCollectionVersion = this._record.FieldLayout.Fields.Version;
		//        }

		//        return this._sparseArray;
		//    }
		//}

		//        #endregion // SparseArray

		#endregion //Obsolete code	

        #endregion //Properties

        #region Methods

            #region Public Methods

                #region Contains

        /// <summary>
        /// Returns true if the cell is in the collection
        /// </summary>
        public bool Contains(Cell item)
        {
			int index = this.IndexOf(item);

            return index >= 0;
        }

                #endregion //Contains

                #region CopyTo

        /// <summary>
        /// Copies the records into an array
        /// </summary>
        public void CopyTo(Cell[] array, int arrayIndex)
        {
 			this.EnsureAllCellsAreAllocated();
			
			if ( this._cellArray != null )
				Array.Copy(this._cellArray, 0, array, arrayIndex, this._cellArray.Length);
        }

                #endregion //CopyTo

                #region IndexOf

        /// <summary>
        /// Returns the zero-based index of the record in the collection
        /// </summary>
        public int IndexOf(Cell item)
        {
 			if (item.Record != this._record)
				return -1;
			
			Field fld = item.Field;

			if (fld == null)
				return -1;

            return fld.Index;
        }

                #endregion //IndexOf

				#region IsCellAllocated

		/// <summary>
		/// Determines if the cell has been allocated
		/// </summary>
		/// <param name="field">The associated field.</param>
		/// <returns>True if allocated</returns>
		public bool IsCellAllocated(Field field)
		{
			if (field.Owner != this._record.FieldLayout)
				return false;

			//int index = this.Record.FieldLayout.Fields.IndexOf(field);
			int index = field.Index;

			if (index < 0)
				return false;

			return this.IsCellAllocated(index);
		}

		/// <summary>
		/// Determines if the cell has been allocated
		/// </summary>
		/// <param name="index">The zero based index of the cell.</param>
		/// <returns>True if allocated</returns>
		public bool IsCellAllocated(int index)
		{
			this.VerifyVersion();
			
			if ( this._cellArray == null ||
				 index < 0 ||
				 index >= this._cellArray.Length )
				return false;

			return this._cellArray[index] != null;
		}

				#endregion //IsCellAllocated	
    
            #endregion//Public Methods

            #region Internal Methods

                #region AllocateNewCell

        internal Cell AllocateNewCell(int index)
        {
            Debug.Assert(index < this._record.FieldLayout.Fields.Count);

			Field fld = this._record.FieldLayout.Fields[index];

			this._allocatedRecordCount++;

			// SSP 12/16/08 - NAS9.1 Record Filtering
			// Added CreateCell method for the new derived FilterCellCollection to override.
			// 
			
			return this.CreateCell( fld );
			
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			
        }

                #endregion //AllocateNewCell

                // JJD 4/3/08 - added support for printing
                #region CloneAssociatedCellSettings

                internal void CloneAssociatedCellSettings(CellCollection associatedCollection)
                {
                    if (associatedCollection._cellArray != null)
                    {
                        int count = associatedCollection._cellArray.Length;

                        // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
                        //
                        // MBS 7/31/09 - NA9.2 Excel Exporting
                        //DataPresenterReportControl rc = this.DataPresenter as DataPresenterReportControl;                        
                        DataPresenterExportControlBase rc = this.DataPresenter as DataPresenterExportControlBase;

                        Debug.Assert(null != rc);

                        for (int i = 0; i < count; i++)
                        {
                            Cell associatedCell = associatedCollection._cellArray[i];

                            if (associatedCell != null && associatedCell.HasCloneableSettings)
                            {
                                // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
                                // The fields may not have a name.
                                //
                                //Cell cell = this[associatedCell.Field.Name];
                                Cell cell = null != rc
                                    ? this[rc.GetClonedField(associatedCell.Field)]
                                    : this[associatedCell.Field.Name];

                                if (cell != null)
                                    cell.CloneAssociatedCellSettings(associatedCell);
                            }
                        }
                    }
                }

                #endregion //CloneAssociatedCellSettings	

				#region CreateCell

		// SSP 12/16/08 - NAS9.1 Record Filtering
		// Added CreateCell method for the new derived FilterCellCollection to override.
		// 
		/// <summary>
		/// Creates a new Cell.
		/// </summary>
		/// <param name="fld">Field for which to create new cell.</param>
		/// <returns>New Cell for the specified field.</returns>
		internal virtual Cell CreateCell( Field fld )
		{
			if ( fld.IsUnbound )
				return new UnboundCell( this._record, fld );
			else
				return new Cell( this._record, fld );
		}

				#endregion // CreateCell

				// AS 8/7/09 NA 2009.2 Field Sizing
				#region DirtyFieldAutoSizeVersion
		internal void DirtyFieldAutoSizeVersion()
		{
			if (null != _cellArray)
			{
				// there is no verification of the actual cell elements needed
				// so i added this helper method to just dirty any allocated cells
				foreach (Cell cell in _cellArray)
				{
					if (null != cell)
						cell.DirtyFieldAutoSizeVersion();
				}
			}
		}
				#endregion //DirtyFieldAutoSizeVersion

				#region EnsureAllCellsAreAllocated

		internal void EnsureAllCellsAreAllocated()
		{
			int count = this.Count;

			if ( count < 1 ||
				 count == this._allocatedRecordCount)
				return;

			for (int i = 0; i < count; i++)
			{
				Cell cell = this.GetItem(i);
			}
		}

				#endregion //EnsureAllCellsAreAllocated

				#region GetCellIfAllocated

		internal Cell GetCellIfAllocated(Field fld)
		{
			this.VerifyVersion();

			if ( this._cellArray != null )
			{
				// JJD 8/18/20 - TFS37033
				// Make sure the index is valid before accessing the array
				//return this._cellArray[fld.Index];
				int index = fld.Index;

				if (index >= 0)	
					return this._cellArray[index];
			}

			return null;
		}

		internal Cell GetCellIfAllocated( int index )
		{
			this.VerifyVersion( );

			return null != _cellArray ? _cellArray[index] : null;
		}

				#endregion //GetCellIfAllocated	
    
                #region VerifyVersion

        internal void VerifyVersion()
        {
            FieldCollection fields = this._record.FieldLayout.Fields;

			this._cachedCount = fields.Count;

			if ( this._cellArray == null ||
				 // JJD 8/18/20 - TFS37033 - use the InternalVersion which will always get updated
				 //this._fieldCollectionVersion == fields.Version)
				 this._fieldCollectionVersion == fields.InternalVersion)
                return;

			// JJD 8/18/20 - TFS37033 
			// use the InternalVersion which will always get updated
			// Even between calls to BegineUpdate and EndUpdate on the fields collection
            //this._fieldCollectionVersion = fields.Version;
            this._fieldCollectionVersion = fields.InternalVersion;

            Cell cell;
            int i;
            bool recreate = false;


            // if the # of field has decreased then recreate the sparse array
            if ( this._cachedCount != this._cellArray.Length )
                recreate = true;
            else
            {
                // loop over the cells looking for one that is out of order
                for (i = 0 ; i < this._cachedCount; i++)
                {
                    cell = this._cellArray[i];

                    // if there is a cell in this slot and it's corresponding
                    // field isn't at the same index then we need to recreate the
                    // sparse array so set the flag and break out
                    if ( cell != null  &&
                         cell.Field != fields[i] )
                    {
                        recreate = true;
                        break;
                    }
                }

           }

            // if everything is cool at this point we are in sync and can just exit
            if ( !recreate )
                return;

            // copy the cells into an array
            Cell[] oldCells = this._cellArray;

			// create a new array of the correct size
 			this._cellArray = new Cell[this._cachedCount];

			this._allocatedRecordCount = 0;

            //loop over the array of cells and re-insert them at the appropriate index
            for (i = 0; i < oldCells.Length; i++)
            {
                if (oldCells[i] != null)
                {
                    int index = oldCells[i].Field.Index;

					if (index >= 0)
					{
						this._allocatedRecordCount++;
						this._cellArray[index] = oldCells[i];
					}
                }
            }

        }

                #endregion //VerifyVersion

            #endregion //Internal Methods

        #endregion //Methods

        #region Indexer (int)

        /// <summary>
        /// The <see cref="Cell"/> at the specified zero-based index (read-only)
        /// </summary>
        public Cell this[int index]
        {
            get
            {
				this.VerifyVersion();

				return this.GetItem(index);
            }
            set
            {
				throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
            }
        }

		internal Cell GetItem(int index)
		{
			if (this._cellArray == null)
				this._cellArray = new Cell[this._cachedCount];

			Cell cell = this._cellArray[index];

			if (cell == null)
				this._cellArray[index] = this.AllocateNewCell(index);

			return this._cellArray[index];
		}

        #endregion //Indexer (int)	

        #region Indexer (Field)

        /// <summary>
        /// The <see cref="Cell"/> associated with the specified <see cref="Field"/> (read-only)
        /// </summary>
        public Cell this[Field field]
        {
            get
            {
				int index;

				if ( field.Owner == this._record.FieldLayout )
					index = field.Index;
				else
					index = -1;

				return this[index];
            }
        }

        #endregion //Indexer (Field)	

        #region Indexer (string)

        /// <summary>
        /// The <see cref="Cell"/> associated with the specified <see cref="Field"/> (read-only)
        /// </summary>
        public Cell this[string name]
        {
            get
            {
                int index = this._record.FieldLayout.Fields.IndexOf(name);

				// JJD 6/15/07
				// If the name is a string see if it can be parsed as an int.
				if (index < 0 && name != null && name.Trim().Length > 0)
				{
					if (int.TryParse(name, out index) && index >= 0)
						return this[index];

					index = -1;
				}

				return this[index];
            }
        }

        #endregion //Indexer (string)	

        #region IList Members

        int IList.Add(object value)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        void IList.Clear()
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        bool IList.Contains(object value)
        {
            return this.Contains(value as Cell);
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf(value as Cell);
        }

        void IList.Insert(int index, object value)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return true; }
        }

        void IList.Remove(object value)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        void IList.RemoveAt(int index)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
				throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
            }
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
			this.EnsureAllCellsAreAllocated();

			if (this._cellArray != null)
				Array.Copy(this._cellArray, 0, array, index, this._cellArray.Length);
		}

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return this; }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            this.VerifyVersion();
			return new CellEnumerator(this);
        }

        #endregion

        #region IList<Cell> Members

        void IList<Cell>.Insert(int index, Cell item)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        void IList<Cell>.RemoveAt(int index)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        Cell IList<Cell>.this[int index]
        {
            get
            {
                return this[index] as Cell;
            }
            set
            {
				throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
            }
        }

        #endregion

        #region ICollection<Cell> Members

        bool ICollection<Cell>.IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Adds an item to the collection (not supported)
        /// </summary>
        void ICollection<Cell>.Add(Cell item)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }


        /// <summary>
        /// Clears the collection (not supported)
        /// </summary>
        void ICollection<Cell>.Clear()
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        bool ICollection<Cell>.Remove(Cell cell)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }


        #endregion

        #region IEnumerable<Cell> Members

        /// <summary>
        /// Returms an object to enumerate the items
        /// </summary>
        IEnumerator<Cell> IEnumerable<Cell>.GetEnumerator()
        {
            this.VerifyVersion();
            return new CellEnumerator(this);
        }
 
        #endregion

        #region CellEnumerator

        internal class CellEnumerator : IEnumerator, IEnumerator<Cell>
        {
            CellCollection _collection;
			private int _currentPosition;
			private object _currentItem;

			static object UnsetObjectMarker = new object();

            internal CellEnumerator(CellCollection collection)
            {
 				this._collection = collection;
				this._currentPosition = -1;
				this._currentItem = UnsetObjectMarker;
            }

			public void Dispose()
			{
				this.Reset();
			}

			#region IEnumerator Members

			public bool MoveNext()
			{
				int count = this._collection.Count;

				if (this._currentPosition < count - 1)
				{
					this._currentPosition++;
					this._currentItem = this._collection.GetItem(this._currentPosition);
					return true;
				}

				this._currentPosition = count;
				this._currentItem = UnsetObjectMarker;
				return false;
			}

			public void Reset()
			{
				this._currentPosition = -1;
				this._currentItem = UnsetObjectMarker;
			}

			object IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}

			#endregion

			#region IEnumerator<Record> Members

			public Cell Current
			{
				get
				{
					if (this._currentItem == UnsetObjectMarker)
					{
						if (this._currentPosition == -1)
							throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_25" ) );
						else
							throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_26" ) );
					}

					return this._currentItem as Cell;
				}
			}

			#endregion
		}
        #endregion //CellEnumerator	
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