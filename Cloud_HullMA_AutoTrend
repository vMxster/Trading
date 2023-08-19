using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo
{
    [Cloud("Fast MA", "Slow MA")]
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MaCrossCloud : Indicator
    {
        [Parameter("Fast MA Period", DefaultValue = 10)]
        public int FastMaPeriod { get; set; }

        [Parameter("Slow MA Period", DefaultValue = 21)]
        public int SlowMaPeriod { get; set; }

        [Output("Fast MA", LineColor = "#FF6666")]      // Cloud Color Fast MA Above Slow MA
        public IndicatorDataSeries FastMaResult { get; set; }

        [Output("Slow MA", LineColor = "#0071C1")]      // Cloud Color Slow MA Above Fast MA
        public IndicatorDataSeries SlowMaResult { get; set; }
        
        [Output("FastUpTrend", PlotType = PlotType.DiscontinuousLine, LineColor = "#FF01AF50", Thickness = 1)]
        public IndicatorDataSeries FastUpTrend { get; set; }

        [Output("FastDownTrend", PlotType = PlotType.DiscontinuousLine, LineColor = "#DC143C", Thickness = 1)]
        public IndicatorDataSeries FastDownTrend { get; set; }
        
        [Output("SlowUpTrend", PlotType = PlotType.DiscontinuousLine, LineColor = "#FF01AF50", Thickness = 1)]
        public IndicatorDataSeries SlowUpTrend { get; set; }

        [Output("SlowDownTrend", PlotType = PlotType.DiscontinuousLine, LineColor = "#DC143C", Thickness = 1)]
        public IndicatorDataSeries SlowDownTrend { get; set; }
        
        public IndicatorDataSeries FastResult { get; private set; }
        
        public IndicatorDataSeries SlowResult { get; private set; }


        HullMovingAverage FastMa;
        HullMovingAverage SlowMa;

        protected override void Initialize()
        {
            FastResult = CreateDataSeries();
            SlowResult = CreateDataSeries();
            FastMa = Indicators.HullMovingAverage(Bars.ClosePrices, FastMaPeriod);
            SlowMa = Indicators.HullMovingAverage(Bars.ClosePrices, SlowMaPeriod);
        }

        public override void Calculate(int index)
        {
            FastMaResult[index] = FastMa.Result[index];
            SlowMaResult[index] = SlowMa.Result[index];
            
            var barIndex = Bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);
            if (barIndex < 0)
                return;

            var fastresult = FastMa.Result[barIndex];
            var slowresult = SlowMa.Result[barIndex];
            var fasttrend = FastMa.Result.Last(0) - FastMa.Result.Last(1);
            var slowtrend = SlowMa.Result.Last(0) - SlowMa.Result.Last(1);

            for (var i = index; i >= Bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[barIndex]); i--)      // FastHMA
            {
                FastResult[i] = fastresult;

                if (fasttrend > 0)
                {
                    FastUpTrend[i] = fastresult;
                    FastUpTrend[i - 1] = FastResult[i - 1];
                    FastDownTrend[i] = double.NaN;
                }
                else if (fasttrend < 0)
                {
                    FastDownTrend[i] = fastresult;
                    FastDownTrend[i - 1] = FastResult[i - 1];
                    FastUpTrend[i] = double.NaN;
                }
            }
            
            for (var j = index; j >= Bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[barIndex]); j--)      // SlowHMA
            {
                SlowResult[j] = slowresult;

                if (slowtrend > 0)
                {
                    SlowUpTrend[j] = slowresult;
                    SlowUpTrend[j - 1] = SlowResult[j - 1];
                    SlowDownTrend[j] = double.NaN;
                }
                else if (slowtrend < 0)
                {
                    SlowDownTrend[j] = slowresult;
                    SlowDownTrend[j - 1] = SlowResult[j - 1];
                    SlowUpTrend[j] = double.NaN;
                }
            }
        }
    }
}
