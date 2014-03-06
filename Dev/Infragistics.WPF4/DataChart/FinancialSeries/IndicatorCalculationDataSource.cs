using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the data contract between a financial series and the 
    /// decoupled calculation responsibilities which implement its mathmatical logic.
    /// </summary>
    /// <remarks>
    /// The FinancialCalculationDataSource contract represents the subset
    /// of data which needs to be provided in order to test or run a calculation,
    /// and its intentionally a subset of the information available to the series, 
    /// in order to make calculation strategies more easily testable and usable in isolation to
    /// the financial series container.
    /// </remarks>
    public class FinancialCalculationDataSource
    {
        /// <summary>
        /// A read only collection of the open values for the financial series
        /// that is requesting an indicator calculation.
        /// </summary>
        public IList<double> OpenColumn { get; set; }

        /// <summary>
        /// A read only collection of the close values for the financial series
        /// that is requesting an indicator calculation.
        /// </summary>
        public IList<double> CloseColumn { get; set; }

        /// <summary>
        /// A read only collection of the high values for the financial series
        /// that is requesting an indicator calculation
        /// </summary>
        public IList<double> HighColumn { get; set; }

        /// <summary>
        /// A read only collection of the low values for that financial series
        /// that is requesting an indicator calculation.
        /// </summary>
        public IList<double> LowColumn { get; set; }

        /// <summary>
        /// A read only collection of the volume values for the financial series
        /// that is requesting an indicator calculation.
        /// </summary>
        public IList<double> VolumeColumn { get; set; }

        /// <summary>
        /// The output values for the indicator which the strategy calculates.
        /// </summary>
        public IList<double> IndicatorColumn { get; set; }

        /// <summary>
        /// An enumerable list of typical prices provided by the series to use
        /// in calculations.
        /// </summary>
        public CalculatedColumn TypicalColumn { get; set; }

        /// <summary>
        /// And enumerable list of true range values provided by the series to use
        /// in calculations.
        /// </summary>
        public CalculatedColumn TrueRange { get; set; }

        /// <summary>
        /// An enumerable list of true low values provided by the series to use
        /// in calculations.
        /// </summary>
        public CalculatedColumn TrueLow { get; set; }

        /// <summary>
        /// The period to use when calculating, if applicable.
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// The short period to use when calculating, if applicable.
        /// </summary>
        public int ShortPeriod { get; set; }

        /// <summary>
        /// The long period to use when calculating, if applicable.
        /// </summary>
        public int LongPeriod { get; set; }

        /// <summary>
        /// The count of the values in the series.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The starting index from which to calculate
        /// </summary>
        public int CalculateFrom { get; set; }

        /// <summary>
        /// The number of items from the starting index from which to calculate
        /// </summary>
        public int CalculateCount { get; set; }

        /// <summary>
        /// If the calculation supports some sort of scaling factor, 
        /// this value will be used.
        /// </summary>
        public double Multiplier { get; set; }

        /// <summary>
        /// If the calculation determines the range of indicator values,
        /// it will set the minimum and maximux properties.
        /// </summary>
        /// <remarks>
        /// This will contain the previous minimum value when the indicator
        /// calculation is called again, in case this makes the update of the
        /// value speedier.
        /// </remarks>
        public double MinimumValue { get; set; }

        /// <summary>
        /// If the calculation determines the range of indicator values,
        /// it will set the minimum and maximux properties.
        /// </summary>
        /// <remarks>
        /// This will contain the previous minimum value when the indicator
        /// calculation is called again, in case this makes the update of the
        /// value speedier.
        /// </remarks>
        public double MaximumValue { get; set; }

        /// <summary>
        /// The calculation strategy should set this to true if it 
        /// specifes the minimum and maximum value properties.
        /// </summary>
        public bool SpecifiesRange { get; set; }
    }

    /// <summary>
    /// Represents a contract between the financial series and the calculation strategies
    /// detailing the supporting calculation strategies that the financial series will 
    /// provide in order for the indicator calculations to be performed.
    /// </summary>
    public class FinancialCalculationSupportingCalculations 
    {
        //supporting calculation strategies
        
        /// <summary>
        /// The strategy provided to calculate an exponential moving average for a collection.
        /// </summary>
        public ColumnSupportingCalculation EMA { get; set; }

        /// <summary>
        /// The strategy provided to calculate a simple moving average for a collection.
        /// </summary>
        public ColumnSupportingCalculation SMA { get; set; }

        /// <summary>
        /// The strategy provided to calculate a standard deviation for a collection.
        /// </summary>
        public ColumnSupportingCalculation STDEV { get; set; }

        /// <summary>
        /// The strategy provided to calculate a moving sum for a collection.
        /// </summary>
        public ColumnSupportingCalculation MovingSum { get; set; }

        /// <summary>
        /// The strategy provided to calculate the short period moving average for volume oscillator indicators.
        /// </summary>
        public DataSourceSupportingCalculation ShortVolumeOscillatorAverage { get; set; }

        /// <summary>
        /// The strategy provided to calculate the long period moving average for volume oscillator indicators.
        /// </summary>
        public DataSourceSupportingCalculation LongVolumeOscillatorAverage { get; set; }

        /// <summary>
        /// The strategy provided to calculate the short period moving average for price oscillator indicators.
        /// </summary>
        public DataSourceSupportingCalculation ShortPriceOscillatorAverage { get; set; }

        /// <summary>
        /// The strategy provided to calculate the long period moving average for price oscillator indicators.
        /// </summary>
        public DataSourceSupportingCalculation LongPriceOscillatorAverage { get; set; }

        /// <summary>
        /// The strategy provided to convert a lamda expression taking the index into the data to convert into
        /// an enumeration over the input data using a configurable range.
        /// </summary>
        public Func<Func<int, double>, int, int, IEnumerable<double>> ToEnumerableRange { get; set; }

        /// <summary>
        /// The strategy provided to convert a lamda expression taking the index into the data to convert into
        /// an enumeration over the input data using a configurable length.
        /// </summary>
        public Func<Func<int, double>, int, IEnumerable<double>> ToEnumerable { get; set; }

        /// <summary>
        /// The strategy provided to make doubles safe for plotting, by default will just make zero if the value
        /// is invalid.
        /// </summary>
        public Func<double, double> MakeSafe { get; set; }
    }

    /// <summary>
    /// Represents a supporting calculation strategy.
    /// </summary>
    /// <typeparam name="TCalculationStrategy">The type of delegate that performs the strategy.</typeparam>
    public class SupportingCalculation<TCalculationStrategy>
    {
        /// <summary>
        /// Constructs a SupportingCalculation
        /// </summary>
        /// <param name="strategy">The strategy to use.</param>
        public SupportingCalculation(TCalculationStrategy strategy)
        {
            _strategy = strategy;
            _basedOn = new List<string>();
        }

        /// <summary>
        /// Constructs a SupportingCalculation
        /// </summary>
        /// <param name="strategy">The strategy to use.</param>
        /// <param name="basedOn">The properties that the calculation is based on.</param>
        public SupportingCalculation(TCalculationStrategy strategy, IList<string> basedOn)
        {
            _strategy = strategy;
            _basedOn = new List<string>(basedOn);
        }

        private TCalculationStrategy _strategy;
        private List<string> _basedOn;

        /// <summary>
        /// The strategy for the calculation.
        /// </summary>
        public TCalculationStrategy Strategy 
        {
            get
            {
                return _strategy;
            }
        }

        /// <summary>
        /// The properties the calculation is based on.
        /// </summary>
        public IList<string> BasedOn
        {
            get
            {



                return _basedOn.AsReadOnly();

            }
        }
    }

    /// <summary>
    /// A strategy for supporting calculations.
    /// </summary>
    /// <param name="source">The source of the data.</param>
    /// <param name="period">The period for the calculation.</param>
    /// <returns>The calculated data.</returns>
    public delegate IEnumerable<double> SupportingCalculationStrategy(IEnumerable<double> source, int period);

    /// <summary>
    /// Represents a calculation strategy that takes in a column of values
    /// and returns a resulting column of values.
    /// </summary>
    public class ColumnSupportingCalculation
        : SupportingCalculation<SupportingCalculationStrategy>
    {
        /// <summary>
        /// Constructs a ColumnSupportingCalculation
        /// </summary>
        /// <param name="strategy">The strategy to use.</param>
        public ColumnSupportingCalculation(SupportingCalculationStrategy strategy)
            : base(strategy)
        {
        }

        /// <summary>
        /// Constructs a ColumnSupportingCalculation
        /// </summary>
        /// <param name="strategy">The strategy to use.</param>
        /// <param name="basedOn">The properties that the calculation is based on.</param>
        public ColumnSupportingCalculation(SupportingCalculationStrategy strategy,
            IList<string> basedOn)
            :base(strategy, basedOn)
         {
         }
    }

    /// <summary>
    /// A strategy to provide columns for the indicator.
    /// </summary>
    /// <param name="dataSource">The datasource to use.</param>
    /// <returns></returns>
    public delegate IEnumerable<double> ProvideColumnValuesStrategy(FinancialCalculationDataSource dataSource);

    /// <summary>
    /// Represents a calculation strategy that uses the calculation data source
    /// to product a column of values.
    /// </summary>
    public class DataSourceSupportingCalculation
        : SupportingCalculation<ProvideColumnValuesStrategy>
    {
        /// <summary>
        /// Constructs a DataSourceSupportingCalculation
        /// </summary>
        /// <param name="strategy">The strategy to use.</param>
        public DataSourceSupportingCalculation(ProvideColumnValuesStrategy strategy)
            :base(strategy)
        {
        }

        /// <summary>
        /// Constructs a DataSourceSupportingCalculation
        /// </summary>
        /// <param name="strategy">The strategy to use.</param>
        /// <param name="basedOn">The properties that the calculation is based on.</param>
        public DataSourceSupportingCalculation(ProvideColumnValuesStrategy strategy,
            IList<string> basedOn)
            :base(strategy, basedOn)
         {
         }
    }

    /// <summary>
    /// Represents a column that is calculated based on the values of other columns.
    /// </summary>
    public class CalculatedColumn
        : IEnumerable<double>
    {
        /// <summary>
        /// Constructs a new calculated column.
        /// </summary>
        /// <param name="valuesProvider">A list that provides the values of the column.</param>
        /// <param name="basedOn">The columns on which this calculated column is based.</param>
        public CalculatedColumn(IEnumerable<double> valuesProvider, params string[] basedOn)
        {
            this._valuesProvider = valuesProvider;
            this._basedOn = new List<string>(basedOn);
        }

        /// <summary>
        /// Constructs a new calculated column instance.
        /// </summary>
        /// <param name="valuesProvider">A list that provides the values for the column.</param>
        /// <param name="basedOn">The columns on which this calculated column is based.</param>
        public CalculatedColumn(IEnumerable<double> valuesProvider, IList<String> basedOn)
        {
            this._valuesProvider = valuesProvider;
            this._basedOn = new List<string>(basedOn);
        }

        private IEnumerable<double> _valuesProvider;
        private List<string> _basedOn;

        /// <summary>
        /// The columns on which this calculated column is based.
        /// </summary>
        public IList<string> BasedOn
        {
            get
            {



                return _basedOn.AsReadOnly();

            }
        }

        #region IEnumerable<double> Members

        /// <summary>
        /// Provides an enumerator for this calculated column.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<double> GetEnumerator()
        {
            return _valuesProvider.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Provides an enumerator for this calculated column.
        /// </summary>
        /// <returns>The enumerator.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _valuesProvider.GetEnumerator();
        }

        #endregion
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