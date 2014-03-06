using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.DataPresenter;
using System.Windows;

namespace Infragistics.Windows.DataPresenter
{
    /// <summary>
    /// An interface used to handle the exporting of a <see cref="DataPresenterBase"/> derived object.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public interface IDataPresenterExporter
    {
        /// <summary>
        /// Begins the exporting process.
        /// </summary>
        /// <param name="dataPresenter">A clone of the data presenter being exported.  The records are not avaialble at this point.</param>
        /// <param name="exportOptions">A series of options for controlling which aspects of the <i>DataPresenter</i> are honored during exporting.</param>
        void BeginExport(DataPresenterBase dataPresenter, IExportOptions exportOptions);

        /// <summary>
        /// Exports the specified record.
        /// </summary>
        /// <param name="record">The record to export.</param>
        /// <param name="processRecordParams">Options that allow the modification of the export process.</param>
        void ProcessRecord(Record record, ProcessRecordParams processRecordParams);

        /// <summary>
        /// Called after all records have been processed.  If the exporting process was cancelled, such as
        /// by setting the <see cref="ProcessRecordParams.TerminateExport"/> property during the
        /// <see cref="ProcessRecord"/> method, then the <i>cancelled</i> parameter will by <b>True</b>.
        /// </summary>
        /// <param name="cancelled">True if the export process was cancelled.</param>
        void EndExport(bool cancelled);

        /// <summary>
        /// Called when exporting logic is copying various settings from the source DataPresenter to the clone used for exporting.
        /// </summary>
        /// <param name="sourceDependencyObject">The object that is being cloned.</param>
        /// <param name="targetDependencyObject">The the new objec that is being created from the original.</param>
        void OnObjectCloned(DependencyObject sourceDependencyObject, DependencyObject targetDependencyObject);

		// MD 6/7/10 - ChildRecordsDisplayOrder feature
		/// <summary>
		/// Called thwn expoerting logic is about to export the record and its children.
		/// </summary>
		/// <param name="record">The record to initialize.</param>
		/// <param name="processRecordParams">Options that allow the modification of the export process.</param>
		/// <returns>True if the record should be exported; False otherwise.</returns>
		bool InitializeRecord(Record record, ProcessRecordParams processRecordParams);
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