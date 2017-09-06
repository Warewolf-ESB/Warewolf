using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Activities.Presentation.Services;

namespace Warewolf.MergeParser
{
    public class ParseServiceForDifferences
    {
        readonly ModelItem _head;
        readonly ModelItem _mergeHead;

        public ParseServiceForDifferences(ModelItem mergeHead,ModelItem head)
        {
            _mergeHead = mergeHead;
            _head = head;
           
        }


    }
}
