using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Windows.DataPresenter
{
    /// <summary>
    /// Abstract class used to provide the ability to undelete records that were deleted through the <see cref="DataPresenterBase"/> control.
    /// </summary>
	/// <remarks>
	/// <p class="body">The DataPresenter controls provide the end-user with the ability to delete records through 
	/// the user interface if the underlying data source supports deleting records. When undo functionality is enabled 
	/// (see <see cref="DataPresenterBase.IsUndoEnabled"/>), you may allow the end user to undelete these records. To 
	/// enable this capability, the <see cref="Infragistics.Windows.DataPresenter.Events.RecordsDeletingEventArgs.UndeleteStrategy"/> property must be set to 
	/// a derived instance of a UndeleteRecordsStrategy that can perform the undeletion for the 
	/// <see cref="Infragistics.Windows.DataPresenter.Events.RecordsDeletingEventArgs.Records"/> that are about to be deleted.</p>
	/// <p class="body">When an undo of deleted records is to be performed, the <see cref="Undelete"/> method is 
	/// invoked. Derived classes implement this method and perform the undeletion either recreating new objects 
	/// that have the same information as the deleted records or when possible reinserting the deleted records into 
	/// the datasource. After the undeletion is performed, the <see cref="ProcessUndeletedRecords"/> method is invoked 
	/// to allow you to set properties on the records such as the values of unbound cells.</p>
	/// <p class="body">If the records that were deleted had child records and the <see cref="RestoreDescendantActions"/> 
	/// property returns true when the records are being deleted, the DataPresenter will attempt to reconnect any 
	/// undo/redo actions associated with the descendants of the records that were deleted. If any actions were associated 
	/// with the descendant records, the <see cref="ProvideDescendantMapping"/> method will be invoked to provide a 
	/// means to map the old record information to the new dataitem. By default, this will only work if the underlying 
	/// data items for the descendant records do not change.</p>
	/// </remarks>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Events.RecordsDeletingEventArgs.UndeleteStrategy"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Events.RecordsDeletingEventArgs"/>
	/// <seealso cref="DataPresenterBase.IsUndoEnabled"/>
	/// <seealso cref="DataPresenterCommands.DeleteSelectedDataRecords"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ClipboardSupport)]
    public abstract class UndeleteRecordsStrategy
    {
        #region Constructor
        /// <summary>
        /// Initializes a new <see cref="UndeleteRecordsStrategy"/>
        /// </summary>
        protected UndeleteRecordsStrategy()
        {
        }
        #endregion //Constructor

		#region Properties

		#region RestoreDescendantActions
		/// <summary>
		/// Returns a boolean indicating whether actions associated with descendant records of the 
		/// records being deleted should be restored.
		/// </summary>
		/// <seealso cref="ProvideDescendantMapping"/>
		internal protected virtual bool RestoreDescendantActions
		{
			get { return true; }
		}
		#endregion //RestoreDescendantActions

		#endregion //Properties

		#region Methods

		#region CanUndelete

		/// <summary>
        /// Invoked to determine if an undeletion of the specified data items is possible.
        /// </summary>
		/// <param name="oldRecords">A list of objects containing information about the records that were deleted.</param>
		/// <seealso cref="Undelete"/>
        public abstract bool CanUndelete(IList<RecordInfo> oldRecords);

        #endregion //CanUndelete

        #region Undelete

        /// <summary>
        /// Invoked to perform an undeletion of records associated with the specified data items.
        /// </summary>
		/// <param name="oldRecords">A list of objects containing information about the records that were deleted.</param>
		/// <returns>Returns a dictionary that provides a mapping from the provided RecordInfo instances to the new data items.</returns>
		/// <seealso cref="CanUndelete"/>
        public abstract IDictionary<RecordInfo, object> Undelete(IList<RecordInfo> oldRecords);

        #endregion //Undelete

		#region ProcessUndeletedRecords
		/// <summary>
		/// Invoked after the call to <see cref="Undelete(IList{RecordInfo})"/> to allow additional processing of the new records.
		/// </summary>
		/// <param name="recordsCreated">A list of the <see cref="DataRecord"/> instances for the undeleted records based on the specified mapping.</param>
		public virtual void ProcessUndeletedRecords(IList<DataRecord> recordsCreated)
		{
		}

		#endregion //ProcessUndeletedRecords

		#region ProvideDescendantMapping
		/// <summary>
		/// Invoked after the records are deleted to allow providing a mapping between the old dataitems for the descendant records and that of the new descendant records.
		/// </summary>
		/// <param name="descendants">A list of <see cref="RecordInfo"/> instances that represent the descendant records</param>
		/// <returns>A dictionary that provides map between the old data item and the new data item for the descendant records or null to indicate that the 
		/// old items are not valid and therefore no mapping could take place.</returns>
		/// <remarks>
		/// <p class="body">This method is invoked if the <see cref="RestoreDescendantActions"/> returns true and there were 
		/// undo or redo actions associated with one or more descendants of the records that were deleted. This method provides 
		/// a means to indicate what new data items are now associated with the previous data items. By default, this method will 
		/// assume that the same data item instance is being used.</p>
		/// <p class="note">Note, this method may be called multiple times since it will be invoked separately for each 
		/// set of descendant records. For example, if there were grandchild records of the delete records, this will first 
		/// be invoked with the record info for the child records (i.e. the ancestors of those grandchild records).</p>
		/// </remarks>
		/// <seealso cref="RestoreDescendantActions"/>
		public virtual IDictionary<RecordInfo, object> ProvideDescendantMapping(IList<RecordInfo> descendants)
		{
			Dictionary<RecordInfo, object> mapping = new Dictionary<RecordInfo, object>();

			foreach (RecordInfo recordInfo in descendants)
			{
				mapping[recordInfo] = recordInfo.DataItem;
			}

			return mapping;
		} 
		#endregion //ProvideDescendantMapping

		#endregion //Methods

		#region RecordInfo class
		/// <summary>
		/// Class used to provide information about the records to be undeleted.
		/// </summary>
		public class RecordInfo : IEquatable<RecordInfo>
		{
			#region Member Variables

			private object _dataItem;
			private RecordManager _recordManager;
			private int _dataItemIndex;

			private Dictionary<Field, IList<RecordInfo>> _childRecords;

			#endregion //Member Variables

			#region Constructor
			private RecordInfo(DataRecord record)
			{
				_dataItem = record.DataItem;

				Debug.Assert(null != _dataItem);

				_dataItemIndex = record.DataItemIndex;
			}
			#endregion //Constructor

			#region Base class overrides
			/// <summary>
			/// Overridden. Returns the hash code.
			/// </summary>
			/// <returns>Returns the hash code.</returns>
			public override int GetHashCode()
			{
				return _dataItem.GetHashCode();
			}

			/// <summary>
			/// Overridden. Returns true if the specified object equals this object.
			/// </summary>
			/// <param name="obj">Object to compare.</param>
			/// <returns>True if objects are equal.</returns>
			public override bool Equals(object obj)
			{
				return this.Equals(obj as RecordInfo);
			}
			#endregion //Base class overrides

			#region Properties

			internal Dictionary<Field, IList<RecordInfo>> Children
			{
				get { return _childRecords; }
			}

			/// <summary>
			/// Returns the associated object from the deleted record's <see cref="DataRecord.DataItem"/>
			/// </summary>
			public object DataItem
			{
				get { return _dataItem; }
			}

			/// <summary>
			/// Returns the original <see cref="DataRecord.DataItemIndex"/> at the time when the record was being deleted.
			/// </summary>
			public int DataItemIndex
			{
				get { return _dataItemIndex; }
			}

			/// <summary>
			/// Returns the underlying data item.
			/// </summary>
			internal object DataItemForComparison
			{
				get { return Infragistics.Windows.Internal.DataBindingUtilities.GetObjectForComparison(_dataItem); }
			}

			/// <summary>
			/// Returns the <see cref="RecordManager"/> associated with the item.
			/// </summary>
			/// <remarks>
			/// <p class="note"><b>Note:</b> Whereas the other properties identify the information of the 
			/// record at the time it was deleted, this property is updated, when possible, to reflect the 
			/// RecordManager to which the records would belong if they weren't deleted. This is usually used 
			/// during the <see cref="UndeleteRecordsStrategy.Undelete"/> method to determine the 
			/// <see cref="Infragistics.Windows.DataPresenter.RecordManager.SourceItems"/> to which the items 
			/// should be added during the undelete.</p>
			/// </remarks>
			public RecordManager RecordManager
			{
				get { return _recordManager; }
				internal set { _recordManager = value; }
			}
			#endregion //Properties

			#region Methods

			#region AddChildren
			internal void AddChildren(DataRecord record, DescendantRecordInfo descendantInfo)
			{
				Debug.Assert(record != null && record.DataItem == _dataItem);
				IList<Field> expandableFields = descendantInfo.GetExpandableFields(record.FieldLayout);

				if (null == expandableFields || expandableFields.Count == 0)
					return;

				foreach (Field field in expandableFields)
				{
					ExpandableFieldRecord expandableFieldRecord = record.ChildRecords[field];

					// see if there were any actions associated with the expandablefield record
					IList<Record> children = descendantInfo.GetChildren(expandableFieldRecord);

					if (null == children || children.Count == 0)
						continue;

					// if there are then we need to create RecordInfos to represent the children
					// so we can remap those items to the new records after the undelete
					if (_childRecords == null)
						_childRecords = new Dictionary<Field, IList<RecordInfo>>();

					// AS 5/27/09
					// Changed from array to list since some datarecords may not be able to
					// have RecordInfo's created - e.g. filter records.
					//
					List<RecordInfo> recordInfos = new List<RecordInfo>(children.Count);

					for (int i = 0, count = children.Count; i < count; i++)
					{
						DataRecord dr = children[i] as DataRecord;

						Debug.Assert(null != dr);

						RecordInfo ri = RecordInfo.Create(dr);

						if (null != ri)
						{
							recordInfos.Add(ri);

							// store any of its descendant info as well
							ri.AddChildren(dr, descendantInfo);
						}
					}

					_childRecords[field] = recordInfos;
				}
			} 
			#endregion //AddChildren

			#region Create
			internal static RecordInfo Create(DataRecord record)
			{
				if (record == null)
					return null;

				// AS 5/27/09
				// Instead of checking IsAddRecordTemplate, check if its a DataRecord since
				// we rely on the data item for comparison.
				//
				if (!record.IsDataRecord)
					return null;

				RecordInfo ri = new RecordInfo(record);

				ri.RecordManager = record.RecordManager;

				return ri;
			}
			#endregion //Create

			#endregion //Methods

			#region IEquatable<RecordInfo>

			/// <summary>
			/// Returns true if the object is a <see cref="RecordInfo"/> with the same data item.
			/// </summary>
			/// <param name="other">The UndeleteRecordInfo instance to compare to.</param>
			/// <returns>True if the object represent the same data item</returns>
			public bool Equals(RecordInfo other)
			{
				return null != other &&
					ClipboardOperationInfo.IsSameDataItem(other._dataItem, this._dataItem);
			}

			#endregion //IEquatable<RecordInfo>
		}
		#endregion //RecordInfo class
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