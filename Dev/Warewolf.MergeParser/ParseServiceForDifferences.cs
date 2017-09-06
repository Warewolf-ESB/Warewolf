using System.Activities.Presentation.Model;

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
