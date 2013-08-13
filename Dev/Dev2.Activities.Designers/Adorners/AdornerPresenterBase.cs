using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using Dev2.Activities.Designers;

namespace Dev2.Activities.Adorners
{
    /// <summary>
    /// Abstract base clas sused to wrap adorners. The activity designer base unwraps these adorner wrappers and 
    /// place the buttons in the tree as well as manages the content.
    /// The Content attribute indicates that default xaml content will be added to the Content Property.
    /// Inherited from contentcontrol 
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/07/23</date>
    [ContentProperty("Content")]
    public abstract class AdornerPresenterBase : ContentControl, IAdornerPresenter
    {
        #region public properties

        /// <summary>
        /// Gets or sets the type of the overlay.
        /// </summary>
        /// <value>
        /// The type of the overlay.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/23</date>
        public virtual OverlayType OverlayType { get; set; }

        /// <summary>
        /// Gets or sets the associated activity designer.
        /// </summary>
        /// <value>
        /// The associated activity designer.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/25</date>
        public ActivityDesignerBase AssociatedActivityDesigner { get; set; }

        #endregion

        #region dependency properties

        /// <summary>
        /// Gets or sets the image source used for the adorner button
        /// </summary>
        /// <value>
        /// The image source URI.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/23</date>
        public virtual string ImageSourceUri
        {
            get { return (string)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSourceUri", typeof(string), 
            typeof(AdornerPresenterBase), new PropertyMetadata(null));

        #endregion

        #region abstract properties

        /// <summary>
        /// The button which gets added to the options adorner
        /// </summary>
        /// <value>
        /// The button.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/23</date>
        public abstract ButtonBase Button { get; }

        #endregion        
    }
}
