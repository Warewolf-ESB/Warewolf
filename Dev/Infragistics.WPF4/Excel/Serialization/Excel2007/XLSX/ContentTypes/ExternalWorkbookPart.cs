using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;




namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.ContentTypes

{
	internal class ExternalWorkbookPart : XLSXContentTypeBase
	{
		#region Constants

        public const string BasePartName = "/xl/externalLinks/externalLink.xml";
		public const string ContentTypeValue = "application/vnd.openxmlformats-officedocument.spreadsheetml.externalLink+xml";
		public const string RelationshipTypeValue = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/externalLink";
        private const string RelationshipTypeExternalLinkPath = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/externalLinkPath";

		#endregion Constants

		#region Base Class Overrides

		#region ContentType

		public override string ContentType
		{
			get { return ExternalWorkbookPart.ContentTypeValue; }
		}

		#endregion ContentType

		#region RelationshipType

		public override string RelationshipType
		{
			get { return ExternalWorkbookPart.RelationshipTypeValue; }
		}

		#endregion RelationshipType

		#region RootElementType

		public override XLSXElementType RootElementType
		{
			get { return XLSXElementType.externalLink; }
		}

		#endregion RootElementType

        #region Save

        public override void Save(Excel2007WorkbookSerializationManager manager, Stream contentTypeStream)
        {
            ExternalWorkbookReference workbookRef = (ExternalWorkbookReference)manager.ContextStack[typeof(ExternalWorkbookReference)];
            if (workbookRef == null)
            {
                Utilities.DebugFail("Could not get external workbook reference");
                return;
            }

            // Replace the directory separator char with the '/' used by Uri.  If we don't use the '/'
            // then calling EscapeUriString will causes the separator to also be escaped
            string fileName = Uri.EscapeUriString(workbookRef.FileName.Replace(Path.DirectorySeparatorChar, '/'));

            Uri uri;            
            if (manager.FilePath != null)
            {                
                if (Path.IsPathRooted(workbookRef.FileName) == false ||
                    Path.GetPathRoot(workbookRef.FileName) == Path.GetPathRoot(manager.FilePath))
                {
                    // We need to use the original path structure for building the relative paths.  We will
                    // be returned a Uri that can be cleanly escaped
                    string path = PackageUtilities.GetRelativePath(manager.FilePath, workbookRef.FileName, System.IO.Path.DirectorySeparatorChar, true);
                    uri = new Uri(Uri.EscapeUriString(path), UriKind.RelativeOrAbsolute);
                }
                else
                {
                    // If the Uri doesn't start with "file:///", Excel won't read it
                    if (fileName.StartsWith("file:///") == false)
                        fileName = fileName.Insert(0, "file:///");

                    // Since the path must be rooted to get here, we can pass off the absolute Uri
                    uri = new Uri(fileName, UriKind.Absolute);
                }
            }
            else
            {
                // If we have an absolute path, we need to add "file:///"
                if (Path.IsPathRooted(workbookRef.FileName))
                {
                    // If the Uri doesn't start with "file:///", Excel won't read it
                    if (fileName.StartsWith("file:///") == false)
                        fileName = fileName.Insert(0, "file:///");

                    // Since the path must be rooted to get here, we can pass off the absolute Uri
                    uri = new Uri(fileName, UriKind.Absolute);
                }
                else
                    uri = new Uri(fileName, UriKind.RelativeOrAbsolute);
            }

            string relationshipId = manager.CreateRelationshipInPackage(uri, ExternalWorkbookPart.RelationshipTypeExternalLinkPath, RelationshipTargetMode.External, false);
            manager.ContextStack.Push(new RelationshipIdHolder(relationshipId));            
            base.Save(manager, contentTypeStream);
            manager.ContextStack.Pop();
        }
        #endregion //Save

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