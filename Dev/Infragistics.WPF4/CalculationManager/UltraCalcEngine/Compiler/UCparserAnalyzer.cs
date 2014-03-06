
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


using PerCederberg.Grammatica.Parser;


namespace Infragistics.Calculations.Engine







{

    



    internal abstract class UCparserAnalyzer : Analyzer {

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public override void Enter(Node node) {
            switch (node.GetId()) {
            case (int) UCparserConstants.LEFT_PAREN:
                EnterLeftParen((Token) node);
                break;
            case (int) UCparserConstants.RIGHT_PAREN:
                EnterRightParen((Token) node);
                break;
            case (int) UCparserConstants.NUMBER:
                EnterNumber((Token) node);
                break;
            case (int) UCparserConstants.TEXT:
                EnterText((Token) node);
                break;
            case (int) UCparserConstants.OP_DOT:
                EnterOpDot((Token) node);
                break;
            case (int) UCparserConstants.OP_EQUAL:
                EnterOpEqual((Token) node);
                break;
            case (int) UCparserConstants.OP_GT:
                EnterOpGt((Token) node);
                break;
            case (int) UCparserConstants.OP_LT:
                EnterOpLt((Token) node);
                break;
            case (int) UCparserConstants.OP_GE:
                EnterOpGe((Token) node);
                break;
            case (int) UCparserConstants.OP_LE:
                EnterOpLe((Token) node);
                break;
            case (int) UCparserConstants.OP_NE:
                EnterOpNe((Token) node);
                break;
            case (int) UCparserConstants.OP_ALT_NE:
                EnterOpAltNe((Token) node);
                break;
            case (int) UCparserConstants.OP_CONCAT:
                EnterOpConcat((Token) node);
                break;
            case (int) UCparserConstants.OP_PLUS:
                EnterOpPlus((Token) node);
                break;
            case (int) UCparserConstants.OP_MINUS:
                EnterOpMinus((Token) node);
                break;
            case (int) UCparserConstants.OP_TIMES:
                EnterOpTimes((Token) node);
                break;
            case (int) UCparserConstants.OP_DIV:
                EnterOpDiv((Token) node);
                break;
            case (int) UCparserConstants.OP_EXPON:
                EnterOpExpon((Token) node);
                break;
            case (int) UCparserConstants.OP_PERCENT:
                EnterOpPercent((Token) node);
                break;
            case (int) UCparserConstants.ARG_SEP:
                EnterArgSep((Token) node);
                break;
            case (int) UCparserConstants.RANGE_SEP:
                EnterRangeSep((Token) node);
                break;
            case (int) UCparserConstants.REFERENCE:
                EnterReference((Token) node);
                break;
            case (int) UCparserConstants.QUOTED_STRING:
                EnterQuotedString((Token) node);
                break;
            case (int) UCparserConstants.FORMULA:
                EnterFormula((Production) node);
                break;
            case (int) UCparserConstants.COMPARISON_OP:
                EnterComparisonOp((Production) node);
                break;
            case (int) UCparserConstants.COMPARISON_TERM:
                EnterComparisonTerm((Production) node);
                break;
            case (int) UCparserConstants.CONCAT_OP:
                EnterConcatOp((Production) node);
                break;
            case (int) UCparserConstants.CONCAT_TERM:
                EnterConcatTerm((Production) node);
                break;
            case (int) UCparserConstants.ADDITIVE_OP:
                EnterAdditiveOp((Production) node);
                break;
            case (int) UCparserConstants.ADDITIVE_TERM:
                EnterAdditiveTerm((Production) node);
                break;
            case (int) UCparserConstants.MULT_OP:
                EnterMultOp((Production) node);
                break;
            case (int) UCparserConstants.MULT_TERM:
                EnterMultTerm((Production) node);
                break;
            case (int) UCparserConstants.EXPON_OP:
                EnterExponOp((Production) node);
                break;
            case (int) UCparserConstants.POSTFIX_OP:
                EnterPostfixOp((Production) node);
                break;
            case (int) UCparserConstants.EXPON_TERM:
                EnterExponTerm((Production) node);
                break;
            case (int) UCparserConstants.PREFIX_OP:
                EnterPrefixOp((Production) node);
                break;
            case (int) UCparserConstants.POSTFIX_TERM:
                EnterPostfixTerm((Production) node);
                break;
            case (int) UCparserConstants.CONSTANT:
                EnterConstant((Production) node);
                break;
            case (int) UCparserConstants.TERM:
                EnterTerm((Production) node);
                break;
            case (int) UCparserConstants.RANGE:
                EnterRange((Production) node);
                break;
            case (int) UCparserConstants.FUNC_ID:
                EnterFuncId((Production) node);
                break;
            case (int) UCparserConstants.FUNC_ARGS:
                EnterFuncArgs((Production) node);
                break;
            case (int) UCparserConstants.FUNC_ARG:
                EnterFuncArg((Production) node);
                break;
            case (int) UCparserConstants.FUNCTION:
                EnterFunction((Production) node);
                break;
            }
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public override Node Exit(Node node) {
            switch (node.GetId()) {
            case (int) UCparserConstants.LEFT_PAREN:
                return ExitLeftParen((Token) node);
            case (int) UCparserConstants.RIGHT_PAREN:
                return ExitRightParen((Token) node);
            case (int) UCparserConstants.NUMBER:
                return ExitNumber((Token) node);
            case (int) UCparserConstants.TEXT:
                return ExitText((Token) node);
            case (int) UCparserConstants.OP_DOT:
                return ExitOpDot((Token) node);
            case (int) UCparserConstants.OP_EQUAL:
                return ExitOpEqual((Token) node);
            case (int) UCparserConstants.OP_GT:
                return ExitOpGt((Token) node);
            case (int) UCparserConstants.OP_LT:
                return ExitOpLt((Token) node);
            case (int) UCparserConstants.OP_GE:
                return ExitOpGe((Token) node);
            case (int) UCparserConstants.OP_LE:
                return ExitOpLe((Token) node);
            case (int) UCparserConstants.OP_NE:
                return ExitOpNe((Token) node);
            case (int) UCparserConstants.OP_ALT_NE:
                return ExitOpAltNe((Token) node);
            case (int) UCparserConstants.OP_CONCAT:
                return ExitOpConcat((Token) node);
            case (int) UCparserConstants.OP_PLUS:
                return ExitOpPlus((Token) node);
            case (int) UCparserConstants.OP_MINUS:
                return ExitOpMinus((Token) node);
            case (int) UCparserConstants.OP_TIMES:
                return ExitOpTimes((Token) node);
            case (int) UCparserConstants.OP_DIV:
                return ExitOpDiv((Token) node);
            case (int) UCparserConstants.OP_EXPON:
                return ExitOpExpon((Token) node);
            case (int) UCparserConstants.OP_PERCENT:
                return ExitOpPercent((Token) node);
            case (int) UCparserConstants.ARG_SEP:
                return ExitArgSep((Token) node);
            case (int) UCparserConstants.RANGE_SEP:
                return ExitRangeSep((Token) node);
            case (int) UCparserConstants.REFERENCE:
                return ExitReference((Token) node);
            case (int) UCparserConstants.QUOTED_STRING:
                return ExitQuotedString((Token) node);
            case (int) UCparserConstants.FORMULA:
                return ExitFormula((Production) node);
            case (int) UCparserConstants.COMPARISON_OP:
                return ExitComparisonOp((Production) node);
            case (int) UCparserConstants.COMPARISON_TERM:
                return ExitComparisonTerm((Production) node);
            case (int) UCparserConstants.CONCAT_OP:
                return ExitConcatOp((Production) node);
            case (int) UCparserConstants.CONCAT_TERM:
                return ExitConcatTerm((Production) node);
            case (int) UCparserConstants.ADDITIVE_OP:
                return ExitAdditiveOp((Production) node);
            case (int) UCparserConstants.ADDITIVE_TERM:
                return ExitAdditiveTerm((Production) node);
            case (int) UCparserConstants.MULT_OP:
                return ExitMultOp((Production) node);
            case (int) UCparserConstants.MULT_TERM:
                return ExitMultTerm((Production) node);
            case (int) UCparserConstants.EXPON_OP:
                return ExitExponOp((Production) node);
            case (int) UCparserConstants.POSTFIX_OP:
                return ExitPostfixOp((Production) node);
            case (int) UCparserConstants.EXPON_TERM:
                return ExitExponTerm((Production) node);
            case (int) UCparserConstants.PREFIX_OP:
                return ExitPrefixOp((Production) node);
            case (int) UCparserConstants.POSTFIX_TERM:
                return ExitPostfixTerm((Production) node);
            case (int) UCparserConstants.CONSTANT:
                return ExitConstant((Production) node);
            case (int) UCparserConstants.TERM:
                return ExitTerm((Production) node);
            case (int) UCparserConstants.RANGE:
                return ExitRange((Production) node);
            case (int) UCparserConstants.FUNC_ID:
                return ExitFuncId((Production) node);
            case (int) UCparserConstants.FUNC_ARGS:
                return ExitFuncArgs((Production) node);
            case (int) UCparserConstants.FUNC_ARG:
                return ExitFuncArg((Production) node);
            case (int) UCparserConstants.FUNCTION:
                return ExitFunction((Production) node);
            }
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public override void Child(Production node, Node child) {
            switch (node.GetId()) {
            case (int) UCparserConstants.FORMULA:
                ChildFormula(node, child);
                break;
            case (int) UCparserConstants.COMPARISON_OP:
                ChildComparisonOp(node, child);
                break;
            case (int) UCparserConstants.COMPARISON_TERM:
                ChildComparisonTerm(node, child);
                break;
            case (int) UCparserConstants.CONCAT_OP:
                ChildConcatOp(node, child);
                break;
            case (int) UCparserConstants.CONCAT_TERM:
                ChildConcatTerm(node, child);
                break;
            case (int) UCparserConstants.ADDITIVE_OP:
                ChildAdditiveOp(node, child);
                break;
            case (int) UCparserConstants.ADDITIVE_TERM:
                ChildAdditiveTerm(node, child);
                break;
            case (int) UCparserConstants.MULT_OP:
                ChildMultOp(node, child);
                break;
            case (int) UCparserConstants.MULT_TERM:
                ChildMultTerm(node, child);
                break;
            case (int) UCparserConstants.EXPON_OP:
                ChildExponOp(node, child);
                break;
            case (int) UCparserConstants.POSTFIX_OP:
                ChildPostfixOp(node, child);
                break;
            case (int) UCparserConstants.EXPON_TERM:
                ChildExponTerm(node, child);
                break;
            case (int) UCparserConstants.PREFIX_OP:
                ChildPrefixOp(node, child);
                break;
            case (int) UCparserConstants.POSTFIX_TERM:
                ChildPostfixTerm(node, child);
                break;
            case (int) UCparserConstants.CONSTANT:
                ChildConstant(node, child);
                break;
            case (int) UCparserConstants.TERM:
                ChildTerm(node, child);
                break;
            case (int) UCparserConstants.RANGE:
                ChildRange(node, child);
                break;
            case (int) UCparserConstants.FUNC_ID:
                ChildFuncId(node, child);
                break;
            case (int) UCparserConstants.FUNC_ARGS:
                ChildFuncArgs(node, child);
                break;
            case (int) UCparserConstants.FUNC_ARG:
                ChildFuncArg(node, child);
                break;
            case (int) UCparserConstants.FUNCTION:
                ChildFunction(node, child);
                break;
            }
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterLeftParen(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitLeftParen(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterRightParen(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitRightParen(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterNumber(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitNumber(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterText(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitText(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterOpDot(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitOpDot(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterOpEqual(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitOpEqual(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterOpGt(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitOpGt(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterOpLt(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitOpLt(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterOpGe(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitOpGe(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterOpLe(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitOpLe(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterOpNe(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitOpNe(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterOpAltNe(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitOpAltNe(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterOpConcat(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitOpConcat(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterOpPlus(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitOpPlus(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterOpMinus(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitOpMinus(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterOpTimes(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitOpTimes(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterOpDiv(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitOpDiv(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterOpExpon(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitOpExpon(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterOpPercent(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitOpPercent(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterArgSep(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitArgSep(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterRangeSep(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitRangeSep(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterReference(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitReference(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterQuotedString(Token node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitQuotedString(Token node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterFormula(Production node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitFormula(Production node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public virtual void ChildFormula(Production node, Node child) {
            node.AddChild(child);
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterComparisonOp(Production node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitComparisonOp(Production node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public virtual void ChildComparisonOp(Production node, Node child) {
            node.AddChild(child);
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterComparisonTerm(Production node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitComparisonTerm(Production node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public virtual void ChildComparisonTerm(Production node, Node child) {
            node.AddChild(child);
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterConcatOp(Production node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitConcatOp(Production node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public virtual void ChildConcatOp(Production node, Node child) {
            node.AddChild(child);
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterConcatTerm(Production node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitConcatTerm(Production node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public virtual void ChildConcatTerm(Production node, Node child) {
            node.AddChild(child);
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterAdditiveOp(Production node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitAdditiveOp(Production node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public virtual void ChildAdditiveOp(Production node, Node child) {
            node.AddChild(child);
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterAdditiveTerm(Production node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitAdditiveTerm(Production node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public virtual void ChildAdditiveTerm(Production node, Node child) {
            node.AddChild(child);
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterMultOp(Production node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitMultOp(Production node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public virtual void ChildMultOp(Production node, Node child) {
            node.AddChild(child);
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterMultTerm(Production node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitMultTerm(Production node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public virtual void ChildMultTerm(Production node, Node child) {
            node.AddChild(child);
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterExponOp(Production node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitExponOp(Production node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public virtual void ChildExponOp(Production node, Node child) {
            node.AddChild(child);
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterPostfixOp(Production node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitPostfixOp(Production node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public virtual void ChildPostfixOp(Production node, Node child) {
            node.AddChild(child);
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterExponTerm(Production node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitExponTerm(Production node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public virtual void ChildExponTerm(Production node, Node child) {
            node.AddChild(child);
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterPrefixOp(Production node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitPrefixOp(Production node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public virtual void ChildPrefixOp(Production node, Node child) {
            node.AddChild(child);
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterPostfixTerm(Production node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitPostfixTerm(Production node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public virtual void ChildPostfixTerm(Production node, Node child) {
            node.AddChild(child);
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterConstant(Production node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitConstant(Production node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public virtual void ChildConstant(Production node, Node child) {
            node.AddChild(child);
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterTerm(Production node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitTerm(Production node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public virtual void ChildTerm(Production node, Node child) {
            node.AddChild(child);
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterRange(Production node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitRange(Production node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public virtual void ChildRange(Production node, Node child) {
            node.AddChild(child);
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterFuncId(Production node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitFuncId(Production node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public virtual void ChildFuncId(Production node, Node child) {
            node.AddChild(child);
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterFuncArgs(Production node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitFuncArgs(Production node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public virtual void ChildFuncArgs(Production node, Node child) {
            node.AddChild(child);
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterFuncArg(Production node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitFuncArg(Production node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public virtual void ChildFuncArg(Production node, Node child) {
            node.AddChild(child);
        }

        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        public virtual void EnterFunction(Production node) {
        }

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        public virtual Node ExitFunction(Production node) {
            return node;
        }

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        public virtual void ChildFunction(Production node, Node child) {
            node.AddChild(child);
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