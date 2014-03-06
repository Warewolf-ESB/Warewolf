using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows;

namespace Infragistics.Windows.DataPresenter
{
    internal class DataPresenterExportControl :DataPresenterExportControlBase
    {
        #region Private Members

		private DataPresenterReportControl.SectionContainer _sectionContainer; // AS 8/25/11 TFS82921
        private IDataPresenterExporter _exporter;

        #endregion //Private Members

        #region Constructor

        internal DataPresenterExportControl(DataPresenterBase sourceDataPresenter, IDataPresenterExporter exporter)
            : base(sourceDataPresenter)
        {
            if (exporter == null)
                throw new ArgumentNullException("exporter");

            this._exporter = exporter;
        }
        #endregion //Constructor

        #region Properties

        #region Exporter

        internal IDataPresenterExporter Exporter
        {
            get { return this._exporter; }
        }
        #endregion //Exporter

        #endregion //Properties

        #region Methods

		// AS 8/25/11 TFS82921
		// Since we are adding the DP to the logical tree we need to make sure we let it clean up.
		//
		#region OnEndExport
		internal void OnEndExport()
		{
			// JJD 9/30/08
			if (this._sectionContainer != null)
			{
				// null out the Section property of the container which will
				// remove it from the logical tree
				this._sectionContainer.Section = null;
				this.SourceDataPresenter.InternalRemoveLogicalChild(this._sectionContainer);
				this._sectionContainer = null;
			}
		}
		#endregion //OnEndExport

        #region ProcessRecords

		// AS 3/3/11 NA 2011.1 - Async Exporting
		// Refactor the enumeration logic into the ExportRecordEnumeratorBase class so 
		// we can break up the operation for asynchronous processing.
		//
//#if DEBUG
//        /// <summary>
//        /// A method for traversing the records and exporting them.
//        /// </summary>        
//        /// <param name="options"></param>
//        /// <returns>True if the rows were successfully traversed.</returns>
//#endif
//        internal bool ProcessRecords(IExportOptions options)
//        {
//            ViewableRecordCollection vrc = this.ViewableRecords;
//
//            return this.TraverseVisibleRecords(vrc, null, options, null);
//        }
        #endregion //ProcessRecords

        #endregion //Methods

        #region Base Class Overrides

		#region OnBeginCloneSource

		protected override void OnBeginCloneSource(Infragistics.Windows.Reporting.ReportSection section)
		{
			Debug.Assert(section == null);

			base.OnBeginCloneSource(section);

			// AS 8/25/11 TFS82921
			// Copied a modified version of what the DataPresenterReportControl did.
			//
			// JD 9/30/08
			// We need to create a container for the section so we can use it as a 
			// convenient plave to prevent routed events from bubbling up to
			// the ui datapresenter
			if (this._sectionContainer == null)
			{
				Debug.Assert(LogicalTreeHelper.GetParent(this) == null);

				this._sectionContainer = new DataPresenterReportControl.SectionContainer();
				this._sectionContainer.Section = this;

				// Make the section a logical child of the source UI datapresenter
				//
				this.SourceDataPresenter.InternalAddLogicalChild(this._sectionContainer);
			}
			else
				this._sectionContainer.Section = this;
		}
		#endregion //OnBeginCloneSource

        #region OnTraverseRecord

		// AS 3/3/11 NA 2011.1 - Async Exporting
		// Refactor the enumeration logic into the ExportRecordEnumeratorBase class so 
		// we can break up the operation for asynchronous processing.
		//
		//protected override bool OnTraverseRecord(Record record, Visibility visibility, RecordManager recordManager, IExportOptions options,
		//    List<Record> flattenedList, out bool skipSiblings)
		//{
		//    if (!base.OnTraverseRecord(record, visibility, recordManager, options, flattenedList, out skipSiblings))
		//        return false;
		//
		//    ProcessRecordParams processRecordParams = null;
		//    // AS 2/22/11 NA 2011.1 Word Writer
		//    // I'm not sure why we had decided to do this - perhaps it was because the excel exporter
		//    // was not going to show the expandable field records - but without including them the 
		//    // exporter won't have the option of including them. I've updated the excel exporter to 
		//    // ignore these records to maintain the current behavior.
		//    // 
		//    //ExpandableFieldRecord expandableFieldRecord = record as ExpandableFieldRecord;
		//
		//    // MD 6/7/10 - ChildRecordsDisplayOrder feature
		//    // We had to split the initializing of the record from the processing of the record, so we could determine whether 
		//    // to export children when they are displayed above their parent. So initialize the record here.
		//    bool shouldProcessRecord = true;
		//    // AS 2/22/11 NA 2011.1 Word Writer
		//    //if (expandableFieldRecord == null)
		//    {
		//        processRecordParams = new ProcessRecordParams();
		//        shouldProcessRecord = this._exporter.InitializeRecord(record, processRecordParams);
		//
		//        if (processRecordParams.TerminateExport)
		//            return false;
		//    }
		//
		//    // MD 6/7/10 - ChildRecordsDisplayOrder feature
		//    // Only process the record first here if it should be beofre its children and we should process the record.
		//    //if (expandableFieldRecord == null)
		//    bool areChildrenAfterParent = record.AreChildrenAfterParent;
		//    // AS 2/22/11 NA 2011.1 Word Writer
		//    //if (areChildrenAfterParent && shouldProcessRecord && expandableFieldRecord == null)
		//    if (areChildrenAfterParent && shouldProcessRecord)
		//    {
		//        // MD 6/7/10 - ChildRecordsDisplayOrder feature
		//        // This code has been moved up to where we first initialize the record.
		//        //processRecordParams = new ProcessRecordParams();
		//
		//        this._exporter.ProcessRecord(record, processRecordParams);
		//		
		//        if (processRecordParams.TerminateExport)
		//            return false;
		//    }
		//
		//    ViewableRecordCollection children = record.ViewableChildRecords;
		//    if ((processRecordParams == null || !processRecordParams.SkipDescendants)
		//        && children != null && children.Count != 0 && record.HasChildren)
		//    {
		//        if (!this.TraverseVisibleRecords(children, recordManager, options, flattenedList))
		//            return false;
		//    }
		//
		//    // MD 6/7/10 - ChildRecordsDisplayOrder feature
		//    // Only process the record here if it should be after its children and we should process the record.
		//    // AS 2/22/11 NA 2011.1 Word Writer
		//    //if (areChildrenAfterParent == false && shouldProcessRecord && expandableFieldRecord == null)
		//    if (areChildrenAfterParent == false && shouldProcessRecord)
		//    {
		//        this._exporter.ProcessRecord(record, processRecordParams);
		//
		//        if (processRecordParams.TerminateExport)
		//            return false;
		//    }
		//
		//    if (processRecordParams != null && processRecordParams.SkipSiblings)
		//        skipSiblings = true;
		//
		//    return true;
		//}
        #endregion //OnTraverseRecord

        #endregion //Base Class Overrides
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