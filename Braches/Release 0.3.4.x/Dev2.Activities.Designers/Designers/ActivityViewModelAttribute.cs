using System;
using System.ComponentModel.Composition;

namespace Dev2.Activities.Designers
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ActivityViewModelAttribute : ExportAttribute
    {
        public ActivityViewModelAttribute() : base(typeof(ActivityViewModelBase)) { }

        public Type ActivityType { get; set; }
    }

    public interface IActivityViewModelMetadata
    {
        Type ActivityType { get; }
    }
}
