using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace msmq_generator
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
            buy = GetRateInitalValue();
            sell = GetRateInitalValue();
            id = pId;
            SetRate();
        }
        private void SetRate()
        {
            rateChanger = new System.Timers.Timer(670);

            rateChanger.Elapsed += OnTimedEvent;
            rateChanger.AutoReset = true;
            rateChanger.Enabled = true;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            buy = buy * GetDoubleRandomNumber(0.8, 1.2);
            sell= sell * GetDoubleRandomNumber(0.8, 1.2);
        }

        private double GetDoubleRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        private int GetRateInitalValue()
        {
            Random random = new Random();
            return random.Next(450, 550);
        }
    }
}
