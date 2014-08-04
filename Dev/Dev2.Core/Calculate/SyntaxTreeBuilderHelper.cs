using System.Parsing;
using System.Parsing.Intellisense;

namespace Dev2.Calculate
{


        public interface ISyntaxTreeBuilderHelper
        {
            ParseEventLog EventLog { get; }
            bool HasEventLogs { get; }
            SyntaxTreeBuilder Builder { get; }
            Node[] Build(string inputText, out Token[] tokens);
            Node[] Build(string inputText, bool expectedPartialTokens, out Token[] tokens);
        }

        public class SyntaxTreeBuilderHelper : ISyntaxTreeBuilderHelper
        {
            public SyntaxTreeBuilderHelper()
            {
                Builder = new SyntaxTreeBuilder();
            }

            public ParseEventLog EventLog
            {
                get
                {
                    return Builder.EventLog;
                }
            }

            public bool HasEventLogs
            {
                get
                {
                    return Builder.EventLog.HasEventLogs;
                }
            }

            public SyntaxTreeBuilder Builder { get; private set; }

            public Node[] Build(string inputText, out Token[] tokens)
            {
                return Builder.Build(inputText, out tokens);
            }

            public Node[] Build(string inputText, bool expectedPartialTokens, out Token[] tokens)
            {
                return Builder.Build(inputText, expectedPartialTokens, out tokens);
            }
        }
    
}
