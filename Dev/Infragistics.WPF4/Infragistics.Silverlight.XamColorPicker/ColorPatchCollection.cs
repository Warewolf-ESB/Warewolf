using System.Windows.Media;
using Infragistics.Collections;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// A collection of <see cref="ColorPatch"/> objects.
    /// </summary>
    public class ColorPatchCollection : CollectionBase<ColorPatch>
    {
        #region Properties

        #region MaximumEntries

        /// <summary>
        /// Gets / sets the maximum number of elements that this collection expects.
        /// </summary>
        protected internal int? MaximumEntries { get; set; }

        #endregion // MaximumEntries

        #endregion // Properties

        #region Methods

        #region Contains

        /// <summary>
        /// Returns true if the inputted color is represented by a <see cref="ColorPatch"/> in this collection.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool Contains(Color c)
        {
            foreach (ColorPatch cp in this.Items)
            {
                if (cp.Color == c)
                    return true;
            }

            return false;
        }

        #endregion // Contains

        #region Add

        /// <summary>
        /// Adds a <see cref="ColorPatch"/> to the collection based on the inputted color.
        /// </summary>
        /// <param name="c"></param>
        public void Add(Color c)
        {
            this.Add(new ColorPatch(c));
        }

        #endregion // Add

        #endregion // Methods
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