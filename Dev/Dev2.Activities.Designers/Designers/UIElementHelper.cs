using System;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dev2.Activities.Designers
{
    public interface IUIElementProvider
    {
        Border GetColoursBorder(ActivityDesigner designer);
        Rectangle GetDisplayNameWidthSetter(ActivityDesigner designer);
        TextBox GetTitleTextBox(ActivityDesigner designer);
    }

    public class UIElementProvider : IUIElementProvider
    {
        public Border GetColoursBorder(ActivityDesigner designer)
        {
            var hostGrid = GetHostGrid(designer);
            if(hostGrid == null)
            {
                return null;
            }

            try
            {
                for(var i = 0; i < VisualTreeHelper.GetChildrenCount(hostGrid); i++)
                {
                    var child = VisualTreeHelper.GetChild(hostGrid, i);

                    var childBorder = child as Border;
                    if(childBorder != null && childBorder.Name != "debuggerVisuals")
                    {
                        return childBorder;
                    }
                }

                return null;
            }
            catch
            {
                // avoid bubbling to studio ;)
                return null;
            }
        }

        public Rectangle GetDisplayNameWidthSetter(ActivityDesigner designer)
        {
            var hostGrid = GetHostGrid(designer);

            if(hostGrid == null)
            {
                return null;
            }

            for(var i = 0; i < VisualTreeHelper.GetChildrenCount(hostGrid); i++)
            {
                var child = VisualTreeHelper.GetChild(hostGrid, i);

                var childRectangle = child as Rectangle;
                if(childRectangle != null && childRectangle.Name == "displayNameWidthSetter")
                {
                    return childRectangle;
                }
            }

            return null;
        }

        public TextBox GetTitleTextBox(ActivityDesigner designer)
        {
            var hostGrid = GetHostGrid(designer);

            if(hostGrid == null)
            {
                return null;
            }

            for(var i = 0; i < VisualTreeHelper.GetChildrenCount(hostGrid); i++)
            {
                var child = VisualTreeHelper.GetChild(hostGrid, i);

                var childRectangle = child as TextBox;
                if(childRectangle != null && childRectangle.Name.Contains("DisplayNameBox"))
                {
                    return childRectangle;
                }
            }

            return null;
        }

        Grid GetHostGrid(ActivityDesigner designer)
        {
            if(VisualTreeHelper.GetChildrenCount(designer) > 0)
            {
            try
            {
                var firstBorder = VisualTreeHelper.GetChild(designer, 0) as Border;

                    if(firstBorder == null)
                    {
                        return null;
                    }

                    var decorator = firstBorder.Child as AdornerDecorator;

                    if(decorator == null)
                {
                    return null;
                }

                    return decorator.Child as Grid;
            }
            catch
            {
                // stop the studio from crashing and restarting ;)
                return null;
            }



            }
            return null;
        }
    }
}
