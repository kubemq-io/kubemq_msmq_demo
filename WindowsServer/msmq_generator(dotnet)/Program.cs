using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace kubemq_msmq_rates_generator
{
    class Program
    {
        private static readonly AutoResetEvent waitHandle = new AutoResetEvent(false);
        static void Main(string[] args)
        {
            NLog.LogManager.GetCurrentClassLogger().Info("~~~~ Starting kubemqMSMQ-RateGenerator ~~~~");

            var servicesProvider = Startup.Init();


            Manager manager = servicesProvider.GetRequiredService<Manager>();
            Console.CancelKeyPress += (o, e) =>
            {
                Console.WriteLine("Exit");

                // Allow the main thread to continue and exit...
                waitHandle.Set();
            };
            //wait
            waitHandle.WaitOne();
        }

    }

}
