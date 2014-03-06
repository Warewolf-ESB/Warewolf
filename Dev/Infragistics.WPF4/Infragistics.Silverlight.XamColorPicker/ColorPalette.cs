using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// An object which unifies a group of colors together.
    /// </summary>
    public class ColorPalette : DependencyObjectNotifier
    {
        #region Members

        private ColorPatchCollection _colors;

        #endregion // Members

        #region Properties

        #region Colors
        /// <summary>
        /// Gets / sets the <see cref="ColorPatchCollection"/> which contains the colors which will be shown by this palette.
        /// </summary>
        public ColorPatchCollection Colors
        {
            get
            {
                if (this._colors == null)
                {
                    this._colors = new ColorPatchCollection();
                    this._colors.CollectionChanged += Colors_CollectionChanged;
                }
                return this._colors;
            }
            set
            {
                if (this._colors != value)
                {
                    if (this._colors != null)
                    {
                        this._colors.CollectionChanged -= Colors_CollectionChanged;
                    }
                    this._colors = value;
                    if (this._colors != null)
                    {
                        this._colors.CollectionChanged += Colors_CollectionChanged;                        
                    }
                    this.OnPropertyChanged("Colors");
                }                
            }
        }
        #endregion // Colors

        #region MaximumColorCount

        /// <summary>
        /// Identifies the <see cref="MaximumColorCount"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MaximumColorCountProperty = DependencyProperty.Register("MaximumColorCount", typeof(int?), typeof(ColorPalette), new PropertyMetadata(new PropertyChangedCallback(MaximumColorCountChanged)));

        /// <summary>
        /// Gets / sets the maximum number of colors which will be allowed in the palette.  This is set to null if there is to be no limit.
        /// </summary>
        protected internal int? MaximumColorCount
        {
            get { return (int?)this.GetValue(MaximumColorCountProperty); }
            set { this.SetValue(MaximumColorCountProperty, value); }
        }

        private static void MaximumColorCountChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColorPalette cp = (ColorPalette)obj;
            cp.OnPropertyChanged("MaximumColorCount");
        }

        #endregion // MaximumColorCount

        #region DefaultColor

        /// <summary>
        /// Identifies the <see cref="DefaultColor"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty DefaultColorProperty = DependencyProperty.Register("DefaultColor", typeof(Color), typeof(ColorPalette), new PropertyMetadata(new PropertyChangedCallback(DefaultColorChanged)));

        /// <summary>
        /// Gets / sets the <see cref="Color"/> which will be used to fill for items which are not explictly set.
        /// </summary>
        public Color DefaultColor
        {
            get { return (Color)this.GetValue(DefaultColorProperty); }
            set { this.SetValue(DefaultColorProperty, value); }
        }

        private static void DefaultColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColorPalette cp = (ColorPalette)obj;
            cp.OnPropertyChanged("DefaultColor");
        }

        #endregion // DefaultColor

        #endregion // Properties

        #region Methods
        #endregion // Methods

        #region Overrides
        #endregion // Overrides

        #region Event Handlers

        void Colors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (this.MaximumColorCount != null)
            {
                int maxSize = (int)this.MaximumColorCount;

                if (maxSize < this.Colors.Count)
                {
                    this._colors.CollectionChanged -= Colors_CollectionChanged;
                    while (maxSize < this.Colors.Count)
                    {
                        this.Colors.RemoveAt(0);
                    }
                    this._colors.CollectionChanged += Colors_CollectionChanged;
                }

            }
            this.OnPropertyChanged("ColorPalette");
        }

        #endregion // Event Handlers
    }

    #region ColorPaletteCollection

    /// <summary>
    /// A collection of <see cref="ColorPalette"/> objects.
    /// </summary>
    public class ColorPaletteCollection : ObservableCollection<ColorPalette>
    {

    }

    #endregion // ColorPaletteCollection

}
#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved