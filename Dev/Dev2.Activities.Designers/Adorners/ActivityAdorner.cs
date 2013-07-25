using System.Windows;
using System.Windows.Documents;

namespace Dev2.Activities.Adorners
{
    /// <summary>
    /// Abstract base class used for shared adorner functionality
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/07/23</date>
    public abstract class ActivityAdorner : Adorner
    {
 
        #region Constructors

        /// <summary>
        /// Intializes a new instance of the <see cref="ActivityAdorner"/>.
        /// </summary>
        /// <param name="adornedElement">
        /// The user interface <see cref="UIElement"/> to render on.
        /// </param>
        protected  ActivityAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
        }

        #endregion Constructors


        /// <summary>
        /// Hides the content of the adorner.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/23</date>
        public abstract void HideContent();

        /// <summary>
        /// Shows the content of the adorner.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/23</date>
        public abstract void ShowContent();

    }
}
