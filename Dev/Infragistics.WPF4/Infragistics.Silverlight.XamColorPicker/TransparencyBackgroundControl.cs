using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;



using System.Windows.Shapes;


namespace Infragistics.Controls.Editors.Primitives
{
    /// <summary>
    /// For Internal use only.
    /// </summary>
    [TemplatePart(Name = "Root", Type = typeof(Grid))]
    public class TransparencyBackgroundControl : Control
    {
        #region Members

        private Grid _root;
        private Color _color1 = Colors.White;
        private Color _color2 = Color.FromArgb(0xFF, 0xCC, 0xCC, 0xCC);
        private int _squareSize = 8;



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        private readonly VisualBrush _visualBrush = new VisualBrush
        {
            TileMode = TileMode.Tile,
            Viewport = new Rect(0, 0, 16, 16),
            ViewboxUnits = BrushMappingMode.Absolute,
            Viewbox = new Rect(0, 0, 16, 16),
            ViewportUnits = BrushMappingMode.Absolute
        };

        private readonly Canvas _canvas = new Canvas { SnapsToDevicePixels = true, UseLayoutRounding = true};
        private readonly Rectangle _tile1 = new Rectangle { Width = 8, Height = 8, SnapsToDevicePixels = true, UseLayoutRounding = true };
        private readonly Rectangle _tile2 = new Rectangle { Width = 8, Height = 8, SnapsToDevicePixels = true, UseLayoutRounding = true };


        #endregion // Members

        #region Constructor

        static TransparencyBackgroundControl()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TransparencyBackgroundControl), new FrameworkPropertyMetadata(typeof(TransparencyBackgroundControl)));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="TransparencyBackgroundControl"/> class.
        /// </summary>
        public TransparencyBackgroundControl()
        {
            this.IsTabStop = false;





            _canvas = new Canvas();
            Canvas.SetLeft(_tile2, 8);
            Canvas.SetTop(_tile2, 8);
            _canvas.Children.Add(_tile1);
            _canvas.Children.Add(_tile2);
            _visualBrush.Visual = _canvas;
            this.Focusable = false;
            this.SnapsToDevicePixels = true;
            this.UseLayoutRounding = true;

        }

        #endregion // Constructor

        #region Overrides

        #region OnApplyTemplate

        /// <summary>
        /// Invoked whenever application code or internal processes (such as a rebuilding layout pass) call ApplyTemplate.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (this._root != null)
            {

                this._root.SizeChanged -= RootSizeChanged;



            }

            base.OnApplyTemplate();

            this._root = this.GetTemplateChild("Root") as Grid;

            if (this._root != null)
            {







                this._root.Background = this._visualBrush;


                this._root.SizeChanged += RootSizeChanged;
            }
        }

        #endregion // OnApplyTemplate

        #region MeasureOverride

        /// <summary>
        /// Provides the behavior for the Measure pass of the layout. Classes can override this method to define their own Measure pass behavior.
        /// </summary>
        /// <param name="availableSize">The available size that this object can give to child objects. Infinity (<see cref="F:System.Double.PositiveInfinity"/>) can be specified as a value to indicate that the object will size to whatever content is available.</param>
        /// <returns>
        /// The size that this object determines it needs during layout, based on its calculations of the allocated sizes for child objects; or based on other considerations, such as a fixed container size.
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (this._root != null)
            {
                this._root.Measure(availableSize);
            }

            return new Size(0, 0);
        }

        #endregion // MeasureOverride

        #endregion // Overrides

        #region Properties

        #region Color1

        /// <summary>
        /// Primary color
        /// </summary>
        public Color Color1
        {
            get { return _color1; }
            set
            {
                _color1 = value;



                Invalidate();
            }
        }

        #endregion // Color1

        #region Color2

        /// <summary>
        /// Secondary color
        /// </summary>
        public Color Color2
        {
            get { return _color2; }
            set
            {
                _color2 = value;




                Invalidate();
            }
        }

        #endregion // Color2

        #region SquareSize

        /// <summary>
        /// Gets or sets the size of the square.
        /// </summary>
        /// <value>
        /// The size of the square.
        /// </value>
        public int SquareSize
        {
            get
            {
                return _squareSize;
            }

            set
            {
                if (_squareSize != value)
                {
                    if (value <= 0)
                    {
                        throw new ArgumentException(SR.GetString("SquareSizeOutOfRange"));
                    }

                    _squareSize = value;
                    Invalidate();
                }
            }
        }

        #endregion // SquareSize

        #endregion // Properties

        #region Methods

        #region ColorToArgb32Int







        #endregion // ColorToArgb32Int

        #region Invalidate

        private void Invalidate()
        {


#region Infragistics Source Cleanup (Region)




























#endregion // Infragistics Source Cleanup (Region)

            this._canvas.Background = new SolidColorBrush(_color1);
            this._tile1.Fill = new SolidColorBrush(_color2);
            this._tile2.Fill = new SolidColorBrush(_color2);
            this._tile1.Width = this._tile1.Height = this._tile2.Width = this._tile2.Height = this._squareSize;
            Canvas.SetLeft(_tile2, _squareSize);
            Canvas.SetTop(_tile2, _squareSize);
            _visualBrush.Viewport = _visualBrush.Viewport = new Rect(0, 0, 2 * _squareSize, 2 * _squareSize);


        }

        #endregion // Invalidate

        #endregion // Methods

        #region EventHandlers

        #region RootSizeChanged

        private void RootSizeChanged(object sender, SizeChangedEventArgs e)
        {




            _canvas.Width = (int)Math.Ceiling(e.NewSize.Width);
            _canvas.Height = (int)Math.Ceiling(e.NewSize.Height);

            this.Invalidate();
        }

        #endregion // RootSizeChanged

        #endregion // EventHandlers
    }
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