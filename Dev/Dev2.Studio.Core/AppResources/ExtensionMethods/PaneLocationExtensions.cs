
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Infragistics.Windows.DockManager;

// ReSharper disable once CheckNamespace
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
    }
}
