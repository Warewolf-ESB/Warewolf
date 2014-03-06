using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Controls.Layouts.Primitives;
using System.Drawing;
using Infragistics.Windows.DataPresenter.Internal;
using Infragistics.Documents.Excel;

namespace Infragistics.Windows.DataPresenter.ExcelExporter
{
    internal class DataPresenterExportCache
    {
        #region Enums

        internal enum PlaceholderType 
        { 
            ColumnHeaders, 
            Values, 
            Summaries 
        }

        internal enum CacheType
        {
            Excel            
        }
        #endregion Enums        

        #region Private Members

        private Dictionary<FieldSettings, FormatSettings> _cellFormatSettingsTable = new Dictionary<FieldSettings, FormatSettings>();
        private Dictionary<FieldLayout, ExportLayoutInformation> _documentFieldLayoutInfoTable = new Dictionary<FieldLayout, ExportLayoutInformation>();
        private Dictionary<FieldLayout, ExportLayoutInformation> _documentFieldLayoutHeaderInfoTable = new Dictionary<FieldLayout, ExportLayoutInformation>();
        private ExportOptions _exportOptions;
        private Dictionary<Field, FormatSettings> _fieldCellFormatSettingsTable = new Dictionary<Field, FormatSettings>();
        private Dictionary<Field, FormatSettings> _fieldGroupByFormatSettingsTable = new Dictionary<Field, FormatSettings>();
        private Dictionary<Field, FormatSettings> _fieldLabelFormatSettingsTable = new Dictionary<Field, FormatSettings>();
        private Dictionary<Field, FormatSettings> _fieldSummaryCellFormatSettingsTable = new Dictionary<Field, FormatSettings>();
        private Dictionary<Field, FormatSettings> _fieldSummaryLabelFormatSettingsTable = new Dictionary<Field, FormatSettings>();
        private Dictionary<FieldSettings, FormatSettings> _groupFormatSettingsTable = new Dictionary<FieldSettings, FormatSettings>();
        private Dictionary<FieldSettings, FormatSettings> _labelFormatSettingsTable = new Dictionary<FieldSettings, FormatSettings>();
        private Dictionary<RecordCollectionBase, bool> _recordCollectionHeadersExportedTable = new Dictionary<RecordCollectionBase, bool>();
        private Dictionary<FieldLayout, FormatSettings> _resolvedFieldLayoutCellFormatSettingsTable = new Dictionary<FieldLayout, FormatSettings>();
        private Dictionary<FieldLayout, FormatSettings> _resolvedFieldLayoutGroupFormatSettingsTable = new Dictionary<FieldLayout, FormatSettings>();
        private Dictionary<FieldLayout, FormatSettings> _resolvedFieldLayoutLabelFormatSettingsTable = new Dictionary<FieldLayout, FormatSettings>();
        private Dictionary<FieldLayout, FormatSettings> _resolvedFieldLayoutSummaryCellFormatSettingsTable = new Dictionary<FieldLayout, FormatSettings>();
        private Dictionary<FieldLayout, FormatSettings> _resolvedFieldLayoutSummaryLabelFormatSettingsTable = new Dictionary<FieldLayout, FormatSettings>();
        private Dictionary<FieldSettings, FormatSettings> _summaryCellFormatSettingsTable = new Dictionary<FieldSettings, FormatSettings>();
        private Dictionary<FieldSettings, FormatSettings> _summaryLabelFormatSettingsTable = new Dictionary<FieldSettings, FormatSettings>();

        #endregion //Private Members

        #region Properties

        #region CurrentExportOptions

        public ExportOptions ExportOptions
        {
            get { return this._exportOptions; }            
        }
        #endregion //CurrentExportOptions

        #endregion //Properties

        #region Methods

        #region BuildReportInfoCache

        internal void BuildReportInfoCache(DataPresenterBase dataPresenter, ExportOptions exportOptions)
        {
            this._exportOptions = exportOptions;
            foreach (FieldLayout layout in dataPresenter.FieldLayouts)
                this.BuildReportInfoCache(layout);
        }

