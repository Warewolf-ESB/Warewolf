using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.IO;




using Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes

{
	internal class ThemePart : XmlContentTypeBase
	{
		#region Constants

		public const string ContentTypeValue = "application/vnd.openxmlformats-officedocument.theme+xml";
		public const string BasePartName = "/xl/theme/theme.xml";

        //  BF 10/11/10  NA 2011.1 - Infragistics.Word
        public const string BasePartNameWord = "/word/theme/theme.xml";

		public const string RelationshipTypeValue = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/theme";
        public const string DefaultNamespace = "http://schemas.openxmlformats.org/drawingml/2006/main";
        public const string DefaultNamespacePrefix = "a";

		#endregion Constants

		#region Base Class Overrides

		#region ContentType

		public override string ContentType
		{
			get { return ThemePart.ContentTypeValue; }
		} 

		#endregion ContentType

		#region RelationshipType

		public override string RelationshipType
		{
			get { return ThemePart.RelationshipTypeValue; }
		}

		#endregion RelationshipType

        #region Save

        public override void Save(Excel2007WorkbookSerializationManager manager, Stream contentTypeStream)
        {
            ThemePart.SaveHelper( manager, contentTypeStream );
        }

        private static void SaveHelper( Excel2007WorkbookSerializationManager manager, Stream contentTypeStream )
        {
            //For now since we don't support themes we will just write out a default theme file
            //
            //Roundtrip - We should probably store the themes file that was given to us
            //if we load an existing workbook and need to save it
			









			using (Stream stream = typeof(ThemePart).Assembly.GetManifestResourceStream("Infragistics.Documents.Excel.Serialization.Excel2007.DefaultTheme.xml"))


            {
                int readCount;
                byte[] buffer = new byte[1024];
                while ((readCount = stream.Read(buffer, 0, 1024)) != 0)
                {
                    contentTypeStream.Write(buffer, 0, readCount);
                }
            }
        }
        #endregion //Save

        #region SaveElements

		public override void SaveElements( Excel2007WorkbookSerializationManager manager, ExcelXmlDocument document )
        {
            ////Utilities.DebugFail( "Add the root element to the theme part." );
            //XmlElement themeElement = document.CreateElement(
            //    ThemePart.DefaultNamespacePrefix,
            //    ThemeElement.LocalName,
            //    ThemePart.DefaultNamespace);

            //document.AppendChild(themeElement);
        }
        #endregion //SaveElements

        #endregion Base Class Overrides
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