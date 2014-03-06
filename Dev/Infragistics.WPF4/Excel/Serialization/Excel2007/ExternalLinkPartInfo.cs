using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Security.Permissions;
using System.Net;
using System.IO;

namespace Infragistics.Documents.Excel.Serialization.Excel2007
{
    internal class ExternalLinkPartInfo
    {
        #region Members
        
        private string fullPartPath;

        #endregion //Members

        #region Constructor

        public ExternalLinkPartInfo(Uri partPath, string loadingPath)
        {
            if (partPath.IsAbsoluteUri)
			{
				// MD 10/24/11 - TFS91531
				// The LocalPath excludes the domain from web addresses. We should use the AbsoluteUri instead.
                //this.fullPartPath = Uri.UnescapeDataString(partPath.LocalPath);
				this.fullPartPath = Uri.UnescapeDataString(partPath.AbsoluteUri);
			}
            else
            {
                string root = String.Empty;
                string path = partPath.OriginalString;

                // We need to take specific action if the path starts with '/', since this means
                // that the path is relative to the current root directory that the main 
                // workbook resides on (i.e. 'C:\')
                if (path.StartsWith("/"))
                {
                    // If we are loading from a stream or otherwise don't have access
                    // to the path that we are loading the current workbook from,
                    // we need to hack the string so that it can be parsed as a valid
                    // formula.  Should we ever implement functionality to parse and 
                    // solve external references/links, then we would have to 
                    // perform some different logic, likely.
                    if (loadingPath == null)
                    {
                        try
                        {
                            // Use the drive of the current working directory as a default
                            root = System.IO.Path.GetPathRoot(Environment.CurrentDirectory);
                        }
                        catch
                        {
                            // In the case that we don't actually have permission to access the current
                            // directory, we'll just use the default "C:\"
                            root = "C:";
                        }
                    }
                    else
                        root = System.IO.Path.GetPathRoot(loadingPath);
                }
                else
                {
                    // If the loading path is null (i.e. we're loading from a stream), we can't try
                    // to create a full path as it's arbitrary. 
                    if (loadingPath != null)
                        root = loadingPath;
                    else
                        root = String.Empty;
                }

				// MD 1/17/11 - TFS63019
				// If the root is empty and the path is not absolute, we are going to get an InvalidOperationException when trying 
				// to get the URI's LocalPath below. Since we can't get the absolutel path without the root anyway, just use the 
				// relative path. It's the best we can do.
				if (String.IsNullOrEmpty(root))
				{
					this.fullPartPath = path;
					return;
				}

                string pathSeparator = root.EndsWith("\\") ? String.Empty : "\\";

                Uri fullPath;
                if (Uri.TryCreate(String.Format("{0}{1}{2}", root, pathSeparator, path), UriKind.RelativeOrAbsolute, out fullPath))
                {
                    this.fullPartPath = Uri.UnescapeDataString(fullPath.LocalPath);
                }
                else
                    Utilities.DebugFail("Could not create a valid URI from the specified parameters");
            }
        }
        #endregion //Constructor

		#region Methods


		// MD 10/24/11 - TFS91531
		#region GetStream

		public Stream GetStream()
		{
			if (this.fullPartPath.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
			{
				WebClient client = new WebClient();
				return new MemoryStream(client.DownloadData(this.fullPartPath));
			}

			return new FileStream(this.fullPartPath, FileMode.Open);
		}

		#endregion  // GetStream


		#endregion  // Methods

        #region Properties

        #region FullPath

        public string FullPath
        {
            get
            {
                //if (this.fullPartPath == null)
                //    return null;

                //return Uri.UnescapeDataString(this.fullPartPath.LocalPath);
                return this.fullPartPath;
            }
        }
        #endregion //FullPath

        #endregion //Properties
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