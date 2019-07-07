using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace kubemq_msmq_rates_generator
{

    /// <summary>
    /// Rates: A class that contain the rate detail and generate is value
    /// </summary>
    internal class Rates
    {
        internal bool isActive;
        internal string rateName;
        internal double buy;
        internal double sell;
        internal int id;
        private static System.Timers.Timer rateChanger;
        public Rates(string pName,int pId)
        {
            isActive = true;
            rateName = pName;
            buy = GetRateInitialValue();
            sell = GetRateInitialValue();
            id = pId;
            SetRateChangeTimer();
        }
        /// <summary>
        /// Set a timer changing the rate.
        /// </summary>
        private void SetRateChangeTimer()
        {
            rateChanger = new System.Timers.Timer(670);

            rateChanger.Elapsed += OnRateChangeEvent;
            rateChanger.AutoReset = true;
            rateChanger.Enabled = true;
        }

        /// <summary>
        /// Rate Change event.
        /// </summary>
        private void OnRateChangeEvent(object sender, ElapsedEventArgs e)
        {
            buy =  GetDoubleRandomNumber(0.995, 1.005, buy);
            sell=  GetDoubleRandomNumber(0.995, 1.005, sell);
        }

        /// <summary>
        /// Generate random relative positive double.
        /// </summary>
        /// <param name="minimum">Minimum Change</param>
        /// <param name="maximum">Maximum Change</param>
        /// <param name="currentValue">The current Value</param>
        /// <returns>A relative value that is not under 2000 and does not exceed 10000</returns>
        private double GetDoubleRandomNumber(double minimum, double maximum,double currentValue)
        {
            double randDouble= Manager.rnd.NextDouble() * (maximum - minimum) + minimum;
            currentValue = currentValue * randDouble;
            if (currentValue < 2000)
            {
                currentValue = 2200;
            }
            else if (currentValue > 10000)
            {
                currentValue = 9800;
            }
            return currentValue;
        }

        /// <summary>
        /// Get Initial value for rate.
        /// </summary>
        /// <returns> int between 4500-7800</returns>
        private int GetRateInitialValue()
        {
            Random random = new Random();
            return random.Next(4500, 7800);
        }
    }
}
