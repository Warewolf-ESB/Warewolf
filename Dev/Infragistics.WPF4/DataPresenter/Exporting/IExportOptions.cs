using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Windows.DataPresenter
{
    /// <summary>
    /// An interface exposing various options that determine how a <see cref="DataPresenterBase"/> should be exported
    /// via the <see cref="IDataPresenterExporter"/> interface.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public interface IExportOptions
    {
        /// <summary>
        /// Determines if each record's IsExpanded state is honored during the exporting process.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If set to true this means that we don't want to copy over each <see cref="Record"/>'s <see cref="Record.IsExpanded"/> state from the 
        /// <see cref="DataPresenterBase"/> being exported.  Default value is false. </p>
        /// </remarks>        
        bool ExcludeExpandedState { get; }

        /// <summary>
        /// Determines if the FieldLayoutSettings of the DataPresenter are honored during the exporting process.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If set to true this means that we don't want to copy the <see cref="FieldLayoutSettings"/> from the <see cref="DataPresenterBase"/>
        /// being exported. Default value is false. </p>
        /// </remarks>        
        /// <seealso cref="FieldLayout.Settings"/>
        bool ExcludeFieldLayoutSettings { get; }

        /// <summary>
        /// Determines if the FieldSettings of the DataPresenter are honored during the exporting process.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If set to true this means that we don't want to copy the <see cref="FieldSettings"/> from the <see cref="DataPresenterBase"/>
        /// being exported. Default value is false. </p>
        /// </remarks>
        /// <seealso cref="DataPresenterBase.FieldSettings"/>
        /// <seealso cref="FieldLayout.FieldSettings"/>
        /// <seealso cref="Field.Settings"/>
        bool ExcludeFieldSettings { get; }

        /// <summary>
        /// Determines if the groupby settings of the DataPresenter are honored during the exporting process.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If set to true this means that we don't want to copy the groupby settings from the <see cref="DataPresenterBase"/>
        /// being exported. Default value is false. </p>
        /// </remarks>
        /// <seealso cref="FieldSortDescription.IsGroupBy"/>
        /// <seealso cref="FieldLayout.SortedFields"/>
        bool ExcludeGroupBySettings { get; }

        /// <summary>
        /// Determines if record filter criteria of the DataPresenter is honored during the exporting process.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If set to true this means that we don't want to copy over the record filter criteria from the <see cref="DataPresenterBase"/>
        /// being exported. Default value is false. </p>
        /// </remarks>
        /// <seealso cref="DataRecord.IsFilteredOut"/>
        /// <seealso cref="RecordManager.RecordFilters"/>
        bool ExcludeRecordFilters { get; }

        /// <summary>
        /// Determines if each record's Visibility setting of the DataPresenter is honored during the exporting process.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If set to true this means that we don't want to copy over each <see cref="Record"/>'s <see cref="Record.Visibility"/> property setting
        /// from the <see cref="DataPresenterBase"/> being exported. Default value is false. </p>
        /// </remarks>
        /// <seealso cref="Record.Visibility"/>
        bool ExcludeRecordVisibility { get; }

        /// <summary>
        /// Determines if the sorted fields of the DataPresenter are honored during the exporting process.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If set to true this means that we don't want to copy the <see cref="FieldLayout.SortedFields"/> from the associated
        /// <see cref="DataPresenterBase"/> being exported. Default value is false. </p>
        /// </remarks>
        /// <seealso cref="FieldLayout.SortedFields"/>
        bool ExcludeSortOrder { get; }

        /// <summary>
        /// Determines if the summary settings of the DataPresenter are honored during the exporting process.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If set to true this means that we don't want to copy the summary settings of the associated 
        /// <see cref="DataPresenterBase"/> being exported. Default value is false. </p>
        /// </remarks>
        /// <seealso cref="FieldSettings.AllowSummaries"/>
        /// <seealso cref="FieldSettings.SummaryUIType"/>
        /// <seealso cref="FieldSettings.SummaryDisplayArea"/>
        /// <seealso cref="FieldLayoutSettings.SummaryDescriptionVisibility"/>
        /// <seealso cref="FieldLayout.SummaryDefinitions"/>
        bool ExcludeSummaries { get; }
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