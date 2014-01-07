using Infragistics.Windows.DockManager;
using System;

namespace Dev2.Studio.Core.AppResources.ExtensionMethods
{
    public static class PaneLocationEnum
    {
        public static InitialPaneLocation ToInitialPaneLocation(this PaneLocation location)
        {
            InitialPaneLocation initialPaneLocation;

            switch(location)
            {
                case PaneLocation.DockedLeft:
                case PaneLocation.DockedRight:
                case PaneLocation.DockedTop:
                case PaneLocation.DockedBottom:
                case PaneLocation.FloatingOnly:

                    initialPaneLocation = (InitialPaneLocation)Enum.Parse(typeof(InitialPaneLocation), location.ToString());

                    break;
                case PaneLocation.Floating: //DockableFloating
                    initialPaneLocation = (InitialPaneLocation)Enum.Parse(typeof(InitialPaneLocation), "DockableFloating");
                    break;

                case PaneLocation.Unpinned: //Not
                case PaneLocation.Document: //Not
                case PaneLocation.Unknown: //Not
                    throw new InvalidOperationException("Can not convert PaneLocation to InitialPaneLocation");

                default:
                    throw new ArgumentOutOfRangeException("location");
            }

            return initialPaneLocation;
        }

        public static PaneLocation ToPaneLocation(this InitialPaneLocation location)
        {
            PaneLocation paneLocation;

            switch(location)
            {
                case InitialPaneLocation.DockedLeft:
                case InitialPaneLocation.DockedTop:
                case InitialPaneLocation.DockedRight:
                case InitialPaneLocation.DockedBottom:
                case InitialPaneLocation.FloatingOnly:

                    paneLocation = (PaneLocation)Enum.Parse(typeof(PaneLocation), location.ToString());

                    break;
                case InitialPaneLocation.DockableFloating:

                    paneLocation = (PaneLocation)Enum.Parse(typeof(PaneLocation), "Floating");

                    break;
                default:
                    throw new ArgumentOutOfRangeException("location");
            }

            return paneLocation;
        }
    }
}
