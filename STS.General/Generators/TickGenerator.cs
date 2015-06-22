using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS.General.Generators
{
    public class TickGenerator
    {
        private System.Random random = new System.Random();
        private DateTime dateTime = DateTime.Now;

        private string[] symbols;
        private int[] digits;
        private double[] pipsizes;
        private double[] prices;
        private string[] providers;

        private static DateTime MaxDateTime = DateTime.MaxValue.AddSeconds(-30);

        public TickGenerator()
        {
            //2013-11-12 13:00 
            //<symbol>;<digits>;<price>
            var data = new string[] { "USDCHF;4;0.9197", "GBPUSD;4;1.5880", "EURUSD;4;1.3403", "USDJPY;2;99.73", "EURCHF;4;1.2324", "AUDBGN;4;1.3596", "AUDCHF;4;0.8567", "AUDJPY;2;92.96", 
                "BGNJPY;2;68.31", "BGNUSD;4;0.6848", "CADBGN;4;1.3901", "CADCHF;4;0.8759", "CADUSD;4;0.9527", "CHFBGN;4;1.5862", "CHFJPY;2;108.44", "CHFUSD;4;1.0875", "EURAUD;4;1.4375", "EURCAD;4;1.4064", 
                "EURGBP;4;0.8438", "EURJPY;4;133.66", "GBPAUD;4;1.7031", "GBPBGN;4;2.3169", "GBPCAD;4;1.6661", "GBPCHF;4;1.4603", "GBPJPY;2;158.37", "NZDUSD;4;0.8217", "USDBGN;4;1.4594", "USDCAD;4;1.0493",
                "XAUUSD;2;1281.15", "XAGUSD;2;21.21", "$DAX;2;9078.20","$FTSE;2;6707.49","$NASDAQ;2;3361.02","$SP500;2;1771.32"};

            providers = new string[] { "eSignal", "Gain", "NYSE", "TSE", "NASDAQ", "Euronext", "LSE", "SSE", "ASE", "SE", "NSEI" };

            symbols = new string[data.Length];
            digits = new int[data.Length];
            pipsizes = new double[data.Length];
            prices = new double[data.Length];

            var format = new NumberFormatInfo();
            format.NumberDecimalSeparator = ".";

            for (int i = 0; i < data.Length; i++)
            {
                var tokens = data[i].Split(';');

                symbols[i] = tokens[0];
                digits[i] = Int32.Parse(tokens[1]);
                pipsizes[i] = Math.Round(Math.Pow(10, -digits[i]), digits[i]);
                prices[i] = Math.Round(Double.Parse(tokens[2], format), digits[i]);
            }
        }

        public Tick Next()
        {
            //random symbol
            int id = random.Next(symbols.Length);
            string symbol = symbols[id];
            int d = digits[id];
            double pipSize = pipsizes[id];

            //random walk movement
            int direction = random.Next() % 2 == 0 ? 1 : -1;
            int pips = random.Next(0, 10);
            int spread = random.Next(2, 30);
            int seconds = random.Next(1, 30);

            //random values
            if (dateTime < MaxDateTime)
                dateTime = dateTime.AddSeconds(seconds);
            else
                dateTime = DateTime.Now;

            double bid = Math.Round(prices[id] + direction * pips * pipSize, d);
            double ask = Math.Round(bid + spread * pipSize, d);
            int bidSize = random.Next(0, 10000);
            int askSize = random.Next(0, 10000);
            string provider = providers[random.Next(providers.Length)];

            //create tick
            Tick tick = new Tick(symbol, dateTime, bid, ask, bidSize, askSize, provider);

            prices[id] = bid;

            return tick;
        }
    }
}
