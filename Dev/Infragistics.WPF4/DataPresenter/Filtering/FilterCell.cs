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
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Infragistics.Shared;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Internal;
using Infragistics.Windows.DataPresenter.Internal;
using Infragistics.Windows.Reporting;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.Editors;
using System.Windows.Markup;
using System.ComponentModel;

namespace Infragistics.Windows.DataPresenter
{

	#region FilterCell Class

	/// <summary>
	/// Class used to represent a cell in the FilterRecord.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>Note</b> that to actually get or specify filter conditions, use the FieldLayout's
	/// <see cref="FieldLayout.RecordFilters"/> or RecordManager's <see cref="RecordManager.RecordFilters"/>
	/// properties. Any changes made by the user through the FilterCell will be reflected in one of those
	/// collections (depending on the setting of the <see cref="FieldLayoutSettings.RecordFilterScope"/>
	/// property).
	/// </para>
	/// </remarks>
	/// <seealso cref="FilterRecord"/>
	/// <seealso cref="FieldSettings.AllowRecordFiltering"/>
	/// <seealso cref="FieldLayoutSettings.FilterUIType"/>
	/// <seealso cref="FieldLayout.RecordFilters"/>
	/// <seealso cref="RecordManager.RecordFilters"/>
	public class FilterCell : Cell
	{
		#region Member Vars

		private bool _cachedHasActiveFilters;
		private ResolvedRecordFilterCollection.FieldFilterInfo _fieldFilterInfo;

		#endregion // Member Vars

		#region Constructors

		internal FilterCell( FilterRecord record, Field field )
			: base( record, field )
		{
		}

		#endregion //Constructors

		#region Base class overrides

        // JJD 2/17/09 - TFS13820
        #region IsTabStop






        internal override bool IsTabStop
        {
            get
            {
                switch (this.Field.FilterOperandUITypeResolved)
                {
                    case FilterOperandUIType.Disabled:
                    case FilterOperandUIType.None:
                        return false;
                }

                return base.IsTabStop;
            }
        }

        #endregion // IsTabStop

        #region OnActiveCellChanged

        internal override void OnActiveCellChanged()
        {
            base.OnActiveCellChanged();

            if (this.IsActive == false)
            {
                switch (this.Field.FilterEvaluationTriggerResolved)
                {
                    case FilterEvaluationTrigger.OnLeaveCell:
                    case FilterEvaluationTrigger.OnEnterKeyOrLeaveCell:
                        this.RecordFilter.ParentCollection.ApplyPendingFilters();
                        break;
                }
            }
        }

        #endregion //OnActiveCellChanged

        #region IsSelectable






        internal override bool IsSelectable
        {
            get
            {
                 return false;
            }
        }

        #endregion // IsSelectable

		#region ToString

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		public override string ToString( )
		{
			StringBuilder sb = new StringBuilder( );

			sb.Append( "FilterCell: " );
			sb.Append( this.Field.ToString( ) );
			sb.Append( ", " );
			sb.Append( this.Record.ToString( ) );

			return sb.ToString( );
		}

		#endregion //ToString

		#region ValueInternal

        // JJD 2/17/09 - TFS14029
        // Made property internal
        // /// <summary>
		// /// Overridden. Gets/sets the value of the filter cell.
		// /// </summary>
		//public override object Value
		internal object ValueInternal
		{
			get
			{
				return this.RecordFilter.CurrentUIOperand;
			}
			set
			{
                this.RecordFilter.CurrentUIOperand = value;
                this.RaisePropertyChangedEvent("Value");
			}
		}

		#endregion // ValueInternal

		#endregion Base class overrides

		#region Properties

		#region Public Properties

		#region Record

		/// <summary>
		/// Returns the associated filter record.
		/// </summary>
		/// <seealso cref="FilterRecord"/>
		public new FilterRecord Record
		{
			get
			{
				return (FilterRecord)base.Record;
			}
		}

		#endregion // Record

		#region HasActiveFilters

		/// <summary>
		/// Returns true if there are active filters.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>HasActiveFilters</b> property indicates whether there are active filters with which records are
		/// currently being filtered. It doesn't necessarily mean that any records are actually filtered out 
		/// (for example if all records match the filter criteria). It just means that there are current filter 
		/// criteria.
		/// </para>
		/// </remarks>
        /// <seealso cref="ClearActiveFilters()"/>
		public bool HasActiveFilters
		{
			get
			{
				this.VerifyHasActiveFilters( );

				return _cachedHasActiveFilters;
			}
		}

