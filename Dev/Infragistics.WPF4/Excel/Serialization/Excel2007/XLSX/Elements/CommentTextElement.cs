using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	internal class CommentTextElement : XLSXElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_Rst">
		//    <sequence>
		//        <element name="t" type="ST_Xstring" minOccurs="0" maxOccurs="1"/>
		//        <element name="r" type="CT_RElt" minOccurs="0" maxOccurs="unbounded"/>
		//        <element name="rPh" type="CT_PhoneticRun" minOccurs="0" maxOccurs="unbounded"/>
		//        <element name="phoneticPr" minOccurs="0" maxOccurs="1" type="CT_PhoneticPr"/>
		//    </sequence>
		//</complexType>

		#endregion //XML Schema Fragment

		#region Constants






		public const string LocalName = "text";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			CommentTextElement.LocalName;

		#endregion //Constants

		#region Base class overrides

		#region Type
		/// <summary>
		/// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
		/// </summary>
		public override XLSXElementType Type
		{
			get { return XLSXElementType.text; }
		}
		#endregion Type

		#region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			
		}

		#endregion Load

		#region Save


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
			WorksheetCellCommentData commentHolder = (WorksheetCellCommentData)manager.ContextStack[ typeof( WorksheetCellCommentData ) ];

			if ( commentHolder == null )
			{
				Utilities.DebugFail( "Could not get the comment holder from the context stack" );
				return;
			}

			FormattedString formattedString = commentHolder.Comment.Text;

			// MD 11/3/10 - TFS49093
			// The formatted string data is now stored on the FormattedStringElement.
			//manager.ContextStack.Push( formattedString );
			// MD 4/12/11 - TFS67084
			// Removed the FormattedStringProxy class. The FormattedString holds the element directly now.
			//FormattedStringElement formattedStringElement = formattedString.Proxy.Element;
			StringElement stringElement = formattedString.Element;
			manager.ContextStack.Push(stringElement);

			// MD 11/3/10 - TFS49093
			// Wrapped in an if statement so we don't lazily create the FormattingRuns collection.
			//ListContext<FormattedStringRun> formattingRunsContext = new ListContext<FormattedStringRun>( formattedString.FormattingRuns );
			// MD 1/31/12 - TFS100573
			//if (formattedStringElement.HasFormatting)
			FormattedStringElement formattedStringElement = stringElement as FormattedStringElement;
			if (formattedStringElement != null && formattedStringElement.HasFormatting)
			{
			// MD 11/9/11 - TFS85193
			//ListContext<FormattedStringRun> formattingRunsContext = new ListContext<FormattedStringRun>(formattedStringElement.FormattingRuns);
			ListContext<FormattingRunBase> formattingRunsContext = new ListContext<FormattingRunBase>(formattedStringElement.FormattingRuns);

			manager.ContextStack.Push( formattingRunsContext );

			// Add an 'r' element for each run that we have
			// MD 11/3/10 - TFS49093
			// Use the count on the context.
			//XmlElementBase.AddElements( element, RichTextRunElement.QualifiedName, formattedString.FormattingRuns.Count );
			XmlElementBase.AddElements(element, RichTextRunElement.QualifiedName, formattingRunsContext.Count);
			}
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