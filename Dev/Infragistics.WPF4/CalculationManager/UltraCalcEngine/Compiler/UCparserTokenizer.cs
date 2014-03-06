
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


using System.IO;

using PerCederberg.Grammatica.Parser;







namespace Infragistics.Calculations.Engine








{

    


    internal class UCparserTokenizer : Tokenizer {

        
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

        public UCparserTokenizer(TextReader input)
            : base(input) {

            CreatePatterns();
        }

        
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        private void CreatePatterns() {
            TokenPattern  pattern;

            pattern = new TokenPattern((int) UCparserConstants.LEFT_PAREN,
                                       // AS 5/12/05 Localization
                                       //"LEFT_PAREN",
                                       SR.GetString("TokenPattern_LeftParen"),
                                       TokenPattern.PatternType.STRING,
                                       "(");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.RIGHT_PAREN,
                                       // AS 5/12/05 Localization
                                       //"RIGHT_PAREN",
                                       SR.GetString("TokenPattern_RightParen"),
                                       TokenPattern.PatternType.STRING,
                                       ")");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.NUMBER,
                                       // AS 5/12/05 Localization
                                       //"NUMBER",
                                       SR.GetString("TokenPattern_Number"),
                                       TokenPattern.PatternType.REGEXP,
                                       "[0-9]+");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.TEXT,
                                       // AS 5/12/05 Localization
                                       //"TEXT",
                                       SR.GetString("TokenPattern_Text"),
                                       TokenPattern.PatternType.REGEXP,
                                       "[A-Za-z_]+");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.WHITESPACE,
                                       // AS 5/12/05 Localization
                                       //"WHITESPACE",
                                       SR.GetString("TokenPattern_WhiteSpace"),
                                       TokenPattern.PatternType.REGEXP,
                                       "[ \\t\\n\\r]+");
            pattern.SetIgnore();
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.OP_DOT,
                                       // AS 5/12/05 Localization
                                       //"OP_DOT",
                                       SR.GetString("TokenPattern_OperatorDot"),
                                       TokenPattern.PatternType.STRING,
                                       ".");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.OP_EQUAL,
                                       // AS 5/12/05 Localization
                                       //"OP_EQUAL",
                                       SR.GetString("TokenPattern_OperatorEqual"),
                                       TokenPattern.PatternType.STRING,
                                       "=");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.OP_GT,
                                       // AS 5/12/05 Localization
                                       //"OP_GT",
                                       SR.GetString("TokenPattern_OperatorGreaterThan"),
                                       TokenPattern.PatternType.STRING,
                                       ">");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.OP_LT,
                                       // AS 5/12/05 Localization
                                       //"OP_LT",
                                       SR.GetString("TokenPattern_OperatorLessThan"),
                                       TokenPattern.PatternType.STRING,
                                       "<");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.OP_GE,
                                       // AS 5/12/05 Localization
                                       //"OP_GE",
                                       SR.GetString("TokenPattern_OperatorGreaterThanEqual"),
                                       TokenPattern.PatternType.STRING,
                                       ">=");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.OP_LE,
                                       // AS 5/12/05 Localization
                                       //"OP_LE",
                                       SR.GetString("TokenPattern_OperatorLessThanEqual"),
                                       TokenPattern.PatternType.STRING,
                                       "<=");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.OP_NE,
                                       // AS 5/12/05 Localization
                                       //"OP_NE",
                                       SR.GetString("TokenPattern_OperatorNotEqual"),
                                       TokenPattern.PatternType.STRING,
                                       "!=");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.OP_ALT_NE,
                                       // AS 5/12/05 Localization
                                       //"OP_ALT_NE",
                                       SR.GetString("TokenPattern_OperatorNotEqualAlternate"),
                                       TokenPattern.PatternType.STRING,
                                       "<>");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.OP_CONCAT,
                                       // AS 5/12/05 Localization
                                       //"OP_CONCAT",
                                       SR.GetString("TokenPattern_OperatorConcatenate"),
                                       TokenPattern.PatternType.STRING,
                                       "&");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.OP_PLUS,
                                       // AS 5/12/05 Localization
                                       //"OP_PLUS",
                                       SR.GetString("TokenPattern_OperatorPlus"),
                                       TokenPattern.PatternType.STRING,
                                       "+");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.OP_MINUS,
                                       // AS 5/12/05 Localization
                                       //"OP_MINUS",
                                       SR.GetString("TokenPattern_OperatorMinus"),
                                       TokenPattern.PatternType.STRING,
                                       "-");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.OP_TIMES,
                                       // AS 5/12/05 Localization
                                       //"OP_TIMES",
                                       SR.GetString("TokenPattern_OperatorMultiply"),
                                       TokenPattern.PatternType.STRING,
                                       "*");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.OP_DIV,
                                       // AS 5/12/05 Localization
                                       //"OP_DIV",
                                       SR.GetString("TokenPattern_OperatorDivide"),
                                       TokenPattern.PatternType.STRING,
                                       "/");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.OP_EXPON,
                                       // AS 5/12/05 Localization
                                       //"OP_EXPON",
                                       SR.GetString("TokenPattern_OperatorExponent"),
                                       TokenPattern.PatternType.STRING,
                                       "^");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.OP_PERCENT,
                                       // AS 5/12/05 Localization
                                       //"OP_PERCENT",
                                       SR.GetString("TokenPattern_OperatorPercent"),
                                       TokenPattern.PatternType.STRING,
                                       "%");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.ARG_SEP,
                                       // AS 5/12/05 Localization
                                       //"ARG_SEP",
                                       SR.GetString("TokenPattern_ArgumentSeparator"),
                                       TokenPattern.PatternType.STRING,
                                       ",");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.RANGE_SEP,
                                       // AS 5/12/05 Localization
                                       //"RANGE_SEP",
                                       SR.GetString("TokenPattern_RangeSeparator"),
                                       TokenPattern.PatternType.REGEXP,
                                       "\\.\\.|\\:");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.REFERENCE,
                                       // AS 5/12/05 Localization
                                       //"REFERENCE",
                                       SR.GetString("TokenPattern_Reference"),
                                       TokenPattern.PatternType.REGEXP,
                                       "\\[([^\\[\\]\\\\\"]|\\\\.|\"([^\"]|\\\\.)*\")+\\]");
            AddPattern(pattern);

            pattern = new TokenPattern((int) UCparserConstants.QUOTED_STRING,
                                       // AS 5/12/05 Localization
                                       //"QUOTED_STRING",
                                       SR.GetString("TokenPattern_QuotedString"),
                                       TokenPattern.PatternType.REGEXP,
                                       "\"([^\"]|\\\\.)*\"|'([^']|\\\\.)*'");
            AddPattern(pattern);
        }
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