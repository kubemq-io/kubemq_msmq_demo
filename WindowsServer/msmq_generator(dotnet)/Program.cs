﻿using System;
using System.IO;
using System.Messaging;


namespace msmq_generator
{
    class Program
    {

        static void Main(string[] args)
        {
            //Start with creating a queue on the server machine:
            MessageQueue queue = null;
            try
            {
                queue = new MessageQueue(@".\private$\RAQueue", QueueAccessMode.Send);
                //queue.SetPermissions("Everyone", MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow);
                //queue.Authenticate = false;
                //queue.EncryptionRequired = EncryptionRequired.None;
            }
            catch (Exception ex)
            {

            }

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

            if (queue != null)
            {

                using (StreamReader readFile = new StreamReader("rates.csv"))
                {
                    string line;
                    string[] row;                 
                    while ((line = readFile.ReadLine()) != null)
                    {

                        row = line.Split(',');
                        Message message = new Message()
                        {
                            ID = row[0],
                            Name = row[1],
                            Ask = row[2],
                            Bid = row[3]
                        };

                        try
                        {
                            string strLabel = "";


                            System.Messaging.Message newMessage = new System.Messaging.Message(message, new BinaryMessageFormatter());
                            newMessage.Label = strLabel;
                            queue.Send(newMessage, MessageQueueTransactionType.None);
                        }
                        catch (Exception ex)
                        {

                        }

                    }


                }
            }

        }

    }

    [Serializable]
    class Message
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Ask { get; set; }
        public string Bid { get; set; }   

    }
}
