using System.ComponentModel;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System;
using System.Globalization;
using System.Collections.Specialized;

namespace Infragistics
{
    /// <summary>
    /// Simple collection of Brush objects
    /// </summary>

    [TypeConverter(typeof(SolidBrushCollectionConverter))]

    public class BrushCollection : ObservableCollection<Brush>
    {

        protected override void InsertItem(int index, Brush item)
        {
            if (item.CanFreeze)
            {
                item.Freeze();
            }
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, Brush item)
        {
            if (item.CanFreeze)
            {
                item.Freeze();
            }
            base.SetItem(index, item);
        }


        private static Random random = new Random();

        /// <summary>
        /// Returns a random brush in the collection.
        /// </summary>
        /// <returns>A random brush in this collection.</returns>
        public Brush SelectRandom()
        {
            return this[random.Next(Count)];
        }
        /// <summary>
        /// Returns a random brush interpolated from the brushes in this collection.
        /// </summary>
        /// <returns>A random brush interpolated from the brushes in this collection.</returns>
        public Brush InterpolateRandom()
        {
            return GetInterpolatedBrush(random.NextDouble() * (Count - 1));
        }


        /// <summary>
        /// Sets or gets the interpolation mode used to interpolate brushes.
        /// </summary>
        public InterpolationMode InterpolationMode
        {
            get { return interpolationMode; }
            set
            {
                if (interpolationMode != value)
                {
                    interpolationMode = value;

                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }
        }
        private InterpolationMode interpolationMode = InterpolationMode.RGB;


        /// <summary>
        /// Gets a brush at the specified real-precision index. For non-integer
        /// indices, this requires interpolation.
        /// </summary>
        /// <param name="index">real-precision index</param>
        /// <returns>Brush for specified index(may be interpolated)</returns>
        public Brush this[double index]
        {
            get
            {
                return GetInterpolatedBrush(index);
            }
        }


        /// <summary>
        /// Gets or sets a brush at the specified index.
        /// </summary>
        /// <param name="index">The index of the brush to return.</param>
        /// <returns>A brush for the specified index.</returns>
        new public Brush this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    return null;
                }

                return base[index];
            }

            set
            {
                base[index] = value;
            }
        }
        /// <summary>
        /// Gets the brush at the specified index.  If index is not an integer, the result of this function call will be a brush interpolated between the brushes at the previous and following index values.
        /// </summary>
        /// <param name="index">The index of the brush to return.</param>
        /// <returns>The brush at the specified index, or if the specified index is not an integer, an interpolated brush between the previous and next index.</returns>
        public Brush GetInterpolatedBrush(double index)
        {
            if (Double.IsNaN(index))
            {
                return null;
            }

            index = MathUtil.Clamp(index, 0.0, Count - 1.0);

            int i = (int)System.Math.Floor(index);

            if (i == index)
            {
                return this[i];
            }




            return this[i].GetInterpolation(index - i, this[i + 1], InterpolationMode);

        }



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

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