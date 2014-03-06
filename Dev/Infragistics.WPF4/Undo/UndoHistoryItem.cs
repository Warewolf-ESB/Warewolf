using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infragistics.Undo
{
	/// <summary>
	/// Represents an item in the undo or redo history.
	/// </summary>
	public class UndoHistoryItem : PropertyChangeNotifier
	{
		#region Member Variables

		private UndoUnit _unit;
		private UndoHistoryItemType _itemType;
		private UndoManager _undoManager;
		private string _shortDescription;
		private string _longDescription;

		#endregion //Member Variables

		#region Constructor
		internal UndoHistoryItem(UndoUnit unit, UndoHistoryItemType itemType, UndoManager undoManager)
		{
			CoreUtilities.ValidateNotNull(unit, "unit");
			CoreUtilities.ValidateNotNull(undoManager, "undoManager");

			_unit = unit;
			_itemType = itemType;
			_undoManager = undoManager;
		} 
		#endregion //Constructor

		#region Properties

		#region ItemType
		/// <summary>
		/// Returns an enumeration indicating the type of operation that will be performed.
		/// </summary>
		public UndoHistoryItemType ItemType
		{
			get { return _itemType; }
		}

		#endregion //ItemType

		#region LongDescription
		/// <summary>
		/// Returns a more detailed representation of the item.
		/// </summary>
		/// <seealso cref="ShortDescription"/>
		/// <seealso cref="UndoUnit.GetDescription(UndoHistoryItemType,bool)"/>
		public string LongDescription
		{
			get
			{
				if (null == _longDescription)
					_longDescription = _unit.GetDescription(_itemType, true) ?? string.Empty;

				return _longDescription;
			}
		}
		#endregion //LongDescription

		#region ShortDescription
		/// <summary>
		/// Returns a basic string representation of the item.
		/// </summary>
		/// <seealso cref="LongDescription"/>
		/// <seealso cref="UndoUnit.GetDescription(UndoHistoryItemType,bool)"/>
		public string ShortDescription
		{
			get
			{
				if (null == _shortDescription)
					_shortDescription = _unit.GetDescription(_itemType, false) ?? string.Empty;

				return _shortDescription;
			}
		}
		#endregion //ShortDescription

		#region UndoManager
		/// <summary>
		/// Returns the associated <see cref="UndoManager"/>
		/// </summary>
		public UndoManager UndoManager
		{
			get { return _undoManager; }
		} 
		#endregion //UndoManager

		#region Unit
		/// <summary>
		/// Returns the action that will be invoked.
		/// </summary>
		public UndoUnit Unit
		{
			get { return _unit; }
		}

		#endregion //Unit

		#endregion //Properties

		#region Methods

		#region PerformUndoRedo
		/// <summary>
		/// Executes all the items in the associated history up to and including this instance.
		/// </summary>
		public void Execute()
		{
			this.PerformUndoRedo(true);
		}
		#endregion //PerformUndoRedo

		#region Internal Methods

		#region CanUndoRedo
		internal bool CanUndoRedo()
		{
			var list = this.ItemType == UndoHistoryItemType.Redo ? _undoManager.RedoHistory : _undoManager.UndoHistory;
			return list.IndexOf(this) >= 0;
		}
		#endregion //CanUndoRedo

		#region PerformUndoRedo
		internal void PerformUndoRedo(bool validate)
		{
			bool isRedo = _itemType == UndoHistoryItemType.Redo;
			var list = isRedo ? _undoManager.RedoHistory : _undoManager.UndoHistory;
			int index = list.IndexOf(this);

			if (index >= 0)
				_undoManager.PerformUndoRedo(index + 1, !isRedo);
			else if (validate)
				throw new InvalidOperationException(Utils.GetString("LE_HistoryItemNotInCurrentHistory"));
		}
		#endregion //PerformUndoRedo

		#region ResetDescription
		internal void ResetDescription()
		{
			if (_shortDescription != null)
			{
				_shortDescription = null;
				this.OnPropertyChanged("ShortDescription");
			}

			if (_longDescription != null)
			{
				_longDescription = null;
				this.OnPropertyChanged("LongDescription");
			}
		}
		#endregion //ResetDescription

		#endregion //Internal Methods

		#endregion //Methods
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