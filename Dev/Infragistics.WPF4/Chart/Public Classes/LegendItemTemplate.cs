
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// This class is used to create custom legend item appearance with data templates. 
    /// </summary>
    public class LegendItemTemplate : Infragistics.PropertyChangeNotifier
    {
        #region Fields

        private Brush _fill = new SolidColorBrush(Colors.White);
        private Brush _stroke = new SolidColorBrush(Colors.Black);
        private string _text = "";
        private double _height;
        private double _width;
        private double _strokeThickness;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the Brush that specifies how to paint the interior of the shape. 
        /// </summary>
        //[Description("Gets or sets the Brush that specifies how to paint the interior of the shape. ")]
        public Brush Fill
        {
            get { return _fill; }
            set { _fill = value; this.RaisePropertyChangedEvent("Fill"); }
        }

        /// <summary>
        /// Gets or sets the Brush that specifies how to paint the Shape outline.
        /// </summary>
        //[Description("Gets or sets the Brush that specifies how to paint the Shape outline.")]
        public Brush Stroke
        {
            get { return _stroke; }
            set { _stroke = value; this.RaisePropertyChangedEvent("Stroke"); }
        }

        /// <summary>
        /// Gets or sets the width of the Shape outline. 
        /// </summary>
        //[Description("Gets or sets the width of the Shape outline.")]
        public double StrokeThickness
        {
            get { return _strokeThickness; }
            set { _strokeThickness = value; this.RaisePropertyChangedEvent("StrokeThickness"); }
        }

        /// <summary>
        /// Gets or sets the text for legend item.
        /// </summary>
        //[Description("Gets or sets the text for legend item.")]
        public string Text
        {
            get { return _text; }
            set { _text = value; this.RaisePropertyChangedEvent("Text"); }
        }

        /// <summary>
        /// Gets or sets the legend item Width.
        /// </summary>
        //[Description("Gets or sets the legend item Width.")]
        public double Width
        {
            get { return _width; }
            set { _width = value; this.RaisePropertyChangedEvent("Width"); }
        }

        /// <summary>
        /// Gets or sets the legend item Height.
        /// </summary>
        //[Description("Gets or sets the legend item Height.")]
        public double Height
        {
            get { return _height; }
            set { _height = value; this.RaisePropertyChangedEvent("Height"); }
        }

        #endregion Properties
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