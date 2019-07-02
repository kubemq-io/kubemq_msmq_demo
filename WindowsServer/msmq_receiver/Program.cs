using System;

namespace msmq_receiver
{
    class Program
    {
        
        static void Main(string[] args)
        {
            // will start by connecting to kubemq
            // will run as a container
            // will pull msmq messages from server queue
            // will send it to presistent channel 
            // Will listen to request for adding a new msmq message and send it to msmq_generator
            // Will write logs 


            Console.WriteLine("Hello World!");

        }
    }
}
