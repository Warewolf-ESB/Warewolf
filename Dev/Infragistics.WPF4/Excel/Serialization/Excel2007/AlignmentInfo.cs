using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Documents.Excel.Serialization.Excel2007
{





    internal class AlignmentInfo
    {
		#region Members

		private HorizontalCellAlignment horizontal = AlignmentInfo.DEFAULT_HORIZONTAL;
		private int indent = AlignmentInfo.DEFAULT_INDENT;
		private bool justifyLastLine = AlignmentInfo.DEFAULT_JUSTIFYLASTLINE;
		private int readingOrder = AlignmentInfo.DEFAULT_READINGORDER;
		private int relativeIndent = AlignmentInfo.DEFAULT_RELATIVEINDENT;
		private bool shrinkToFit = AlignmentInfo.DEFAULT_SHRINKTOFIT;
		private int textRotation = AlignmentInfo.DEFAULT_TEXTROTATION;
		private VerticalCellAlignment vertical = AlignmentInfo.DEFAULT_VERTICAL;
		private bool wrapText = AlignmentInfo.DEFAULT_WRAPTEXT;

		#endregion Members

        #region Constructors

        public AlignmentInfo()
        {
        }

        private AlignmentInfo(HorizontalCellAlignment HorizontalAlignment, int Indent, bool JustifyLastLine, int ReadingOrder, int RelativeIndent, int TextRotation, ExcelDefaultableBoolean ShrinkToFit, VerticalCellAlignment VerticalAlignment, ExcelDefaultableBoolean WrapText)
        {
            this.horizontal = HorizontalAlignment;
            this.indent = Indent;
            this.justifyLastLine = JustifyLastLine;
            this.readingOrder = ReadingOrder;
            this.relativeIndent = RelativeIndent;
            this.shrinkToFit = (ShrinkToFit == ExcelDefaultableBoolean.True)? true: false;
            this.textRotation = TextRotation;
            this.vertical = VerticalAlignment;
            this.wrapText = (WrapText == ExcelDefaultableBoolean.True) ? true : false;
        }

        #endregion Constructors

        #region Constants

        internal const HorizontalCellAlignment DEFAULT_HORIZONTAL = HorizontalCellAlignment.General;
        internal const int DEFAULT_INDENT = 0;
        internal const bool DEFAULT_JUSTIFYLASTLINE = false;
        internal const int DEFAULT_READINGORDER = 0;
        internal const int DEFAULT_RELATIVEINDENT = 0;
        internal const bool DEFAULT_SHRINKTOFIT = false;
        internal const int DEFAULT_TEXTROTATION = 0;
        internal const VerticalCellAlignment DEFAULT_VERTICAL = VerticalCellAlignment.Bottom;
        internal const bool DEFAULT_WRAPTEXT = false;


        #endregion Contstants

		#region Base Class Overrides

		// MD 1/8/12 - 12.1 - Cell Format Updates
		#region Equals

		public override bool Equals(object obj)
		{
			return AlignmentInfo.HasSameData(this, obj as AlignmentInfo);
		}

		#endregion // Equals

		// MD 1/9/12 - 12.1 - Cell Format Updates
		#region GetHashCode

		public override int GetHashCode()
		{
			int hashCode = 0;

			hashCode ^= (int)this.horizontal;
			hashCode ^= (int)this.indent << 1;
			hashCode ^= Convert.ToInt32(this.justifyLastLine) << 2;
			hashCode ^= (int)this.readingOrder << 3;
			hashCode ^= (int)this.relativeIndent << 4;
			hashCode ^= Convert.ToInt32(this.shrinkToFit) << 5;
			hashCode ^= (int)this.textRotation << 6;
			hashCode ^= (int)this.vertical << 7;
			hashCode ^= Convert.ToInt32(this.wrapText) << 8;

			return hashCode;
		}

		#endregion  // GetHashCode

		#endregion // Base Class Overrides

        #region Properties

        #region Horizontal






        public HorizontalCellAlignment Horizontal
        {
            get { return this.horizontal; }
            set { this.horizontal = value; }
        }

        #endregion Horizontal

        #region Indent






        public int Indent
        {
            get { return this.indent; }
            set { this.indent = value; }
        }

        #endregion Indent

        #region JustifyLastLine






        public bool JustifyLastLine
        {
            get { return this.justifyLastLine; }
            set { this.justifyLastLine = value; }
        }

        #endregion JustifyLastLine

        #region ReadingOrder



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        public int ReadingOrder
        {
            get { return this.readingOrder; }
            set { this.readingOrder = value; }
        }

        #endregion ReadingOrder

        #region RelativeIndent







        public int RelativeIndent
        {
            get { return this.relativeIndent; }
            set { this.relativeIndent = value; }
        }

        #endregion RelativeIndent

        #region ShrinkToFit






        public bool ShrinkToFit
        {
            get { return this.shrinkToFit; }
            set { this.shrinkToFit = value; }
        }

        #endregion ShrinkToFit

        #region TextRotation






        public int TextRotation
        {
            get { return this.textRotation; }
            set { this.textRotation = value; }
        }

        #endregion TextRotation

        #region Vertical






        public VerticalCellAlignment Vertical
        {
            get { return this.vertical; }
            set { this.vertical = value; }
        }

        #endregion Vertical

        #region WrapText






        public bool WrapText
        {
            get { return this.wrapText; }
            set { this.wrapText = value; }
        }

        #endregion WrapText

        #region IsDefault






        public bool IsDefault
        {
            get
            {
                return (this.horizontal == AlignmentInfo.DEFAULT_HORIZONTAL &&
                    this.indent == AlignmentInfo.DEFAULT_INDENT &&
                    this.justifyLastLine == AlignmentInfo.DEFAULT_JUSTIFYLASTLINE &&
                    this.readingOrder == AlignmentInfo.DEFAULT_READINGORDER &&
                    this.relativeIndent == AlignmentInfo.DEFAULT_RELATIVEINDENT &&
                    this.shrinkToFit == AlignmentInfo.DEFAULT_SHRINKTOFIT &&
                    this.textRotation == AlignmentInfo.DEFAULT_TEXTROTATION &&
                    this.vertical == AlignmentInfo.DEFAULT_VERTICAL &&
                    this.wrapText == AlignmentInfo.DEFAULT_WRAPTEXT);
            }
        }

        #endregion IsDefault

        #endregion Properties

        #region Methods

		// MD 12/21/11 - 12.1 - Cell Format Updates
		// Moved this code from the FormatInfo.CreateWorksheetCellFormatData so it can be used in other places.
		#region ApplyTo

		internal void ApplyTo(WorksheetCellFormatData formatData)
		{
			// MD 3/21/12 - TFS104630
			// We need to round-trip the AddIndent value.
			formatData.AddIndent = this.JustifyLastLine;

			formatData.Alignment = this.Horizontal;
			formatData.Rotation = this.TextRotation;
			formatData.WrapText = (this.WrapText) ? ExcelDefaultableBoolean.True : ExcelDefaultableBoolean.False;
			formatData.VerticalAlignment = this.Vertical;
			formatData.Indent = this.Indent;
			formatData.ShrinkToFit = (this.ShrinkToFit) ? ExcelDefaultableBoolean.True : ExcelDefaultableBoolean.False;
		}

		#endregion // ApplyTo

		// MD 1/8/12 - 12.1 - Cell Format Updates
		#region CreateAlignmentInfo

		public static AlignmentInfo CreateAlignmentInfo(WorksheetCellFormatData formatData)
		{
			return AlignmentInfo.CreateAlignmentInfo(formatData, false);
		}

		public static AlignmentInfo CreateAlignmentInfo(WorksheetCellFormatData formatData, bool forceCreate)
		{
			// MD 3/21/12 - TFS104630
			// We need to round-trip the AddIndent value.
			bool addIndent = formatData.AddIndent;

			HorizontalCellAlignment alignment = formatData.AlignmentResolved;
			int indent = formatData.IndentResolved;
			int rotation = formatData.RotationResolved;
			ExcelDefaultableBoolean shrinkToFit = formatData.ShrinkToFitResolved;
			VerticalCellAlignment verticalAlignment = formatData.VerticalAlignmentResolved;
			ExcelDefaultableBoolean wrapText = formatData.WrapTextResolved;

			if (forceCreate == false &&
				addIndent == false &&	// MD 3/21/12 - TFS104630
				alignment == AlignmentInfo.DEFAULT_HORIZONTAL &&
				indent == 0 &&
				rotation == 0 &&
				shrinkToFit == ExcelDefaultableBoolean.False &&
				wrapText == ExcelDefaultableBoolean.False &&
				verticalAlignment == AlignmentInfo.DEFAULT_VERTICAL)
			{
				return null;
			}

			return new AlignmentInfo(
				alignment,
				indent,
				// MD 3/21/12 - TFS104630
				// We need to round-trip the AddIndent value.
				//false,
				addIndent,
				0,
				0,
				rotation,
				shrinkToFit,
				verticalAlignment,
				wrapText);
		}

		#endregion // CreateAlignmentInfo

		#region HasSameData

		internal static bool HasSameData(AlignmentInfo align1, AlignmentInfo align2)
		{
			if (ReferenceEquals(align1, null) &&
				ReferenceEquals(align2, null))
				return true;
			if (ReferenceEquals(align1, null) ||
				ReferenceEquals(align2, null))
				return false;
			return (align1.horizontal == align2.horizontal &&
				align1.indent == align2.indent &&
				align1.justifyLastLine == align2.justifyLastLine &&
				align1.readingOrder == align2.readingOrder &&
				align1.relativeIndent == align2.relativeIndent &&
				align1.shrinkToFit == align2.shrinkToFit &&
				align1.textRotation == align2.textRotation &&
				align1.vertical == align2.vertical &&
				align1.wrapText == align2.wrapText);
		} 

		#endregion // HasSameData

        #endregion Methods
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