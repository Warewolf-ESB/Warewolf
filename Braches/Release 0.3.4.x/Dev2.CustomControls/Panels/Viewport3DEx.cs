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
            if (this.AllowTransparency)
                base.Children.Insert(1, model);                
            else
                base.Children.Add(model);
        }

        /// <summary>
        /// Adds the specified model to the front of the 3D scene.
        /// </summary>
        /// <param name="model">The front item in the scene.</param>
        internal void AddToFront(Viewport2DVisual3D model)
        {
            if (this.AllowTransparency)
                base.Children.Add(model);
            else
                base.Children.Insert(1, model);
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
                if (base.Children.Count < 2)
                    return null;

                if (this.AllowTransparency)
                    return base.Children[1] as Viewport2DVisual3D;
                else
                    return base.Children[base.Children.Count - 1] as Viewport2DVisual3D;
            }
        }

        /// <summary>
        /// Returns the model at the specified model index.
        /// </summary>
        internal Viewport2DVisual3D GetModelAt(int modelIndex)
        {
            // Add 1 to account for the scene's light source, which is the first element.
            return base.Children[modelIndex + 1] as Viewport2DVisual3D;
        }

        /// <summary>
        /// Returns an enumerable object with which the models are enumerated, from front to back.
        /// </summary>
        internal IEnumerable<Viewport2DVisual3D> GetModels()
        {
            if (this.AllowTransparency)
            {
                for (int i = base.Children.Count - 1; 0 < i; --i)
                    yield return base.Children[i] as Viewport2DVisual3D;
            }
            else 
            {
                for (int i = 1; i < base.Children.Count; ++i)
                    yield return base.Children[i] as Viewport2DVisual3D;
            }
        }

        /// <summary>
        /// Returns the model at the front of the 3D scene.
        /// </summary>
        internal Viewport2DVisual3D FrontModel
        {
            get
            {
                if (base.Children.Count < 2)
                    return null;

                if (this.AllowTransparency)
                    return base.Children[base.Children.Count - 1] as Viewport2DVisual3D;
                else
                    return base.Children[1] as Viewport2DVisual3D;
            }
        }

        /// <summary>
        /// Returns the index of the specified model where 0 is the front item.
        /// If the model is not in the scene, returns -1.
        /// </summary>
        internal int GetVisualIndex(Viewport2DVisual3D model)
        {
            int modelIndex = base.Children.IndexOf(model);
            if (modelIndex < 0)
                return -1;            

            if (this.AllowTransparency)
                return this.ModelCount - modelIndex;
            else
                return modelIndex - 1;
        }

        /// <summary>
        /// Returns the number of Viewport2DVisual3D objects in the Children collection.
        /// </summary>
        internal int ModelCount
        {
            get { return base.Children.Count - 1; }
        }

        /// <summary>
        /// Removes all of the models from the scene.
        /// </summary>
        internal void RemoveAllModels()
        {
            // Remove all models, except for the light source.
            for (int i = base.Children.Count - 1; 0 < i; --i)
                base.Children.RemoveAt(i);
        }

        /// <summary>
        /// Removes and returns the back model in the scene.
        /// </summary>
        internal Viewport2DVisual3D RemoveBackModel()
        {
            var backModel = this.BackModel;

            if (backModel == null)
                return null;

            base.Children.Remove(backModel);
            return backModel;
        }

        /// <summary>
        /// Removes and returns the front model in the scene.
        /// </summary>
        internal Viewport2DVisual3D RemoveFrontModel()
        {
            var frontModel = this.FrontModel;

            if (frontModel == null)
                return null;

            base.Children.Remove(frontModel);
            return frontModel;
        }
    }
}