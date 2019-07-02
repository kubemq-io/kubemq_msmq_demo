using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

namespace MSMQTester
{
    class Program
    {
        private static readonly AutoResetEvent waitHandle = new AutoResetEvent(false);
        static void Main(string[] args)
        {
            NLog.LogManager.GetCurrentClassLogger().Info("~~~~ Starting MSMQWORKER ~~~~");

            var servicesProvider = Startup.Init();


            Manager manager = servicesProvider.GetRequiredService<Manager>();
            if (!IsConsole())
            {
                // Handle Control+C or Control+Break
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

        private static bool IsConsole()
        {
            bool isConsole = !(Console.IsOutputRedirected && Console.IsInputRedirected);

            NLog.LogManager.GetCurrentClassLogger().Info($"Is running as Console: {isConsole}");

            return isConsole;
        }
    }
}
