using System;
using Infragistics.Windows.Design.SmartTagFramework;

namespace Infragistics.Windows.Design.SmartTagFramework
{
    /// <summary>
    /// TreeItem represents an item from the current project's hierarchy
    /// </summary>
    public class TreeItem
    {
        #region Member Variables

        private string _originalPath;
        private bool _isFile;
        private string _name;
        private string _path;

        #endregion //Member Variables

        #region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="originalPath"></param>
		/// <param name="parentDirectoryPath"></param>
        public TreeItem(string originalPath, string parentDirectoryPath)
        {
            if (string.IsNullOrEmpty(originalPath))
            {
                throw new ApplicationException("Empty OriginalPath path.");
            }

            if (!originalPath.Contains(";") || !originalPath.Contains(".") || !originalPath.Contains(@"/"))
            {
                throw new ApplicationException("Bad resource path.");
            }

            _originalPath = originalPath;
            string[] resourcePathParts = originalPath.Split(new Char[] { ';' });

            if (resourcePathParts.Length < 2)
            {
                throw new ApplicationException("Bad resource path.");
            }

            string resourcePath = resourcePathParts[1].Replace(@"component", string.Empty);
            string pathLeft = resourcePath.Substring(resourcePath.IndexOf(parentDirectoryPath) + parentDirectoryPath.Length + 1);

            if (pathLeft.Contains(@"/"))
            {
                _isFile = false;
            }
            else
            {
                _isFile = true;
            }

            if (this.IsDirectory)
            {
                string[] pathParts = pathLeft.Split(new Char[] { '/' });
                _name = pathParts[0];
            }
            else
            {
                _name = pathLeft;
            }

            _path = string.Format("{0}/{1}", parentDirectoryPath, _name);

			_name = _name.Replace("%20", " ");
		}
        
        #endregion //Description

        #region Base class overrides
                
        #region Equals

		/// <summary>
		/// Returns true if the resource Path equal the specified object
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
        public override bool Equals(object obj)
        {
			if (false == (obj is TreeItem))
				return false;

            return this.Path.Equals(((TreeItem)obj).Path);
        }

        #endregion //Equals

        #region GetHashCode

		/// <summary>
		/// Returns the instance's hash code.
		/// </summary>
		/// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion //GetHashCode

        #region ToString

		/// <summary>
		/// Returns a string representation of the item.
		/// </summary>
		/// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion //ToString

        #endregion //Base class overrides
        
        #region Properties

        #region Public Properties
        
        #region IsFile

		/// <summary>
		/// Returns true if the item represents a file.
		/// </summary>
        public bool IsFile
        {
            get
            {
                return _isFile;
            }
        }

        #endregion //IsFile	
    
        #region IsDirectory

		/// <summary>
		/// Returns true if the item represents a directory.
		/// </summary>
        public bool IsDirectory
        {
            get
            {
                return !_isFile;
            }
        }

        #endregion //IsDirectory	
    
        #region Name

		/// <summary>
		/// Returns the name of the item.
		/// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        #endregion //Name	

        #region OriginalPath

		/// <summary>
		/// Returns the original path of the item.
		/// </summary>
        public string OriginalPath
        {
            get
            {
                return _originalPath;
            }
        }

        #endregion //OriginalPath
    
        #region Path

		/// <summary>
		/// Returns the path of the item.
		/// </summary>
        public string Path
        {
            get
            {
                return _path;
            }
        }

        #endregion //Path	
    
        #region ResourceType

		/// <summary>
		/// Returns the resource type of the item.
		/// </summary>
        public ResourceType ResourceType
        {
            get;
            set;
        }

        #endregion //ResourceType

        #endregion Public Properties

        #endregion Properties
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