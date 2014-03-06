using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Documents.Core;


using System.Windows;
using System.Windows.Media;





namespace Infragistics.Documents.Word
{
    #region WordDocumentProperties class
    /// <summary>
    /// Encapsulates the properties of an OfficeML-compliant document
    /// such as author, subject, title, etc.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class WordDocumentProperties : OfficeDocumentProperties
    {
        #region Constants

        /// <summary>
        /// Returns the default value of the LatinCulture property ('en-US').
        /// </summary>
        public const string DefaultLatinCulture = "en-US";

        /// <summary>
        /// Returns the default value of the EastAsiaCulture property ('ja-JP').
        /// </summary>
        public const string DefaultEastAsiaCulture = "ja-JP";

        /// <summary>
        /// Returns the default value of the ComplexScriptCulture property ('ar-SA').
        /// </summary>
        public const string DefaultComplexScriptCulture = "ar-SA";

        /// <summary>
        /// Returns the default value of the Template property ('Normal.dotm').
        /// </summary>
        public const string DefaultTemplate = "Normal.dotm";

        /// <summary>
        /// Returns the default value of the Application property ('Microsoft Office Word').
        /// </summary>
        public const string DefaultApplication = "Microsoft Word";

        #endregion Constants

        #region Member variables

        private string application = WordDocumentProperties.DefaultApplication;
        private string latinCulture = null;
        private string eastAsiaCulture = null;
        private string complexScriptCulture = null;

        private string template = WordDocumentProperties.DefaultTemplate;

        #endregion Member variables

        #region Properties

        #region Application
        /// <summary>
        /// Returns or sets the name of the consuming application.
        /// </summary>
        public override string Application
        {
            get { return string.IsNullOrEmpty(this.application) == false ? this.application : WordDocumentProperties.DefaultApplication; }
            set { this.application = value; }
        }
        #endregion Application

        #region LatinCulture
        /// <summary>
        /// Specifies the language which shall be used to check spelling
        /// and grammar (if requested) when processing the contents of text
        /// runs which contain Latin characters, as determined by the Unicode
        /// character values of the run content.
        /// </summary>
        public string LatinCulture
        {
            get { return string.IsNullOrEmpty(this.latinCulture) == false ? this.latinCulture : WordDocumentProperties.DefaultLatinCulture; }

            set
            {
                CommonUtilities.VerifyCulture( value );
                this.latinCulture = value;
            }
        }
        #endregion LatinCulture

        #region EastAsiaCulture
        /// <summary>
        /// Specifies the language which is used when processing
        /// the contents of text runs which contain East Asian
        /// characters, as determined by the Unicode character
        /// values of the run content.
        /// </summary>
        public string EastAsiaCulture
        {
            get { return string.IsNullOrEmpty(this.eastAsiaCulture) == false ? this.eastAsiaCulture : WordDocumentProperties.DefaultEastAsiaCulture; }

            set
            {
                CommonUtilities.VerifyCulture( value );
                this.eastAsiaCulture = value;
            }
        }
        #endregion EastAsiaCulture

        #region ComplexScriptCulture
        /// <summary>
        /// Specifies the language which is used when processing
        /// the contents of text runs which contain complex script
        /// characters, as determined by the Unicode character
        /// values of the run content.
        /// </summary>
        public string ComplexScriptCulture
        {
            get { return string.IsNullOrEmpty(this.complexScriptCulture) == false ? this.complexScriptCulture : WordDocumentProperties.DefaultComplexScriptCulture; }

            set
            {
                CommonUtilities.VerifyCulture( value );
                this.complexScriptCulture = value;
            }
        }
        #endregion ComplexScriptCulture

        #region Template
        /// <summary>
        /// Returns the document template to be used by the consuming application.
        /// This property currently only supports a value of 'Normal.dotm'
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public string Template
        {
            get { return CommonUtilities.StringPropertyGetHelper(this.template); }
        }
        #endregion Template

        #endregion Properties

        #region Methods

        #region GetHeadingPairsAndTitlesOfParts

        internal override void GetHeadingPairsAndTitlesOfParts(
            out string headingPairs,
            out List<string> titlesOfParts )
        {
            headingPairs = "Title";
            titlesOfParts = new List<string>( new string[]{ string.Empty } );
        }

        #endregion GetHeadingPairsAndTitlesOfParts

        #region GetTemplate
        internal override string GetTemplate()
        {
            return this.Template;
        }
        #endregion GetTemplate

        #region ShouldSerialize
        internal override bool ShouldSerialize()
        {
            return
                base.ShouldSerialize() ||
                string.IsNullOrEmpty(this.latinCulture) == false ||
                string.IsNullOrEmpty(this.eastAsiaCulture) == false ||
                string.IsNullOrEmpty(this.complexScriptCulture) == false ||
                string.IsNullOrEmpty(this.template);
        }
        #endregion ShouldSerialize

        #region Reset
        /// <summary>
        /// Restores all properties of this class to their
        /// respective default values.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            this.latinCulture = null;
            this.eastAsiaCulture = null;
            this.complexScriptCulture = null;
            this.template = WordDocumentProperties.DefaultTemplate;
        }
        #endregion Reset

        #region InitializeFrom
        internal void InitializeFrom( WordDocumentProperties source )
        {
            base.InitializeFrom( source );

            this.application = source.application;
            this.complexScriptCulture = source.complexScriptCulture;
            this.eastAsiaCulture = source.eastAsiaCulture;
            this.latinCulture = source.latinCulture;
            this.template = source.template;
        }
        #endregion InitializeFrom

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