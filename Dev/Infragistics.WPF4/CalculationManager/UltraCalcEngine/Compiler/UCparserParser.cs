
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


using System.IO;

using PerCederberg.Grammatica.Parser;


namespace Infragistics.Calculations.Engine







{

    


    internal class UCparserParser : RecursiveDescentParser {

        



        private enum SynteticPatterns {
            SUBPRODUCTION_1 = 3001,
            SUBPRODUCTION_2 = 3002,
            SUBPRODUCTION_3 = 3003,
            SUBPRODUCTION_4 = 3004,
            SUBPRODUCTION_5 = 3005,
            SUBPRODUCTION_6 = 3006,
            SUBPRODUCTION_7 = 3007,
            SUBPRODUCTION_8 = 3008,
            SUBPRODUCTION_9 = 3009,
            SUBPRODUCTION_10 = 3010,
            SUBPRODUCTION_11 = 3011
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public UCparserParser(TextReader input)
            : base(new UCparserTokenizer(input)) {

            CreatePatterns();
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public UCparserParser(TextReader input, Analyzer analyzer)
            : base(new UCparserTokenizer(input), analyzer) {

            CreatePatterns();
        }

        
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        private void CreatePatterns() {
            ProductionPattern             pattern;
            ProductionPatternAlternative  alt;

            pattern = new ProductionPattern((int) UCparserConstants.FORMULA,
                                            "formula");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) UCparserConstants.COMPARISON_TERM, 1, 1);
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_1, 0, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) UCparserConstants.COMPARISON_OP,
                                            "comparison_op");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.OP_EQUAL, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.OP_GT, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.OP_LT, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.OP_GE, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.OP_LE, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.OP_NE, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.OP_ALT_NE, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) UCparserConstants.COMPARISON_TERM,
                                            "comparison_term");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) UCparserConstants.CONCAT_TERM, 1, 1);
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_2, 0, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) UCparserConstants.CONCAT_OP,
                                            "concat_op");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.OP_CONCAT, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) UCparserConstants.CONCAT_TERM,
                                            "concat_term");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) UCparserConstants.ADDITIVE_TERM, 1, 1);
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_3, 0, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) UCparserConstants.ADDITIVE_OP,
                                            "additive_op");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.OP_PLUS, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.OP_MINUS, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) UCparserConstants.ADDITIVE_TERM,
                                            "additive_term");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) UCparserConstants.MULT_TERM, 1, 1);
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_4, 0, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) UCparserConstants.MULT_OP,
                                            "mult_op");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.OP_TIMES, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.OP_DIV, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) UCparserConstants.MULT_TERM,
                                            "mult_term");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) UCparserConstants.EXPON_TERM, 1, 1);
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_5, 0, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) UCparserConstants.EXPON_OP,
                                            "expon_op");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.OP_EXPON, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) UCparserConstants.POSTFIX_OP,
                                            "postfix_op");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.OP_PERCENT, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) UCparserConstants.EXPON_TERM,
                                            "expon_term");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) UCparserConstants.POSTFIX_TERM, 1, 1);
            alt.AddProduction((int) UCparserConstants.POSTFIX_OP, 0, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) UCparserConstants.PREFIX_OP,
                                            "prefix_op");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.OP_PLUS, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.OP_MINUS, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) UCparserConstants.POSTFIX_TERM,
                                            "postfix_term");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) UCparserConstants.PREFIX_OP, 0, 1);
            alt.AddProduction((int) UCparserConstants.TERM, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) UCparserConstants.CONSTANT,
                                            "constant");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_9, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) UCparserConstants.TERM,
                                            "term");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) UCparserConstants.CONSTANT, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.QUOTED_STRING, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) UCparserConstants.FUNCTION, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.REFERENCE, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.LEFT_PAREN, 1, 1);
            alt.AddProduction((int) UCparserConstants.FORMULA, 1, 1);
            alt.AddToken((int) UCparserConstants.RIGHT_PAREN, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) UCparserConstants.RANGE,
                                            "range");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.REFERENCE, 1, 1);
            alt.AddToken((int) UCparserConstants.RANGE_SEP, 1, 1);
            alt.AddToken((int) UCparserConstants.REFERENCE, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) UCparserConstants.FUNC_ID,
                                            "func_id");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.TEXT, 1, 1);
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_10, 0, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) UCparserConstants.FUNC_ARGS,
                                            "func_args");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) UCparserConstants.FUNC_ARG, 1, 1);
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_11, 0, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) UCparserConstants.FUNC_ARG,
                                            "func_arg");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) UCparserConstants.RANGE, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) UCparserConstants.FORMULA, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) UCparserConstants.FUNCTION,
                                            "function");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) UCparserConstants.FUNC_ID, 1, 1);
            alt.AddToken((int) UCparserConstants.LEFT_PAREN, 1, 1);
            alt.AddProduction((int) UCparserConstants.FUNC_ARGS, 0, 1);
            alt.AddToken((int) UCparserConstants.RIGHT_PAREN, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_1,
                                            "Subproduction1");
            pattern.SetSyntetic(true);
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) UCparserConstants.COMPARISON_OP, 1, 1);
            alt.AddProduction((int) UCparserConstants.COMPARISON_TERM, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_2,
                                            "Subproduction2");
            pattern.SetSyntetic(true);
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) UCparserConstants.CONCAT_OP, 1, 1);
            alt.AddProduction((int) UCparserConstants.CONCAT_TERM, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_3,
                                            "Subproduction3");
            pattern.SetSyntetic(true);
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) UCparserConstants.ADDITIVE_OP, 1, 1);
            alt.AddProduction((int) UCparserConstants.ADDITIVE_TERM, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_4,
                                            "Subproduction4");
            pattern.SetSyntetic(true);
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) UCparserConstants.MULT_OP, 1, 1);
            alt.AddProduction((int) UCparserConstants.MULT_TERM, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_5,
                                            "Subproduction5");
            pattern.SetSyntetic(true);
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) UCparserConstants.EXPON_OP, 1, 1);
            alt.AddProduction((int) UCparserConstants.EXPON_TERM, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_6,
                                            "Subproduction6");
            pattern.SetSyntetic(true);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.OP_DOT, 1, 1);
            alt.AddToken((int) UCparserConstants.NUMBER, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_7,
                                            "Subproduction7");
            pattern.SetSyntetic(true);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.OP_DOT, 1, 1);
            alt.AddToken((int) UCparserConstants.NUMBER, 0, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_8,
                                            "Subproduction8");
            pattern.SetSyntetic(true);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.NUMBER, 1, 1);
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_7, 0, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_9,
                                            "Subproduction9");
            pattern.SetSyntetic(true);
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_6, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_8, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_10,
                                            "Subproduction10");
            pattern.SetSyntetic(true);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.TEXT, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.NUMBER, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_11,
                                            "Subproduction11");
            pattern.SetSyntetic(true);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) UCparserConstants.ARG_SEP, 1, 1);
            alt.AddProduction((int) UCparserConstants.FUNC_ARG, 1, 1);
            pattern.AddAlternative(alt);
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