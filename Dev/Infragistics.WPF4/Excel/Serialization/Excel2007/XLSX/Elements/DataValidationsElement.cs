using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	internal class DataValidationsElement : XLSXElementBase
	{
		#region Constants






		public const string LocalName = "dataValidations";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			DataValidationsElement.LocalName;

		public const string CountAttributeName = "count";

		#endregion //Constants

		#region Base class overrides

		#region Type
		/// <summary>
		/// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
		/// </summary>
		public override XLSXElementType Type
		{
			get { return XLSXElementType.dataValidations; }
		}
		#endregion Type

		#region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			// MD 2/1/11 - Data Validation support
			// We now fully support data validations, so we don't have to store round trip data.
			//Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];
			//
			//if ( worksheet == null )
			//{
			//    Utilities.DebugFail( "Could not get the worksheet off of the context stack" );
			//    return;
			//}
			//
			//worksheet.DataValidationInfo2007 = XmlElementBase.CacheElementValues( this, element );
			//
			//XmlElementBase.BeforeLoadElementCallback beforeLoadElementHandler = delegate( 
			//    XmlElementBase elementHandler, ElementDataCache parentElementCache, ref List<ElementDataCache> parentElementCacheCollection )
			//{
			//    if ( parentElementCacheCollection == null )
			//        parentElementCacheCollection = worksheet.DataValidationInfo2007.Elements;
			//};
			//
			//XmlElementBase.LoadChildElements( manager, element, beforeLoadElementHandler, null, ref isReaderOnNextNode );
		}

		#endregion Load

		// MD 2/1/11 - Data Validation support
		// We now fully support data validations.
		//#region LoadChildNodes
		//
		//protected override bool LoadChildNodes
		//{
		//    get { return false; }
		//} 
		//
		//#endregion LoadChildNodes

		#region Save


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			// MD 2/1/11 - Data Validation support
			// We now fully support data validations, so we don't have to store round trip data.
			//if ( worksheet == null || worksheet.DataValidationInfo2007 == null )
			//{
			//    Utilities.DebugFail( "Could not get the worksheet off of the context stack or it has no dataValidations element." );
			//    return;
			//}
			//
			//ElementDataCache.ReplaceConsumedValueCallback replaceConsumedValueCallback = delegate( HandledAttributeIdentifier handledAttributeIdentifier )
			//    {
			//        Utilities.DebugFail( "There are no consumed values in or under the dataValidations element." );
			//        return null;
			//    };
			//
			//worksheet.DataValidationInfo2007.SaveDataInElement( element, replaceConsumedValueCallback );
			if ( worksheet == null || worksheet.HasDataValidationRules == false )
			{
				Utilities.DebugFail( "Could not get the worksheet off of the context stack or it has no dataValidations element." );
				return;
			}

			string attributeValue = null;

			attributeValue = XmlElementBase.GetXmlString(worksheet.DataValidationRules.Count, DataType.Int32);
			XmlElementBase.AddAttribute(element, DataValidationsElement.CountAttributeName, attributeValue);

			DictionaryContext<DataValidationRule, WorksheetReferenceCollection> context =
				new DictionaryContext<DataValidationRule, WorksheetReferenceCollection>(worksheet.DataValidationRules);

			manager.ContextStack.Push(context);
			XmlElementBase.AddElements(element, DataValidationElement.QualifiedName, worksheet.DataValidationRules.Count);
		}
		#endregion Save

		#endregion Base class overrides
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