        private void BuildReportInfoCache(FieldLayout layout)
        {            
            this._documentFieldLayoutInfoTable[layout] = new ExportLayoutInformation(layout, false);
            this._documentFieldLayoutHeaderInfoTable[layout] = new ExportLayoutInformation(layout, true);            
        }
        #endregion //BuildReportInfoCache

        #region Clear

        internal void Clear()
        {
            this._documentFieldLayoutHeaderInfoTable.Clear();
            this._documentFieldLayoutInfoTable.Clear();
            this._exportOptions = null;
            this._recordCollectionHeadersExportedTable.Clear();

            this.ClearCachedFormatSettings();
        }
        #endregion //Clear

        #region ClearCachedFormatSettings

        private void ClearCachedFormatSettings()
        {
            this._cellFormatSettingsTable.Clear();
            this._labelFormatSettingsTable.Clear();
            this._groupFormatSettingsTable.Clear();
            this._fieldCellFormatSettingsTable.Clear();
            this._fieldGroupByFormatSettingsTable.Clear();
            this._fieldLabelFormatSettingsTable.Clear();
            this._fieldSummaryCellFormatSettingsTable.Clear();
            this._fieldSummaryLabelFormatSettingsTable.Clear();
            this._resolvedFieldLayoutCellFormatSettingsTable.Clear();
            this._resolvedFieldLayoutGroupFormatSettingsTable.Clear();
            this._resolvedFieldLayoutLabelFormatSettingsTable.Clear();
            this._resolvedFieldLayoutSummaryCellFormatSettingsTable.Clear();
            this._resolvedFieldLayoutSummaryLabelFormatSettingsTable.Clear();
            this._summaryCellFormatSettingsTable.Clear();
            this._summaryLabelFormatSettingsTable.Clear();
        }
        #endregion //ClearCachedFormatSettings

        #region GetCellFormatSettings

        private FormatSettings GetCellFormatSettings(FieldSettings fieldSettings)
        {
            if (fieldSettings == null)
                return null;

            FormatSettings formatSettings;
            if (this._cellFormatSettingsTable.TryGetValue(fieldSettings, out formatSettings))
                return formatSettings;

            formatSettings = DataPresenterExcelExporter.GetExcelCellFormatSettings(fieldSettings);
            if (formatSettings != null)
                this._cellFormatSettingsTable.Add(fieldSettings, formatSettings);

            return formatSettings;
        }
        #endregion //GetCellFormatSettings

        #region GetDocumentFieldLayoutInfo

        internal ExportLayoutInformation GetDocumentFieldLayoutInfo(FieldLayout fieldLayout)
        {
            return this._documentFieldLayoutInfoTable[fieldLayout];
        }
        #endregion //GetDocumentFieldLayoutInfo

        #region GetDocumentLabelLayoutInfo

        internal ExportLayoutInformation GetDocumentLabelLayoutInfo(FieldLayout fieldLayout)
        {
            return this._documentFieldLayoutHeaderInfoTable[fieldLayout];
        }
        #endregion //GetDocumentLabelLayoutInfo

        #region GetGroupFormatSettings

        private FormatSettings GetGroupFormatSettings(FieldSettings fieldSettings)
        {
            if (fieldSettings == null)
                return null;

            FormatSettings formatSettings;
            if (this._groupFormatSettingsTable.TryGetValue(fieldSettings, out formatSettings))
                return formatSettings;

            formatSettings = DataPresenterExcelExporter.GetExcelGroupFormatSettings(fieldSettings);
            if (formatSettings != null)
                this._groupFormatSettingsTable.Add(fieldSettings, formatSettings);

            return formatSettings;
        }
        #endregion //GetGroupFormatSettings

        #region GetLabelFormatSettings

        private FormatSettings GetLabelFormatSettings(FieldSettings fieldSettings)
        {
            if (fieldSettings == null)
                return null;

            FormatSettings formatSettings;
            if (this._labelFormatSettingsTable.TryGetValue(fieldSettings, out formatSettings))
                return formatSettings;

            formatSettings = DataPresenterExcelExporter.GetExcelLabelFormatSettings(fieldSettings);
            if (formatSettings != null)
                this._labelFormatSettingsTable.Add(fieldSettings, formatSettings);

            return formatSettings;
        }
        #endregion //GetLabelFormatSettings

