using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.DataPresenter;
using System.Windows;
using System.Windows.Automation.Provider;
using System.Windows.Automation;
using System.Diagnostics;
using Infragistics.AutomationPeers;

namespace Infragistics.Windows.Automation.Peers.DataPresenter
{
	/// <summary>
	/// Exposes <see cref="Cell"/> types to UI Automation
	/// </summary>
	public class CellAutomationPeer : AutomationPeerProxy,
		ITableItemProvider,
		IValueProvider,
		ISelectionItemProvider,
		IScrollItemProvider
	{
		#region Member Variables

		private Cell _cell;

		// JM 08-20-09 NA 9.2 EnhancedGridView	
		private IRecordListAutomationPeer _recordListAutomationPeer;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Creates a new instance of the <see cref="RecordAutomationPeer"/> class
		/// </summary>
		/// <param name="cell">The <see cref="Cell"/> for which the peer is being created</param>
		/// <param name="recordListAutomationPeer">The <see cref="IRecordListAutomationPeer"/> that is ultimately controlling the <see cref="CellAutomationPeer"/> being created.</param>
		// JM 08-20-09 NA 9.2 EnhancedGridView	
		//public CellAutomationPeer(Cell cell)
		public CellAutomationPeer(Cell cell, IRecordListAutomationPeer recordListAutomationPeer)
		{
			if (null == cell)
				throw new ArgumentNullException("cell");

			this._cell = cell;

			// JM 08-20-09 NA 9.2 EnhancedGridView	
			if (null == recordListAutomationPeer)
				throw new ArgumentNullException("recordListAutomationPeer");
			this._recordListAutomationPeer = recordListAutomationPeer;
		}
		#endregion //Constructor

		#region Base class overrides

		#region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>Custom</b> enumeration value</returns>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.Custom;
		}
		#endregion //GetAutomationControlTypeCore

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="Cell"/>
		/// </summary>
		/// <returns>A string that contains 'Cell'</returns>
		protected override string GetClassNameCore()
		{
			return "Cell";
		}
		#endregion //GetClassNameCore

		#region GetLocalizedControlTypeCore
		/// <summary>
		/// Gets the localized version of the control type for the <see cref="Infragistics.Windows.DataPresenter.Cell"/> that is associated with this <see cref="CellAutomationPeer"/>.
		/// </summary>
		/// <returns>A string that contains "cell".</returns>
		protected override string GetLocalizedControlTypeCore()
		{
			return "cell";
		} 
		#endregion //GetLocalizedControlTypeCore

		#region GetNameCore
		/// <summary>
		/// Returns the text label for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The text label</returns>
		protected override string GetNameCore()
		{
			string name = base.GetNameCore();
			
			if (string.IsNullOrEmpty(name))
			{
				name = ((IValueProvider)this).Value;
			}

			return name;
		}
		#endregion //GetNameCore

