using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;
using Infragistics.Controls.Charts;
using System.ComponentModel;




namespace InfragisticsWPF4.Controls.Charts.XamDataChart.Design



{
    internal partial class MetadataStore : IProvideAttributeTable
    {
        private void AddCustomAttributes(AttributeTableBuilder builder)
        {

            builder.AddCustomAttributes(typeof(Infragistics.Controls.Charts.XamDataChart), "Series", new NewItemTypesAttribute(
                                                                             typeof(AreaSeries),
                                                                             typeof(ColumnSeries),
                                                                             typeof(BarSeries),
                                                                             typeof(LineSeries),
                                                                             typeof(SplineAreaSeries),
                                                                             typeof(SplineSeries),
                                                                             typeof(StepAreaSeries),
                                                                             typeof(StepLineSeries),
                                                                             typeof(WaterfallSeries),
                                                                             typeof(RangeAreaSeries),
                                                                             typeof(RangeColumnSeries),
                                                                             typeof(ScatterLineSeries),
                                                                             typeof(ScatterSeries),
                                                                             typeof(ScatterSplineSeries),
                                                                             typeof(FinancialPriceSeries),
                                                                             typeof(AbsoluteVolumeOscillatorIndicator),
                                                                             typeof(AccumulationDistributionIndicator),
                                                                             typeof(AverageDirectionalIndexIndicator),
                                                                             typeof(AverageTrueRangeIndicator),
                                                                             typeof(BollingerBandWidthIndicator),
                                                                             typeof(ChaikinOscillatorIndicator),
                                                                             typeof(CommodityChannelIndexIndicator),
                                                                             typeof(CustomIndicator),
                                                                             typeof(DetrendedPriceOscillatorIndicator),
                                                                             typeof(EaseOfMovementIndicator),
                                                                             typeof(FastStochasticOscillatorIndicator),
                                                                             typeof(ForceIndexIndicator),
                                                                             typeof(FullStochasticOscillatorIndicator),
                                                                             typeof(MarketFacilitationIndexIndicator),
                                                                             typeof(MedianPriceIndicator),
                                                                             typeof(WeightedCloseIndicator),
                                                                             typeof(MassIndexIndicator),
                                                                             typeof(MoneyFlowIndexIndicator),
                                                                             typeof(MovingAverageConvergenceDivergenceIndicator),
                                                                             typeof(NegativeVolumeIndexIndicator),
                                                                             typeof(OnBalanceVolumeIndicator),
                                                                             typeof(PercentagePriceOscillatorIndicator),
                                                                             typeof(PercentageVolumeOscillatorIndicator),
                                                                             typeof(PositiveVolumeIndexIndicator),
                                                                             typeof(PriceVolumeTrendIndicator),
                                                                             typeof(RateOfChangeAndMomentumIndicator),
                                                                             typeof(RelativeStrengthIndexIndicator),
                                                                             typeof(SlowStochasticOscillatorIndicator),
                                                                             typeof(StandardDeviationIndicator),
                                                                             typeof(StochRSIIndicator),
                                                                             typeof(TRIXIndicator),
                                                                             typeof(TypicalPriceIndicator),
                                                                             typeof(UltimateOscillatorIndicator),
                                                                             typeof(WilliamsPercentRIndicator),
                                                                             typeof(BollingerBandsOverlay),
                                                                             typeof(PriceChannelOverlay),
                                                                             typeof(PolarAreaSeries),
                                                                             typeof(PolarLineSeries),
                                                                             typeof(PolarScatterSeries),
                                                                             typeof(PolarSplineSeries),
                                                                             typeof(RadialAreaSeries),
                                                                             typeof(RadialColumnSeries),
                                                                             typeof(RadialLineSeries),
                                                                             typeof(RadialPieSeries),
                                                                             typeof(BubbleSeries),
                                                                             typeof(ValueOverlay),
                                                                             typeof(StackedColumnSeries),
                                                                             typeof(Stacked100ColumnSeries),
                                                                             typeof(StackedBarSeries),
                                                                             typeof(Stacked100BarSeries),
                                                                             typeof(StackedAreaSeries),
                                                                             typeof(Stacked100AreaSeries),
                                                                             typeof(StackedLineSeries),
                                                                             typeof(Stacked100LineSeries),
                                                                             typeof(StackedSplineSeries),
                                                                             typeof(Stacked100SplineSeries),
                                                                             typeof(StackedSplineAreaSeries),
                                                                             typeof(Stacked100SplineAreaSeries),
                                                                             typeof(PointSeries)
                                                                             ));
            builder.AddCustomAttributes(typeof(Infragistics.Controls.Charts.XamDataChart), "Axes", new NewItemTypesAttribute(
                                                                           typeof(CategoryXAxis),
                                                                           typeof(CategoryYAxis),
                                                                           typeof(CategoryDateTimeXAxis),
                                                                           typeof(NumericXAxis),
                                                                           typeof(NumericYAxis),
                                                                           typeof(NumericAngleAxis),
                                                                           typeof(NumericRadiusAxis),
                                                                           typeof(CategoryAngleAxis)
                                                                           ));
            builder.AddCustomAttributes(typeof(Infragistics.Controls.Charts.Series), "View", new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden));
            builder.AddCustomAttributes(typeof(Infragistics.Controls.Charts.Series), "Chart", new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden));
            builder.AddCustomAttributes(typeof(Infragistics.Controls.Charts.Series), "SyncLink", new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden));
            builder.AddCustomAttributes(typeof(Infragistics.Controls.Charts.Series), "SeriesViewer", new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden));
            builder.AddCustomAttributes(typeof(Infragistics.Controls.Charts.Axis), "Chart", new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden));
            builder.AddCustomAttributes(typeof(Infragistics.Controls.Charts.Axis), "SeriesViewer", new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden));
            builder.AddCustomAttributes(typeof(Infragistics.Controls.Charts.NumericAxisBase), "ActualTickmarkValues", new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden));
        }

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