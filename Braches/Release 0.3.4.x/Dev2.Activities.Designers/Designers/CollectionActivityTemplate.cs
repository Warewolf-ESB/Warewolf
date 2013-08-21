using System.Windows.Controls;

namespace Dev2.Activities.Designers
{
    public abstract class CollectionActivityTemplate : ActivityTemplate
    {
        public virtual ItemsControl ItemsControl
        {
            get
            {
                return new DataGridFocusTextOnLoadBehavior().GetVisualChild<ItemsControl>(this);
            }
        }
    }
}