		#region GetPattern
		/// <summary>
		/// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the <see cref="CellValuePresenter"/> that corresponds with this <see cref="CellValuePresenterAutomationPeer"/>.
		/// </summary>
		/// <param name="patternInterface">The pattern being requested</param>
		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.TableItem)
				return this;
				
			if (patternInterface == PatternInterface.GridItem	||
				patternInterface == PatternInterface.Value		||
				patternInterface == PatternInterface.ScrollItem ||
				patternInterface == PatternInterface.SelectionItem)
			{
				return this;
			}

			return base.GetPattern(patternInterface);
		}
		#endregion //GetPattern

		#region GetUnderlyingPeer
		/// <summary>
		/// Returns the automation peer for which this proxy is associated.
		/// </summary>
		/// <returns>A <see cref="CellValuePresenterAutomationPeer"/></returns>
		protected override AutomationPeer GetUnderlyingPeer()
		{
			AutomationPeer peer = null;
			UIElement element = this._cell.AssociatedCellValuePresenter;

			if (element != null)
			{
				peer = UIElementAutomationPeer.CreatePeerForElement(element);

				if (peer == null)
				{
					if (element is FrameworkElement)
						peer = new FrameworkElementAutomationPeer((FrameworkElement)element);
					else
						peer = new UIElementAutomationPeer(element);
				}
			}

			if (null != peer)
				peer.EventsSource = this;

			return peer;
		}
				#endregion //GetUnderlyingPeer

		#region IsEnabledCore
		/// <summary>
		/// Returns a value indicating whether the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> can receive and send events.
		/// </summary>
		/// <returns><b>True</b> if the <see cref="UIElement"/> can send and receive events; otherwise, <b>false</b>.</returns>
		protected override bool IsEnabledCore()
		{
			// AS 9/4/09 TFS18355
			if (_cell.Record == null || _cell.Record.IsEnabledResolved == false)
				return false;

			AutomationPeer peer = this.GetUnderlyingPeer();
			return peer != null
				? peer.IsEnabled()
				: true;
		}
		#endregion //IsEnabledCore

		#region SetFocusCore
		/// <summary>
		/// Sets the keyboard input focus on the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		protected override void SetFocusCore()
		{
			// bring the item into view or the element won't be available
			((IScrollItemProvider)this).ScrollIntoView();

			base.SetFocusCore();
		}
		#endregion //SetFocusCore

		#endregion //Base class overrides

		#region Properties

		#region FieldIndex
		private int FieldIndex
		{
			get 
			{
				RecordAutomationPeer recordPeer = this._cell.Record != null
					? this._cell.Record.AutomationPeer
					: null;

				// JM 08-20-09 NA 9.2 EnhancedGridView
				//HeaderAutomationPeer headerPeer = recordPeer != null
				//    ? recordPeer.ListAutomationPeer.GetHeaderPeer(this._cell.Record)
				//    : null;
				HeaderAutomationPeer headerPeer = recordPeer != null
					? recordPeer.RecordListAutomationPeer.GetHeaderPeer(this._cell.Record)
					: null;

				return headerPeer != null
					? headerPeer.GetFieldIndex(this._cell.Field)
					: -1;
			}
		} 
		#endregion //FieldIndex

		#region HeaderItem
		private IRawElementProviderSimple[] HeaderItems
		{
			get
			{
				// JM 08-20-09 NA 9.2 EnhancedGridView
				//RecordListControlAutomationPeer listPeer = this.ListControlPeer;
				IRecordListAutomationPeer listPeer = this.ListControlPeer;

				if (null != listPeer)
				{
					HeaderAutomationPeer headerPeer = listPeer.GetHeaderPeer(this._cell.Record);

					if (null != headerPeer)
					{
						LabelAutomationPeer label = headerPeer.GetHeaderItem(this._cell);

						if (null != label)
							return new IRawElementProviderSimple[] { this.ProviderFromPeer(label) };
					}
				}

				return null;
			}
		} 
		#endregion //HeaderItem

		#region IsHorizontalRowLayout
		internal bool IsHorizontalRowLayout
		{
			get
			{
				// JM 08-20-09 NA 9.2 EnhancedGridView
				//RecordListControlAutomationPeer listPeer = this.ListControlPeer;
				IRecordListAutomationPeer listPeer = this.ListControlPeer;

				return listPeer != null
					? listPeer.IsHorizontalRowLayout
					: false;
			}
		}
		#endregion //IsHorizontalRowLayout

		#region ListControlPeer
		// JM 08-20-09 NA 9.2 EnhancedGridView
		//private RecordListControlAutomationPeer ListControlPeer
		private IRecordListAutomationPeer ListControlPeer
		{
			get
			{
				// JM 08-20-09 NA 9.2 EnhancedGridView
				//Record record = this._cell.Record;
				//
				//RecordListControl rlc = record != null ? record.ParentRecordList : null;
				//return rlc != null
				//    ? UIElementAutomationPeer.FromElement(rlc) as RecordListControlAutomationPeer
				//    : null;
				return this._recordListAutomationPeer;
			}
		}
		#endregion //ListControlPeer

		#region RecordIndex
		private int RecordIndex
		{
			get
			{
				// JM 08-20-09 NA 9.2 EnhancedGridView
				//RecordListControlAutomationPeer listPeer = this.ListControlPeer;
				IRecordListAutomationPeer listPeer = this.ListControlPeer;

				Debug.Assert(listPeer != null, "Unable to get to the listpeer so we cannot know what row to return!");

				return listPeer != null
					? listPeer.GetTableRowIndex(this._cell)
					: 0;
			}
		} 
		#endregion //RecordIndex

		#endregion //Properties

		#region Methods

		#region GetValueAsText
		private string GetValueAsText(object value)
		{
			CellValuePresenter cvp = this._cell.AssociatedCellValuePresenter;
			string valueAsText;
			Exception error;
			bool converted;

			if (cvp != null && cvp.Editor != null)
			{
				converted = cvp.Editor.ConvertValueToText(value, out valueAsText, out error);
			}
			else
			{
				try
				{
					// AS 9/1/09 Optimization
					// This routine can generate first chance exceptions with DBNull. We should probably
					// just let the field do the conversion as we do for other scenarios.
					//
					//valueAsText = (string)Utilities.ConvertDataValue(value, typeof(string), null, null);
					CellTextConverterInfo textConverter = CellTextConverterInfo.GetCachedConverter(_cell.Field);
					valueAsText = textConverter.ConvertCellValue(value);
					converted = true;
				}
				catch (Exception ex)
				{
					valueAsText = string.Empty;
					error = ex;
					converted = false;
				}
			}

			return converted
				? valueAsText
				: string.Empty;

		}
		#endregion //GetValueAsText

		#region RaiseValuePropertyChangedEvent
		internal void RaiseValuePropertyChangedEvent(object oldValue, object newValue)
		{
			string oldTextValue = GetValueAsText(oldValue);
			string newTextValue = GetValueAsText(newValue);

			if (oldTextValue != newTextValue)
			{
				this.RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, oldTextValue, newTextValue);
			}
		}
		#endregion //RaiseValuePropertyChangedEvent

		#endregion //Methods

		#region ITableItemProvider

		IRawElementProviderSimple[] ITableItemProvider.GetColumnHeaderItems()
		{
			return this.IsHorizontalRowLayout
				? null
				: this.HeaderItems;
		}

		IRawElementProviderSimple[] ITableItemProvider.GetRowHeaderItems()
		{
			return this.IsHorizontalRowLayout
				? this.HeaderItems
				: null;
		}

		#endregion //ITableItemProvider

		#region IGridItemProvider

		int IGridItemProvider.Column
		{
			get
			{
				return this.IsHorizontalRowLayout
					? this.RecordIndex
					: this.FieldIndex;
			}
		}

		int IGridItemProvider.ColumnSpan
		{
			get { return 1; }
		}

		IRawElementProviderSimple IGridItemProvider.ContainingGrid
		{
			get
			{
				// JM 08-20-09 NA 9.2 EnhancedGridView
				//RecordListControlAutomationPeer listPeer = this.ListControlPeer;
				IRecordListAutomationPeer listPeer = this.ListControlPeer;

				return listPeer != null
					? base.ProviderFromPeer(listPeer.GetContainingGrid(this._cell))
					: null;
			}
		}

		int IGridItemProvider.Row
		{
			get
			{
				return this.IsHorizontalRowLayout
					? this.FieldIndex
					: this.RecordIndex;
			}
		}

		int IGridItemProvider.RowSpan
		{
			get { return 1; }
		}

		#endregion //IGridItemProvider

		#region IValueProvider

		bool IValueProvider.IsReadOnly
		{
			get
			{
				return this._cell.IsEditingAllowed == false;
			}
		}

		void IValueProvider.SetValue(string value)
		{
			if (this.IsEnabled() == false)
				throw new ElementNotEnabledException();

			this._cell.IsActive = true;

			if (this._cell.IsActive == false)
				throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_1" ) );

			// bring the cell into view
			((IScrollItemProvider)this).ScrollIntoView();

			CellValuePresenter cvp = this._cell.AssociatedCellValuePresenter;

			Debug.Assert(null != cvp, "We couldn't get to the cell!");

			if (cvp == null)
				throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_2" ) );

			if (cvp.StartEditMode() == false)
				throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_2" ) );

			try
			{
				cvp.Editor.Text = value;
			}
			catch (Exception ex)
			{
				if (ex is ArgumentException)
					throw;
				else
					throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_1" ), ex );
			}
		}

		string IValueProvider.Value
		{
			get
			{
				object value = this._cell.Value;
				return this.GetValueAsText(value);
			}
		}

		#endregion //IValueProvider

		#region IScrollItemProvider

		void IScrollItemProvider.ScrollIntoView()
		{
			bool scrolledIntoView = false;

			Record record = this._cell.Record;

			if (record != null)
			{
				Record parent = record.ParentRecord;

				// make sure the record can be in view
				if (null != parent)
					parent.IsExpanded = true;

				DataPresenterBase dp = record.DataPresenter;
				IViewPanel panelNavigator = dp != null ? dp.CurrentPanel as IViewPanel : null;
				scrolledIntoView = panelNavigator.EnsureRecordIsVisible(record);

				if (scrolledIntoView)
				{
					dp.UpdateLayout();
					dp.BringCellIntoView(this._cell);
					scrolledIntoView = true;
				}
			}

			if (false == scrolledIntoView)
				throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_3", this._cell.GetType( ).Name ) );
		}

		#endregion //IScrollItemProvider

		#region ISelectionItemProvider

		void ISelectionItemProvider.AddToSelection()
		{
			if (this.IsEnabled() == false)
				throw new ElementNotEnabledException();

			// JM 08-20-09 NA 9.2 EnhancedGridView
			//RecordListControlAutomationPeer peer = this.ListControlPeer;
			IListAutomationPeer peer = this.ListControlPeer as IListAutomationPeer;
			
			ISelectionProvider provider = peer != null
				? peer.GetPattern(PatternInterface.Selection) as ISelectionProvider
				: null;

			if (null != provider &&
				provider.CanSelectMultiple == false &&
				provider.GetSelection() != null)
			{
				throw new InvalidOperationException();
			}

			// select the cell
			this._cell.IsSelected = true;
		}

		bool ISelectionItemProvider.IsSelected
		{
			get { return this._cell.IsSelected; }
		}

		void ISelectionItemProvider.RemoveFromSelection()
		{
			if (this.IsEnabled() == false)
				throw new ElementNotEnabledException();

			this._cell.IsSelected = false;
		}

		void ISelectionItemProvider.Select()
		{
			if (this.IsEnabled() == false)
				throw new ElementNotEnabledException();

			DataPresenterBase dp = this._cell.DataPresenter;

			if (null != dp)
				dp.InternalSelectItem(this._cell, true, true);
		}

		IRawElementProviderSimple ISelectionItemProvider.SelectionContainer
		{
			get
			{
				// JM 08-20-09 NA 9.2 EnhancedGridView
				//RecordListControlAutomationPeer peer = this.ListControlPeer;
				AutomationPeer peer = this.ListControlPeer as AutomationPeer;
				return peer != null
					? this.ProviderFromPeer(peer)
					: null;
			}
		}
		#endregion // ISelectionItemProvider
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