		#endregion // HasActiveFilters

		#endregion //Public Properties

		#region Internal Properties

		#region FieldFilterInfo

		internal ResolvedRecordFilterCollection.FieldFilterInfo FieldFilterInfo
		{
			get
			{
				if ( null == _fieldFilterInfo )
				{
					ResolvedRecordFilterCollection resolvedFilterColl = this.Record.Filters;
					if ( null != resolvedFilterColl )
					{
						_fieldFilterInfo = new ResolvedRecordFilterCollection.FieldFilterInfo( resolvedFilterColl, this.Field );

						_fieldFilterInfo.PropertyChanged += new PropertyChangedEventHandler( OnFieldFilterInfo_PropertyChanged );						
					}
				}

				return _fieldFilterInfo;
			}
		}

		private void OnFieldFilterInfo_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			string propName = e.PropertyName;
			switch ( propName )
			{
				case "HasActiveFilters":
					this.VerifyHasActiveFilters( );
					break;
				case "RecordFilter":
				{
					// AS 8/4/09 NA 2009.2 Field Sizing
					this.NotifyAutoSizeInfo();

					this.RaisePropertyChangedEvent( "RecordFilter" );
					break;
				}
				// AS 8/4/09 NA 2009.2 Field Sizing
				case "RecordFilterVersion":
					this.NotifyAutoSizeInfo();
					break;
			}
		}

		#endregion // FieldFilterInfo

		#region RecordFilter

		/// <summary>
		/// Returns the associated RecordFilter instance. If none is allocated, allocates one.
		/// </summary>
		public RecordFilter RecordFilter
		{
			get
			{
				ResolvedRecordFilterCollection.FieldFilterInfo info = this.FieldFilterInfo;
				return null != info ? info.RecordFilter : null;
			}
		}

		#endregion // RecordFilter

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

        // JJD 12/30/08 NA 2009 Vol 1 - Record filtering
        #region ClearActiveFilters

        /// <summary>
        /// Clears all active filters
        /// </summary>
        /// <seealso cref="HasActiveFilters"/>
		public void ClearActiveFilters()
		{
			this.ClearActiveFilters(false);
		}

		// AS 5/28/09
		// Added an overload so we can raise the changing/ed events when cleared by the ui commands.
		//
		internal void ClearActiveFilters(bool raiseEvents)
        {
			// JJD 07/16/12 - FS102024
			// If we are in edit mode then end it so we don't end up raising the RecordFilterChanged
			// event twice.
			if (this.IsInEditMode)
			{
				this.EndEditMode(false , false );

				// JJD 07/16/12 - FS102024
				// if the end was canceled then bail
				if (this.IsInEditMode)
					return;
			}

			if (this.HasActiveFilters)
            {
                if (this.IsInEditMode) 
                    this.EndEditMode(false, true);

				// AS 5/28/09 NA 2009.2 Undo/Redo
                //this.RecordFilter.Clear(raiseEvents);
				this.RecordFilter.Clear(raiseEvents, raiseEvents);
            }
        }

        #endregion //ClearActiveFilters	

		#region Internal Methods

		#region VerifyHasActiveFilters

		/// <summary>
		/// Sets the HasActiveFilters property. It also raises property changed notification if the property value has changed.
		/// </summary>
		internal void VerifyHasActiveFilters( )
		{
			bool hasActiveFilters = null != this.FieldFilterInfo && this.FieldFilterInfo.HasActiveFilters;

			if ( _cachedHasActiveFilters != hasActiveFilters )
			{
				_cachedHasActiveFilters = hasActiveFilters;
				this.RaisePropertyChangedEvent( "HasActiveFilters" );

				this.Record.VerifyActiveFilters( );
			}
		}

		#endregion // VerifyHasActiveFilters

		#endregion //Internal Methods

		#region Private Methods

		// AS 8/4/09 NA 2009.2 Field Sizing
		#region NotifyAutoSizeInfo
		private void NotifyAutoSizeInfo()
		{
			FieldLayout fl = this.Field.Owner;

			if (null != fl)
				fl.AutoSizeInfo.OnFilterCellChanged(this);
		}
		#endregion //NotifyAutoSizeInfo

		#endregion //Private Methods

		#endregion //Methods
	}

	#endregion // FilterCell Class

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