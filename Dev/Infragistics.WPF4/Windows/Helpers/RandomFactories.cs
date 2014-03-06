using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Markup;

namespace Infragistics.Windows.Helpers
{
    /// <summary>
    /// Class used to generate a random SolidColorBrush within an explicit color range
    /// </summary>
    [TypeConverter(typeof(RandomBrushFactory.RandomBrushFactoryTypeConverter))]
    public class RandomBrushFactory : DependencyObject
    {
        #region Private Members

        private Random _rnd;

        #endregion //Private Members	
    
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public RandomBrushFactory()
        {
        }

        #endregion //Constructor	
    
        #region GetBrush

        /// <summary>
        /// Returns a solid color brush within the range of StartColor and EndColor
        /// </summary>
        public SolidColorBrush GetBrush()
        {
            Color start = this.StartColor;
            Color end = this.EndColor;

            if ( this._rnd == null )
                this._rnd = new Random();

            int a = this._rnd.Next(start.A, end.A);
            int r = this._rnd.Next(start.R, end.R);
            int g = this._rnd.Next(start.G, end.G);
            int b = this._rnd.Next(start.B, end.B);

            return new SolidColorBrush(Color.FromArgb((byte)a,(byte)r,(byte)g,(byte)b));
        }

        #endregion //GetBrush

        #region Factory

        /// <summary>
        /// Returns this instance (read-only)
        /// </summary>
        public RandomBrushFactory Factory { get { return this; } }

        #endregion //Factory	
    
        #region EndColor

        /// <summary>
        /// Identifies the 'EndColor' dependency property
        /// </summary>
        public static readonly DependencyProperty EndColorProperty = DependencyProperty.Register("EndColor",
            typeof(Color), typeof(RandomBrushFactory), new FrameworkPropertyMetadata(Color.FromArgb(255, 255, 255, 255)));

        /// <summary>
        /// The color to end with.
        /// </summary>
        //[Description("The color to end with.")]
        //[Category("Behavior")]
        public Color EndColor
        {
            get
            {
                return (Color)this.GetValue(RandomBrushFactory.EndColorProperty);
            }
            set
            {
                this.SetValue(RandomBrushFactory.EndColorProperty, value);
            }
        }

        #endregion //EndColor

        #region StartColor

        /// <summary>
        /// Identifies the 'StartColor' dependency property
        /// </summary>
        public static readonly DependencyProperty StartColorProperty = DependencyProperty.Register("StartColor",
            typeof(Color), typeof(RandomBrushFactory), new FrameworkPropertyMetadata(Color.FromArgb(255, 0, 0, 0)));

        /// <summary>
        /// The color value to start with.
        /// </summary>
        //[Description("The color value to start with.")]
        //[Category("Behavior")]
        public Color StartColor
        {
            get
            {
                return (Color)this.GetValue(RandomBrushFactory.StartColorProperty);
            }
            set
            {
                this.SetValue(RandomBrushFactory.StartColorProperty, value);
            }
        }

        #endregion //StartColor

        #region RandomBrushFactoryTypeConverter private class

        private class RandomBrushFactoryTypeConverter : TypeConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof(Brush))
                    return true;

                return base.CanConvertTo(context, destinationType);
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                RandomBrushFactory factory = value as RandomBrushFactory;

                if (factory != null && destinationType == typeof(Brush))
                    return factory.GetBrush();

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        #endregion //RandomBrushFactoryTypeConverter	
    
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