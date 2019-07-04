using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Timers;

namespace msmq_generator
{
    /// <summary>
    /// Manager run the progrem main logic
    /// </summary>
    class Manager
    {
        private Dictionary<string,Rates> rateCollection;
        private string ListenPath;
        private string SendPath;
        private static System.Timers.Timer timer;
        public Manager(string pListenQueuePath, string pSendRateQueue)
        {
            ListenPath = pListenQueuePath;
            SendPath = pSendRateQueue;
            StartListen();
            startSendingRates();
            Console.WriteLine($"initialized Manger on path {ListenPath}");

        }
        /// <summary>
        /// Start sending rate to MSMQ under selected path
        /// </summary>
        private void startSendingRates()
        {
            bool exist = false;
            try
            {
                exist = MessageQueue.Exists(SendPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to check if Queue {SendPath} exists on exception {ex}");
                throw ex;
            }
            if (!exist)
            {
                Console.WriteLine($"Queue {SendPath} does not exists, creating new Queue");
                try
                {
                    MessageQueue rateQueue = MessageQueue.Create(SendPath);
                    Console.WriteLine($"Queue {SendPath} created");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create Queue {SendPath} on exception {ex}");
                    throw ex;
                }
            }
            rateCollection = new Dictionary<string, Rates>
            {

                {"Golden Coin", new Rates("Golden Coin") },
                {"GIL",new Rates("GIL") },
                {"Zenny", new Rates("Zenny") },
                {"Dollar", new Rates("Dollar") },
                {"Red Orbs", new Rates("Red Orbs") },
                {"Credit", new Rates("Credit") },
                {"Vespene gas",new Rates("Vespene gas") }
            };
            Console.WriteLine($"Starting to generate rates, will send them to {SendPath}");
            SetTimer();
        }
        /// <summary>
        /// Starts receiving request from MSMQ under selected path
        /// </summary>
        private void StartListen()
        {
            bool exist = false;
            try
            {
                exist = MessageQueue.Exists(ListenPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to check if Queue {ListenPath} exists on exception {ex}");
                throw ex;
            }
            if (!exist)
            {
                Console.WriteLine($"Queue {ListenPath} does not exists, creating new Queue");
                try
                {
                    MessageQueue rateQueue = MessageQueue.Create(ListenPath);
                    Console.WriteLine($"Queue {ListenPath} created");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create Queue {ListenPath} on exception {ex}");
                    throw ex;
                }
            }
            MessageQueue messageQueue = new MessageQueue(ListenPath);
            messageQueue.Formatter = new XmlMessageFormatter(new Type[]
            {typeof(String)});
            messageQueue.ReceiveCompleted += new ReceiveCompletedEventHandler((s, a) => MyReciveCompleted(s, a));
            messageQueue.BeginReceive();
        }

        private void MyReciveCompleted(object source, ReceiveCompletedEventArgs asyncResult)
        {
            Console.WriteLine($"Received message to queue");
            // Connect to the queue.
            MessageQueue mq = (MessageQueue)source;

            // End the asynchronous Receive operation.
            Message m = mq.EndReceive(asyncResult.AsyncResult);
            RateRequest request = new RateRequest();
            // Display message information on the screen.
            try
            {

                request= JsonConvert.DeserializeObject<RateRequest>((string)m.Body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Message: {(string)m.Body}");
                throw;
            }
            Console.WriteLine($"Message: {(string)m.Body}");
            switch (request.Active)
            {
                case true:
                        rateCollection[request.Name].isActive = true;
                    break;
                case false:
                    rateCollection[request.Name].isActive = false;
                    break;
                default:
                    break;
            }
            // Restart the asynchronous Receive operation.
            mq.BeginReceive();

            return;
        }


        private void SetTimer()
        {

            timer = new System.Timers.Timer(800);

            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            foreach (var rate in rateCollection)
            {
                if (rate.Value.isActive)
                {
                    SenderMessageBody message = new SenderMessageBody()
                    {
                        Name = rate.Value.rateName,
                        Ask = rate.Value.buy.ToString(),
                        Bid = rate.Value.sell.ToString()
                    };
                    try
                    {
                        string strLabel = "";


                        System.Messaging.Message newMessage = new System.Messaging.Message(message, new BinaryMessageFormatter());
                        newMessage.Label = strLabel;
                        MessageQueue queue = new MessageQueue(SendPath, QueueAccessMode.Send);
                        queue.Send(newMessage, MessageQueueTransactionType.None);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send rates to queue on ex {ex}");
                    }
                }
            }
        }
    }
    /// <summary>
    /// Rate sending to the Client
    /// </summary>
    [Serializable]
    class SenderMessageBody
    {
        public string Name { get; set; }
        public string Ask { get; set; }
        public string Bid { get; set; }
    }

    /// <summary>
    /// Request Receiving from the Client
    /// </summary>
    [Serializable]
    public class RateRequest
    {
        public string Name { get; set; }
        public bool Active { get; set; }
    }
}
