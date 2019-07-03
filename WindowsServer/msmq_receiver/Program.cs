using System;

namespace msmq_receiver
{
    class Program
    {
        
        static void Main(string[] args)
        {
            // will start by connecting to kubemq
            try
            {
                queue = new MessageQueue(MSMQPath, QueueAccessMode.Receive);
                //queue.SetPermissions("Everyone", MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow);
                //queue.Authenticate = false;
                //queue.EncryptionRequired = EncryptionRequired.None;
            }
            catch (Exception ex)
            {

            }

            // will run as a container
            // will pull msmq messages from server queue
            // will send it to presistent channel 
            // Will listen to request for adding a new msmq message and send it to msmq_generator
            // Will write logs 


            Console.WriteLine("Hello World!");

        }
    }
}