        #region GetResolvedFieldCellFormatSettings



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal FormatSettings GetResolvedFieldCellFormatSettings(Field field, FormatSettings parentSettings)
        {
            // Check to see if we've cached any settings for the labels, such as when the user sets the properties
            // in the InitializeField event
            FormatSettings fieldFormatSettings;
            if (!this._fieldCellFormatSettingsTable.TryGetValue(field, out fieldFormatSettings))
            {
                if (field.HasSettings)
                {
                    fieldFormatSettings = this.GetCellFormatSettings(field.Settings);

                    // MBS 9/18/09
                    // Noticed that I wasn't actually caching the value
                    this._fieldCellFormatSettingsTable[field] = fieldFormatSettings;
                }
            }

            // We need to merge the two objects at this point
            FormatSettings formatSettings = DataPresenterExportCache.MergeFormatSettings(parentSettings, fieldFormatSettings);
            return formatSettings;
        }
        #endregion //GetResolvedFieldCellFormatSettings

        // MBS 9/18/09
        #region GetResolvedFieldGroupByFormatSettings



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal FormatSettings GetResolvedFieldGroupByFormatSettings(Field field, FormatSettings parentSettings)
        {
            // Check to see if we've cached any settings for the labels, such as when the user sets the properties
            // in the InitializeField event
            FormatSettings fieldFormatSettings;
            if (!this._fieldGroupByFormatSettingsTable.TryGetValue(field, out fieldFormatSettings))
            {
                if (field.HasSettings)
                {
                    fieldFormatSettings = this.GetGroupFormatSettings(field.Settings);
                    this._fieldGroupByFormatSettingsTable[field] = fieldFormatSettings;
                }
            }

            // We need to merge the two objects at this point
            FormatSettings formatSettings = DataPresenterExportCache.MergeFormatSettings(parentSettings, fieldFormatSettings);
            return formatSettings;
        }
        #endregion //GetResolvedFieldGroupByFormatSettings

        #region GetResolvedFieldLabelFormatSettings



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal FormatSettings GetResolvedFieldLabelFormatSettings(Field field, FormatSettings parentSettings)
        {
            // Check to see if we've cached any settings for the labels, such as when the user sets the properties
            // in the InitializeField event
            FormatSettings fieldFormatSettings;
            if (!this._fieldLabelFormatSettingsTable.TryGetValue(field, out fieldFormatSettings))
            {
                if (field.HasSettings)
                {
                    fieldFormatSettings = this.GetLabelFormatSettings(field.Settings);

                    // MBS 9/18/09
                    // Noticed that I wasn't actually caching the value
                    this._fieldLabelFormatSettingsTable[field] = fieldFormatSettings;
                }
            }

            // We need to merge the two objects at this point
            FormatSettings formatSettings = DataPresenterExportCache.MergeFormatSettings(parentSettings, fieldFormatSettings);
            return formatSettings;
        }
        #endregion //GetResolvedFieldLabelFormatSettings

        #region GetResolvedFieldLayoutCellFormatSettings



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal FormatSettings GetResolvedFieldLayoutCellFormatSettings(FieldLayout layout)
        {
            FormatSettings formatSettings;
            if (this._resolvedFieldLayoutCellFormatSettingsTable.TryGetValue(layout, out formatSettings))
                return formatSettings;

            FormatSettings layoutFormatSettings = null;
            if (layout.HasFieldSettings)
                layoutFormatSettings = this.GetCellFormatSettings(layout.FieldSettings);

            FormatSettings dpFormatSettings = null;
            if (layout.DataPresenter != null)
                dpFormatSettings = this.GetCellFormatSettings(layout.DataPresenter.FieldSettings);

            // We need to merge the two objects at this point
            formatSettings = DataPresenterExportCache.MergeFormatSettings(dpFormatSettings, layoutFormatSettings);
            this._resolvedFieldLayoutCellFormatSettingsTable.Add(layout, formatSettings);
            return formatSettings;
        }
        #endregion //GetResolvedFieldLayoutCellFormatSettings

