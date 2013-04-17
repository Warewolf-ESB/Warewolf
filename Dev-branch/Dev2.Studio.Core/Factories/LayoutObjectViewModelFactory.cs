using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Framework;

namespace Dev2.Studio.Core.Factories 
{
    public static class LayoutObjectViewModelFactory 
    {
        internal static ILayoutObjectViewModel CreateLayoutObject()
        {
            ILayoutObjectViewModel newLayoutObjectViewModel = new LayoutObjectViewModel();

            return newLayoutObjectViewModel;
        }

        public static ILayoutObjectViewModel CreateLayoutObject(int col, int row)
        {
            ILayoutObjectViewModel newLayoutObject = CreateLayoutObject();
            newLayoutObject.GridColumn = col;
            newLayoutObject.GridRow = row;
            return newLayoutObject;
        }

        public static ILayoutObjectViewModel CreateLayoutObject(ILayoutGridViewModel grid, int col, int row)
        {
            ILayoutObjectViewModel newLayoutObject = CreateLayoutObject();
            newLayoutObject.SetGrid(grid);
            newLayoutObject.GridColumn = col;
            newLayoutObject.GridRow = row;
            return newLayoutObject;
        }

        public static ILayoutObjectViewModel CreateLayoutObject(ILayoutGridViewModel grid)
        {
            ILayoutObjectViewModel newLayoutObject = CreateLayoutObject();
            newLayoutObject.SetGrid(grid);
            return newLayoutObject;
        }

        public static ILayoutObjectViewModel CreateLayoutObject(string webPartServiceName, string webPartDisplayName, string iconPath, string xmlConfiguration)
        {
            ILayoutObjectViewModel newLayoutObject = CreateLayoutObject();
            newLayoutObject.WebpartServiceName = webPartServiceName;
            newLayoutObject.WebpartServiceDisplayName = webPartDisplayName;            
            newLayoutObject.IconPath = iconPath;
            newLayoutObject.XmlConfiguration = xmlConfiguration;

            return newLayoutObject;
        }

    }
}
