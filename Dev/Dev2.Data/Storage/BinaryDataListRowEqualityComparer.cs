using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Dev2.Data.Storage
{
    public class BinaryDataListRowEqualityComparer : IEqualityComparer<IndexBasedBinaryDataListRow>
    {
        readonly List<int> _compareCols;

        #region Implementation of IEqualityComparer<in IndexBasedBinaryDataListRow>

        public BinaryDataListRowEqualityComparer(List<int> compareCols)
        {
            if(compareCols == null)
            {
                throw new ArgumentNullException("compareCols");
            }
            _compareCols = compareCols;
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type <paramref>
        ///                                              <name>T</name>
        ///                                          </paramref>
        ///     to compare.</param>
        /// <param name="y">The second object of type <paramref>
        ///                                               <name>T</name>
        ///                                           </paramref>
        ///     to compare.</param>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        public bool Equals(IndexBasedBinaryDataListRow x, IndexBasedBinaryDataListRow y)
        {
            var columnMatches = (from compareCol in _compareCols
                                 let xValue = x.Row.FetchValue(compareCol, -1)
                                 let yValue = y.Row.FetchValue(compareCol, -1)
                                 let correctTypingForComparison = GetCorrectTypingForComparison(xValue, yValue)
                                 let correctXValue = Convert.ChangeType(xValue, correctTypingForComparison)
                                 let correctYValue = Convert.ChangeType(yValue, correctTypingForComparison)
                                 select correctXValue.Equals(correctYValue)).ToList();
            return columnMatches.TrueForAll(b => b);
        }

        Type GetCorrectTypingForComparison(string xValue, string yValue)
        {
            int xIntValue;
            int yIntValue;
            if(int.TryParse(xValue, out xIntValue) && int.TryParse(yValue, out yIntValue))
            {
                return typeof(int);
            }
            long xLongValue;
            long yLongValue;
            if(long.TryParse(xValue, out xLongValue) && long.TryParse(yValue, out yLongValue))
            {
                return typeof(long);
            }
            float xFloatValue;
            float yFloatValue;
            if(float.TryParse(xValue, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out xFloatValue) && float.TryParse(yValue, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out yFloatValue))
            {
                return typeof(float);
            }

            DateTime yDateTimeValue;
            DateTime xDateTimeValue;
            if(DateTime.TryParse(xValue, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out xDateTimeValue) && DateTime.TryParse(yValue, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out yDateTimeValue))
            {
                return typeof(DateTime);
            }
            return typeof(string);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param><exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
        public int GetHashCode(IndexBasedBinaryDataListRow obj)
        {
            return 0; //Always return 0 so that the equals logic is executed
        }

        #endregion
    }
}