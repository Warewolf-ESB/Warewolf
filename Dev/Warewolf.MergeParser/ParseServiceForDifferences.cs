using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Activities.Presentation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Studio.Core.Activities.Utils;

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

        public List<ModelItem> MergeHeadNodes { get; private set; }
        public List<ModelItem> HeadNodes { get; private set; }

        public List<(Guid uniqueId,ModelItem activity, bool conflict)> GetDifferences()
        {
            var conflictList = new List<(Guid uniqueId, ModelItem activity, bool conflict)>();
            using (var editingContext = new System.Activities.Presentation.EditingContext())
            {
                var modelService = editingContext.Services.GetService<ModelService>();
                MergeHeadNodes = modelService.Find(_mergeHead, typeof(FlowNode)).ToList();
                HeadNodes = modelService.Find(_head, typeof(FlowNode)).ToList();

                var equalItems = MergeHeadNodes.Intersect(HeadNodes);
                var nodesDifferentInMergeHead = MergeHeadNodes.Except(HeadNodes);
                var nodesDifferentInHead = HeadNodes.Except(MergeHeadNodes);

                var allDifferences = nodesDifferentInMergeHead.Union(nodesDifferentInHead);

                foreach (var item in equalItems)
                {
                    var equalItem = (item.GetProperty<Guid>("UniqueID"), item, false);
                    conflictList.Add(equalItem);
                }

                foreach (var item in allDifferences)
                {
                    var diffItem = (item.GetProperty<Guid>("UniqueID"), item, true);
                    conflictList.Add(diffItem);
                }

            }
            return conflictList;
            
        }
    }
}
