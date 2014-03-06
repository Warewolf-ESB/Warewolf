using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;








using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.ContentTypes;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007

{
	internal abstract class ContentTypeBase
	{
		#region Static Variables

		[ThreadStatic]
		private static Dictionary<string, ContentTypeBase> contentTypes;

		#endregion Static Variables

		public abstract string ContentType { get; }
		public abstract string RelationshipType { get; }
		public abstract object Load( Excel2007WorkbookSerializationManager manager, Stream contentTypeStream );
		public abstract void Save( Excel2007WorkbookSerializationManager manager, Stream contentTypeStream );

        //  BF 11/23/10 IGWordStreamer
        internal virtual void Save( IOfficeDocumentExportManager manager, Stream stream, out bool closeStream )
        {
            closeStream = true;
            throw new NotSupportedException();
        }

		// MD 4/6/12 - TFS102169
		// Added support for loading some relationships after the current part is loaded.





		public virtual string[] PostLoadRelationshipTypes { get { return null; } }



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        public virtual Dictionary<string, int> RelationshipLoadPriorityDictionary { get { return null; } }

		#region ContentTypes

		private static Dictionary<string, ContentTypeBase> ContentTypes
		{
			get
			{
				if ( ContentTypeBase.contentTypes == null )
					ContentTypeBase.contentTypes = new Dictionary<string, ContentTypeBase>( StringComparer.InvariantCultureIgnoreCase );

				return ContentTypeBase.contentTypes;
			}
		}

		#endregion ContentTypes

		#region GetContentType

		public static ContentTypeBase GetContentType( string contentTypeValue )
		{
			ContentTypeBase contentType;

			if ( ContentTypeBase.ContentTypes.TryGetValue( contentTypeValue, out contentType ) )
				return contentType;

			contentType = ContentTypeBase.CreateContentType( contentTypeValue );

			Debug.Assert( 
				contentType == null || String.Equals( contentType.ContentType, contentTypeValue, StringComparison.InvariantCultureIgnoreCase ), 
				"An incorrect content type was created for the content type value: " + contentTypeValue );

			ContentTypeBase.ContentTypes.Add( contentTypeValue, contentType );

			return contentType;
		}

		#endregion GetContentType

		#region CreateContentType

		private static ContentTypeBase CreateContentType( string contentTypeValue )
		{
			switch ( contentTypeValue )
			{
				case CalculationChainPart.ContentTypeValue:			return new CalculationChainPart();
				case ChartPart.ContentTypeValue:					return new ChartPart();							// MD 10/12/10 - TFS49853
				case CommentsPart.ContentTypeValue:					return new CommentsPart();
				case CorePropertiesPart.ContentTypeValue:			return new CorePropertiesPart();
				case CustomXmlPart.ContentTypeValue:				return new CustomXmlPart();						// MD 10/8/10 - TFS44359
				case CustomXmlPropertiesPart.ContentTypeValue:		return new CustomXmlPropertiesPart();			// MD 10/8/10 - TFS44359
				case DrawingsPart.ContentTypeValue:					return new DrawingsPart();
				case ExtendedPropertiesPart.ContentTypeValue:		return new ExtendedPropertiesPart();
				case ExternalWorkbookPart.ContentTypeValue:			return new ExternalWorkbookPart();
				case GifImagePart.ContentTypeValue:					return new GifImagePart();
				case JpegImagePart.ContentTypeValue:				return new JpegImagePart();
				case LegacyDrawingsPart.ContentTypeValue:			return new LegacyDrawingsPart();
				case MacroEnabledTemplatePart.ContentTypeValue:		return new MacroEnabledTemplatePart();			// MD 5/7/10 - 10.2 - Excel Templates
				case MacroEnabledWorkbookPart.ContentTypeValue:		return new MacroEnabledWorkbookPart();			// MD 10/1/08 - TFS8471
				case PngImagePart.ContentTypeValue:					return new PngImagePart();
				case SharedStringTablePart.ContentTypeValue:		return new SharedStringTablePart();
				case TablePart.ContentTypeValue:					return new TablePart();							// MD 12/6/11 - 12.1 - Table Support
				case TemplatePart.ContentTypeValue:					return new TemplatePart();						// MD 5/7/10 - 10.2 - Excel Templates
				case ThemePart.ContentTypeValue:					return new ThemePart();
				case TiffImagePart.ContentTypeValue:				return new TiffImagePart();
				case WorkbookPart.ContentTypeValue:					return new WorkbookPart();
				case WorkbookStylesPart.ContentTypeValue:			return new WorkbookStylesPart();
				case WorksheetPart.ContentTypeValue:				return new WorksheetPart();
				case VBAProjectPart.ContentTypeValue:				return new VBAProjectPart();					// MD 10/1/08 - TFS8471

                // Roundtrip - These types need to be round-tripped
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.printerSettings":
                case "application/vnd.openxmlformats-officedocument.custom-properties+xml":
				case "application/vnd.ms-excel.controlproperties+xml": // MD 10/31/11 - TFS90733
                    return null;
			}

			Utilities.DebugFail( "Unhandled content type: " + contentTypeValue );
			return null;
		}

		#endregion CreateContentType

		// MD 4/6/12 - TFS102169
		#region OnLoadComplete






		public virtual void OnLoadComplete(Excel2007WorkbookSerializationManager manager) { }

		#endregion // OnLoadComplete
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