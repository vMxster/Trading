using System.Linq;
using cAlgo.API;
using cAlgo.API.Requests;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot()]
    public class PersistentAnti : Robot
    {
        // Main input parameters
        // Tenkan line period. The fast "moving average".
        [Parameter(DefaultValue = 9, MinValue = 2)]
        public int Tenkan { get; set; }

        // Kijun line period. The slow "moving average".
        [Parameter(DefaultValue = 26, MinValue = 2)]
        public int Kijun { get; set; }

        // Senkou period. Used for Kumo (Cloud) spans.
        [Parameter(DefaultValue = 52, MinValue = 2)]
        public int Senkou { get; set; }

        // Money management
        // Basic position size used with MM = 0.
        [Parameter(DefaultValue = 10000, MinValue = 0)]
        public int Volume { get; set; }

        // Miscellaneous
        [Parameter(DefaultValue = "Ichimoku-Chikou-Cross")]
        public string Comment { get; set; }

        // Tolerated slippage in brokers' pips.
        [Parameter(DefaultValue = 100, MinValue = 0)]
        public int Slippage { get; set; }

        // Common
        private bool HaveLongPosition;
        private bool HaveShortPosition;

        // Entry signals
        private bool ChikouPriceBull = false;
        private bool ChikouPriceBear = false;
        private bool KumoBullConfirmation = false;
        private bool KumoBearConfirmation = false;
        private bool KumoChikouBullConfirmation = false;
        private bool KumoChikouBearConfirmation = false;

        // Indicator handles
        private IchimokuKinkoHyo Ichimoku;

        protected override void OnStart()
        {
            Ichimoku = Indicators.IchimokuKinkoHyo(Tenkan, Kijun, Senkou);
        }

        private Position position
        {
            get { return Positions.FirstOrDefault(pos => ((pos.Label == Comment) && (SymbolName == Symbol.Name))); }
        }

        protected override void OnBar()
        {
            int latest_bar = Bars.ClosePrices.Count - 1;
            // Latest bar index
            // Chikou/Price Cross
            double ChikouSpanLatest = Ichimoku.ChikouSpan[latest_bar - (Kijun + 1)];
            // Latest closed bar with Chikou.
            double ChikouSpanPreLatest = Ichimoku.ChikouSpan[latest_bar - (Kijun + 2)];
            // Bar older than latest closed bar with Chikou.
            // Bullish entry condition
            if ((ChikouSpanLatest > Bars.ClosePrices[latest_bar - (Kijun + 1)]) && (ChikouSpanPreLatest <= Bars.ClosePrices[latest_bar - (Kijun + 2)]))
            {
                ChikouPriceBull = true;
                ChikouPriceBear = false;
            }
            // Bearish entry condition
            else if ((ChikouSpanLatest < Bars.ClosePrices[latest_bar - (Kijun + 1)]) && (ChikouSpanPreLatest >= Bars.ClosePrices[latest_bar - (Kijun + 2)]))
            {
                ChikouPriceBull = false;
                ChikouPriceBear = true;
            }
            // Voiding entry conditions if cross is ongoing.
            else if (ChikouSpanLatest == Bars.ClosePrices[latest_bar - (Kijun + 1)])
            {
                ChikouPriceBull = false;
                ChikouPriceBear = false;
            }

            // Kumo confirmation. When cross is happening current price (latest close) should be above/below both Senkou Spans, or price should close above/below both Senkou Spans after a cross.
            double SenkouSpanALatestByPrice = Ichimoku.SenkouSpanA[latest_bar - 1];
            // Senkou Span A at time of latest closed price bar.
            double SenkouSpanBLatestByPrice = Ichimoku.SenkouSpanB[latest_bar - 1];
            // Senkou Span B at time of latest closed price bar.
            if ((Bars.ClosePrices[latest_bar - 1] > SenkouSpanALatestByPrice) && (Bars.ClosePrices[latest_bar - 1] > SenkouSpanBLatestByPrice))
                KumoBullConfirmation = true;
            else
                KumoBullConfirmation = false;
            if ((Bars.ClosePrices[latest_bar - 1] < SenkouSpanALatestByPrice) && (Bars.ClosePrices[latest_bar - 1] < SenkouSpanBLatestByPrice))
                KumoBearConfirmation = true;
            else
                KumoBearConfirmation = false;

            // Kumo/Chikou confirmation. When cross is happening Chikou at its latest close should be above/below both Senkou Spans at that time, or it should close above/below both Senkou Spans after a cross.
            double SenkouSpanALatestByChikou = Ichimoku.SenkouSpanA[latest_bar - (Kijun + 1)];
            // Senkou Span A at time of latest closed bar of Chikou span.
            double SenkouSpanBLatestByChikou = Ichimoku.SenkouSpanB[latest_bar - (Kijun + 1)];
            // Senkou Span B at time of latest closed bar of Chikou span.
            if ((ChikouSpanLatest > SenkouSpanALatestByChikou) && (ChikouSpanLatest > SenkouSpanBLatestByChikou))
                KumoChikouBullConfirmation = true;
            else
                KumoChikouBullConfirmation = false;
            if ((ChikouSpanLatest < SenkouSpanALatestByChikou) && (ChikouSpanLatest < SenkouSpanBLatestByChikou))
                KumoChikouBearConfirmation = true;
            else
                KumoChikouBearConfirmation = false;

            GetPositionStates();

            if (ChikouPriceBull)
            {
                if (HaveShortPosition)
                    ClosePrevious();
                if ((KumoBullConfirmation) && (KumoChikouBullConfirmation))
                {
                    ChikouPriceBull = false;
                    fBuy();
                }
            }
            else if (ChikouPriceBear)
            {
                if (HaveLongPosition)
                    ClosePrevious();
                if ((KumoBearConfirmation) && (KumoChikouBearConfirmation))
                {
                    fSell();
                    ChikouPriceBear = false;
                }
            }
        }

        private void GetPositionStates()
        {
            if (position != null)
            {
                if (position.TradeType == TradeType.Buy)
                {
                    HaveLongPosition = true;
                    HaveShortPosition = false;
                    return;
                }
                else if (position.TradeType == TradeType.Sell)
                {
                    HaveLongPosition = false;
                    HaveShortPosition = true;
                    return;
                }
            }
            HaveLongPosition = false;
            HaveShortPosition = false;
        }

        private void ClosePrevious()
        {
            if (position == null)
                return;
            ClosePosition(position);
        }

        private void fBuy()
        {
            ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume, Comment);
        }

        private void fSell()
        {
            ExecuteMarketOrder(TradeType.Sell, SymbolName, Volume, Comment);
        }
    }
}
