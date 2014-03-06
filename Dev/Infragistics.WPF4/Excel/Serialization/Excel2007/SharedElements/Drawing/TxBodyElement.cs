using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;


using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{



	internal class TxBodyElement : XmlElementBase
	{
		#region XML Schema fragment <docs>
		// <complexType name="CT_TextBody">
		// <sequence>
		// <element name="bodyPr" type="CT_TextBodyProperties" minOccurs="1" maxOccurs="1"/>
		// <element name="lstStyle" type="CT_TextListStyle" minOccurs="0" maxOccurs="1"/>
		// <element name="p" type="CT_TextParagraph" minOccurs="1" maxOccurs="unbounded"/>
		// </sequence>
		// </complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>txBody</summary>
		public const string LocalName = "txBody";
		
		/// <summary>http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing/txBody</summary>
		public const string QualifiedName =
			DrawingsPart.DefaultNamespace +
			XmlElementBase.NamespaceSeparator +
			TxBodyElement.LocalName;


		#endregion Constants

		#region Base class overrides

			#region ElementName

		public override string ElementName
		{
			get { return TxBodyElement.QualifiedName; }
		}

			#endregion ElementName

			#region Load

		/// <summary>Loads the data for this element from the specified manager.</summary>
		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
            //  Get the owning shape off the context stack, and push the FormattedString
            //  instance (which holds the value of its Text property) onto the stack.
			WorksheetShapeWithText shape = (WorksheetShapeWithText)manager.ContextStack[ typeof( WorksheetShapeWithText ) ];

			if ( shape == null )
			{
				Utilities.DebugFail( "Could not find the shape on the context stack." );
				return;
			}

			// MD 11/3/10 - TFS49093
			// The formatted string data is now stored on the FormattedStringElement.
            //manager.ContextStack.Push( shape.Text );
			// MD 4/12/11 - TFS67084
			// Removed the FormattedStringProxy class. The FormattedString holds the element directly now.
			//manager.ContextStack.Push(shape.Text.Proxy.Element);
			// MD 11/8/11 - TFS85193
			// We now have new types to deal with formatted strings with paragraphs.
			//manager.ContextStack.Push(shape.Text.Element);
			manager.ContextStack.Push(new FormattedText());
		}

			#endregion Load

			#region OnAfterLoadChildElements

		protected override void OnAfterLoadChildElements( Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache )
		{
			base.OnAfterLoadChildElements( manager, elementCache );

			WorksheetShapesHolder shapesHolder = (WorksheetShapesHolder)manager.ContextStack[ typeof( WorksheetShapesHolder ) ];

			if ( shapesHolder == null )
			{
				Utilities.DebugFail( "Could not find the shapes holder on the context stack." );
				return;
			}

			shapesHolder.CurrentSerializationManager.TxBodyElementWrapper = elementCache;

			// MD 11/8/11 - TFS85193
			// Store the text on shape after it is fully loaded.
			WorksheetShapeWithText shape = (WorksheetShapeWithText)manager.ContextStack[ typeof( WorksheetShapeWithText ) ];
			FormattedText fs = (FormattedText)manager.ContextStack[typeof(FormattedText)];
			if (shape == null || fs == null)
			{
				Utilities.DebugFail("Could not find the WorksheetShapeWithText or the FormattedText on the context stack.");
				return;
			}

			if (fs.Paragraphs.Count > 0)
				shape.Text = fs;
		} 

			#endregion OnAfterLoadChildElements

			#region Save

		/// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
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