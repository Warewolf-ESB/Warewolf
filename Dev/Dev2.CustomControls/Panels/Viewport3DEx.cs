/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace WPF.JoshSmith.Panels
{
    /// <summary>
    ///     A Viewport3D subclass that shields Panel3D from needing to arrange the
    ///     3D models in a specific order.  For models to be truly transparent, and
    ///     have the models 'behind' them be visible, they must exist in the Children
    ///     collection in the opposite order of the way you see them on-screen.  The
    ///     front model must be the last child, and the back model must be the first
    ///     child (not including the scene's light source).
    /// </summary>
    internal class Viewport3DEx : Viewport3D
    {
        /// <summary>
        ///     Gets/sets whether the models in the scene support
        ///     showing the models behind through them.
        /// </summary>
        internal bool AllowTransparency { get; set; }

        /// <summary>
        ///     Returns the model at the back of the 3D scene.
        /// </summary>
        internal Viewport2DVisual3D BackModel
        {
            get
            {
                if (Children.Count < 2)
                    return null;

                if (AllowTransparency)
                    return Children[1] as Viewport2DVisual3D;

                return Children[Children.Count - 1] as Viewport2DVisual3D;
            }
        }

        /// <summary>
        ///     Returns the model at the front of the 3D scene.
        /// </summary>
        internal Viewport2DVisual3D FrontModel
        {
            get
            {
                if (Children.Count < 2)
                    return null;

                if (AllowTransparency)
                    return Children[Children.Count - 1] as Viewport2DVisual3D;

                return Children[1] as Viewport2DVisual3D;
            }
        }

        /// <summary>
        ///     Returns the number of Viewport2DVisual3D objects in the Children collection.
        /// </summary>
        internal int ModelCount => Children.Count - 1;
    }
}