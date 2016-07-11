using System.Text;
using Dev2.Data.TO;

namespace Dev2.Data.Interfaces
{
    internal interface IParserHelper
    {
        bool ProcessOpenRegion(string payload, bool openRegion, int i, ref ParseTO currentNode, ref StringBuilder region, ref char cur);
        ParseTO CurrentNode(ParseTO currentNode, StringBuilder region, int i);
        bool ShouldAddToRegion(string payload, char cur, char prev, int i, bool shouldAddToRegion, char charToCheck);
    }
}