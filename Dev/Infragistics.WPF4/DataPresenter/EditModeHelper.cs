using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Windows.DataPresenter
{
	// AS 7/27/09 NA 2009.2 Field Sizing
	internal class EditModeHelper
	{
		#region Member Variables

		private DataPresenterBase _owner;
		private CellValuePresenter _cvp;
		private Field.FieldResizeInfo _autoSizeInfo;
		private FieldResizeInfoAction _resizeUndoAction;

		#endregion //Member Variables

		#region Constructor
		internal EditModeHelper(DataPresenterBase owner)
		{
			Debug.Assert(null != owner);
			_owner = owner;
		} 
		#endregion //Constructor

		#region Properties

		#region CellValuePresenter
		internal CellValuePresenter CellValuePresenter
		{
			get { return _cvp; }
		}
		#endregion //CellValuePresenter

		#region PreEditAutoSize
		internal Field.FieldResizeInfo PreEditAutoSize
		{
			get { return _autoSizeInfo; }
		} 
		#endregion //PreEditAutoSize

		#endregion //Properties

		#region Methods

		#region ClearCellValuePresenter
		internal void ClearCellValuePresenter()
		{
			_cvp = null;
		} 
		#endregion //ClearCellValuePresenter

		#region CreateCommitEditUndoAction
		/// <summary>
		/// Used to create a composite action to undo a commit of an edit change to undo the size 
		/// change that may have occurred when the field was resized as a result of editing the cell.
		/// </summary>
		internal DataPresenterAction CreateCommitEditUndoAction(DataPresenterAction editAction)
		{
			Debug.Assert(null != editAction);

			if (_resizeUndoAction != null)
				return new DataPresenterCompositeAction(editAction, _resizeUndoAction);

			return editAction;
		} 
		#endregion //CreateCommitEditUndoAction

		#region EndEditMode
		internal void EndEditMode(bool acceptChanges, bool force)
		{
			if (null != _cvp)
				_cvp.EndEditMode(acceptChanges, force);
		} 
		#endregion //EndEditMode

		#region IsEditCell
		internal bool IsEditCell(CellValuePresenter cvp)
		{
			return cvp == _cvp;
		}
		#endregion //IsEditCell

		#region OnEditValueChanged
		internal void OnEditValueChanged(CellValuePresenter cvp)
		{
			if (_autoSizeInfo != null)
				this.ProcessEditAutoSize(cvp);
		} 
		#endregion //OnEditValueChanged

		#region OnEndEditMode
		internal void OnEndEditMode(CellValuePresenter cvp, bool changesAccepted)
		{
			Debug.Assert(cvp == _cvp);

			if (cvp != _cvp)
				return;

			Field.FieldResizeInfo autoSizeInfo = _autoSizeInfo;

			// clear out any cached copy of the previous autosize info
			_autoSizeInfo = null;
			_cvp = null;
			_resizeUndoAction = null;

			// note, I'm only doing this when the edit was cancelled to avoid a 
			// resize after committing the edit because the edit template may 
			// be bigger than the render template
			// size
			if (!changesAccepted)
				this.ProcessEditAutoSize(cvp, autoSizeInfo);
		}
		#endregion //OnEndEditMode

		#region OnEnterEditMode
		internal void OnEnterEditMode(CellValuePresenter cvp)
		{
			Debug.Assert(null != cvp && _cvp == null);

			if (null != _cvp)
				OnEndEditMode(_cvp, true);

			_cvp = cvp;

			Field f = cvp.Field;
			Debug.Assert(null != f);
			FieldLayout fl = f.Owner;

			// if this field is in auto size mode then we want to store the original 
			// autosize info to use as the minimum extent
			if (fl.AutoSizeInfo.IsInAutoSizeMode(f, false))
			{
				_autoSizeInfo = f.ExplicitResizeInfo.Clone();

				_resizeUndoAction = FieldResizeInfoAction.Create(fl, !fl.IsHorizontal);
			}
			else
				_autoSizeInfo = null;

			// commenting this out for now. in theory the cell may need to get 
			// bigger when it goes into edit mode because the edit template may 
			// be bigger than the render template (e.g. a dropdown button is only 
			// visible while in edit mode) but then you would get a jump in the cell 
			// size as you were navigating around using the tab key
			//
			//this.ProcessEditAutoSize(cvp);
		}
		#endregion //OnEnterEditMode

		#region ProcessEditAutoSize
		private void ProcessEditAutoSize(CellValuePresenter cvp)
		{
			this.ProcessEditAutoSize(cvp, _autoSizeInfo);
		}

		private void ProcessEditAutoSize(CellValuePresenter cvp, Field.FieldResizeInfo autoSizeInfo)
		{
			if (null != cvp)
			{
				Field f = cvp.Field;
				FieldLayout fl = f.Owner;

				fl.AutoSizeInfo.OnEditCellValueChanged(cvp, autoSizeInfo);
			}
		} 
		#endregion //ProcessEditAutoSize

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