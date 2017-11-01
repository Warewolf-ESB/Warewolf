using System;
using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace Dev2.Common.Interfaces
{
    public interface IMergeToolModel
    {
        ImageSource MergeIcon { get; set; }
        string MergeDescription { get; set; }
        bool IsMergeChecked { get; set; }
        bool IsMergeEnabled { get; set; }
        IMergeToolModel Parent { get; set; }
        ObservableCollection<IMergeToolModel> Children { get; set; }
        Guid UniqueId { get; set; }
        string ParentDescription { get; set; }
        bool HasParent { get; set; }
        event ModelToolChanged SomethingModelToolChanged;
        FlowNode FlowNode { get; set; }
        ModelItem ModelItem { get; set; }
        Point NodeLocation { get; set; }
        bool IsMergeVisible { get; set; }
        IToolConflict Container { get; set; }
        bool IsTrueArm { get; set; }
        string NodeArmDescription { get; set; }
    }
    public delegate void ModelToolChanged(object sender, IMergeToolModel args);
}
