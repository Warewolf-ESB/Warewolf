using System.Windows;
using Infragistics.Controls.Grids;
using Infragistics.Controls.Menus;

namespace Warewolf.Studio.Themes.Luna.XamGrid
{
    public class ContextMenuOpeningEventArgs
    {
        public bool Cancel { get; set; }

        public CellBase Cell { get; set; }

        public XamContextMenu Menu { get; internal set; }

        public Point MouseClickLocation { get; set; }
    }
}
