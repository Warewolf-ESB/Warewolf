using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Parsing.Intellisense;
using System.Parsing.Tokenization;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Unlimited.UnitTest.Framework.Parsing
{
    [TestClass]    
    public class TokenizerTest
    {
        #region RequiredDefinitions Tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RequiredDefinitions_EnsureNotNull_Test()
        {
#pragma warning disable 219
            // ReSharper disable ObjectCreationAsStatement
            new RequiredTokenDefinitions<TokenKind>(TokenKind.Whitespace, TokenKind.LineBreak, null, TokenKind.EOF);
#pragma warning restore 219
            new RequiredTokenDefinitions<TokenKind>(TokenKind.Whitespace, TokenKind.LineBreak, TokenKind.EOF, null);
            new RequiredTokenDefinitions<TokenKind>(TokenKind.Whitespace, null, TokenKind.LineBreak, TokenKind.EOF);
            new RequiredTokenDefinitions<TokenKind>(null, TokenKind.LineBreak, TokenKind.LineBreak, TokenKind.EOF);
            // ReSharper restore ObjectCreationAsStatement
        }
        #endregion

        #region Tokenization Handler Tests
        [TestMethod]
        public void Range_EnsureSuccess_Test()
        {
            RequiredTokenDefinitions<TokenKind> definitions = new RequiredTokenDefinitions<TokenKind>(TokenKind.Whitespace, TokenKind.LineBreak, TokenKind.Unknown, TokenKind.EOF);
            Tokenizer<Token, TokenKind> tokenizer = new Tokenizer<Token, TokenKind>(definitions);
            tokenizer.Handlers.Add(new CSharpNumericLiteralTokenizationHandler());
            tokenizer.Handlers.Add(new WhitespaceTokenizationHandler<Token, TokenKind>());
            Token[] tokens = tokenizer.Tokenize("10 10.0 10.0d 10.0D 10.0f 10.0F 10u 10U 10ul 10UL 10lu 10LU 10l 10L 10e5 10e-5", 14, 11, false);

            for(Token i = tokens[0]; !i.Definition.IsEndOfFile; i = i.NextNWS)
            {
                Assert.AreEqual(true, i.Definition.IsRealLiteral || i.Definition.IsIntegerLiteral);
            }
        }

        [TestMethod]
        public void NumericTokenizationHandler_EnsureSuccess_Test()
        {
            RequiredTokenDefinitions<TokenKind> definitions = new RequiredTokenDefinitions<TokenKind>(TokenKind.Whitespace, TokenKind.LineBreak, TokenKind.Unknown, TokenKind.EOF);
            Tokenizer<Token, TokenKind> tokenizer = new Tokenizer<Token, TokenKind>(definitions);
            tokenizer.Handlers.Add(new CSharpNumericLiteralTokenizationHandler());
            tokenizer.Handlers.Add(new WhitespaceTokenizationHandler<Token, TokenKind>());
            Token[] tokens = tokenizer.Tokenize("10 10.0 10.0d 10.0D 10.0f 10.0F 10u 10U 10ul 10UL 10lu 10LU 10l 10L 10e5 10e-5");

            for(Token i = tokens[0]; !i.Definition.IsEndOfFile; i = i.NextNWS)
            {
                Assert.AreEqual(true, i.Definition.IsRealLiteral || i.Definition.IsIntegerLiteral);
            }
        }

        [TestMethod]
        public void NumericTokenizationHandler_EnsureFailure_Test()
        {
            RequiredTokenDefinitions<TokenKind> definitions = new RequiredTokenDefinitions<TokenKind>(TokenKind.Whitespace, TokenKind.LineBreak, TokenKind.Unknown, TokenKind.EOF);
            Tokenizer<Token, TokenKind> tokenizer = new Tokenizer<Token, TokenKind>(definitions);
            tokenizer.Handlers.Add(new CSharpNumericLiteralTokenizationHandler());
            tokenizer.Handlers.Add(new WhitespaceTokenizationHandler<Token, TokenKind>());
            Token[] tokens = tokenizer.Tokenize("10 10.0 10.0d 10.0D 10.0f 10.0F 10u 10U 10ul 10UL 10lu 10LU 10l 10L 10e5 10e-5 asd");

            Assert.AreEqual(true, tokens[tokens.Length - 1].PreviousNWS.Definition == TokenKind.Unknown);
        }

        [TestMethod]
        public void CharTokenizationHandler_EnsureSuccess_Test()
        {
            RequiredTokenDefinitions<TokenKind> definitions = new RequiredTokenDefinitions<TokenKind>(TokenKind.Whitespace, TokenKind.LineBreak, TokenKind.Unknown, TokenKind.EOF);
            Tokenizer<Token, TokenKind> tokenizer = new Tokenizer<Token, TokenKind>(definitions);
            CharacterLiteralTokenizationHandler handler = new CharacterLiteralTokenizationHandler();
            tokenizer.Handlers.Add(handler);
            tokenizer.Handlers.Add(handler);
            tokenizer.Handlers.Add(new WhitespaceTokenizationHandler<Token, TokenKind>());
            Token[] tokens = tokenizer.Tokenize("\'r\' \'\\u0066\' \'\\\'\' \'\\x000a\' ");

            for(Token i = tokens[0]; !i.Definition.IsEndOfFile; i = i.NextNWS)
            {
                Assert.AreEqual(true, i.Definition.IsCharLiteral);
            }
        }

        [TestMethod]
        public void CharTokenizationHandler_EnsureFailure_Test()
        {
            RequiredTokenDefinitions<TokenKind> definitions = new RequiredTokenDefinitions<TokenKind>(TokenKind.Whitespace, TokenKind.LineBreak, TokenKind.Unknown, TokenKind.EOF);
            Tokenizer<Token, TokenKind> tokenizer = new Tokenizer<Token, TokenKind>(definitions);
            CharacterLiteralTokenizationHandler handler = new CharacterLiteralTokenizationHandler();
            tokenizer.Handlers.Add(handler);
            tokenizer.Handlers.Add(handler);
            tokenizer.Handlers.Add(new WhitespaceTokenizationHandler<Token, TokenKind>());
            Token[] tokens = tokenizer.Tokenize("\'r\' \'\\u0066\' \'\\\'\' \'\\x000a\' asd");
            Assert.AreEqual(true, tokens[tokens.Length - 1].PreviousNWS.Definition == TokenKind.Unknown);
        }

        [TestMethod]
        public void StringTokenizationHandler_EnsureSuccess_Test()
        {
            RequiredTokenDefinitions<TokenKind> definitions = new RequiredTokenDefinitions<TokenKind>(TokenKind.Whitespace, TokenKind.LineBreak, TokenKind.Unknown, TokenKind.EOF);
            Tokenizer<Token, TokenKind> tokenizer = new Tokenizer<Token, TokenKind>(definitions);
            tokenizer.Handlers.Add(new StringLiteralTokenizationHandler());
            tokenizer.Handlers.Add(new WhitespaceTokenizationHandler<Token, TokenKind>());
            Token[] tokens = tokenizer.Tokenize("@\"asd\"\"asd\" \"asd\\\"asd\"");

            for(Token i = tokens[0]; !i.Definition.IsEndOfFile; i = i.NextNWS)
            {
                Assert.AreEqual(true, i.Definition.IsStringLiteral);
            }
        }

        [TestMethod]
        public void StringTokenizationHandler_EnsureFailure_Test()
        {
            RequiredTokenDefinitions<TokenKind> definitions = new RequiredTokenDefinitions<TokenKind>(TokenKind.Whitespace, TokenKind.LineBreak, TokenKind.Unknown, TokenKind.EOF);
            Tokenizer<Token, TokenKind> tokenizer = new Tokenizer<Token, TokenKind>(definitions);
            tokenizer.Handlers.Add(new StringLiteralTokenizationHandler());
            tokenizer.Handlers.Add(new WhitespaceTokenizationHandler<Token, TokenKind>());
            Token[] tokens = tokenizer.Tokenize("@\"asd\"\"asd\" \"asd\\\"asd\" asd");
            Assert.AreEqual(true, tokens[tokens.Length - 1].PreviousNWS.Definition == TokenKind.Unknown);
        }
        #endregion

        #region AbstractSyntaxTreeBuilder Tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AbstractSyntaxTreeBuilder_RegisterDuplicate_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.RegisterGrammer(new InfrigisticsGrammer());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AbstractSyntaxTreeBuilder_RegisterMultipleFromGrammerGroup_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.RegisterGrammer(new CSharpNumericLiteralGrammer());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AbstractSyntaxTreeBuilder_MultipleNonUnaryTokenizationHandlers_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.RegisterGrammer(new TestGrammer(0));
            builder.Build("5 + 4");
        }

        [TestMethod]
        public void AbstractSyntaxTreeBuilder_InferredUnaryDefinitionsMatchExpected()
        {
            TestGrammer grammer;
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.RegisterGrammer(grammer = new TestGrammer(3));
            builder.Build("5");
            int present = 0;

            for(int i = 0; i < grammer.Tokenizer.Handlers.Count; i++)
                if(grammer.Tokenizer.Handlers[i] is UnaryTokenizationHandler<Token, TokenKind>)
                    present++;

            Assert.AreEqual(1, present);
        }

        [TestMethod]
        public void AbstractSyntaxTreeBuilder_InferredKeywordDefinitionsMatchExpected()
        {
            TestGrammer grammer;
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.RegisterGrammer(grammer = new TestGrammer(3));
            builder.Build("5");
            Assert.AreEqual(true, grammer.Tokenizer.Keywords.ContainsKey("true"));
            Assert.AreEqual(true, grammer.Tokenizer.Keywords.ContainsKey("false"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AbstractSyntaxTreeBuilder_RegisterAfterBuild_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("5");
            builder.RegisterGrammer(new CharLiteralGrammer());
        }
        #endregion

        #region ASTGrammerBehaviourRegistry Tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ASTGrammerBehaviourRegistry_RegisterTrigger_NullDefinition_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.RegisterGrammer(new TestGrammer(1));
            builder.Build("5 + 4");
        }

        [TestMethod]
        [ExpectedException(typeof(ReadOnlyException))]
        public void ASTGrammerBehaviourRegistry_RegisterTrigger_OutOfBand_Test()
        {
            TestGrammer grammer;
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.RegisterGrammer(grammer = new TestGrammer(2));
            builder.Build("5");
            grammer.CachedRegistry.Register(typeof(CollectionNode));

        }
        #endregion

        #region DataListGrammer Tests
        [TestMethod]
        public void DataListGrammer_IsolationGrammer_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[entry]]");
            Assert.AreEqual(false, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void DataListGrammer_IsolationGrammer_EnsureFailure_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[entry]2]");
            Assert.AreEqual(true, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void DataListGrammer_IsolationGrammer_NDepthRecursion_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[entry([[entry([[entry(5)]])]])]]");
            Assert.AreEqual(false, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void DataListGrammer_IsolationGrammer_NDepthRecursion_EnsureFailure_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[entry([[entry([[entry(5)]])]2])]]");
            Assert.AreEqual(true, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void DataListGrammer_IsolationGrammer_CompositeIdentifier_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[[[identifier]](5)]]");
            Assert.AreEqual(false, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void DataListGrammer_IsolationGrammer_CompositeIdentifier_EnsureFailure_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[2[[[identifier]](5)]]");
            Assert.AreEqual(true, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void DataListGrammer_IsolationGrammer_CompositeField_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[identifier(5).[[field]]]]");
            Assert.AreEqual(false, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void DataListGrammer_IsolationGrammer_CompositeField_EnsureFailure_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[identifier(5).[[field]]]2]");
            Assert.AreEqual(true, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void DataListGrammer_IsolationGrammer_CompositeIdentifier_CompositeField_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[[[identifier]](5).[[field]]]]");
            Assert.AreEqual(false, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void DataListGrammer_IsolationGrammer_CompositeIdentifier_CompositeField_EnsureFailure_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[[[identifier]2](5).[[field]]]]");
            Assert.AreEqual(true, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void DataListGrammer_IsolationGrammer_ComplexParameter_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[identifier([[parameter]]).field]]");
            Assert.AreEqual(false, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void DataListGrammer_IsolationGrammer_ComplexParameter_EnsureFailure_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[2[identifier([[parameter]]).field]]");
            Assert.AreEqual(true, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void DataListGrammer_IsolationGrammer_NestedComposite_ComplexParameter_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[[[identifier]]([[parameter]]).[[field]]]]");
            Assert.AreEqual(false, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void DataListGrammer_IsolationGrammer_NestedComposite_ComplexParameter_EnsureFailure_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[[[identifier]]([[parameter]2]).[[field]]]]");
            Assert.AreEqual(true, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void DataListGrammer_IsolationGrammer_NestedComposite_ComplexParameter_NestedSimple_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[[[identifier]]([[parameter(10).field]]).[[field]]]]");
            Assert.AreEqual(false, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void DataListGrammer_IsolationGrammer_NestedComposite_ComplexParameter_NestedSimple_EnsureFailure_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[[[identifier]]([[parameter(10).field]]).[2[field]]]]");
            Assert.AreEqual(true, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        [Owner("Tshepo")]
        [TestCategory("DataListGrammer_IsolationGrammer")]
        public void DataListGrammer_IsolationGrammer_SingleVariable_HasEventLogsIsFalse()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[entry]]");
            Assert.IsFalse(builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        [Owner("Tshepo")]
        [TestCategory("DataListGrammer_IsolationGrammer")]
        public void DataListGrammer_IsolationGrammer_VariablesSideBySide_HasEventLogsIsFalse()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[entry]][[var]]");
            Assert.IsFalse(builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        [Owner("Tshepo")]
        [TestCategory("SyntaxTreeBuilder_Build")]
        public void SyntaxTreeBuilder_Build_VariablesSideBySideWithNewLine_HasEventLogsIsFalse()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            var input = "[[entry]]" + Environment.NewLine + "[[var]]";
            builder.Build(input);
            Assert.IsFalse(builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        [Owner("Tshepo")]
        [TestCategory("DataListGrammer_IsolationGrammer")]
        public void DataListGrammer_IsolationGrammer_VariablesSideBySideWithAnAdditionOperator_HasEventLogsIsFalse()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[entry]]+[[var]]");
            Assert.IsFalse(builder.EventLog.HasEventLogs);
        }


        [TestMethod]
        [Owner("Tshepo")]
        [TestCategory("DataListGrammer_IsolationGrammer")]
        public void DataListGrammer_IsolationGrammer_VariablesSideBySide_WithModOperator_HasEventLogsIsFalse()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[rec().a]]%[[rec().b]] ");
            Assert.IsFalse(builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        [Owner("Tshepo")]
        [TestCategory("DataListGrammer_IsolationGrammer")]
        public void DataListGrammer_IsolationGrammer_InvalidVar_HasEventLogsIsTrue()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("[[entry");
            Assert.IsTrue(builder.EventLog.HasEventLogs);
        }
        #endregion

        #region UltracalcGrammer Tests
        [TestMethod]
        public void UltracalcGrammer_StringLiteralGrammer_EnsureSuccess_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("\"as\\nd\" & \"jack\"");
            Assert.AreEqual(false, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void UltracalcGrammer_StringLiteralGrammer_NestedDatalist_EnsureSuccess_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("\"asd[[bob]]\" & \"jack\"");
            Assert.AreEqual(false, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void UltracalcGrammer_StringLiteralGrammer_NestedDatalist_Infragistics_EnsureSuccess_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("\"asd[[bob(sum(5,-1)).field]]\" & \"jack\"");
            Assert.AreEqual(false, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void UltracalcGrammer_StringLiteralGrammer_CompositeNestedDatalist_Infragistics_EnsureSuccess_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("\"asd[[[[Jon]](sum(5,-1)).[[Field]]]]\" & \"jack\"");
            Assert.AreEqual(false, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void UltracalcGrammer_StringLiteralGrammer_PartialDatalist_EnsureSuccess_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("\"asd[[asd\" & \"jack\"");
            Assert.AreEqual(false, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void UltracalcGrammer_IsolationGrammer_EnsureSuccess_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("10 + 5");
            Assert.AreEqual(false, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void UltracalcGrammer_IsolationGrammer_NDepthRecursion_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("10 + (5 - (10 + (5 - (10 + 5))))");
            Assert.AreEqual(false, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void UltracalcGrammer_IsolationGrammer_CompositeIdentifier_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("10 + sum(5 - (10 + (5 - (10 + [[entry]]))))");
            Assert.AreEqual(false, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void UltracalcGrammer_IsolationGrammer_CompositeField_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("10 + (5 - (10 + (5 - (10 + [[entry(5).[[field]]]]))))");
            Assert.AreEqual(false, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void UltracalcGrammer_IsolationGrammer_CompositeIdentifier_CompositeField_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("10 + sum(5 - (10 + (5 - (10 + [[entry(5).[[field]]]]))), 10)");
            Assert.AreEqual(false, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void UltracalcGrammer_IsolationGrammer_ComplexParameter_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("10 + sum(5 - (10 + (5 - (10 + [[entry(5).[[field]]]]))), 50 / 2 ^ 8, [[identifier]])");
            Assert.AreEqual(false, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void UltracalcGrammer_IsolationGrammer_NestedComposite_ComplexParameter_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("10 + sum(5 - (10 + (5 - (10 + [[entry(5).[[field]]]]))), 50 / 2 ^ 8, [[identifier]], 10 + sum(5 - (10 + (5 - (10 + [[entry(5).[[field]]]]))), 50 / 2 ^ 8, [[identifier]]))");
            Assert.AreEqual(false, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void UltracalcGrammer_IsolationGrammer_EnsureFailure_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("10 + 5 asd");
            Assert.AreEqual(true, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void UltracalcGrammer_IsolationGrammer_NDepthRecursion_EnsureFailure_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("10 + (5 - (10 + (5 -  as d(10 + 5))))");
            Assert.AreEqual(true, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void UltracalcGrammer_IsolationGrammer_CompositeIdentifier_EnsureFailure_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("10 + sum(5 - (10 + (5 - (10 + [asd[entry]]))))");
            Assert.AreEqual(true, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void UltracalcGrammer_IsolationGrammer_CompositeField_EnsureFailure_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("10 + (5 - (10 + (5 - (10 + [[entry(5).[[field]] fds] asdfa]))))");
            Assert.AreEqual(true, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void UltracalcGrammer_IsolationGrammer_CompositeIdentifier_CompositeField_EnsureFailure_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("10 + sum(5 - - (10 + (5 - (10 + [[entry(5).[[field]]]]))), 10)");
            Assert.AreEqual(true, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void UltracalcGrammer_IsolationGrammer_ComplexParameter_EnsureFailure_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("10 + sum(5 - (10 + (5 - (10 + [[entry(5).[[field]]]]))), 50 / 2 ^ 8, [[identifier]]) +");
            Assert.AreEqual(true, builder.EventLog.HasEventLogs);
        }

        [TestMethod]
        public void UltracalcGrammer_IsolationGrammer_NestedComposite_ComplexParameter_EnsureFailure_Test()
        {
            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            builder.Build("10 + sum(5 - (10 + (5 - (10 + [[entry(5).[[field]]]]))), 50 / 2 ^ 8, [[identifier]], 10 + sum(5 - (10 + (5 - (10 + [[entry(5).[[field]]]]))), 50 / 2 != 8, [[identifier]]))");
            Assert.AreEqual(true, builder.EventLog.HasEventLogs);
        }
        #endregion

        #region TestGrammer
        private sealed class TestGrammer : System.Parsing.SyntaxAnalysis.AbstractSyntaxTreeGrammer<Token, TokenKind, Node>
        {
            private static readonly System.Parsing.SyntaxAnalysis.GrammerGroup _testGroup = new System.Parsing.SyntaxAnalysis.GrammerGroup("Test Grammer");

            private int _operation;

            public System.Parsing.SyntaxAnalysis.ASTGrammerBehaviourRegistry CachedRegistry;
            public Tokenizer<Token, TokenKind> Tokenizer;

            public override System.Parsing.SyntaxAnalysis.GrammerGroup GrammerGroup { get { return _testGroup; } }

            public TestGrammer(int operation)
            {
                _operation = operation;
            }

            protected override void OnConfigureTokenizer(Tokenizer<Token, TokenKind> tokenizer)
            {
                if(_operation == 0)
                {
                    tokenizer.Handlers.Add(new StringLiteralTokenizationHandler());
                    tokenizer.Handlers.Add(new StringLiteralTokenizationHandler());
                }
                else if(_operation == 3)
                {
                    Tokenizer = tokenizer;
                }
            }

            protected override Node BuildNode(System.Parsing.SyntaxAnalysis.AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder, Node container, Token start, Token last)
            {
                throw new NotImplementedException();
            }

            protected override void OnRegisterTriggers(System.Parsing.SyntaxAnalysis.ASTGrammerBehaviourRegistry triggerRegistry)
            {
                if(_operation == 1)
                {
                    triggerRegistry.Register((TokenDefinition)null);
                }
                else if(_operation == 2)
                {
                    CachedRegistry = triggerRegistry;
                }
            }
        }
        #endregion
    }
}
