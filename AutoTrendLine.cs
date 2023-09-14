using System;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, AccessRights = AccessRights.None)]
    public class AutoTrendLine : Indicator
    {
        [Parameter(DefaultValue = 25, MinValue = 14)]
        public int Period { get; set; }

        protected override void Initialize()
        {
            Redrawing();
        }

        public override void Calculate(int index)
        {
            if (IsLastBar)
                Redrawing();
        }

        private void Redrawing()
        {
            int count = Bars.ClosePrices.Count;

            int max1 = FindNextPivot(Bars.HighPrices, count - 1, true);
            int max2 = FindNextPivot(Bars.HighPrices, max1 - Period, true);

            int min1 = FindNextPivot(Bars.LowPrices, count - 1, false);
            int min2 = FindNextPivot(Bars.LowPrices, min1 - Period, false);

            int startPoint = Math.Min(max2, min2) - 100;
            int endPoint = count + 100;

            DrawTrendLine("high", startPoint, endPoint, max1, Bars.HighPrices[max1], max2, Bars.HighPrices[max2]);

            DrawTrendLine("low", startPoint, endPoint, min1, Bars.LowPrices[min1], min2, Bars.LowPrices[min2]);
        }

        private void DrawTrendLine(string lineName, int startIndex, int endIndex, int index1, double value1, int index2, double value2)
        {
            double gradient = (value2 - value1) / (index2 - index1);

            double startValue = value1 + (startIndex - index1) * gradient;
            double endValue = value1 + (endIndex - index1) * gradient;

            Chart.DrawTrendLine(lineName, startIndex, startValue, endIndex, endValue, Color.Gray);
            Chart.DrawTrendLine(lineName + "_red", index1, value1, index2, value2, Color.Red);
        }

        private int FindNextPivot(DataSeries series, int maxIndex, bool findMax)
        {
            for (int index = maxIndex; index >= 0; index--)
            {
                if (IsPivot(series, index, findMax))
                {
                    return index;
                }
            }
            return 0;
        }

        private bool IsPivot(DataSeries series, int index, bool findMax)
        {
            int end = Math.Min(index + Period, series.Count - 1);
            int start = Math.Max(index - Period, 0);

            double value = series[index];

            for (int i = start; i < end; i++)
            {
                if (findMax && value < series[i])
                    return false;

                if (!findMax && value > series[i])
                    return false;
            }
            return true;
        }
    }
}
