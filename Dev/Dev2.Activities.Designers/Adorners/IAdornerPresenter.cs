using System.Windows.Controls.Primitives;
using Dev2.Activities.Designers;

namespace Dev2.Activities.Adorners
{
    /// <summary>
    /// Interface used to identify adorner presenters added to an activity designers adorner collection
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/07/23</date>
    public interface IAdornerPresenter
    {
        string ImageSourceUri { get; set; }
        ButtonBase Button { get; }
        OverlayType OverlayType { get; set; }
        object Content { get; }
        ActivityDesignerBase AssociatedActivityDesigner { get; set; }
    }
}
