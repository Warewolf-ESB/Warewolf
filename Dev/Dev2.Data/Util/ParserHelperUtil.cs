using System.Text;
using Dev2.Data.Enums;
using Dev2.Data.Exceptions;
using Dev2.Data.Interfaces;
using Dev2.Data.TO;
using Warewolf.Resource.Errors;

namespace Dev2.Data.Util
{
    internal class ParserHelperUtil: IParserHelper {
        #region Implementation of IParserHelper

        public bool ProcessOpenRegion(string payload, bool openRegion, int i, ref ParseTO currentNode, ref StringBuilder region, ref char cur)
        {
            if (openRegion)
            {
                openRegion = CloseNode(currentNode, i, region);

                currentNode = ProcessNode(payload, currentNode, ref region, ref openRegion);
                cur = '\0';
            }
            else
            {
                throw new Dev2DataLanguageParseError(ErrorResource.InvalidOpenRegion, 0, payload.Length, enIntellisenseErrorCode.SyntaxError);
            }
            return openRegion;
        }

        private bool CloseNode(ParseTO currentNode, int i, StringBuilder region)
        {
            const bool OpenRegion = false;
            currentNode.EndIndex = i;
            currentNode.HangingOpen = false;
            currentNode.Payload = region.ToString();
            region.Clear();
            currentNode.EndIndex = i - 2;
            return OpenRegion;
        }

        public ParseTO CurrentNode(ParseTO currentNode, StringBuilder region, int i)
        {
            currentNode.Payload = region.ToString();
            region.Clear();
            ParseTO child = new ParseTO();
            currentNode.Child = child;
            child.HangingOpen = true;
            child.Parent = currentNode;
            child.EndIndex = -1;
            child.StartIndex = i;
            currentNode = child;
            return currentNode;
        }

        private ParseTO ProcessNode(string payload, ParseTO currentNode, ref StringBuilder region, ref bool openRegion)
        {
            if (!currentNode.IsRoot)
            {
                currentNode = currentNode.Parent;
                region = new StringBuilder(currentNode.Payload);
                openRegion = true;
            }
            else if (currentNode.IsRoot && !currentNode.IsLeaf && currentNode.Child.HangingOpen)
            {
                throw new Dev2DataLanguageParseError(ErrorResource.InvalidSyntaxCreatingVariable, 0, payload.Length, enIntellisenseErrorCode.SyntaxError);
            }
            return currentNode;
        }

        public bool ShouldAddToRegion(string payload, char cur, char prev, int i, bool shouldAddToRegion, char charToCheck)
        {
            if (cur == charToCheck && prev != charToCheck)
            {
                var checkIndex = i + 1;
                if (checkIndex < payload.Length)
                {
                    if (payload[checkIndex] == charToCheck)
                    {
                        shouldAddToRegion = false;
                    }
                }
            }
            return shouldAddToRegion;
        }

        #endregion
    }
}