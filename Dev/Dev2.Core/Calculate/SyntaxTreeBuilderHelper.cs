
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
