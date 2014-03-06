using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel
{
	// MD 11/9/11 - TFS85193
	// Renamed this class and split it into a base an derived class to we could share some logic with the FormattedTextRun.
	#region Old Code

	//#if DEBUG
	//    /// <summary>
	//    /// Represents a contiguous group of characters in a string which share the same format.
	//    /// </summary>  
	//#endif
	//    internal class FormattingRun : IComparable<FormattingRun>
	//    {
	//        #region Member Variables

	//        // MD 11/3/10 - TFS49093
	//        // The formatted string data is now stored on the FormattedStringElement, so it is the owner of this run.
	//        //private FormattedString formattedString;
	//        private FormattedStringElement formattedString;

	//        private WorkbookFontProxy font;
	//        private Workbook workbook;
	//        private int firstFormattedChar;

	//        #endregion Member Variables

	//        #region Constructor

	//        // MD 11/3/10 - TFS49093
	//        // The formatted string data is now stored on the FormattedStringElement, so it is the owner of this run.
	//        //private FormattingRun( FormattedString formattedString, int firstFormattedChar, Workbook workbook, WorkbookFontProxy font )
	//        private FormattingRun(FormattedStringElement formattedString, int firstFormattedChar, Workbook workbook, WorkbookFontProxy font)
	//            : this( formattedString, firstFormattedChar, workbook )
	//        {
	//            this.font = font;
	//        }

	//        public FormattingRun(FormattedString formattedString, int firstFormattedChar, Workbook workbook)
	//            // MD 11/3/10 - TFS49093
	//            // Moved all code to the new constructor
	//            // MD 4/12/11 - TFS67084
	//            // Removed the FormattedStringProxy class. The FormattedString holds the element directly now.
	//            //: this(formattedString.Proxy.Element, firstFormattedChar, workbook) { }
	//            : this(formattedString.Element, firstFormattedChar, workbook) { }

	//        // MD 11/3/10 - TFS49093
	//        // Added a new constructor because the formatted string data is now stored on the FormattedStringElement, so it is the owner of this run.
	//        public FormattingRun(FormattedStringElement formattedString, int firstFormattedChar, Workbook workbook)
	//        {
	//            Debug.Assert( formattedString != null, "The owning string cannot be null." );

	//            this.formattedString = formattedString;
	//            this.firstFormattedChar = firstFormattedChar;
	//            this.workbook = workbook;
	//        }

	//        #endregion Constructor

	//        #region Base Class Overrides

	//        #region Equals

	//        public override bool Equals( object obj )
	//        {
	//            FormattingRun run = obj as FormattingRun;

	//            if ( run == null )
	//                return false;

	//            if ( this.firstFormattedChar != run.firstFormattedChar )
	//                return false;

	//            // MD 3/24/11 - TFS69901
	//            // If this.font is null and so is run.font, we will get a NullReferenceException when calling
	//            // Equals on this.font below. Instead, just call Object.Equals, which does the null checks for us.
	//            //if ( this.font == null && run.font != null )
	//            //    return false;
	//            //
	//            //if ( this.font.Equals( run.font ) == false )
	//            //    return false;
	//            if (Object.Equals(this.font, run.font) == false)
	//                return false;

	//            return true;
	//        }

	//        #endregion Equals

	//        #region GetHashCode

	//        public override int GetHashCode()
	//        {
	//            return this.Font.GetHashCode() + this.firstFormattedChar;
	//        }

	//        #endregion GetHashCode

	//        #endregion Base Class Overrides

	//        #region Methods

	//        #region Clone

	//#if DEBUG
	//        /// <summary>
	//        /// Clones the formatting run so it can be applied to the specified formatted string.
	//        /// </summary>  
	//#endif
	//        // MD 11/3/10 - TFS49093
	//        // The formatted string data is now stored on the FormattedStringElement, so it is the owner of this run.
	//        //public FormattingRun Clone( FormattedString newFormattedString )
	//        public FormattingRun Clone(FormattedStringElement newFormattedString)
	//        {
	//            WorkbookFontProxy clonedFont = null;

	//            if ( this.font != null )
	//                clonedFont = new WorkbookFontProxy( this.font, this.font.Collection, this.workbook );

	//            return new FormattingRun( newFormattedString, this.firstFormattedChar, this.workbook, clonedFont );
	//        }

	//        #endregion Clone

	//        #endregion Methods

	//        #region Properties

	//        #region FirstFormattedChar

	//        public int FirstFormattedChar
	//        {
	//            get { return this.firstFormattedChar; }
	//            // MD 5/2/08 - BR32461/BR01870
	//            // Added a set accessor so this can be changed.
	//            set { this.firstFormattedChar = value; }
	//        }

	//        #endregion FirstFormattedChar

	//        #region UnformattedString
	//#if DEBUG
	//        /// <summary>
	//        /// Returns the substring this run represents.
	//        /// </summary>
	//#endif
	//        internal string UnformattedString
	//        {
	//            get
	//            {
	//                List<FormattingRun> runs = this.formattedString.FormattingRuns;
	//                int index = runs.IndexOf( this );

	//                if ( index < 0 )
	//                    return string.Empty;

	//                string text = this.formattedString.UnformattedString;
	//                int length = text.Length;
	//                FormattingRun nextRun = index < (runs.Count - 1) ? runs[index + 1] : null;
	//                int firstChar = this.FirstFormattedChar;
	//                return nextRun == null ?
	//                    text.Substring( firstChar ) :
	//                    text.Substring( firstChar, nextRun.FirstFormattedChar - firstChar);
	//            }
	//        }

	//        #endregion UnformattedString

	//        #region Font

	//        public WorkbookFontProxy Font
	//        {
	//            get
	//            {
	//                if ( this.font == null )
	//                {
	//                    GenericCachedCollection<WorkbookFontData> fonts = null;

	//                    // MD 9/2/08 - Cell Comments
	//                    //if ( this.formattedString.OwningCell != null )
	//                    // MD 11/3/10 - TFS49093
	//                    // The formatted string data is now stored on the FormattedStringElement, so it is the owner of this run.
	//                    //if ( this.formattedString.Owner != null )
	//                    if (this.formattedString.Workbook != null)
	//                        fonts = this.workbook.Fonts;

	//                    // MD 2/15/11 - TFS66333
	//                    // Use the EmptyElement for the initial data element. The DefaultElement will be populated with data if 
	//                    // the workbook was loaded from a file or stream.
	//                    //this.font = new WorkbookFontProxy( this.workbook.Fonts.DefaultElement, fonts, this.workbook );
	//                    this.font = new WorkbookFontProxy(this.workbook.Fonts.EmptyElement, fonts, this.workbook);
	//                }

	//                return this.font;
	//            }
	//        }

	//        public bool HasFont
	//        {
	//            // MBS 9/8/08 - Excel 2007
	//            // If this has a first formatted character of 0, it is for a comment and it must be forced to write out a font, 
	//            // so indicate it has one that needs to be written out.
	//            //get { return this.font != null; }
	//            get { return this.font != null || this.FirstFormattedChar == 0; }
	//        }

	//        #endregion Font

	//        #endregion Properties


	//        #region IComparable<FormattingRun> Members

	//        // MD 1/24/08
	//        // Made changes to allow for VS2008 style unit test accessors
	//        //int IComparable<FormattingRun>.CompareTo( FormattingRun other )
	//        public int CompareTo( FormattingRun other )
	//        {
	//            return this.firstFormattedChar - other.firstFormattedChar;
	//        }

	//        #endregion
	//    }

	#endregion  // Old Code





	internal class FormattedStringRun : FormattingRunBase, IComparable<FormattedStringRun>
	{
		#region Member Variables

		private WorkbookFontProxy font;

		#endregion Member Variables

		#region Constructor

		public FormattedStringRun(FormattedStringElement formattedString, int firstFormattedChar)
			: base(formattedString, firstFormattedChar) { }

		#endregion Constructor

		#region Base Class Overrides

		#region Equals

		public override bool Equals( object obj )
		{
			FormattedStringRun run = obj as FormattedStringRun;

			if ( run == null )
				return false;

			if (this.FirstFormattedCharInOwner != run.FirstFormattedCharInOwner)
				return false;

			if (Object.Equals(this.font, run.font) == false)
				return false;

			return true;
		}

		#endregion Equals

		#region GetFont

		public override IWorkbookFont GetFont(Workbook workbook)
		{
			return this.GetFontInternal(workbook);
		}

		#endregion  // GetFont

		#region GetFontInternal

		public override WorkbookFontProxy GetFontInternal(Workbook workbook)
		{
			return this.GetFontInternal(workbook, ref this.font);
		}

		#endregion  // GetFontInternal

		#region GetHashCode

		public override int GetHashCode()
		{
			return this.GetFontInternal().GetHashCode() + this.FirstFormattedCharAbsolute;
		}

		#endregion GetHashCode

		#region HasFont

		public override bool HasFont
		{
			// If this has a first formatted character of 0, it is for a comment and it must be forced to write out a font, 
			// so indicate it has one that needs to be written out.
			get { return this.font != null || this.FirstFormattedCharAbsolute == 0; }
		}

		#endregion HasFont

		#endregion Base Class Overrides

		#region Interfaces

		#region IComparable<FormattedStringRun> Members

		public int CompareTo(FormattedStringRun other)
		{
			return this.FirstFormattedCharInOwner - other.FirstFormattedCharInOwner;
		}

		#endregion

		#endregion  // Interfaces

		#region Properties

		#region FirstFormattedCharAbsolute






		public override int FirstFormattedCharAbsolute
		{
			get { return this.FirstFormattedCharInOwner; }
			set { this.FirstFormattedCharInOwner = value; }
		}

		#endregion FirstFormattedCharAbsolute

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