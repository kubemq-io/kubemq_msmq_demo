using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using MsmqAutoTester;
namespace MSMQWorkerConsole
{
    class Program
    {
        private static readonly AutoResetEvent waitHandle = new AutoResetEvent(false);
        static void Main(string[] args)
        {
            NLog.LogManager.GetCurrentClassLogger().Info("~~~~ Starting MSMQWorker ~~~~");

            var servicesProvider = Startup.Init();


            Manager manager = servicesProvider.GetRequiredService<Manager>();
            Console.CancelKeyPress += (o, e) =>
            {
                Console.WriteLine("Exit");

                // Allow the manin thread to continue and exit...
                waitHandle.Set();
            };
            //wait
            waitHandle.WaitOne();
        }
    }
}