        #region GetResolvedFieldLayoutGroupFormatSettings



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal FormatSettings GetResolvedFieldLayoutGroupFormatSettings(FieldLayout layout)
        {
            FormatSettings formatSettings;
            if (this._resolvedFieldLayoutGroupFormatSettingsTable.TryGetValue(layout, out formatSettings))
                return formatSettings;

            FormatSettings layoutFormatSettings = null;
            if (layout.HasFieldSettings)
                layoutFormatSettings = this.GetGroupFormatSettings(layout.FieldSettings);

            FormatSettings dpFormatSettings = null;
            if (layout.DataPresenter != null)
                dpFormatSettings = this.GetGroupFormatSettings(layout.DataPresenter.FieldSettings);

            // We need to merge the two objects at this point
            formatSettings = DataPresenterExportCache.MergeFormatSettings(dpFormatSettings, layoutFormatSettings);
            this._resolvedFieldLayoutGroupFormatSettingsTable.Add(layout, formatSettings);
            return formatSettings;
        }
        #endregion //GetResolvedFieldLayoutGroupFormatSettings

        #region GetResolvedFieldLayoutLabelFormatSettings



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal FormatSettings GetResolvedFieldLayoutLabelFormatSettings(FieldLayout layout)
        {
            FormatSettings formatSettings;
            if (this._resolvedFieldLayoutLabelFormatSettingsTable.TryGetValue(layout, out formatSettings))
                return formatSettings;

            FormatSettings layoutFormatSettings = null;
            if (layout.HasFieldSettings)
                layoutFormatSettings = this.GetLabelFormatSettings(layout.FieldSettings);

            FormatSettings dpFormatSettings = null;
            if (layout.DataPresenter != null)
                dpFormatSettings = this.GetLabelFormatSettings(layout.DataPresenter.FieldSettings);

            // We need to merge the two objects at this point
            formatSettings = DataPresenterExportCache.MergeFormatSettings(dpFormatSettings, layoutFormatSettings);
            this._resolvedFieldLayoutLabelFormatSettingsTable.Add(layout, formatSettings);
            return formatSettings;
        }
        #endregion //GetResolvedFieldLayoutLabelFormatSettings

        #region GetResolvedFieldLayoutSummaryCellFormatSettings



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal FormatSettings GetResolvedFieldLayoutSummaryCellFormatSettings(FieldLayout layout)
        {
            FormatSettings formatSettings;
            if (this._resolvedFieldLayoutSummaryCellFormatSettingsTable.TryGetValue(layout, out formatSettings))
                return formatSettings;

            FormatSettings layoutFormatSettings = null;
            if (layout.HasFieldSettings)
                layoutFormatSettings = this.GetSummaryCellFormatSettings(layout.FieldSettings);

            FormatSettings dpFormatSettings = null;
            if (layout.DataPresenter != null)
                dpFormatSettings = this.GetSummaryCellFormatSettings(layout.DataPresenter.FieldSettings);

            // We need to merge the two objects at this point
            formatSettings = DataPresenterExportCache.MergeFormatSettings(dpFormatSettings, layoutFormatSettings);
            this._resolvedFieldLayoutSummaryCellFormatSettingsTable.Add(layout, formatSettings);
            return formatSettings;
        }
        #endregion //GetResolvedFieldLayoutSummaryCellFormatSettings

