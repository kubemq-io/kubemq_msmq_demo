using System;
using System.IO;
using System.Messaging;


namespace msmq_generator
{
    class Program
    {

        static void Main(string[] args)
        {

            // Will listen to msmq to remove/add an instrument by client request
            // Will write logs 

            Manager manager = new Manager(@".\private$\receiver", @".\private$\RAQueue");
            Console.ReadLine();

            //try
            //{
            //    queuerec = new MessageQueue(MSMQPath, QueueAccessMode.Receive);
            //    //queue.SetPermissions("Everyone", MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow);
            //    //queue.Authenticate = false;
            //    //queue.EncryptionRequired = EncryptionRequired.None;
            //}
            //catch (Exception ex)
            //{

            //}
  

        }

    }
}
