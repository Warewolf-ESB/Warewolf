using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007
{
	internal abstract class XmlContentTypeBase : ContentTypeBase
	{
		#region Base Class Overrides

		#region Load

		public override object Load( Excel2007WorkbookSerializationManager manager, Stream contentTypeStream )
		{
			ExcelXmlDocument document = new ExcelXmlDocument( contentTypeStream, true );

			// MD 9/25/09 - TFS21642
			// We will now load the document as we go.
			//document.Load( contentTypeStream );

			ChildDataItem dataItem = new ChildDataItem();
			manager.ContextStack.Push( dataItem );

			bool isReaderOnNextNode = false;
			XmlElementBase.LoadChildElements( manager, document, ref isReaderOnNextNode );

			// MD 9/25/09 - TFS21642
			document.Reader.Close();

			manager.ContextStack.Pop(); // dataItem
			return dataItem.Data;
		}

		#endregion Load

		#region Save

		public override void Save( Excel2007WorkbookSerializationManager manager, Stream contentTypeStream )
		{
            ExcelXmlDocument document = new ExcelXmlDocument( contentTypeStream, false );

            // MD 9/25/09 - TFS21642
            // Since we are now using a custom document type, we don't need to set this.
            //// MD 1/28/09 - TFS12701
            //// Setting this to True will stop automatic indenting and will save file space.
            //document.PreserveWhitespace = true;

            if ( this.IncludeXmlDeclaration )
            {
                // Add the Xml declaration element
                ExcelXmlDeclaration declaration = document.CreateXmlDeclaration( "1.0", "UTF-8", "yes" );
                document.AppendChild( declaration );
            }

            this.SaveElements( manager, document );
            XmlElementBase.SaveChildElements( manager, document );

            // MD 9/25/09 - TFS21642
            // Close the XmlWriter so it writes it contents to the stream.
            //document.Save( contentTypeStream );
            document.Writer.Close();
		}

        #endregion Save

		#endregion Base Class Overrides

		#region IncludeXmlDeclaration






		protected virtual bool IncludeXmlDeclaration
		{
			get { return true; }
		} 

		#endregion IncludeXmlDeclaration

		public abstract void SaveElements( Excel2007WorkbookSerializationManager manager, ExcelXmlDocument document );
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