        #region GetResolvedFieldLayoutSummaryLabelFormatSettings



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal FormatSettings GetResolvedFieldLayoutSummaryLabelFormatSettings(FieldLayout layout)
        {
            FormatSettings formatSettings;
            if (this._resolvedFieldLayoutSummaryLabelFormatSettingsTable.TryGetValue(layout, out formatSettings))
                return formatSettings;

            FormatSettings layoutFormatSettings = null;
            if (layout.HasFieldSettings)
                layoutFormatSettings = this.GetSummaryLabelFormatSettings(layout.FieldSettings);

            FormatSettings dpFormatSettings = null;
            if (layout.DataPresenter != null)
                dpFormatSettings = this.GetSummaryLabelFormatSettings(layout.DataPresenter.FieldSettings);

            // We need to merge the two objects at this point
            formatSettings = DataPresenterExportCache.MergeFormatSettings(dpFormatSettings, layoutFormatSettings);
            this._resolvedFieldLayoutSummaryLabelFormatSettingsTable.Add(layout, formatSettings);
            return formatSettings;
        }
        #endregion //GetResolvedFieldLayoutSummaryLabelFormatSettings

        // MBS 9/18/09
        #region GetResolvedFieldSummaryCellFormatSettings



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal FormatSettings GetResolvedFieldSummaryCellFormatSettings(Field field, FormatSettings parentSettings)
        {
            // Check to see if we've cached any settings for the labels, such as when the user sets the properties
            // in the InitializeField event
            FormatSettings fieldFormatSettings;
            if (!this._fieldSummaryCellFormatSettingsTable.TryGetValue(field, out fieldFormatSettings))
            {
                if (field.HasSettings)
                {
                    fieldFormatSettings = this.GetSummaryCellFormatSettings(field.Settings);
                    this._fieldSummaryCellFormatSettingsTable[field] = fieldFormatSettings;
                }
            }

            // We need to merge the two objects at this point
            FormatSettings formatSettings = DataPresenterExportCache.MergeFormatSettings(parentSettings, fieldFormatSettings);
            return formatSettings;
        }
        #endregion //GetResolvedFieldSummaryCellFormatSettings
        //
        #region GetResolvedFieldSummaryLabelFormatSettings



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal FormatSettings GetResolvedFieldSummaryLabelFormatSettings(Field field, FormatSettings parentSettings)
        {
            // Check to see if we've cached any settings for the labels, such as when the user sets the properties
            // in the InitializeField event
            FormatSettings fieldFormatSettings;
            if (!this._fieldSummaryLabelFormatSettingsTable.TryGetValue(field, out fieldFormatSettings))
            {
                if (field.HasSettings)
                {
                    fieldFormatSettings = this.GetSummaryLabelFormatSettings(field.Settings);
                    this._fieldSummaryLabelFormatSettingsTable[field] = fieldFormatSettings;
                }
            }

            // We need to merge the two objects at this point
            FormatSettings formatSettings = DataPresenterExportCache.MergeFormatSettings(parentSettings, fieldFormatSettings);
            return formatSettings;
        }
        #endregion //GetResolvedFieldSummaryLabelFormatSettings

        #region GetSummaryCellFormatSettings

        private FormatSettings GetSummaryCellFormatSettings(FieldSettings fieldSettings)
        {
            if (fieldSettings == null)
                return null;

            FormatSettings formatSettings;
            if (this._summaryCellFormatSettingsTable.TryGetValue(fieldSettings, out formatSettings))
                return formatSettings;

            formatSettings = DataPresenterExcelExporter.GetExcelSummaryCellFormatSettings(fieldSettings);
            if (formatSettings != null)
                this._summaryCellFormatSettingsTable.Add(fieldSettings, formatSettings);

            return formatSettings;
        }
        #endregion //GetSummaryCellFormatSettings

        #region GetSummaryLabelFormatSettings

        private FormatSettings GetSummaryLabelFormatSettings(FieldSettings fieldSettings)
        {
            if (fieldSettings == null)
                return null;

            FormatSettings formatSettings;
            if (this._summaryLabelFormatSettingsTable.TryGetValue(fieldSettings, out formatSettings))
                return formatSettings;

            formatSettings = DataPresenterExcelExporter.GetExcelSummaryLabelFormatSettings(fieldSettings);
            if (formatSettings != null)
                this._summaryLabelFormatSettingsTable.Add(fieldSettings, formatSettings);

            return formatSettings;
        }
        #endregion //GetSummaryLabelFormatSettings

        #region HasExportedHeadersForRecordCollection

