using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace WPF.JoshSmith.Panels
{
    /// <summary>
    /// A Viewport3D subclass that shields Panel3D from needing to arrange the 
    /// 3D models in a specific order.  For models to be truly transparent, and
    /// have the models 'behind' them be visible, they must exist in the Children
    /// collection in the opposite order of the way you see them on-screen.  The
    /// front model must be the last child, and the back model must be the first
    /// child (not including the scene's light source).
    /// </summary>
    internal class Viewport3DEx : Viewport3D
    {
        /// <summary>
        /// Adds the specified model to the rear of the 3D scene.
        /// </summary>
        /// <param name="model">The rear item in the scene.</param>
        internal void AddToBack(Viewport2DVisual3D model)
        {
            if(AllowTransparency)
                Children.Insert(1, model);
            else
                Children.Add(model);
        }

        /// <summary>
        /// Adds the specified model to the front of the 3D scene.
        /// </summary>
        /// <param name="model">The front item in the scene.</param>
        internal void AddToFront(Viewport2DVisual3D model)
        {
            if(AllowTransparency)
                Children.Add(model);
            else
                Children.Insert(1, model);
        }

        /// <summary>
        /// Gets/sets whether the models in the scene support 
        /// showing the models behind through them.
        /// </summary>
        internal bool AllowTransparency { get; set; }

        /// <summary>
        /// Returns the model at the back of the 3D scene.
        /// </summary>
        internal Viewport2DVisual3D BackModel
        {
            get
            {
                if(Children.Count < 2)
                    return null;

                if(AllowTransparency)
                    return Children[1] as Viewport2DVisual3D;

                return Children[Children.Count - 1] as Viewport2DVisual3D;
            }
        }

        /// <summary>
        /// Returns the model at the specified model index.
        /// </summary>
        internal Viewport2DVisual3D GetModelAt(int modelIndex)
        {
            // Add 1 to account for the scene's light source, which is the first element.
            return Children[modelIndex + 1] as Viewport2DVisual3D;
        }

        /// <summary>
        /// Returns an enumerable object with which the models are enumerated, from front to back.
        /// </summary>
        internal IEnumerable<Viewport2DVisual3D> GetModels()
        {
            if(AllowTransparency)
            {
                for(int i = Children.Count - 1; 0 < i; --i)
                    yield return Children[i] as Viewport2DVisual3D;
            }
            else
            {
                for(int i = 1; i < Children.Count; ++i)
                    yield return Children[i] as Viewport2DVisual3D;
            }
        }

        /// <summary>
        /// Returns the model at the front of the 3D scene.
        /// </summary>
        internal Viewport2DVisual3D FrontModel
        {
            get
            {
                if(Children.Count < 2)
                    return null;

                if(AllowTransparency)
                    return Children[Children.Count - 1] as Viewport2DVisual3D;

                return Children[1] as Viewport2DVisual3D;
            }
        }

        /// <summary>
        /// Returns the index of the specified model where 0 is the front item.
        /// If the model is not in the scene, returns -1.
        /// </summary>
        internal int GetVisualIndex(Viewport2DVisual3D model)
        {
            int modelIndex = Children.IndexOf(model);
            if(modelIndex < 0)
                return -1;

            if(AllowTransparency)
                return ModelCount - modelIndex;

            return modelIndex - 1;
        }

        /// <summary>
        /// Returns the number of Viewport2DVisual3D objects in the Children collection.
        /// </summary>
        internal int ModelCount
        {
            get { return Children.Count - 1; }
        }

        /// <summary>
        /// Removes all of the models from the scene.
        /// </summary>
        internal void RemoveAllModels()
        {
            // Remove all models, except for the light source.
            for(int i = Children.Count - 1; 0 < i; --i)
                Children.RemoveAt(i);
        }

        /// <summary>
        /// Removes and returns the back model in the scene.
        /// </summary>
        internal Viewport2DVisual3D RemoveBackModel()
        {
            var backModel = BackModel;

            if(backModel == null)
                return null;

            Children.Remove(backModel);
            return backModel;
        }

        /// <summary>
        /// Removes and returns the front model in the scene.
        /// </summary>
        internal Viewport2DVisual3D RemoveFrontModel()
        {
            var frontModel = FrontModel;

            if(frontModel == null)
                return null;

            Children.Remove(frontModel);
            return frontModel;
        }
    }
}