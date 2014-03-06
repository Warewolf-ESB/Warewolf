using System.Collections;


using System.Collections.Generic;


namespace Infragistics.Controls.Charts.Util
{
    /// <summary>
    /// A column of doubles.
    /// </summary>
    public class DoubleColumn
    {


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        /// <summary>
        /// Sets or gets the values of the double column
        /// </summary>
        public IList<double> Values { get; set; }


        /// <summary>
        /// Sets the values of the double column.
        /// </summary>
        /// <param name="values">The values to set.</param>
        public void SetValues(object values)
        {



            Values = values as IList<double>;

        }
    }

    /// <summary>
    /// A column of strings.
    /// </summary>
    public class StringColumn
    {


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        /// <summary>
        /// Gets or sets the values of the string column.
        /// </summary>
        public IList<string> Values { get; set; }


        /// <summary>
        /// Sets the values of the string column.
        /// </summary>
        /// <param name="values">The values to set.</param>
        public void SetValues(object values)
        {



            Values = values as IList<string>;

        }
    }

    /// <summary>
    /// A column of objects.
    /// </summary>
    public class ObjectColumn
    {


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        /// <summary>
        /// The values of the object column.
        /// </summary>
        public IList<object> Values { get; set; }


        /// <summary>
        /// Sets the values of an object column.
        /// </summary>
        /// <param name="values"></param>
        public void SetValues(object values)
        {



            Values = values as IList<object>;

        }
    }

    /// <summary>
    /// A column of integers.
    /// </summary>
    public class IntColumn
        : IEnumerable
    {
        /// <summary>
        /// Instantiates an int column.
        /// </summary>
        public IntColumn()
        {



            Values = new List<int>();

        }

        /// <summary>
        /// Populates the values in the column.
        /// </summary>
        /// <param name="count">The number of values to populate.</param>
        public void Populate(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Values.Add(i);
            }
        }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        /// <summary>
        /// The values of the column.
        /// </summary>
        public List<int> Values { get; set; }


        /// <summary>
        /// Sorts the column by the given comparison method.
        /// </summary>
        /// <param name="comparison">The comparison to use in the sort.</param>
        public void Sort(IntColumnComparison comparison)
        {


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

            Values.Sort((i1, i2) => comparison(i1,i2));

        }

        /// <summary>
        /// Gets an enumarator for the values.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return Values.GetEnumerator();
        }
    }

    /// <summary>
    /// Compares ints in the column.
    /// </summary>
    /// <param name="i1">The first int.</param>
    /// <param name="i2">The second int</param>
    /// <returns>The comparison result.</returns>
    public delegate int IntColumnComparison(int i1, int i2);


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