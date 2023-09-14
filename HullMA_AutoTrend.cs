using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class HMAMTF : Indicator
    {

        [Parameter("Price")]
        public HMA.PriceType Price { get; set; }

        [Parameter("Period", DefaultValue = 10, MinValue = 1)]
        public int Period { get; set; }

        [Parameter("MA Type", DefaultValue = MovingAverageType.Weighted)]
        public MovingAverageType MaType { get; set; }


        [Output("UpTrend", PlotType = PlotType.DiscontinuousLine, LineColor = "#00FFFF", Thickness = 3)]
        public IndicatorDataSeries UpTrend { get; set; }

        [Output("DownTrend", PlotType = PlotType.DiscontinuousLine, LineColor = "#DC143C", Thickness = 3)]
        public IndicatorDataSeries DownTrend { get; set; }

        public IndicatorDataSeries Result { get; private set; }

        HullMovingAverage Hma;

        protected override void Initialize()
        {
            Result = CreateDataSeries();
            Hma = Indicators.HullMovingAverage(Bars.ClosePrices, Period);
        }

        public override void Calculate(int index)
        {
            var barIndex = Bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);
            if (barIndex < 0)
                return;

            var result = Hma.Result[barIndex];
            var trend = Hma.Result.Last(0) - Hma.Result.Last(1);

            for (var i = index; i >= Bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[barIndex]); i--)
            {
                Result[i] = result;

                if (trend > 0)
                {
                    UpTrend[i] = result;
                    UpTrend[i - 1] = Result[i - 1];
                    DownTrend[i] = double.NaN;
                }
                else if (trend < 0)
                {
                    DownTrend[i] = result;
                    DownTrend[i - 1] = Result[i - 1];
                    UpTrend[i] = double.NaN;
                }
            }
        }
    }
}