        internal bool HasExportedHeadersForRecordCollection(RecordCollectionBase recordCollection)
        {
            bool hasDiplayedHeaders;
            if (this._recordCollectionHeadersExportedTable.TryGetValue(recordCollection, out hasDiplayedHeaders))
                return hasDiplayedHeaders;

            // Default to false if we haven't encountered the record collection yet
            return false;
        }
        #endregion //HasExportedHeadersForRecordCollection

        #region MergeFormatSettings

        internal static FormatSettings MergeFormatSettings(FormatSettings parentSettings, FormatSettings childSettings)
        {
            if (parentSettings == null && childSettings == null)
                return new FormatSettings();

            if (parentSettings == null)
                return childSettings;

            if (childSettings == null)
                return parentSettings;

            FormatSettings formatSettings = new FormatSettings();
            if (childSettings.BorderColor != null)
                formatSettings.BorderColor = childSettings.BorderColor;
            else
                formatSettings.BorderColor = parentSettings.BorderColor;

            if (childSettings.BorderStyle != CellBorderLineStyle.Default)
                formatSettings.BorderStyle = childSettings.BorderStyle;
            else
                formatSettings.BorderStyle = parentSettings.BorderStyle;

            if (childSettings.BottomBorderColor != null)
                formatSettings.BottomBorderColor = childSettings.BottomBorderColor;
            else
                formatSettings.BottomBorderColor = parentSettings.BottomBorderColor;

            if (childSettings.BottomBorderStyle != CellBorderLineStyle.Default)
                formatSettings.BottomBorderStyle = childSettings.BottomBorderStyle;
            else
                formatSettings.BottomBorderStyle = parentSettings.BottomBorderStyle;

            if (childSettings.FillPattern != FillPatternStyle.Default)
                formatSettings.FillPattern = childSettings.FillPattern;
            else
                formatSettings.FillPattern = parentSettings.FillPattern;

            if (childSettings.FillPatternBackgroundColor != null)
                formatSettings.FillPatternBackgroundColor = childSettings.FillPatternBackgroundColor;
            else
                formatSettings.FillPatternBackgroundColor = parentSettings.FillPatternBackgroundColor;

            if (childSettings.FillPatternForegroundColor != null)
                formatSettings.FillPatternForegroundColor = childSettings.FillPatternForegroundColor;
            else
                formatSettings.FillPatternForegroundColor = parentSettings.FillPatternForegroundColor;

            if (childSettings.FontFamily != null)
                formatSettings.FontFamily = childSettings.FontFamily;
            else
                formatSettings.FontFamily = parentSettings.FontFamily;

            if (childSettings.FontSize != null)
                formatSettings.FontSize = childSettings.FontSize;
            else
                formatSettings.FontSize = parentSettings.FontSize;

            if (childSettings.FontStrikeout != ExcelDefaultableBoolean.Default)
                formatSettings.FontStrikeout = childSettings.FontStrikeout;
            else
                formatSettings.FontStrikeout = parentSettings.FontStrikeout;

            if (childSettings.FontStyle != null)
                formatSettings.FontStyle = childSettings.FontStyle;
            else
                formatSettings.FontStyle = parentSettings.FontStyle;

            if (childSettings.FontSuperscriptSubscriptStyle != FontSuperscriptSubscriptStyle.Default)
                formatSettings.FontSuperscriptSubscriptStyle = childSettings.FontSuperscriptSubscriptStyle;
            else
                formatSettings.FontSuperscriptSubscriptStyle = parentSettings.FontSuperscriptSubscriptStyle;

            if (childSettings.FontUnderlineStyle != FontUnderlineStyle.Default)
                formatSettings.FontUnderlineStyle = childSettings.FontUnderlineStyle;
            else
                formatSettings.FontUnderlineStyle = parentSettings.FontUnderlineStyle;

            if (childSettings.FontWeight != null)
                formatSettings.FontWeight = childSettings.FontWeight;
            else
                formatSettings.FontWeight = parentSettings.FontWeight;

            if (childSettings.FontColor != null)
                formatSettings.FontColor = childSettings.FontColor;
            else
                formatSettings.FontColor = parentSettings.FontColor;

            if (childSettings.FormatString != null)
                formatSettings.FormatString = childSettings.FormatString;
            else
                formatSettings.FormatString = parentSettings.FormatString;

            if (childSettings.HorizontalAlignment != HorizontalCellAlignment.Default)
                formatSettings.HorizontalAlignment = childSettings.HorizontalAlignment;
            else
                formatSettings.HorizontalAlignment = parentSettings.HorizontalAlignment;

            if (childSettings.Indent != null)
                formatSettings.Indent = childSettings.Indent;
            else
                formatSettings.Indent = parentSettings.Indent;

            if (childSettings.LeftBorderColor != null)
                formatSettings.LeftBorderColor = childSettings.LeftBorderColor;
            else
                formatSettings.LeftBorderColor = parentSettings.LeftBorderColor;

            if (childSettings.LeftBorderStyle != CellBorderLineStyle.Default)
                formatSettings.LeftBorderStyle = childSettings.LeftBorderStyle;
            else
                formatSettings.LeftBorderStyle = parentSettings.LeftBorderStyle;

            if (childSettings.Locked != ExcelDefaultableBoolean.Default)
                formatSettings.Locked = childSettings.Locked;
            else
                formatSettings.Locked = parentSettings.Locked;

            if (childSettings.RightBorderColor != null)
                formatSettings.RightBorderColor = childSettings.RightBorderColor;
            else
                formatSettings.RightBorderColor = parentSettings.RightBorderColor;

            if (childSettings.RightBorderStyle != CellBorderLineStyle.Default)
                formatSettings.RightBorderStyle = childSettings.RightBorderStyle;
            else
                formatSettings.RightBorderStyle = parentSettings.RightBorderStyle;

            if (childSettings.Rotation != null)
                formatSettings.Rotation = childSettings.Rotation;
            else
                formatSettings.Rotation = parentSettings.Rotation;

            if (childSettings.ShrinkToFit != ExcelDefaultableBoolean.Default)
                formatSettings.ShrinkToFit = childSettings.ShrinkToFit;
            else
                formatSettings.ShrinkToFit = parentSettings.ShrinkToFit;

            if (childSettings.TopBorderColor != null)
                formatSettings.TopBorderColor = childSettings.TopBorderColor;
            else
                formatSettings.TopBorderColor = parentSettings.TopBorderColor;

            if (childSettings.TopBorderStyle != CellBorderLineStyle.Default)
                formatSettings.TopBorderStyle = childSettings.TopBorderStyle;
            else
                formatSettings.TopBorderStyle = parentSettings.TopBorderStyle;

            if (childSettings.VerticalAlignment != VerticalCellAlignment.Default)
                formatSettings.VerticalAlignment = childSettings.VerticalAlignment;
            else
                formatSettings.VerticalAlignment = parentSettings.VerticalAlignment;

            if (childSettings.WrapText != ExcelDefaultableBoolean.Default)
                formatSettings.WrapText = childSettings.WrapText;
            else
                formatSettings.WrapText = parentSettings.WrapText;

            return formatSettings;
        }
        #endregion //MergeFormatSettings

        #region SetHeadersExportedForRecordCollection

        internal void SetHeadersExportedForRecordCollection(RecordCollectionBase recordCollection, bool hasExported)
        {
            this._recordCollectionHeadersExportedTable[recordCollection] = hasExported;
        }
        #endregion //SetHeadersExportedForRecordCollection

        #region VerifyReportInfoCache



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal void VerifyReportInfoCache(FieldLayout layout)
        {
            if(this._documentFieldLayoutInfoTable.ContainsKey(layout) == false)
                this._documentFieldLayoutInfoTable[layout] = new ExportLayoutInformation(layout, false);

            if(this._documentFieldLayoutHeaderInfoTable.ContainsKey(layout) == false)
                this._documentFieldLayoutHeaderInfoTable[layout] = new ExportLayoutInformation(layout, true);    
        }
        #endregion //VerifyReportInfoCache

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