using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;



using System.Windows;
using System.Windows.Media;








namespace Infragistics.Documents.Core
{
    #region OfficeDocumentProperties class
    /// <summary>
    /// Encapsulates the properties of an OfficeML-compliant document
    /// such as author, subject, title, etc.
    /// </summary>

    [InfragisticsFeature( Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public abstract class OfficeDocumentProperties
    {
        #region Constants

        ///// <summary>
        ///// Returns the default value of the AppVersion property ('12.0000').
        ///// </summary>
        //[Browsable(false)]
        //[EditorBrowsable(EditorBrowsableState.Advanced)]
        //public const string DefaultAppVersion = "12.0000";
        internal const string DefaultAppVersion = "12.0000";

        #endregion Constants

        #region Member variables

        private string author = null;
		private string category = null;
		private string comments = null;
		private string company = null;
		private string keywords = null;
		private string manager = null;
		private string status = null;
		private string subject = null;
		private string title = null;
        
        //private string appVersion = OfficeDocumentProperties.DefaultAppVersion;

        #endregion Member variables

        #region Properties

        #region Application
        /// <summary>
        /// Returns or sets the name of the consuming application.
        /// </summary>
        public abstract string Application { get; set; }
        #endregion Application

        #region AppVersion
        ///// <summary>
        ///// Returns or sets the version of the consuming application.
        ///// </summary>
        ///// <remarks>
        ///// <p class="body">
        ///// By default, this property returns a value of "12.0000", the
        ///// version number of the original release of Microsoft Office 2007.
        ///// </p>
        ///// </remarks>
        //[Browsable(false)]
        //[EditorBrowsable(EditorBrowsableState.Advanced)]
        //public string AppVersion
        //{
        //    get { return CommonUtilities.StringPropertyGetHelper(this.appVersion); }
        //    set { this.appVersion = value; }
        //}

        internal string AppVersion { get { return OfficeDocumentProperties.DefaultAppVersion; } }
        #endregion AppVersion

        #region Author
        /// <summary>
        /// Returns or sets the author of the document.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The value of this property is persisted to the core properties section
        /// of the document as 'creator'.
        /// </p>
        /// </remarks>
        public string Author
        {
            get { return CommonUtilities.StringPropertyGetHelper( this.author ); }
            set { this.author = value; }
        }
        #endregion Author

        #region Category
        /// <summary>
        /// Returns or sets the category of the document.
        /// </summary>
        public string Category
        {
            get { return CommonUtilities.StringPropertyGetHelper( this.category ); }
            set { this.category = value; }
        }
        #endregion Category

        #region Comments
        /// <summary>
        /// Returns or sets the comments for the document.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The value of this property is persisted to the core properties section
        /// of the document as 'description'.
        /// </p>
        /// </remarks>
        public string Comments
        {
            get { return CommonUtilities.StringPropertyGetHelper( this.comments ); }
            set { this.comments = value; }
        }
        #endregion Comments

        #region Company
        /// <summary>
        /// Returns or sets the company for the document.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The value of this property is persisted to the extended properties section
        /// of the document.
        /// </p>
        /// </remarks>
        public string Company
        {
            get { return CommonUtilities.StringPropertyGetHelper( this.company ); }
            set { this.company = value; }
        }
        #endregion Company

        #region Keywords
        /// <summary>
        /// Returns or sets the keywords for the document.
        /// </summary>
        public string Keywords
        {
            get { return CommonUtilities.StringPropertyGetHelper( this.keywords ); }
            set { this.keywords = value; }
        }
        #endregion Keywords

        #region Manager
        /// <summary>
        /// Returns or sets the manager for the document.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The value of this property is persisted to the extended properties section
        /// of the document.
        /// </p>
        /// </remarks>
        public string Manager
        {
            get { return CommonUtilities.StringPropertyGetHelper( this.manager ); }
            set { this.manager = value; }
        }
        #endregion Manager

        #region Status
        /// <summary>
        /// Returns or sets the status for the document.
        /// </summary>
        /// <remarks>
        /// The value of this property is persisted to the core properties section
        /// of the document as 'contentStatus'.
        /// </remarks>
        public string Status
        {
            get { return CommonUtilities.StringPropertyGetHelper( this.status ); }
            set { this.status = value; }
        }
        #endregion Status

        #region Subject
        /// <summary>
        /// Returns or sets the subject of the document.
        /// </summary>
        public string Subject
        {
            get { return CommonUtilities.StringPropertyGetHelper( this.subject ); }
            set { this.subject = value; }
        }
        #endregion Subject

        #region Title
        /// <summary>
        /// Returns or sets the title of the document.
        /// </summary>
        public string Title
        {
            get { return CommonUtilities.StringPropertyGetHelper( this.title ); }
            set { this.title = value; }
        }
        #endregion Title

        #endregion Properties

        #region Methods

        #region InitializeFrom
        internal void InitializeFrom( OfficeDocumentProperties source )
        {
            this.author = source.author;
            this.category = source.category;
            this.comments = source.comments;
            this.company = source.company;
            this.keywords = source.keywords;
            this.manager = source.manager;
            this.status = source.status;
            this.subject = source.subject;
            this.title = source.title;
        }
        #endregion InitializeFrom

        #region GetHeadingPairsAndTitlesOfParts
        internal abstract void GetHeadingPairsAndTitlesOfParts(
            out string headingPairs,
            out List<string> titlesOfParts );
        #endregion GetHeadingPairsAndTitlesOfParts

        #region GetTemplate
        internal virtual string GetTemplate(){ return null; }
        #endregion GetTemplate

        #region ShouldSerialize
        internal virtual bool ShouldSerialize()
        {
            return
                string.IsNullOrEmpty(this.author) == false ||
                string.IsNullOrEmpty(this.category) == false ||
                string.IsNullOrEmpty(this.comments) == false ||
                string.IsNullOrEmpty(this.company) == false ||
                string.IsNullOrEmpty(this.keywords) == false ||
                string.IsNullOrEmpty(this.manager) == false ||
                string.IsNullOrEmpty(this.status) == false ||
                string.IsNullOrEmpty(this.subject) == false ||
                string.IsNullOrEmpty(this.title) == false;
                //string.IsNullOrEmpty(this.appVersion) == false;
        }
        #endregion ShouldSerialize

        #region Reset
        /// <summary>
        /// Restores all properties of this class to their
        /// respective default values.
        /// </summary>
        public virtual void Reset()
        {
            this.author = null;
            this.category = null;
            this.comments = null;
            this.company = null;
            this.keywords = null;
            this.manager = null;
            this.status = null;
            this.subject = null;
            this.title = null;
            //this.appVersion = OfficeDocumentProperties.DefaultAppVersion;
        }
        #endregion Reset

        #endregion Methods
    }
    #endregion DocumentProperties class
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