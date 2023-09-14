using System;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, AccessRights = AccessRights.None)]
    public class KAMA : Indicator
    {
        [Parameter]
        public DataSeries Source { get; set; }

        [Parameter(DefaultValue = 2)]
        public int FastMA { get; set; }
		
        [Parameter(DefaultValue = 30)]
        public int SlowMA { get; set; }
		
        [Parameter(DefaultValue = 10)]
        public int KAMA_Period { get; set; }
        
        [Output("KAMA")]
        public IndicatorDataSeries Kama { get; set; }
		
		private IndicatorDataSeries diff;
		double sum;
		protected override void Initialize()
        {
            diff = CreateDataSeries();
        }
		
        public override void Calculate(int index)
        {
			if(index>0)
			{
				diff[index] = Math.Abs(Source[index] - Source [index-1]);
			}
			if(index<KAMA_Period)
			{
				Kama[index] = Source[index];
				return;
			}
			double fastd = 2.0 / (double)(FastMA + 1);
			double slowd = 2.0 / (double)(SlowMA + 1);
			
			double signal = Math.Abs(Source[index] - Source[index-KAMA_Period]);
			sum=0;
			for(int i = 0; i<KAMA_Period;i++)
			{
				sum+= diff[index-i];
			}
			double noise  = sum;
			if (noise == 0) 
			{
				Kama[index] = Kama[index-1];
				return;
			}
				double smooth = Math.Pow((signal / noise) * (fastd - slowd) + slowd, 2);
			
			Kama[index] = Kama[index-1] + smooth * (Source[index]-Kama[index-1]);
        }
    }
}
